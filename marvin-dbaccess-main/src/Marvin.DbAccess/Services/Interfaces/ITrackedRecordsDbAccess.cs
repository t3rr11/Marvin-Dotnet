using Marvin.DbAccess.Models.Tracking;

namespace Marvin.DbAccess.Services.Interfaces;

public interface ITrackedRecordsDbAccess
{
    ValueTask<IEnumerable<TrackedRecord>> GetTrackedRecords(
        CancellationToken cancellationToken);

    ValueTask<IEnumerable<TrackedRecord>> GetAllTrackedRecordsAsync(
        CancellationToken cancellationToken);

    Task SetTrackedStateAsync(
        long hash,
        bool state,
        CancellationToken cancellationToken);

    Task SetReportedStateAsync(
        long hash,
        bool state,
        CancellationToken cancellationToken);

    Task SetDisplayNameAsync(
        long hash,
        string newName,
        CancellationToken cancellationToken);

    Task AddNewTrackedRecordAsync(
        long hash,
        string displayName,
        bool isTracking,
        bool isCharacterScoped,
        bool isReported,
        CancellationToken cancellationToken);

    Task DeleteTrackedRecordAsync(
        long hash,
        CancellationToken cancellationToken);
}