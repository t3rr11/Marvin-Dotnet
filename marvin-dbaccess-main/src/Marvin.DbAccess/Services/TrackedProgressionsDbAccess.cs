using Marvin.DbAccess.Models.Tracking;
using Marvin.DbAccess.Options;
using Marvin.DbAccess.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marvin.DbAccess.Services;

public class TrackedProgressionsDbAccess : PostgresDbAccessBase, ITrackedProgressionsDbAccess
{
    private const string GetTrackedProgressionsQuery = @"
SELECT * FROM tracked_progressions
WHERE is_tracking = true";

    private const string GetAllTrackedProgressionsQuery = @"
SELECT * FROM tracked_progressions";

    private const string SetTrackedProgressionStateQuery = @"
UPDATE tracked_progressions
SET is_tracking = @IsTracking
WHERE hash = @Hash";

    private const string AddTrackedProgressionQuery = @"
INSERT INTO tracked_progressions (hash, display_name, is_tracking)
VALUES (@Hash, @DisplayName, @IsTracking)";

    private const string RemoveProgressionFromTrackingQuery = @"
DELETE FROM tracked_progressions
WHERE hash = @Hash";

    public TrackedProgressionsDbAccess(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger<TrackedProgressionsDbAccess> logger) : base(databaseOptions, logger)
    {
    }

    public async ValueTask<IEnumerable<TrackedProgression>> GetTrackedProgressions(
        CancellationToken cancellationToken)
    {
        return await QueryAsync<TrackedProgression>(GetTrackedProgressionsQuery, null, cancellationToken);
    }

    public async ValueTask<IEnumerable<TrackedProgression>> GetAllTrackedProgressionsAsync(
        CancellationToken cancellationToken)
    {
        return await QueryAsync<TrackedProgression>(GetAllTrackedProgressionsQuery, null, cancellationToken);
    }

    public async Task SetTrackedProgressionStateAsync(
        uint progressionHash,
        bool shouldTrack,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            SetTrackedProgressionStateQuery,
            new
            {
                Hash = (long)progressionHash,
                IsTracking = shouldTrack
            },
            cancellationToken);
    }

    public async Task AddProgressionMetricAsync(
        uint progressionHash,
        bool shouldTrack,
        string displayName,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            AddTrackedProgressionQuery,
            new
            {
                Hash = (long)progressionHash,
                DisplayName = displayName,
                IsTracking = shouldTrack
            },
            cancellationToken);
    }

    public async Task RemoveProgressionFromTrackingAsync(
        uint progressionHash,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            RemoveProgressionFromTrackingQuery, new
            {
                Hash = (long)progressionHash
            },
            cancellationToken);
    }
}