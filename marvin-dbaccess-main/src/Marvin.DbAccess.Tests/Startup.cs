using Marvin.DbAccess.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Marvin.DbAccess.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();

        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new DatabaseOptions()
        {
            ConnectionString = ""
        }));

        services.AddPostgresqlDbAccess();
    }
}