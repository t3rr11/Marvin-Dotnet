using Marvin.DbAccess;
using Marvin.MemoryCache;
using Serilog;
using Serilog.Events;

namespace Marvin.Application.Sdk;

/// <summary>
///     Application builder for generic Marvin app, wrapper of ASP.Net <see cref="WebApplicationBuilder"/>
/// </summary>
public class MarvinApplicationBuilder : IDisposable
{
    private MarvinApplicationBuilder(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine("Logs", "logs.txt"),
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 10 * 1024 * 1024,
                retainedFileCountLimit: 10,
                rollOnFileSizeLimit: true,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .CreateBootstrapLogger();

        ApplicationBuilder = WebApplication.CreateBuilder(args);
    }

    /// <summary>
    ///     ASP.Net application builder that's used by this SDK
    /// </summary>
    public WebApplicationBuilder ApplicationBuilder { get; }
    
    public ConfigurationManager Configuration => ApplicationBuilder.Configuration;
    public IWebHostEnvironment Environment => ApplicationBuilder.Environment;
    public ConfigureHostBuilder Host => ApplicationBuilder.Host;
    public ILoggingBuilder Logging => ApplicationBuilder.Logging;
    public IServiceCollection Services => ApplicationBuilder.Services;
    public ConfigureWebHostBuilder WebHost => ApplicationBuilder.WebHost;
    
    
    /// <summary>
    ///     Configures any required services for this application
    /// <br/>
    ///     Shortcut for <see cref="WebApplicationBuilder.Services"/>
    /// </summary>
    /// <param name="configureServices">Action to perform for services to be configured</param>
    /// <returns></returns>
    public MarvinApplicationBuilder AddServices(Action<IServiceCollection> configureServices)
    {
        try
        {
            configureServices(ApplicationBuilder.Services);
            return this;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Failed to configure services");
            throw;
        }
    }

    /// <summary>
    ///     Adds default services, which are DB support and in-memory caching support
    /// </summary>
    /// <returns></returns>
    public MarvinApplicationBuilder AddDefaultServices()
    {
        return 
            AddDbAccess()
            .AddCacheProvider();
    }

    /// <summary>
    ///     Adds DB support (PostgreSQL db)
    /// </summary>
    /// <returns></returns>
    public MarvinApplicationBuilder AddDbAccess()
    {
        try
        {
            ApplicationBuilder.Services.AddPostgresqlDbAccess();
            return this;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Failed to add Db Access library");
            throw;
        }
    }

    /// <summary>
    ///     Adds memory cache (<see cref="ICacheProvider"/>)
    /// </summary>
    /// <returns></returns>
    public MarvinApplicationBuilder AddCacheProvider()
    {
        try
        {
            ApplicationBuilder.Services.AddCacheProvider();
            return this;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Failed to add Db Access library");
            throw;
        }
    }

    /// <summary>
    ///     Create logging configuration helper
    /// </summary>
    /// <returns></returns>
    public MarvinLoggingConfiguration ConfigureLogging()
    {
        return new MarvinLoggingConfiguration(this);
    }

    /// <summary>
    ///     Finishes building an application and returns object to run this app
    /// </summary>
    /// <returns></returns>
    public MarvinApplication BuildApplication()
    {
        var application = ApplicationBuilder.Build();
        return new MarvinApplication(application);
    }

    /// <summary>
    ///     Create new generic application builder for Marvin environment
    /// <br />
    ///     It's recommended to wrap creation of this app builder in using statement
    /// <br />
    ///     using var appBuilder = MarvinApplicationBuilder.Create(args);
    /// <br />
    ///     This will ensure that all logs are caught and written into text file if startup fails 
    /// </summary>
    /// <param name="args">Application console params</param>
    /// <returns></returns>
    public static MarvinApplicationBuilder Create(string[] args)
    {
        return new MarvinApplicationBuilder(args);
    }

    /// <summary>
    ///     Disposes of logging resources and ensures logs are written
    /// </summary>
    public void Dispose()
    {
        Log.CloseAndFlush();
    }
}