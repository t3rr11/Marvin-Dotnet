using Marvin.DbAccess.Models.SystemLogs;
using Marvin.DbAccess.Options;
using Marvin.DbAccess.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marvin.DbAccess.Services;

public class SystemLogsDbAccess : PostgresDbAccessBase, ISystemLogsDbAccess
{
    public SystemLogsDbAccess(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger<SystemLogsDbAccess> logger) : base(
        databaseOptions, logger)
    {
    }

    private const string AddNewEntryQuery = @"
INSERT INTO system_logs
(
    date,
    log_type,
    payload,
    source    
)
VALUES 
(
    @Date,
    @LogType,
    CAST(@Payload as json),
    @Source
)";

    public async Task AddNewEntryAsync<TPayload>(
        SystemLogEntry<TPayload> entry,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(AddNewEntryQuery, entry, cancellationToken);
    }
}