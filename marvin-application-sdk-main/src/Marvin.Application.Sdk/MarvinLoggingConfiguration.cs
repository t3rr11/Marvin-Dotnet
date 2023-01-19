using System.Reflection;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;

namespace Marvin.Application.Sdk;

/// <summary>
///     Helper class to configure how logging is handled
/// <br/>
///     Wrapper around Serilog library
/// </summary>
public class MarvinLoggingConfiguration
{
    private readonly MarvinApplicationBuilder _builder;

    private List<Action<HostBuilderContext, IServiceProvider, LoggerConfiguration>> _configurers = new();

    internal MarvinLoggingConfiguration(MarvinApplicationBuilder builder)
    {
        _builder = builder;
    }

    /// <summary>
    ///     Adds logging to console
    /// </summary>
    /// <param name="formatter">Specifies how console logs are formatted</param>
    /// <returns></returns>
    public MarvinLoggingConfiguration AddConsole(ITextFormatter? formatter = null)
    {
        _configurers.Add((_, _, configuration) =>
        {
            if (formatter is not null)
            {
                configuration.WriteTo.Console(formatter: formatter);
            }
            else
            {
                configuration.WriteTo.Console();
            }
        });
        return this;
    }

    /// <summary>
    ///     Adds logging to text files
    /// </summary>
    /// <param name="folder">Folder to write logs into</param>
    /// <param name="fileName">File name template</param>
    /// <param name="rollingInterval">How often logs are getting rolled</param>
    /// <param name="fileSizeLimitBytes">Max file size, bytes</param>
    /// <param name="retainedFileCountLimit">How much files are retained within single roll interval</param>
    /// <param name="rollOnFileSizeLimit">Whether should roll on hitting file limit</param>
    /// <param name="shared">File access type</param>
    /// <returns></returns>
    public MarvinLoggingConfiguration AddFile(
        string folder = "Logs",
        string fileName = "logs.txt",
        RollingInterval rollingInterval = RollingInterval.Day,
        long fileSizeLimitBytes = 10_485_760,
        int retainedFileCountLimit = 10,
        bool rollOnFileSizeLimit = true,
        bool shared = true)
    {
        _configurers.Add((_, _, configuration) =>
        {
            configuration.WriteTo.File(
                Path.Combine(folder, fileName),
                rollingInterval: rollingInterval,
                fileSizeLimitBytes: fileSizeLimitBytes,
                retainedFileCountLimit: retainedFileCountLimit,
                rollOnFileSizeLimit: rollOnFileSizeLimit,
                shared: shared,
                flushToDiskInterval: TimeSpan.FromSeconds(1));
        });
        return this;
    }

    /// <summary>
    ///     Adds logging to Elasticsearch
    /// </summary>
    /// <param name="elasticEndpoint">Elasticsearch DB uri</param>
    /// <returns></returns>
    public MarvinLoggingConfiguration AddElasticsearch(
        string elasticEndpoint = "https://logs.marvin.gg/es")
    {
        _configurers.Add((_, _, configuration) =>
        {
            configuration.WriteTo.Elasticsearch(
                new ElasticsearchSinkOptions(new Uri(elasticEndpoint))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.WriteToFailureSink |
                                       EmitEventFailureHandling.RaiseCallback,
                    FailureSink = new FileSink(Path.Combine("Logs", "ElasticErrors.txt"), new JsonFormatter(), null)
                });
        });
        return this;
    }

    /// <summary>
    ///     Adds some additional logging settings
    /// </summary>
    /// <param name="action">Action to perform</param>
    /// <returns></returns>
    public MarvinLoggingConfiguration AddConfiguration(
        Action<HostBuilderContext, IServiceProvider, LoggerConfiguration> action)
    {
        _configurers.Add(action);
        return this;
    }

    /// <summary>
    ///     Registers Serilog to application and adds all registered settings
    /// </summary>
    /// <returns></returns>
    public MarvinApplicationBuilder Apply()
    {
        _builder.ApplicationBuilder.Host.UseSerilog((context, services, configuration) =>
        {
            AddDefaultEnrichers(context, services, configuration);

            foreach (var configurer in _configurers)
            {
                configurer(context, services, configuration);
            }
        });

        _builder.ApplicationBuilder.Services.AddLogging(x => x.AddSerilog());

        return _builder;
    }

    /// <summary>
    ///     Adds default log enrichers for better logging
    /// </summary>
    /// <param name="context"></param>
    /// <param name="provider"></param>
    /// <param name="configuration"></param>
    private void AddDefaultEnrichers(
        HostBuilderContext context,
        IServiceProvider provider,
        LoggerConfiguration configuration)
    {
        var appName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()!.Location);

        var machineCodeName = context
            .Configuration
            .GetSection("MACHINE_SCANNER_IDENTIFIER_CODE")
            .Value ?? "Machine codename is missing";

        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(provider)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("AppName", appName)
            .Enrich.WithProperty("MachineCodeName", machineCodeName)
            .Enrich.WithExceptionDetails();
    }
}
