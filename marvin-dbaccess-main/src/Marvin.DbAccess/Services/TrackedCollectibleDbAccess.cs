using Marvin.DbAccess.Models.Tracking;
using Marvin.DbAccess.Options;
using Marvin.DbAccess.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marvin.DbAccess.Services;

public class TrackedCollectibleDbAccess : PostgresDbAccessBase, ITrackedCollectibleDbAccess
{
    private const string GetAllTrackedCollectiblesQuery = @"
SELECT * FROM tracked_collectibles";

    private const string DisableCollectibleTrackingQuery = @"
UPDATE tracked_collectibles 
SET
    is_broadcasting = false
WHERE hash = @Hash";

    private const string EnableCollectibleTrackingQuery = @"
UPDATE tracked_collectibles 
SET
    is_broadcasting = true
WHERE hash = @Hash";

    private const string AddCollectibleToTrackingQuery = @"
INSERT INTO tracked_collectibles (hash, is_broadcasting, custom_description, type, display_name)
VALUES (@Hash, @IsBroadcasting, @CustomDescription, @Type, @DisplayName)";

    private const string RemoveCollectibleFromTrackingQuery = @"
DELETE FROM tracked_collectibles
WHERE hash = @Hash";

    public TrackedCollectibleDbAccess(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger<TrackedCollectibleDbAccess> logger) : base(databaseOptions, logger)
    {
    }

    public async ValueTask<IEnumerable<TrackedCollectible>> GetAllTrackedCollectibles(
        CancellationToken cancellationToken)
    {
        return await QueryAsync<TrackedCollectible>(GetAllTrackedCollectiblesQuery,
            cancellationToken: cancellationToken);
    }

    public async Task DisableCollectibleTrackingAsync(
        uint collectibleHash,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(DisableCollectibleTrackingQuery,
            new
            {
                Hash = (long)collectibleHash
            },
            cancellationToken);
    }

    public async Task EnableCollectibleTrackingAsync(
        uint collectibleHash,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(EnableCollectibleTrackingQuery,
            new
            {
                Hash = (long)collectibleHash
            },
            cancellationToken);
    }

    public async Task AddCollectibleToTrackingAsync(
        uint collectibleHash,
        bool shouldBroadcast,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(AddCollectibleToTrackingQuery, new
            {
                Hash = (long)collectibleHash,
                IsBroadcasting = shouldBroadcast,
                CustomDescription = (string)null,
                Type = string.Empty,
                DisplayName = (string)null
            },
            cancellationToken);
    }

    public async Task RemoveCollectibleFromTrackingAsync(uint collectibleHash, CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            RemoveCollectibleFromTrackingQuery,
            new
            {
                Hash = (long)collectibleHash
            },
            cancellationToken);
    }
}