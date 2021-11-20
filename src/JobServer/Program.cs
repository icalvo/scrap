using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Figgle;
using Hangfire;
using Hangfire.Logging;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Scrap.DependencyInjection;
using Scrap.JobDefinitions;
using Scrap.JobServer;
using LogLevel = Hangfire.Logging.LogLevel;

TaskScheduler.UnobservedTaskException += (_, eventArgs) => Console.WriteLine(eventArgs.Exception);

var configuration =
    new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .Build();

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConfiguration(configuration)
        .AddSimpleConsole(options => options.SingleLine = true);
});

LogProvider.SetCurrentLogProvider(new MicrosoftLogProvider(loggerFactory));

GlobalConfiguration.Configuration.UseSerializerSettings(new JsonSerializerSettings
{
    Converters = new List<JsonConverter>
    {
        new ResourceRepositoryConfigurationJsonConverter(), new TimeSpanJsonConverter()
    },
    ContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new CamelCaseNamingStrategy()
    }
});

var serviceResolver = new ServicesResolver(loggerFactory, configuration);

var options = new BackgroundJobServerOptions
{
    Activator = new CustomJobActivator(serviceResolver),
    WorkerCount = 1
};

var currentColor = Console.ForegroundColor;
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine(FiggleFonts.Standard.Render("SCRAP"));
Console.WriteLine("Background agent for SCRAP jobs");
Console.ForegroundColor = currentColor;

var storage = new SqlServerStorage(configuration["Hangfire:Database"]);
using var server = new BackgroundJobServer(options, storage);
Console.ReadKey();

namespace Scrap.JobServer
{
    class MicrosoftLog : ILog
    {
        private readonly ILogger _logger;

        public MicrosoftLog(ILogger logger)
        {
            _logger = logger;
        }

        public bool Log(LogLevel logLevel, Func<string>? messageFunc, Exception? exception = null)
        {
            var microsoftLogLevel = logLevel switch
            {
                LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
                LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
                LogLevel.Info => Microsoft.Extensions.Logging.LogLevel.Information,
                LogLevel.Warn => Microsoft.Extensions.Logging.LogLevel.Warning,
                LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
                LogLevel.Fatal => Microsoft.Extensions.Logging.LogLevel.Critical,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
            };
            if (!_logger.IsEnabled(microsoftLogLevel))
            {
                return false;
            }

            if (messageFunc == null && exception == null)
            {
                return true;
            }

            _logger.Log(
                microsoftLogLevel,
                exception,
                "{HangfireMessage}",
                messageFunc?.Invoke());

            return true;
        }
    }
    class MicrosoftLogProvider: ILogProvider
    {
        private readonly ILoggerFactory _loggerFactory;

        public MicrosoftLogProvider(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public ILog GetLogger(string name)
        {
            return new MicrosoftLog(_loggerFactory.CreateLogger(name));
        }
    }

    class CustomJobActivator : JobActivator
    {
        private readonly ServicesResolver _serviceResolver;
        private static JobApplicationService? _jobApplicationService;
        private static JobDefinitionsApplicationService? _jobDefinitionsApplicationService;

        public CustomJobActivator(ServicesResolver serviceResolver)
        {
            _serviceResolver = serviceResolver;
        }

        public override object ActivateJob(Type jobType)
        {
            if (jobType == typeof(JobApplicationService))
            {
                if (_jobApplicationService == null)
                {
                    _jobApplicationService = _serviceResolver.BuildScrapperApplicationService();
                }

                return  _jobApplicationService;
            }
        
            if (jobType == typeof(JobDefinitionsApplicationService))
            {
                if (_jobDefinitionsApplicationService == null)
                {
                    _jobDefinitionsApplicationService = _serviceResolver.BuildJobDefinitionsApplicationServiceAsync().Result;
                }

                return _jobDefinitionsApplicationService;
            }

            return base.ActivateJob(jobType);
        }
    }
}
