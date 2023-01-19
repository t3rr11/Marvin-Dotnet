using Marvin.DbAccess.EntityFramework.DbContext.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Marvin.DbAccess.EntityFramework.DbContext;

public class SingleSetDbContext<TType1> :
    PostgresqlDbContext,
    IDbContext<TType1> where TType1 : class
{
    public DbSet<TType1> Set1 { get; set; }

    public Microsoft.EntityFrameworkCore.DbContext Context => this;

    public SingleSetDbContext(IConfiguration configuration) : base(configuration)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity(DbEntityMappings.GetBindAction<TType1>());
    }
}