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
    internal class CompiledDestinationProvider : IDestinationProvider
    {
        private readonly string _destinationFolderPattern;
        private IDestinationProvider _compiledDestinationProvider = null!;
        private readonly ILogger<CompiledDestinationProvider> _logger;
        private CompiledDestinationProvider(string destinationFolderPattern, ILogger<CompiledDestinationProvider> logger)
        {
            _destinationFolderPattern = destinationFolderPattern;
            _logger = logger;
        }

        public static CompiledDestinationProvider CreateCompiled(string destinationFolderPattern, ILogger<CompiledDestinationProvider> logger)
        {
            var result = new CompiledDestinationProvider(destinationFolderPattern, logger);
            result.Compile();
            return result;
        }

        public Task<string> GetDestinationAsync(
            Uri resourceUrl,
            string destinationRootFolder, Page page)
        {
            return _compiledDestinationProvider.GetDestinationAsync(resourceUrl, destinationRootFolder, page);
        }

        private void Compile()
        {
            string sourceCode = "";
            try
            {
                _logger.LogInformation("Compiling destination expression...");
            
                // define source code, then parse it (to the type used for compilation)
                sourceCode = @"
                using System;
                using System.IO;
                using System.Net;
                using System.Linq;
                using System.Threading.Tasks;
                using HtmlAgilityPack;
                using Scrap.Pages;
                using Scrap.Resources.FileSystem.Extensions;

                namespace Scrap.Resources.FileSystem
                {
                    public class InternalDestinationProvider: IDestinationProvider
                    {
                        public async Task<string> GetDestinationAsync(
                            Uri resourceUrl,
                            string destinationRootFolder,
                            Page page)
                        {
                            return " + _destinationFolderPattern + @";
                        }
                    }
                }";

                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

                // define other necessary objects for compilation
                string assemblyName = Path.GetRandomFileName();

                var references = ReferenceAssemblies.Net50.Concat(new[]
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
                        _logger.LogError("{0}: {1} at {2}", diagnostic.Id, diagnostic.GetMessage(),
                            diagnostic.Location);
                    }

                    throw new Exception("Compilation error");
                }

                // load this 'virtual' DLL so that we can use
                ms.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(ms.ToArray());

                // create instance of the desired class and call the desired function
                var typeName = "Scrap.Resources.FileSystem.InternalDestinationProvider";
                Type type = assembly.GetType(typeName) ?? throw new Exception($"Type {typeName} not found");
                object obj = Activator.CreateInstance(type) ?? throw new Exception("Could not activate instance");

                _compiledDestinationProvider = (IDestinationProvider)obj;
            }
            catch (Exception)
            {
                Console.WriteLine(sourceCode);
                throw;
            }
        }
    }
}