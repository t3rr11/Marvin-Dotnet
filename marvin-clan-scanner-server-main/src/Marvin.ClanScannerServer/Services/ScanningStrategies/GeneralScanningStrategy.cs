using Marvin.ClanScannerServer.Models;
using Unleash;

namespace Marvin.ClanScannerServer.Services.ScanningStrategies;

public class GeneralScanningStrategy : BaseScanningStrategy
{
    private readonly IUnleash _unleash;

    private DateTime _lastTimeFirstTimeScanPerformed = DateTime.UtcNow;

    public GeneralScanningStrategy(
        ILogger<GeneralScanningStrategy> logger,
        ClanQueueHubConnection clanQueueHubConnection,
        ClanScannerService clanScannerService,
        IUnleash unleash,
        IConfiguration configuration) : base(logger, clanQueueHubConnection, clanScannerService, unleash, configuration)
    {
    }

    public override async Task ProcessClansTick(CancellationToken cancellationToken)
    {
        if (!Unleash.IsEnabled("scan-clans-global"))
        {
            await Task.Delay(10000, cancellationToken);
            return;
        }

        if (!Unleash.IsEnabled("scan-clans-general"))
        {
            await Task.Delay(10000, cancellationToken);
            return;
        }

        // if scanner isn't connected to a server
        if (!ClanQueueHubConnection.IsConnected)
        {
            await ForceClearQueueAndCancelAllScans();
            await Task.Delay(10000, cancellationToken);
            return;
        }

        if (Unleash.IsEnabled("scan-clans-first-time"))
        {
            if (ShouldScanFirstTimes())
            {
                var firstTimeScanClans = await ClanQueueHubConnection.GetClansForFirstTimeScanningAsync();

                if (firstTimeScanClans.Any())
                {
                    await Parallel.ForEachAsync(firstTimeScanClans, cancellationToken, async (entry, token) =>
                    {
                        await ClanScannerService.ScanClanAssumedFirstTimeAsync(entry, token);
                        await ClanQueueHubConnection.FirstTimeScanDoneAsync(entry.ClanId);
                    });
                }
                
                _lastTimeFirstTimeScanPerformed = DateTime.UtcNow;
            }
        }
        
        // Try refill queue if empty
        if (CurrentScanQueue.Count == 0)
        {
            var clansToScan = await ClanQueueHubConnection.GetClansForScanningAsync();
            await PerformLockedQueueUpdate((list) => { list.AddRange(clansToScan); });
        }

        // clean up and remove finished tasks
        await CleanUpScanTasks();
        
        // if queue was empty after refill
        if (CurrentScanQueue.Count == 0)
        {
            await Task.Delay(10000, cancellationToken);
            return;
        }

        // if (CheckIfAnyClanIsSlow())
        // {
        //     return;
        // }

        // calculate how many scans to add back into scanning
        var freeSlots = MaxConcurrentScans - CurrentClanScans.Count;

        if (freeSlots == 0)
        {
            return;
        }

        var clanIds = await PerformLockedQueueTask((list) => list.Take(freeSlots).ToList());

        // start up new scans
        foreach (var clanId in clanIds)
        {
            var cts = new CancellationTokenSource();
            await PerformLockedQueueUpdate((list) => { list.Remove(clanId); });
            CurrentClanScans.TryAdd(clanId, new OngoingScan(RunClanScan(clanId, cts.Token), cts));
        }
    }

    private bool ShouldScanFirstTimes()
    {
        return (DateTime.UtcNow - _lastTimeFirstTimeScanPerformed).TotalSeconds > 10;
    }
}