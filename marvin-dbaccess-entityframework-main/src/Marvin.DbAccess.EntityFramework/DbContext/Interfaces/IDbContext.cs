using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Marvin.DbAccess.EntityFramework.DbContext.Interfaces;

public interface IDbContext
{
    Microsoft.EntityFrameworkCore.DbContext Context { get; }
}

public interface IDbContext<TType1> : IAsyncDisposable, IDbContext
    where TType1 : class
{
    DbSet<TType1> Set1 { get; set; }
    DatabaseFacade Database { get; }
    
}

public interface IDbContext<TType1, TType2> : IAsyncDisposable, IDbContext
    where TType1 : class where TType2 : class
{
    DbSet<TType1> Set1 { get; set; }
    DbSet<TType2> Set2 { get; set; }
    DatabaseFacade Database { get; }
}