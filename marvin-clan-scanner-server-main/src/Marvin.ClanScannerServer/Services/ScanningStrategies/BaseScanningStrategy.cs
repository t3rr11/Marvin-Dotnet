using System.Collections.Concurrent;
using Marvin.ClanScannerServer.Models;
using Unleash;

namespace Marvin.ClanScannerServer.Services.ScanningStrategies;

public abstract class BaseScanningStrategy
{
    protected ILogger Logger { get; }
    protected ClanQueueHubConnection ClanQueueHubConnection { get; }
    protected ClanScannerService ClanScannerService { get; }
    public IUnleash Unleash { get; }

    protected List<long> CurrentScanQueue { get; }
    protected SemaphoreSlim Semaphore { get; }
    protected ConcurrentDictionary<long, OngoingScan> CurrentClanScans { get; }

    protected readonly int MaxConcurrentScans;

    protected BaseScanningStrategy(
        ILogger logger,
        ClanQueueHubConnection clanQueueHubConnection,
        ClanScannerService clanScannerService,
        IUnleash unleash,
        IConfiguration configuration)
    {
        CurrentClanScans = new ConcurrentDictionary<long, OngoingScan>();
        Semaphore = new SemaphoreSlim(1, 1);
        CurrentScanQueue = new List<long>();

        Logger = logger;
        ClanQueueHubConnection = clanQueueHubConnection;
        ClanScannerService = clanScannerService;
        Unleash = unleash;
        MaxConcurrentScans = configuration.GetSection("Scanning:MaxConcurrentClans").Get<int>();
    }

    public abstract Task ProcessClansTick(CancellationToken cancellationToken);
    
    protected async ValueTask<T> PerformLockedQueueTask<T>(Func<List<long>, T> action)
    {
        try
        {
            await Semaphore.WaitAsync();
            return action(CurrentScanQueue);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    protected bool CheckIfAnyClanIsSlow()
    {
        var currentTime = DateTime.UtcNow;

        return CurrentClanScans.Any(x => (currentTime - x.Value.TimeStarted).TotalMinutes > 1);
    }

    protected async Task PerformLockedQueueUpdate(Action<List<long>> action)
    {
        try
        {
            await Semaphore.WaitAsync();
            action(CurrentScanQueue);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task OnClanScanAborted(List<long> clanIds)
    {
        Logger.LogInformation("Removing {Count} clans from scan list due to server request", clanIds.Count);
        await PerformLockedQueueUpdate((list) =>
        {
            foreach (var clanId in clanIds)
            {
                list.Remove(clanId);
                if (CurrentClanScans.TryGetValue(clanId, out var scanData))
                {
                    scanData.CancellationTokenSource.Cancel();
                }
            }
        });
    }

    public async Task ForceClearQueueAndCancelAllScans()
    {
        await PerformLockedQueueUpdate((list) =>
        {
            if (CurrentClanScans.IsEmpty && list.Count == 0)
                return;
            
            foreach (var (clanId, ongoingScan) in CurrentClanScans)
            {
                list.Remove(clanId);
                ongoingScan.CancellationTokenSource.Cancel();
            }
            
            list.Clear();
        });
    }

    protected async Task<long> RunClanScan(long clanId, CancellationToken cancellationToken)
    {
        try
        {
            await ClanQueueHubConnection.StartedClanScanAsync(clanId);
            await ClanScannerService.ScanClanAsUsualAsync(clanId, cancellationToken);
            return clanId;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to scan clan {ClanId}", clanId);
            return -1;
        }
    }

    protected async Task CleanUpScanTasks()
    {
        var completedTasks = CurrentClanScans
            .Where(x => x.Value.ScanTask.IsCompleted)
            .ToList();

        foreach (var (clanId, _) in completedTasks)
        {
            CurrentClanScans.TryRemove(clanId, out _);
        }

        if (completedTasks.Count > 0)
        {
            await ClanQueueHubConnection.SuccessfullyScannedClansAsync(completedTasks.Select(x => x.Key).ToArray());
        }
    }
}