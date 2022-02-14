using System.Reflection;
using Basic.Reference.Assemblies;
using HtmlAgilityPack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;
using Scrap.Pages;

namespace Scrap.Resources.FileSystem;

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

    public static async Task<CompiledDestinationProvider> CreateCompiledAsync(string[] destinationFolderPattern, ILogger<CompiledDestinationProvider> logger)
    {
        var result = new CompiledDestinationProvider(destinationFolderPattern, logger);
        await result.Compile();
        return result;
    }

    public Task<string> GetDestinationAsync(
        string destinationRootFolder,
        IPage page,
        int pageIndex,
        Uri resourceUrl,
        int resourceIndex)
    {
        return _compiledDestinationProvider.GetDestinationAsync(destinationRootFolder, page, pageIndex, resourceUrl, resourceIndex);
    }

    private async Task Compile()
    {
        string sourceCode = await GenerateSourceCodeAsync();
        try
        {
            var assembly = CompileSourceCode(sourceCode);
            _compiledDestinationProvider = CreateDestinationProviderInstance(assembly);
        }
        catch (Exception)
        {
            _logger.LogTrace("Source: {SourceCode}", sourceCode);
            throw;
        }
    }

    private async Task<string> GenerateSourceCodeAsync()
    {
        await using var stream =
            typeof(CompiledDestinationProvider).Assembly
                .GetManifestResourceStream("Scrap.Resources.FileSystem.TemplateDestinationProvider.cs")
            ?? throw new Exception("TemplateDestinationProvider resource not found");
        using var reader = new StreamReader(stream);
        
        string sourceCode = await reader.ReadToEndAsync();
        var callChain = string.Join("", _destinationFolderPattern.Select(p => $".C({p})"));
        var pattern = $"rootFolder{callChain}.ToPath()";
        sourceCode = sourceCode.Replace("\"destinationFolderPattern\"", pattern);
        return sourceCode;
    }

    private Assembly CompileSourceCode(string sourceCode)
    {
        _logger.LogTrace("Compiling destination expression...");
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
