namespace Marvin.Application.Sdk;

public class MarvinApplication
{
    public WebApplication Application { get; }
    
    public IConfiguration Configuration => Application.Configuration;
    public IWebHostEnvironment Environment => Application.Environment;
    public IHostApplicationLifetime Lifetime => Application.Lifetime;
    public ILogger Logger => Application.Logger;
    public IServiceProvider Services => Application.Services;
    
    internal MarvinApplication(WebApplication webApplication)
    {
        Application = webApplication;
    }

    public async Task RunAsync()
    {
        await Application.RunAsync();
    }
}