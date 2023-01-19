namespace Marvin.DbAccess.Services.Interfaces;

public interface ITrackedEntitiesDbAccess
{
    ValueTask<List<uint>> GetTrackedMetricHashesCachedAsync(
        CancellationToken cancellationToken);

    ValueTask<List<uint>> GetTrackedProfileRecordHashesCachedAsync(
        CancellationToken cancellationToken);

    ValueTask<List<uint>> GetTrackedCharacterRecordHashesCachedAsync(
        CancellationToken cancellationToken);

    ValueTask<List<uint>> GetTrackedProgressionHashesCachedAsync(
        CancellationToken cancellationToken);

    ValueTask<List<uint>> GetProfileTrackedCollectibleHashesCachedAsync(
        CancellationToken cancellationToken);
}