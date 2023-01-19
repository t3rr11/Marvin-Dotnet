using Marvin.DbAccess.Models.Tracking;

namespace Marvin.DbAccess.Services.Interfaces;

public interface ITrackedCollectibleDbAccess
{
    ValueTask<IEnumerable<TrackedCollectible>> GetAllTrackedCollectibles(CancellationToken cancellationToken);

    Task DisableCollectibleTrackingAsync(uint collectibleHash, CancellationToken cancellationToken);

    Task EnableCollectibleTrackingAsync(uint collectibleHash, CancellationToken cancellationToken);

    Task AddCollectibleToTrackingAsync(uint collectibleHash, bool shouldBroadcast, CancellationToken cancellationToken);

    Task RemoveCollectibleFromTrackingAsync(uint collectibleHash, CancellationToken cancellationToken);
}