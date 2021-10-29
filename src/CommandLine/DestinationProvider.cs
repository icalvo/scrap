using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Basic.Reference.Assemblies;
using HtmlAgilityPack;

namespace Scrap.CommandLine
{
    internal class DestinationProvider : IDestinationProvider
    {
        private readonly string _destinationFolderPattern;
        private readonly IPageRetriever _pageRetriever;
        private readonly Uri _baseUri;
        private IDestinationProvider _compiledDestinationProvider = null!;

        private DestinationProvider(string destinationFolderPattern, IPageRetriever pageRetriever, Uri baseUri)
        {
            _destinationFolderPattern = destinationFolderPattern;
            _pageRetriever = pageRetriever;
            _baseUri = baseUri;
        }

        public static DestinationProvider Create(string destinationFolderPattern, IPageRetriever pageRetriever, Uri baseUri)
        {
            var result = new DestinationProvider(destinationFolderPattern, pageRetriever, baseUri);
            result.Compile();

            return result;
        }

        public string GetDestination(
            Uri resourceUrl,
            string destinationRootFolder, Uri pageUrl, HtmlDocument pageDoc)
        {
            return _compiledDestinationProvider.GetDestination(resourceUrl, destinationRootFolder, pageUrl, pageDoc);
        }

        private void Compile()
        {
            // define source code, then parse it (to the type used for compilation)
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(@"
                using System;
                using System.IO;
                using System.Net;
                using System.Linq;
                using HtmlAgilityPack;
                using Scrap.CommandLine;

                namespace RoslynCompileSample
                {
                    public class InternalDestinationProvider : BaseDestinationProvider
                    {
                        public InternalDestinationProvider(IPageRetriever pageRetriever, Uri baseUri)
                            : base(pageRetriever, baseUri)
                        {
                        }

                        public override string GetDestination(
                            Uri resourceUrl,
                            string destinationRootFolder,
                            Uri pageUrl,
                            HtmlDocument pageDoc)
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
                    Console.Error.WriteLine("{0}: {1} at {2}", diagnostic.Id, diagnostic.GetMessage(), diagnostic.Location);
                }

                throw new Exception("Compilation error");
            }

            // load this 'virtual' DLL so that we can use
            ms.Seek(0, SeekOrigin.Begin);
            Assembly assembly = Assembly.Load(ms.ToArray());

            // create instance of the desired class and call the desired function
            Type type = assembly.GetType("RoslynCompileSample.InternalDestinationProvider") ?? throw new Exception("1");
            object obj = Activator.CreateInstance(type, _pageRetriever, _baseUri) ?? throw new Exception("2");

            _compiledDestinationProvider = (IDestinationProvider)obj;
        }
    }
}