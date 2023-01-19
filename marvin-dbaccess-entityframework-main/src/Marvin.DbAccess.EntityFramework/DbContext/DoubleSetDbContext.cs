using Marvin.DbAccess.EntityFramework.DbContext.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Marvin.DbAccess.EntityFramework.DbContext;

public class DoubleSetDbContext<TType1, TType2> :
    PostgresqlDbContext,
    IDbContext<TType1, TType2>
    where TType1 : class
    where TType2 : class
{
    public DbSet<TType1> Set1 { get; set; }
    public DbSet<TType2> Set2 { get; set; }

    public Microsoft.EntityFrameworkCore.DbContext Context => this;

    public DoubleSetDbContext(IConfiguration configuration) : base(configuration)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity(DbEntityMappings.GetBindAction<TType1>());
        modelBuilder.Entity(DbEntityMappings.GetBindAction<TType2>());
    }
}