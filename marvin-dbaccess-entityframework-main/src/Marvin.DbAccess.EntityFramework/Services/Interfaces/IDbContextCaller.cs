using Marvin.DbAccess.EntityFramework.DbContext.Interfaces;

namespace Marvin.DbAccess.EntityFramework.Services.Interfaces;

public interface IDbContextCaller
{
    Task ExecuteWithinDbContext<TType>(
        Func<IDbContext<TType>, CancellationToken, Task> task,
        CancellationToken cancellationToken = default) where TType : class;
    
    Task<TResult> GetFromDbContext<TType, TResult>(
        Func<IDbContext<TType>, CancellationToken, Task<TResult>> task,
        CancellationToken cancellationToken = default) where TType : class;
    
    Task<TResult> GetFromDbContext<TType1, TType2, TResult>(
        Func<IDbContext<TType1, TType2>, CancellationToken, Task<TResult>> task,
        CancellationToken cancellationToken = default) 
        where TType1 : class
        where TType2 : class;
}