using Marvin.DbAccess.Options;
using Marvin.DbAccess.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marvin.DbAccess.Services;

public class RawPostgresDbAccess : PostgresDbAccessBase, IRawPostgresDbAccess
{
    public RawPostgresDbAccess(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger<RawPostgresDbAccess> logger) : base(databaseOptions, logger)
    {
    }

    public async ValueTask<T?> QueryFirstAsync<T>(
        string query,
        object? parameters = null,
        CancellationToken cancellationToken = default,
        string? methodName = null,
        bool logErrorParameters = true)
    {
        var result = await QueryAsync<T>(query, parameters, cancellationToken, methodName, logErrorParameters);

        return result.FirstOrDefault();
    }
}