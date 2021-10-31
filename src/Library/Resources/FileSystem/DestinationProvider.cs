using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Basic.Reference.Assemblies;
using HtmlAgilityPack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;
using Scrap.Pages;

namespace Scrap.Resources.FileSystem
{
    internal class DestinationProvider : IDestinationProvider
    {
        private readonly string _destinationFolderPattern;
        private IDestinationProvider _compiledDestinationProvider = null!;
        private readonly ILogger<DestinationProvider> _logger;
        private DestinationProvider(string destinationFolderPattern, ILogger<DestinationProvider> logger)
        {
            _destinationFolderPattern = destinationFolderPattern;
            _logger = logger;
        }

        public static DestinationProvider CreateCompiled(string destinationFolderPattern, ILogger<DestinationProvider> logger)
        {
            var result = new DestinationProvider(destinationFolderPattern, logger);
            result.Compile();
            return result;
        }

        public string GetDestination(
            Uri resourceUrl,
            string destinationRootFolder, Page page)
        {
            return _compiledDestinationProvider.GetDestination(resourceUrl, destinationRootFolder, page);
        }

        private void Compile()
        {
            _logger.LogInformation("Compiling destination expression...");
            // define source code, then parse it (to the type used for compilation)
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(@"
                using System;
                using System.IO;
                using System.Net;
                using System.Linq;
                using HtmlAgilityPack;
                using Scrap.Pages;
                using Scrap.Resources.FileSystem.Extensions;

                namespace Scrap.Resources.FileSystem
                {
                    public class InternalDestinationProvider: IDestinationProvider
                    {
                        public string GetDestination(
                            Uri resourceUrl,
                            string destinationRootFolder,
                            Page page)
                        {
                            return " + _destinationFolderPattern + @";
                        }
                    }
                }");

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
    }
}