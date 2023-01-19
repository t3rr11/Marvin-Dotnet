using Marvin.DbAccess.Models.Tracking;
using Marvin.DbAccess.Options;
using Marvin.DbAccess.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marvin.DbAccess.Services;

public class TrackedRecordsDbAccess : PostgresDbAccessBase, ITrackedRecordsDbAccess
{
    private const string GetTrackedRecordsQuery = @"
SELECT * FROM tracked_records
WHERE is_tracking = true";

    private const string GetAllTrackedRecordsQuery = @"
SELECT * FROM tracked_records";

    private const string SetTrackedStateQuery = @"
UPDATE tracked_records 
SET 
    is_tracking = @IsTracking
WHERE hash = @Hash";

    private const string SetReportedStateQuery = @"
UPDATE tracked_records 
SET 
    is_reported = @IsReported
WHERE hash = @Hash";

    private const string SetDisplayNameQuery = @"
UPDATE tracked_records 
SET 
    display_name = @DisplayName
WHERE hash = @Hash";

    private const string AddNewTrackedRecordQuery = @"
INSERT INTO tracked_records (hash, display_name, is_tracking, character_scoped, is_reported)
VALUES (@Hash, @DisplayName, @IsTracking, @IsCharacterScoped, @IsReported)";

    private const string DeleteTrackedRecordQuery = @"
DELETE FROM tracked_records WHERE hash = @Hash";

    public TrackedRecordsDbAccess(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger<TrackedRecordsDbAccess> logger) : base(databaseOptions, logger)
    {
    }


    public async ValueTask<IEnumerable<TrackedRecord>> GetTrackedRecords(
        CancellationToken cancellationToken)
    {
        return await QueryAsync<TrackedRecord>(GetTrackedRecordsQuery, null, cancellationToken);
    }

    public async ValueTask<IEnumerable<TrackedRecord>> GetAllTrackedRecordsAsync(
        CancellationToken cancellationToken)
    {
        return await QueryAsync<TrackedRecord>(GetAllTrackedRecordsQuery, null, cancellationToken);
    }

    public async Task SetTrackedStateAsync(
        long hash,
        bool state,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(SetTrackedStateQuery,
            new
            {
                IsTracking = state,
                Hash = hash
            },
            cancellationToken);
    }

    public async Task SetReportedStateAsync(
        long hash,
        bool state,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            SetReportedStateQuery,
            new
            {
                IsReported = state,
                Hash = hash
            },
            cancellationToken);
    }

    public async Task SetDisplayNameAsync(
        long hash,
        string newName,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            SetDisplayNameQuery,
            new
            {
                DisplayName = newName,
                Hash = hash
            },
            cancellationToken);
    }

    public async Task AddNewTrackedRecordAsync(
        long hash,
        string displayName,
        bool isTracking,
        bool isCharacterScoped,
        bool isReported,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            AddNewTrackedRecordQuery, new
            {
                Hash = hash,
                DisplayName = displayName,
                IsTracking = isTracking,
                IsCharacterScoped = isCharacterScoped,
                IsReported = isReported
            },
            cancellationToken);
    }

    public async Task DeleteTrackedRecordAsync(
        long hash,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            DeleteTrackedRecordQuery,
            new
            {
                Hash = hash
            },
            cancellationToken);
    }
}