using System.Reflection;
using Basic.Reference.Assemblies;
using HtmlAgilityPack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Scrap.Domain.Pages;

namespace Scrap.Domain.Resources.FileSystem;

public class CompiledDestinationProvider : IDestinationProvider
{
    private readonly FileSystemResourceRepositoryConfiguration _config;
    private readonly ILogger<CompiledDestinationProvider> _logger;
    private static IDestinationProvider? _destinationProvider;
    private readonly IFileSystem _fileSystem;

    public CompiledDestinationProvider(
        FileSystemResourceRepositoryConfiguration config,
        IFileSystem fileSystem,
        ILogger<CompiledDestinationProvider> logger)
    {
        _config = config;
        _logger = logger;
        _fileSystem = fileSystem;
    }

    public async Task<string> GetDestinationAsync(
        string destinationRootFolder,
        IPage page,
        int pageIndex,
        Uri resourceUrl,
        int resourceIndex)
    {
        var compiledDestinationProvider = await CompileAsync();
        return await compiledDestinationProvider.GetDestinationAsync(
            destinationRootFolder,
            page,
            pageIndex,
            resourceUrl,
            resourceIndex);
    }

    public Task ValidateAsync() => CompileAsync();

    private async Task<IDestinationProvider> CompileAsync()
    {
        if (_destinationProvider != null)
        {
            return _destinationProvider;
        }

        var destinationFolderPattern = _config.PathFragments;
        var sourceCode = await GenerateSourceCodeAsync(destinationFolderPattern);
        _logger.LogTrace("Source: {SourceCode}", sourceCode);
        var assembly = CompileSourceCode(sourceCode);
        _destinationProvider = CreateDestinationProviderInstance(assembly);
        return _destinationProvider;
    }

    private static async Task<string> GenerateSourceCodeAsync(string[] destinationFolderPattern)
    {
        var sourceCode = await ReadTemplateAsync();
        var callChain = string.Join("", destinationFolderPattern.Select(p => $"ToArray({p}),\n"));
        sourceCode = sourceCode.Replace("/* DestinationPattern */", callChain);
        return sourceCode;
    }

    private static async Task<string> ReadTemplateAsync()
    {
        await using var stream =
            typeof(CompiledDestinationProvider).Assembly.GetManifestResourceStream(
                "Scrap.Domain.Resources.FileSystem.TemplateDestinationProvider.cs") ??
            throw new Exception("TemplateDestinationProvider resource not found");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private Assembly CompileSourceCode(string sourceCode)
    {
        _logger.LogTrace("Compiling destination expression...");
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        // define other necessary objects for compilation
        var assemblyName = Path.GetRandomFileName();

        var references = Net70.References.All.Concat(
            new[]
            {
                MetadataReference.CreateFromFile(typeof(IDestinationProvider).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(HtmlDocument).Assembly.Location)
            }).ToArray();

        // analyse and generate IL code from syntax tree
        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        // write IL code into memory
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            // handle exceptions
            var failures = result.Diagnostics.Where(
                diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

            foreach (var diagnostic in failures)
            {
                _logger.LogError(
                    "{Id}: {Message} at {Location}",
                    diagnostic.Id,
                    diagnostic.GetMessage(),
                    diagnostic.Location);
                _logger.LogDebug("{SourceCode}", sourceCode);
            }

            throw new Exception("Compilation error");
        }

        // load this 'virtual' DLL so that we can use
        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());
        return assembly;
    }

    private IDestinationProvider CreateDestinationProviderInstance(Assembly assembly)
    {
        var typeName = "Scrap.Resources.FileSystem.TemplateDestinationProvider";
        var type = assembly.GetType(typeName) ?? throw new Exception($"Type {typeName} not found");
        var obj = Activator.CreateInstance(type, _fileSystem) ?? throw new Exception("Could not activate instance");

        return (IDestinationProvider)obj;
    }
}
