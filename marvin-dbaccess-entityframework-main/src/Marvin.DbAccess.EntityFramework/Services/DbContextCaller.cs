using Marvin.DbAccess.EntityFramework.DbContext.Interfaces;
using Marvin.DbAccess.EntityFramework.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Marvin.DbAccess.EntityFramework.Services;

public class DbContextCaller : IDbContextCaller
{
    private readonly IServiceProvider _serviceProvider;

    public DbContextCaller(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async  Task<TResult> GetFromDbContext<TType, TResult>(
        Func<IDbContext<TType>, CancellationToken, Task<TResult>> task,
        CancellationToken cancellationToken = default) where TType : class
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext<TType>>();
        return await task(dbContext, cancellationToken);
    }

    public async Task<TResult> GetFromDbContext<TType1, TType2, TResult>(
        Func<IDbContext<TType1, TType2>, CancellationToken, Task<TResult>> task, 
        CancellationToken cancellationToken = default) 
        where TType1 : class 
        where TType2 : class
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext<TType1, TType2>>();
        return await task(dbContext, cancellationToken);
    }

    public async Task ExecuteWithinDbContext<TType>(
        Func<IDbContext<TType>, CancellationToken, Task> task,
        CancellationToken cancellationToken = default) where TType : class
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext<TType>>();
        await task(dbContext, cancellationToken);
    }
}