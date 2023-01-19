using System.Collections;
using System.Runtime.CompilerServices;
using Dapper;
using Dapper.Transaction;
using Marvin.DbAccess.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Marvin.DbAccess.Services;

public abstract class PostgresDbAccessBase
{
    private readonly IOptions<DatabaseOptions> _databaseOptions;
    private readonly ILogger _logger;

    protected PostgresDbAccessBase(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger logger)
    {
        _databaseOptions = databaseOptions;
        _logger = logger;
    }

    public async ValueTask ExecuteAsync(
        string query,
        object? parameters = null,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string? methodName = null,
        bool logErrorParameters = true)
    {
        try
        {
            await using var postgreDbConnection = new NpgsqlConnection(_databaseOptions.Value.ConnectionString);
            await postgreDbConnection.ExecuteAsync(
                new CommandDefinition(
                    query,
                    parameters,
                    cancellationToken: cancellationToken));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("[Postgres]: call was aborted: {MethodName}", methodName);
        }
        catch (Exception e)
        {
            if (logErrorParameters)
                _logger.LogError(
                    e,
                    "[Postgres]: Error from method: {Method}, Query = {Query}, Parameters = {@Parameters}",
                    methodName, query, parameters);
            else
                _logger.LogError(
                    e,
                    "[Postgres]: Error from method: {Method}, Query = {Query}",
                    methodName, query);

            throw;
        }
    }

    public async ValueTask<IEnumerable<T>> QueryAsync<T>(
        string query,
        object? parameters = null,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string? methodName = null,
        bool logErrorParameters = true)
    {
        try
        {
            await using var postgreDbConnection = new NpgsqlConnection(_databaseOptions.Value.ConnectionString);
            return await postgreDbConnection.QueryAsync<T>(
                new CommandDefinition(
                    query,
                    parameters,
                    commandTimeout: 300,
                    cancellationToken: cancellationToken));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("[Postgres]: call was aborted: {MethodName}", methodName);
            throw;
        }
        catch (Exception e)
        {
            if (logErrorParameters)
                _logger.LogError(
                    e,
                    "[Postgres]: Error from method: {Method}, Query = {Query}, Parameters = {@Parameters}",
                    methodName, query, parameters);
            else
                _logger.LogError(
                    e,
                    "[Postgres]: Error from method: {Method}, Query = {Query}",
                    methodName, query);

            throw;
        }
    }
}