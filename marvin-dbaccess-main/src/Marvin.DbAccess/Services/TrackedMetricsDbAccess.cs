using Marvin.DbAccess.Models.Tracking;
using Marvin.DbAccess.Options;
using Marvin.DbAccess.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marvin.DbAccess.Services;

public class TrackedMetricsDbAccess : PostgresDbAccessBase, ITrackedMetricsDbAccess
{
    private const string GetAllTrackedMetricHashesQuery = @"
SELECT hash FROM tracked_metrics
WHERE is_tracking = true";

    private const string GetAllTrackedMetricsQuery = @"
SELECT * FROM tracked_metrics";

    private const string SetTrackedMetricStateQuery = @"
UPDATE tracked_metrics
SET is_tracking = @IsTracking
WHERE hash = @Hash";

    private const string AddTrackedMetricQuery = @"
INSERT INTO tracked_metrics (hash, display_name, is_tracking)
VALUES (@Hash, @DisplayName, @IsTracking)";

    private const string RemoveMetricFromTrackingQuery = @"
DELETE FROM tracked_metrics
WHERE hash = @Hash";

    public TrackedMetricsDbAccess(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger<TrackedMetricsDbAccess> logger) : base(databaseOptions, logger)
    {
    }

    public async ValueTask<IEnumerable<uint>> GetAllTrackedMetricHashes(
        CancellationToken cancellationToken)
    {
        var queryResult = await QueryAsync<long>(
                    GetAllTrackedMetricsQuery,
                    null,
                    cancellationToken);
        return queryResult.Select(x => (uint)x).ToList();
    }

    public async ValueTask<IEnumerable<TrackedMetric>> GetAllTrackedMetricsAsync(
        CancellationToken cancellationToken)
    {
        return await QueryAsync<TrackedMetric>(GetAllTrackedMetricsQuery, cancellationToken: cancellationToken);
    }

    public async Task SetTrackedMetricStateAsync(
        uint metricHash,
        bool shouldTrack,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            SetTrackedMetricStateQuery,
            new
            {
                Hash = (long)metricHash,
                IsTracking = shouldTrack
            },
            cancellationToken);
    }

    public async Task AddTrackedMetricAsync(
        uint metricHash,
        bool shouldTrack,
        string displayName,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            AddTrackedMetricQuery,
            new
            {
                Hash = (long)metricHash,
                DisplayName = displayName,
                IsTracking = shouldTrack
            },
            cancellationToken);
    }

    public async Task RemoveMetricFromTrackingAsync(
        uint metricHash,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            RemoveMetricFromTrackingQuery, new
            {
                Hash = (long)metricHash
            },
            cancellationToken);
    }
}