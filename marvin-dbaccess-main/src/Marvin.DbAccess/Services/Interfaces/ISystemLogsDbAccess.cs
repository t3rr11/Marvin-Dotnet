using Marvin.DbAccess.Models.SystemLogs;

namespace Marvin.DbAccess.Services.Interfaces;

public interface ISystemLogsDbAccess
{
    Task AddNewEntryAsync<TPayload>(
        SystemLogEntry<TPayload> entry, 
        CancellationToken cancellationToken);
}