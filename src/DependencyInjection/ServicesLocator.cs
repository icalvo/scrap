using System.Diagnostics;
using System.Net;
using Dropbox.Api;
using LiteDB;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Scrap.Application;
using Scrap.Application.Scrap;
using Scrap.Common;
using Scrap.DependencyInjection.Factories;
using Scrap.Domain;
using Scrap.Domain.Downloads;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.JobDefinitions.JsonFile;
using Scrap.Domain.Jobs;
using Scrap.Domain.Jobs.Graphs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.DependencyInjection;

public class ServicesLocator
{
    public static async Task<IServiceProvider> Build(IConfiguration config, Action<ILoggingBuilder> configureLogging)
    {
        IServiceCollection container = new ServiceCollection();
        await ConfigureServices(config, container, configureLogging);
        return container.BuildServiceProvider();
    }

    private static async Task ConfigureServices(
        IConfiguration cfg,
        IServiceCollection container,
        Action<ILoggingBuilder> configureLogging)
    { 
        var filesystemType = cfg[ConfigKeys.FileSystemType] ?? "local";
        var appKey = "0lemimx20njvqt2";
        var tokenFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".scrap", "dropboxtoken.txt");
        IFileSystem fileSystem =
            filesystemType.ToLowerInvariant() switch {
            "dropbox" => new DropboxFileSystem(await GetDropboxClientAsync(appKey, tokenFile)),
            "local" => new LocalFileSystem(),
            _ => throw new Exception($"Unknown filesystem type: {filesystemType}")
            };

        container.AddSingleton(cfg);
        container.Configure<DatabaseInfo>(cfg.GetSection("Scrap"));
        container.AddSingleton<DatabaseInfo>(sp => sp.GetRequiredService<IOptions<DatabaseInfo>>().Value);
        container.AddOptions<MemoryCacheOptions>();
        container.AddMemoryCache();
        container.AddLogging(configureLogging);

        container.AddTransient<JobDefinitionsApplicationService>();
        container.AddTransient<ScrapApplicationService>();
        container.AddTransient<IScrapDownloadsService, ScrapDownloadsService>();
        container.AddTransient<IScrapTextService, ScrapTextService>();
        container.AddTransient<IDownloadApplicationService, DownloadApplicationService>();
        container.AddTransient<ITraversalApplicationService, TraversalApplicationService>();
        container.AddTransient<IResourcesApplicationService, ResourcesApplicationService>();

        container.AddSingleton<IAsyncCacheProvider, MemoryCacheProvider>();
        container.AddSingleton(
            sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServicesLocator>>();
                var config = sp.GetRequiredService<IConfiguration>();
                logger.LogDebug("Scrap DB: {ConnectionString}", config[ConfigKeys.Database]);
                return new ConnectionString(config[ConfigKeys.Database]);
            });
        container.AddSingleton<ILiteDatabase, LiteDatabase>();
        container.AddSingleton<IJobDefinitionRepository>(
            sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServicesLocator>>();
                var config = sp.GetRequiredService<IConfiguration>();
                var definitionsFilePath = config[ConfigKeys.Definitions];
                if (definitionsFilePath == null)
                {
                    throw new Exception("No definitions file in the configuration!");
                }

                logger.LogDebug("Definitions file: {DefinitionsPath}", definitionsFilePath);
                return new MemoryJobDefinitionRepository(
                    definitionsFilePath,
                    sp.GetRequiredService<IFileSystem>(),
                    sp.GetRequiredService<ILogger<MemoryJobDefinitionRepository>>());
            });
        container.AddSingleton<IGraphSearch, DepthFirstGraphSearch>();
        container
            .AddSingleton<IResourceRepositoryConfigurationValidator,
                FileSystemResourceRepositoryConfigurationValidator>();

        container.AddSingleton<IVisitedPagesApplicationService, VisitedPagesApplicationService>();

        container.AddSingleton<IFileSystem>(fileSystem);
        container.AddAsyncFactory<JobDto, Job, JobFactory>();
        container.AddFactory<Job, IPageRetriever, PageRetrieverFactory>();
        container.AddFactory<Job, IDownloadStreamProvider, DownloadStreamProviderFactory>();
        container.AddFactory<Job, AsyncPolicyConfiguration, IAsyncPolicy, AsyncPolicyFactory>();
        container.AddFactory<Job, IResourceRepository, ResourceRepositoryFactory>(
            sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return new ResourceRepositoryFactory(
                    config[ConfigKeys.BaseRootFolder],
                    sp.GetRequiredService<ILoggerFactory>(),
                    sp.GetRequiredService<IFileSystem>());
            });
        container.AddSingleton<IFactory<Job, ILinkCalculator>, LinkCalculatorFactory>();
        container
            .AddSingleton<IFactory<IResourceRepositoryConfiguration, IResourceRepositoryConfigurationValidator>,
                ResourceRepositoryConfigurationValidatorFactory>();

        container.AddOptionalFactory<Job, IPageMarkerRepository, PageMarkerRepositoryFactory>();
    }

    private static async Task<DropboxClient> GetDropboxClientAsync(string appKey, string tokenFile)
    {
        string? existingRefreshToken = null;
        if (File.Exists(tokenFile))
        {
            existingRefreshToken = await File.ReadAllTextAsync(tokenFile);
        }

        var (refreshToken, client) = await GetDropboxClientAuxAsync(appKey, existingRefreshToken);

        if (existingRefreshToken != refreshToken)
        {
            await File.WriteAllTextAsync(tokenFile, refreshToken);
        }

        return client;
    }
    
    private static async Task<(string refreshToken, DropboxClient client)> GetDropboxClientAuxAsync(string appKey, string? existingRefreshToken)
    {
        if (existingRefreshToken == null)
        {
            return await GetByAuth();
        }

        var dropboxClient = new DropboxClient(existingRefreshToken, appKey);
        try
        {
            if (await dropboxClient.RefreshAccessToken(null))
            {
                return (existingRefreshToken, dropboxClient);
            }
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode >= HttpStatusCode.InternalServerError)
            {
                throw;
            }
        }

        return await GetByAuth();

        async Task<(string refreshToken, DropboxClient client)> GetByAuth()
        {
            var codeVerifier = DropboxOAuth2Helper.GeneratePKCECodeVerifier();
            var codeChallenge = DropboxOAuth2Helper.GeneratePKCECodeChallenge(codeVerifier);

            var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(oauthResponseType:OAuthResponseType.Code,
                clientId: appKey,
                redirectUri: (string?)null,
                state: null,
                tokenAccessType: TokenAccessType.Offline,
                scopeList: null,
                includeGrantedScopes: IncludeGrantedScopes.None,
                codeChallenge: codeChallenge);
            Process.Start(new ProcessStartInfo { FileName = authorizeUri.ToString(), UseShellExecute = true });
            Console.Write("Dropbox auth code: ");
            var code = Console.ReadLine();
            OAuth2Response tokenResult = await DropboxOAuth2Helper.ProcessCodeFlowAsync(
                code: code,
                appKey: appKey,
                codeVerifier: codeVerifier,
                redirectUri: null);
            var client = new DropboxClient(
                    appKey: appKey,
                    oauth2AccessToken: tokenResult.AccessToken,
                    oauth2RefreshToken: tokenResult.RefreshToken,
                    oauth2AccessTokenExpiresAt: tokenResult.ExpiresAt ?? default(DateTime));
            return (tokenResult.RefreshToken, client);
        }
    }
    
}
