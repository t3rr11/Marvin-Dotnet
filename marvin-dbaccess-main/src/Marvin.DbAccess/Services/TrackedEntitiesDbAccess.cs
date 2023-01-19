using Marvin.DbAccess.Options;
using Marvin.DbAccess.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marvin.DbAccess.Services;

public class TrackedEntitiesDbAccess : PostgresDbAccessBase, ITrackedEntitiesDbAccess
{
    private const string GetTrackedMetricHashes = @"
SELECT hash FROM tracked_metrics
WHERE is_tracking = true";

    private const string GetTrackedProfileRecordHashes = @"
SELECT hash FROM tracked_records
WHERE is_tracking = true AND character_scoped = false";

    private const string GetTrackedCharacterRecordHashes = @"
SELECT hash FROM tracked_records
WHERE is_tracking = true AND character_scoped = true";

    private const string GetTrackedProgressionHashes = @"
SELECT hash FROM tracked_progressions
WHERE is_tracking = true";

    private const string GetProfileTrackedCollectibleHashes = @"
SELECT hash FROM tracked_collectibles
WHERE is_broadcasting = true";

    public TrackedEntitiesDbAccess(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger<TrackedEntitiesDbAccess> logger) : base(databaseOptions, logger)
    {
    }

    #region Metrics

    public async ValueTask<List<uint>> GetTrackedMetricHashesCachedAsync(
        CancellationToken cancellationToken)
    {
        var queryResult = await QueryAsync<long>(
                    GetTrackedMetricHashes,
                    null,
                    cancellationToken);
        return queryResult.Select(x => (uint)x).ToList();
    }

    #endregion

    #region Progressions

    public async ValueTask<List<uint>> GetTrackedProgressionHashesCachedAsync(
        CancellationToken cancellationToken)
    {
        var queryResult = await QueryAsync<long>(
                    GetTrackedProgressionHashes,
                    null,
                    cancellationToken);
        return queryResult.Select(x => (uint)x).ToList();
    }

    #endregion

    #region Collectibles

    public async ValueTask<List<uint>> GetProfileTrackedCollectibleHashesCachedAsync(
        CancellationToken cancellationToken)
    {
        var queryResult = await QueryAsync<long>(
                    GetProfileTrackedCollectibleHashes,
                    null,
                    cancellationToken);
        return queryResult.Select(x => (uint)x).ToList();
    }

    #endregion

    #region Records

    public async ValueTask<List<uint>> GetTrackedProfileRecordHashesCachedAsync(
        CancellationToken cancellationToken)
    {
        var queryResult = await QueryAsync<long>(
                    GetTrackedProfileRecordHashes,
                    null,
                    cancellationToken);
        return queryResult.Select(x => (uint)x).ToList();
    }

    public async ValueTask<List<uint>> GetTrackedCharacterRecordHashesCachedAsync(
        CancellationToken cancellationToken)
    {
        var queryResult = await QueryAsync<long>(
                    GetTrackedCharacterRecordHashes,
                    null,
                    cancellationToken);
        return queryResult.Select(x => (uint)x).ToList();
    }

    #endregion
}