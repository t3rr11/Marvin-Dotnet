using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Marvin.DbAccess.EntityFramework.DbContext;

public class PostgresqlDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    private readonly IConfiguration _configuration;

    protected PostgresqlDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    protected sealed override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _configuration
            .GetSection("DatabaseOptions:ConnectionString").Value;

        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            
        });
    }
}