using Marvin.ClanScannerServer.Models;
using Unleash;

namespace Marvin.ClanScannerServer.Services.ScanningStrategies;

public class PatreonScanningStrategy : BaseScanningStrategy
{
    public PatreonScanningStrategy(
        ILogger<PatreonScanningStrategy> logger, 
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

        if (!Unleash.IsEnabled("scan-clans-patreon"))
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
}