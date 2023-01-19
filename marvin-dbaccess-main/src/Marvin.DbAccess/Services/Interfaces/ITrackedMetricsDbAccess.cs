using Marvin.DbAccess.Models.Tracking;

namespace Marvin.DbAccess.Services.Interfaces;

public interface ITrackedMetricsDbAccess
{
    ValueTask<IEnumerable<uint>> GetAllTrackedMetricHashes(
        CancellationToken cancellationToken);

    ValueTask<IEnumerable<TrackedMetric>> GetAllTrackedMetricsAsync(
        CancellationToken cancellationToken);

    Task SetTrackedMetricStateAsync(
        uint metricHash,
        bool shouldTrack,
        CancellationToken cancellationToken);

    Task AddTrackedMetricAsync(
        uint metricHash,
        bool shouldTrack,
        string displayName,
        CancellationToken cancellationToken);

    Task RemoveMetricFromTrackingAsync(
        uint metricHash,
        CancellationToken cancellationToken);
}