using DotNetBungieAPI.Service.Abstractions;
using Marvin.ClanScannerServer.Extensions;
using Marvin.DbAccess.Services.Interfaces;
using Marvin.MemoryCache;

namespace Marvin.ClanScannerServer.Models;

public class ProfileUpdateContext
{
    public ProfileUpdateContext()
    {
    }

    private ProfileUpdateContext(
        List<uint> trackedMetricHashes,
        List<uint> trackedProfileRecordHashes,
        List<uint> trackedProgressionsHashes,
        List<uint> trackedProfileCollectibleHashes,
        List<(uint TitleHash, uint? GildingHash)> titleHashes)
    {
        TrackedMetricHashes = trackedMetricHashes;
        TrackedProfileRecordHashes = trackedProfileRecordHashes;
        TrackedProgressionsHashes = trackedProgressionsHashes;
        TrackedProfileCollectibleHashes = trackedProfileCollectibleHashes;
        TitleHashes = titleHashes;
    }
    
    public bool ShouldScanSilently { get; set; }
    public List<uint> TrackedMetricHashes { get; }
    public List<uint> TrackedProfileRecordHashes { get; }
    public List<uint> TrackedProgressionsHashes { get; }
    public List<uint> TrackedProfileCollectibleHashes { get; }
    public List<(uint TitleHash, uint? GildingHash)> TitleHashes { get; }

    public static async ValueTask<ProfileUpdateContext> CreateContextAsync(
        ITrackedEntitiesDbAccess dbAccess,
        IBungieClient bungieClient,
        ICacheProvider cacheProvider,
        CancellationToken cancellationToken)
    {
        var trackedMetrics = await cacheProvider.GetAsync(
            nameof(ITrackedEntitiesDbAccess.GetTrackedMetricHashesCachedAsync),
            async () => await dbAccess.GetTrackedMetricHashesCachedAsync(cancellationToken),
            TimeSpan.FromMinutes(5));

        var trackedProfileRecords = await cacheProvider.GetAsync(
            nameof(ITrackedEntitiesDbAccess.GetTrackedProfileRecordHashesCachedAsync),
            async () => await dbAccess.GetTrackedProfileRecordHashesCachedAsync(cancellationToken),
            TimeSpan.FromMinutes(5));

        var trackedCharacterProgressions = await cacheProvider.GetAsync(
            nameof(ITrackedEntitiesDbAccess.GetTrackedProgressionHashesCachedAsync),
            async () => await dbAccess.GetTrackedProgressionHashesCachedAsync(cancellationToken),
            TimeSpan.FromMinutes(5));

        var trackedProfileCollectibles = await cacheProvider.GetAsync(
            nameof(ITrackedEntitiesDbAccess.GetProfileTrackedCollectibleHashesCachedAsync),
            async () => await dbAccess.GetProfileTrackedCollectibleHashesCachedAsync(cancellationToken),
            TimeSpan.FromMinutes(5));

        var titleHashes = await cacheProvider.GetAsync(
            nameof(BungieClientExtensions.GetTitleAndGildRecordHashes),
            async () => bungieClient.GetTitleAndGildRecordHashes(),
            TimeSpan.FromMinutes(5));

        return new ProfileUpdateContext(
            trackedMetrics,
            trackedProfileRecords,
            trackedCharacterProgressions,
            trackedProfileCollectibles,
            titleHashes);
    }
}