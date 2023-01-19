using Marvin.DbAccess.Models.Tracking;

namespace Marvin.DbAccess.Services.Interfaces;

public interface ITrackedProgressionsDbAccess
{
    ValueTask<IEnumerable<TrackedProgression>> GetTrackedProgressions(
        CancellationToken cancellationToken);

    ValueTask<IEnumerable<TrackedProgression>> GetAllTrackedProgressionsAsync(
        CancellationToken cancellationToken);

    Task SetTrackedProgressionStateAsync(
        uint progressionHash,
        bool shouldTrack,
        CancellationToken cancellationToken);

    Task AddProgressionMetricAsync(
        uint progressionHash,
        bool shouldTrack,
        string displayName,
        CancellationToken cancellationToken);

    Task RemoveProgressionFromTrackingAsync(
        uint progressionHash,
        CancellationToken cancellationToken);
}