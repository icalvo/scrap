using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Basic.Reference.Assemblies;
using HtmlAgilityPack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;
using Scrap.Pages;

namespace Scrap.Resources.FileSystem
{
    public class CompiledDestinationProvider : IDestinationProvider
    {
        private readonly string[] _destinationFolderPattern;
        private IDestinationProvider _compiledDestinationProvider = null!;
        private readonly ILogger<CompiledDestinationProvider> _logger;
        private CompiledDestinationProvider(string[] destinationFolderPattern, ILogger<CompiledDestinationProvider> logger)
        {
            _destinationFolderPattern = destinationFolderPattern;
            _logger = logger;
        }

        public static CompiledDestinationProvider CreateCompiled(string[] destinationFolderPattern, ILogger<CompiledDestinationProvider> logger)
        {
            var result = new CompiledDestinationProvider(destinationFolderPattern, logger);
            result.Compile();
            return result;
        }

        public Task<string> GetDestinationAsync(
            string destinationRootFolder,
            Page page,
            int pageIndex,
            Uri resourceUrl,
            int resourceIndex)
        {
            return _compiledDestinationProvider.GetDestinationAsync(destinationRootFolder, page, pageIndex, resourceUrl, resourceIndex);
        }

        private void Compile()
        {
            string sourceCode = GenerateSourceCode();
            try
            {
                var assembly = CompileSourceCode(sourceCode);
                _compiledDestinationProvider = CreateDestinationProviderInstance(assembly);
            }
            catch (Exception)
            {
                _logger.LogDebug("Source: {SourceCode}", sourceCode);
                throw;
            }
        }

        private string GenerateSourceCode()
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ??
                                throw new Exception("Cannot find entry assembly path");
            var sourcePath = Path.Combine(directoryName, @"Resources\FileSystem\TemplateDestinationProvider.cs");

            // define source code, then parse it (to the type used for compilation)
            string sourceCode = File.ReadAllText(sourcePath);
            var callChain = string.Join("", _destinationFolderPattern.Select(p => $".C({p})"));
            var pattern = $"rootFolder{callChain}.ToPath()";
            sourceCode = sourceCode.Replace("\"destinationFolderPattern\"", pattern);
            return sourceCode;
        }

        private Assembly CompileSourceCode(string sourceCode)
        {
            _logger.LogDebug("Compiling destination expression...");
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            // define other necessary objects for compilation
            string assemblyName = Path.GetRandomFileName();

            var references = ReferenceAssemblies.Net60.Concat(new[]
            {
                MetadataReference.CreateFromFile(typeof(IDestinationProvider).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(HtmlDocument).Assembly.Location),
            }).ToArray();

            // analyse and generate IL code from syntax tree
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var ms = new MemoryStream();
            // write IL code into memory
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                // handle exceptions
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                foreach (Diagnostic diagnostic in failures)
                {
                    _logger.LogError("{Id}: {Message} at {Location}", diagnostic.Id, diagnostic.GetMessage(),
                        diagnostic.Location);
                }

                throw new Exception("Compilation error");
            }

            // load this 'virtual' DLL so that we can use
            ms.Seek(0, SeekOrigin.Begin);
            Assembly assembly = Assembly.Load(ms.ToArray());
            return assembly;
        }

        private static IDestinationProvider CreateDestinationProviderInstance(Assembly assembly)
        {
            var typeName = "Scrap.Resources.FileSystem.TemplateDestinationProvider";
            Type type = assembly.GetType(typeName) ?? throw new Exception($"Type {typeName} not found");
            object obj = Activator.CreateInstance(type) ?? throw new Exception("Could not activate instance");

            return (IDestinationProvider)obj;
        }
    }
}
