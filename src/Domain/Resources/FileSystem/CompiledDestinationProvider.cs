using System.Reflection;
using Basic.Reference.Assemblies;
using HtmlAgilityPack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;
using Scrap.Domain.Pages;

namespace Scrap.Domain.Resources.FileSystem;

public class CompiledDestinationProvider : IDestinationProvider
{
    private IDestinationProvider _compiledDestinationProvider = null!;
    private readonly ILogger<CompiledDestinationProvider> _logger;

    public CompiledDestinationProvider(ILogger<CompiledDestinationProvider> logger)
    {
        _logger = logger;
    }

    public async Task<string> GetDestinationAsync(
        FileSystemResourceRepositoryConfiguration config,
        string destinationRootFolder,
        IPage page,
        int pageIndex,
        Uri resourceUrl,
        int resourceIndex)
    {
        await CompileAsync(config);
        return await _compiledDestinationProvider.GetDestinationAsync(config, destinationRootFolder, page, pageIndex, resourceUrl, resourceIndex);
    }

    public async Task CompileAsync(FileSystemResourceRepositoryConfiguration config)
    {
        var destinationFolderPattern = config.PathFragments;
        string sourceCode = await GenerateSourceCodeAsync(destinationFolderPattern);
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

    private async Task<string> GenerateSourceCodeAsync(string[] destinationFolderPattern)
    {
        await using var stream =
            typeof(CompiledDestinationProvider).Assembly
                .GetManifestResourceStream("Scrap.Resources.FileSystem.TemplateDestinationProvider.cs")
            ?? throw new Exception("TemplateDestinationProvider resource not found");
        using var reader = new StreamReader(stream);
        
        string sourceCode = await reader.ReadToEndAsync();
        var callChain = string.Join("", destinationFolderPattern.Select(p => $".C({p})"));
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
