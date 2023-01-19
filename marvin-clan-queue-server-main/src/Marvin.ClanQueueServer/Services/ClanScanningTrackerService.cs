using System.Collections.Concurrent;
using Marvin.ClanQueueServer.Hubs;
using Marvin.ClanQueueServer.Models;
using Marvin.ClanQueueServer.Services.Interfaces;
using Marvin.DbAccess.Models.Clan;
using Marvin.DbAccess.Services.Interfaces;
using Marvin.Hub.Messaging;
using Marvin.Hub.Messaging.Models;
using Microsoft.AspNetCore.SignalR;

namespace Marvin.ClanQueueServer.Services;

public class ClanScanningTrackerService
{
    private readonly List<long> EmptyList = Enumerable.Empty<long>().ToList();
    private readonly List<FirstTimeScanEntry> EmptyFirstTimeList = Enumerable.Empty<FirstTimeScanEntry>().ToList();

    private readonly ConcurrentDictionary<string, ScannerInstanceData> _currentScannerIds;

    private readonly SemaphoreSlim _semaphore;
    private readonly IClansDbAccess _clansDbAccess;
    private readonly ILogger<ClanScanningTrackerService> _logger;
    private readonly IHubContext<ClanQueueHub> _hubContext;
    private readonly IBungieNetHealthCheck _bungieNetHealthCheck;
    private readonly IBungieNetManifestUpdater _bungieNetManifestUpdater;
    public ConcurrentDictionary<long, ClanScanStateModel> GeneralScanStates { get; }
    public ConcurrentDictionary<long, ClanScanStateModel> PatreonScanStates { get; }
    public ConcurrentDictionary<long, ClanFirstTimeScanStateModel> FirstTimeScanStates { get; }

    public event Func<long, string?, Task> OnClanTrackingStopped;

    public ClanScanningTrackerService(
        IClansDbAccess clansDbAccess,
        ILogger<ClanScanningTrackerService> logger,
        IHubContext<ClanQueueHub> hubContext,
        IBungieNetHealthCheck bungieNetHealthCheck,
        IBungieNetManifestUpdater bungieNetManifestUpdater)
    {
        _clansDbAccess = clansDbAccess;
        _logger = logger;
        _hubContext = hubContext;
        _bungieNetHealthCheck = bungieNetHealthCheck;
        _bungieNetManifestUpdater = bungieNetManifestUpdater;
        GeneralScanStates = new ConcurrentDictionary<long, ClanScanStateModel>();
        PatreonScanStates = new ConcurrentDictionary<long, ClanScanStateModel>();
        FirstTimeScanStates = new ConcurrentDictionary<long, ClanFirstTimeScanStateModel>();
        _semaphore = new SemaphoreSlim(1, 1);
        _currentScannerIds = new ConcurrentDictionary<string, ScannerInstanceData>();
    }

    public void AddNewFirstTimeScanClan(FirstTimeScanEntry firstTimeScanEntry)
    {
        PerformLockedActionSync(() =>
        {
            var scanState = new ClanFirstTimeScanStateModel()
            {
                AssignedScannerId = GetRandomScannerId(ServerRunMode.General),
                State = ClanScanState.WaitingForFetch,
                ScanEntry = firstTimeScanEntry
            };

            FirstTimeScanStates.TryAdd(scanState.ScanEntry.ClanId, scanState);
        });
    }

    public async Task<ServerRunMode> AddNewClanScannerAndReassignClans(
        string scannerId)
    {
        return await ExecuteThreadSafeAsync(async () =>
        {
            var currentPatreonScannerCount = _currentScannerIds
                .Count(x => x.Value.ServerRunMode == ServerRunMode.Patreon);

            var scannerInstanceData = new ScannerInstanceData()
            {
                ConnectionId = scannerId,
                ServerRunMode = currentPatreonScannerCount == 0 ? ServerRunMode.Patreon : ServerRunMode.General
            };

            _currentScannerIds.TryAdd(scannerId, scannerInstanceData);

            await DistributeClansBetweenScannersEvenly();

            return scannerInstanceData.ServerRunMode;
        });
    }

    public async Task RemoveClanScannerAndReassignClans(string scannerId)
    {
        await ExecuteThreadSafeAsync(async () =>
        {
            if (!_currentScannerIds.ContainsKey(scannerId))
                return;

            var scanStates = GetClanStatesBasedOnMode(scannerId);

            foreach (var (_, stateModel) in scanStates.Where(x => x.Value.ScanningBy == scannerId))
            {
                stateModel.State = ClanScanState.WaitingForFetch;
            }

            _currentScannerIds.Remove(scannerId, out _);

            await DistributeClansBetweenScannersEvenly();
        });
    }

    public async Task<List<long>> FetchClanIdsForScanner(string connectionId)
    {
        if (!_bungieNetHealthCheck.IsLive)
        {
            return EmptyList;
        }

        if (_bungieNetManifestUpdater.IsUpdating)
        {
            return EmptyList;
        }

        return await ExecuteThreadSafe(() =>
        {
            if (!_currentScannerIds.ContainsKey(connectionId))
            {
                return EmptyList;
            }
            
            var dictToLookUp = GetClanStatesBasedOnMode(connectionId);

            var clansToScan = dictToLookUp
                .Where(x =>
                    x.Value.State == ClanScanState.WaitingForFetch &&
                    x.Value.AssignedScannerId == connectionId)
                .ToList();

            clansToScan.ForEach(x =>
            {
                x.Value.State = ClanScanState.WaitingForScan;
                x.Value.ScanningBy = connectionId;
            });

            return clansToScan
                .OrderBy(x => x.Value.LastUpdated)
                .Select(x => x.Key)
                .ToList();
        });
    }

    public async Task<List<FirstTimeScanEntry>> FetchFirstTimeClanIdsForScanner(string connectionId)
    {
        if (!_bungieNetHealthCheck.IsLive)
        {
            return EmptyFirstTimeList;
        }

        if (_bungieNetManifestUpdater.IsUpdating)
        {
            return EmptyFirstTimeList;
        }

        return await ExecuteThreadSafe(() =>
        {
            var clansToScan = FirstTimeScanStates
                .Where(x =>
                    x.Value.State == ClanScanState.WaitingForFetch &&
                    x.Value.AssignedScannerId == connectionId)
                .ToList();

            clansToScan.ForEach(x =>
            {
                x.Value.State = ClanScanState.WaitingForScan;
                x.Value.ScannedBy = connectionId;
                x.Value.FetchedDate = DateTime.UtcNow;
            });

            return clansToScan
                .Select(x => x.Value.ScanEntry)
                .ToList();
        });
    }

    public async Task ActualiseFromDb(CancellationToken cancellationToken)
    {
        await ExecuteThreadSafeAsync(async () =>
        {
            var mainUpdResult = await ActualiseMainClanQueue(cancellationToken);

            var patreonUpdResult = await ActualisePatreonClanQueue(cancellationToken);

            var firstTimeUpdResult = await ActualiseFirstTimeClanQueue(cancellationToken);

            if (mainUpdResult || patreonUpdResult || firstTimeUpdResult)
            {
                await DistributeClansBetweenScannersEvenly();
            }
        });
    }

    public async Task UpdateScannedClans(string connectionId, long[] clanIds)
    {
        await ExecuteThreadSafe(() =>
        {
            if (!_currentScannerIds.ContainsKey(connectionId))
            {
                return;
            }
        
            var scanStates = GetClanStatesBasedOnMode(connectionId);

            for (var i = 0; i < clanIds.Length; i++)
            {
                var clanId = clanIds[i];
                if (scanStates.TryGetValue(clanId, out var stateModel))
                {
                    stateModel.State = ClanScanState.WaitingForFetch;
                    stateModel.LastUpdated = DateTime.UtcNow;
                    stateModel.ScanningBy = null;
                    stateModel.LastScanStarted = null;
                }
            }
        });
    }

    public async Task FirstTimeScanFinished(long clanId)
    {
        await ExecuteThreadSafe(() =>
        {
            FirstTimeScanStates.TryRemove(clanId, out _);
        });
    }

    public async Task MarkClanScanStarted(string connectionId, long clanId)
    {
        await ExecuteThreadSafe(() =>
        {
            if (!_currentScannerIds.ContainsKey(connectionId))
            {
                return;
            }
            
            var scanStates = GetClanStatesBasedOnMode(connectionId);

            if (scanStates.TryGetValue(clanId, out var scanStateModel))
            {
                scanStateModel.State = ClanScanState.CurrentlyScanning;
                scanStateModel.ScanningBy = connectionId;
                scanStateModel.LastScanStarted = DateTime.UtcNow;
            }
        });
    }

    public async Task<ClanTrackerReport> CollectScanStatesReport()
    {
        return await ExecuteThreadSafe(() =>
        {
            var report = new ClanTrackerReport();

            foreach (var (scannerId, scannerInstanceData) in _currentScannerIds)
            {
                var linkedClans = GetClanStatesBasedOnMode(scannerId)
                    .Where(x => x.Value.AssignedScannerId == scannerId)
                    .ToDictionary(x => x.Key, x => x.Value);

                var clanTrackerDetails = new ClanTrackerDetails();

                clanTrackerDetails.ServerRunMode = scannerInstanceData.ServerRunMode;
                clanTrackerDetails.ClanAmount = linkedClans.Count;

                clanTrackerDetails.ClansAmountPerState.Add(
                    ClanScanState.CurrentlyScanning,
                    linkedClans.Count(x => x.Value.State == ClanScanState.CurrentlyScanning));
                
                clanTrackerDetails.ClansAmountPerState.Add(
                    ClanScanState.WaitingForFetch,
                    linkedClans.Count(x => x.Value.State == ClanScanState.WaitingForFetch));
                
                clanTrackerDetails.ClansAmountPerState.Add(
                    ClanScanState.WaitingForScan,
                    linkedClans.Count(x => x.Value.State == ClanScanState.WaitingForScan));

                report.CurrentScanners.Add(scannerId, clanTrackerDetails);
            }

            return report;
        });
    }

    private async Task<bool> ActualiseMainClanQueue(CancellationToken cancellationToken)
    {
        var anyChangesFound = false;

        var clanIdsForScanning = (await _clansDbAccess.GetGeneralClanIdsForScanning(cancellationToken)).ToHashSet();

        var clansAddedToScanning = 0;

        foreach (var clanId in clanIdsForScanning)
        {
            if (!GeneralScanStates.ContainsKey(clanId))
            {
                GeneralScanStates.TryAdd(
                    clanId,
                    new ClanScanStateModel()
                    {
                        ClanId = clanId,
                        State = ClanScanState.WaitingForFetch,
                        AssignedScannerId = null,
                        ScanningBy = null,
                        LastUpdated = null
                    });

                clansAddedToScanning++;
            }
        }

        if (clansAddedToScanning > 0)
        {
            anyChangesFound = true;
            _logger.LogInformation("Added {ClanAmount} clans to scanning", clansAddedToScanning);
        }

        if (GeneralScanStates.Count != clanIdsForScanning.Count)
        {
            anyChangesFound = true;
            var markedForDeletion = GeneralScanStates.Keys.Where(x => !clanIdsForScanning.Contains(x));

            foreach (var idToRemove in markedForDeletion)
            {
                if (GeneralScanStates.TryRemove(idToRemove, out var scanStateModel))
                {
                    if (OnClanTrackingStopped != null)
                    {
                        await OnClanTrackingStopped.Invoke(scanStateModel.ClanId, scanStateModel.AssignedScannerId);
                    }
                }
            }
        }

        return anyChangesFound;
    }

    private async Task<bool> ActualisePatreonClanQueue(CancellationToken cancellationToken)
    {
        var anyChangesFound = false;

        var clanIdsForScanning = (await _clansDbAccess.GetPatreonClanIdsForScanning(cancellationToken)).ToHashSet();

        var clansAddedToPatreonScanning = 0;

        foreach (var clanId in clanIdsForScanning)
        {
            if (!PatreonScanStates.ContainsKey(clanId))
            {
                PatreonScanStates.TryAdd(
                    clanId,
                    new ClanScanStateModel()
                    {
                        ClanId = clanId,
                        State = ClanScanState.WaitingForFetch,
                        AssignedScannerId = null,
                        ScanningBy = null,
                        LastUpdated = null
                    });

                clansAddedToPatreonScanning++;
            }
        }

        if (clansAddedToPatreonScanning > 0)
        {
            anyChangesFound = true;
            _logger.LogInformation("Added {ClanAmount} clans to patreon scanning", clansAddedToPatreonScanning);
        }

        if (PatreonScanStates.Count != clanIdsForScanning.Count)
        {
            anyChangesFound = true;
            var markedForDeletion = PatreonScanStates.Keys.Where(x => !clanIdsForScanning.Contains(x));

            foreach (var idToRemove in markedForDeletion)
            {
                if (PatreonScanStates.TryRemove(idToRemove, out var scanStateModel))
                {
                    if (OnClanTrackingStopped != null)
                    {
                        await OnClanTrackingStopped.Invoke(scanStateModel.ClanId, scanStateModel.AssignedScannerId);
                    }
                }
            }
        }

        return anyChangesFound;
    }

    private async Task<bool> ActualiseFirstTimeClanQueue(CancellationToken cancellationToken)
    {
        var anyChanges = false;

        var newClans = 0;

        var clansForFirstTimeScanning = (await _clansDbAccess.GetClansForFirstTimeScanning(cancellationToken))
            .ToList();

        foreach (var timeScanEntry in clansForFirstTimeScanning)
        {
            if (!FirstTimeScanStates.ContainsKey(timeScanEntry.ClanId))
            {
                FirstTimeScanStates.TryAdd(
                    timeScanEntry.ClanId,
                    new ClanFirstTimeScanStateModel()
                    {
                        ScanEntry = timeScanEntry,
                        State = ClanScanState.WaitingForScan,
                        AssignedScannerId = null
                    });

                _logger.LogInformation("Added new clan to first-time scanning: {ClanId}", timeScanEntry.ClanId);
                newClans++;
            }
        }

        if (newClans > 0)
        {
            anyChanges = true;
        }

        if (FirstTimeScanStates.Count != clansForFirstTimeScanning.Count)
        {
            anyChanges = true;
            var markedForDeletion = FirstTimeScanStates
                .Keys
                .Where(x => clansForFirstTimeScanning.Count(scanEntry => scanEntry.ClanId == x) == 0);

            foreach (var idToRemove in markedForDeletion)
            {
                if (FirstTimeScanStates.TryRemove(idToRemove, out var _))
                {
                }
            }
        }

        return anyChanges;
    }

    private async Task DistributeClansBetweenScannersEvenly()
    {
        if (_currentScannerIds.Count == 0)
        {
            foreach (var (_, stateModel) in GeneralScanStates)
            {
                stateModel.ResetToWaitingState();
            }

            foreach (var (_, stateModel) in PatreonScanStates)
            {
                stateModel.ResetToWaitingState();
            }

            foreach (var (_, stateModel) in FirstTimeScanStates)
            {
                stateModel.AssignedScannerId = null;
                stateModel.State = ClanScanState.WaitingForFetch;
            }

            return;
        }

        await DistributeClansBetweenScanModeEvenly(ServerRunMode.General);

        await DistributeClansBetweenScanModeEvenly(ServerRunMode.Patreon);

        await DistributeFirstTimeClansBetweenScannersEvenly();
    }

    private async Task DistributeClansBetweenScanModeEvenly(ServerRunMode serverRunMode)
    {
        var clanIdsToRemovePostChange = new Dictionary<string, List<long>>();

        var scannerIdsWithMode = _currentScannerIds
            .Where(x => x.Value.ServerRunMode == serverRunMode)
            .ToList();

        if (scannerIdsWithMode.Count == 0)
            return;

        foreach (var (connectionId, _) in scannerIdsWithMode)
        {
            clanIdsToRemovePostChange.Add(connectionId, new List<long>());
        }

        var modeScanStates = serverRunMode == ServerRunMode.General ? GeneralScanStates : PatreonScanStates;

        var newAmountPerScanner = modeScanStates.Count / scannerIdsWithMode.Count;

        var clansCurrentlyDistributed = 0;
        var scannerIdEnumerator = _currentScannerIds
            .Where(x => x.Value.ServerRunMode == serverRunMode)
            .GetEnumerator();

        scannerIdEnumerator.MoveNext();

        foreach (var (_, clanScanStateModel) in modeScanStates)
        {
            if (clansCurrentlyDistributed < newAmountPerScanner)
            {
                if (clanScanStateModel.AssignedScannerId is not null &&
                    clanScanStateModel.AssignedScannerId != scannerIdEnumerator.Current.Key &&
                    clanScanStateModel.State != ClanScanState.CurrentlyScanning)
                {
                    if (clanIdsToRemovePostChange.TryGetValue(clanScanStateModel.AssignedScannerId!, out var scanList))
                    {
                        scanList.Add(clanScanStateModel.ClanId);
                    }
                }

                clanScanStateModel.AssignedScannerId = scannerIdEnumerator.Current.Key;
                clansCurrentlyDistributed++;
                continue;
            }

            scannerIdEnumerator.MoveNext();
            clansCurrentlyDistributed = 0;
        }

        var notAssignedEntries = modeScanStates
            .Where(x => x.Value.AssignedScannerId is null)
            .ToList();

        foreach (var entry in notAssignedEntries)
        {
            entry.Value.AssignedScannerId = scannerIdsWithMode
                .ElementAt(Random.Shared.Next(0, _currentScannerIds.Count))
                .Key;
        }

        foreach (var (scannerId, listOfClans) in clanIdsToRemovePostChange)
        {
            if (listOfClans.Any())
            {
                await _hubContext
                    .Clients
                    .Client(scannerId)
                    .SendAsync(HubMessages.ServerEvents.ClanScanAborted, listOfClans);
            }
        }
    }

    private async Task DistributeFirstTimeClansBetweenScannersEvenly()
    {
        var newAmountPerScanner = FirstTimeScanStates.Count / _currentScannerIds.Count;

        var clansCurrentlyDistributed = 0;
        var scannerIdEnumerator = _currentScannerIds
            .Where(x => x.Value.ServerRunMode == ServerRunMode.General)
            .GetEnumerator();

        scannerIdEnumerator.MoveNext();

        foreach (var (_, clanScanStateModel) in FirstTimeScanStates)
        {
            if (clansCurrentlyDistributed < newAmountPerScanner)
            {
                clanScanStateModel.AssignedScannerId = scannerIdEnumerator.Current.Key;
                clansCurrentlyDistributed++;
                continue;
            }

            scannerIdEnumerator.MoveNext();
            clansCurrentlyDistributed = 0;
        }

        var notAssignedEntries = FirstTimeScanStates
            .Where(x => x.Value.AssignedScannerId is null).ToList();

        var generalScannersCount = _currentScannerIds
            .Count(x => x.Value.ServerRunMode == ServerRunMode.General);

        if (generalScannersCount == 0)
            return;
        
        foreach (var entry in notAssignedEntries)
        {
            entry.Value.AssignedScannerId = _currentScannerIds
                .Where(x => x.Value.ServerRunMode == ServerRunMode.General)
                .ElementAt(Random.Shared.Next(0, generalScannersCount))
                .Key;
        }
    }

    public bool IsScannerValid(string scannerId)
    {
        return _currentScannerIds.Any(x => x.Value.ConnectionId == scannerId);
    }
    
    public string? GetRandomScannerId(ServerRunMode serverRunMode)
    {
        var respectiveScanners =  _currentScannerIds
            .Where(x => x.Value.ServerRunMode == serverRunMode)
            .ToList();

        if (respectiveScanners.Count == 0)
            return null;

        return respectiveScanners[Random.Shared.Next(0, respectiveScanners.Count)].Value.ConnectionId;
    }
    
    private ConcurrentDictionary<long, ClanScanStateModel> GetClanStatesBasedOnMode(string connectionId)
    {
        return _currentScannerIds[connectionId].ServerRunMode == ServerRunMode.General
            ? GeneralScanStates
            : PatreonScanStates;
    }

    private void PerformLockedActionSync(Action action)
    {
        _semaphore.Wait();
        try
        {
            action();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ExecuteThreadSafe(Action action)
    {
        await _semaphore.WaitAsync();
        try
        {
            action();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<TResult> ExecuteThreadSafe<TResult>(Func<TResult> func)
    {
        await _semaphore.WaitAsync();
        try
        {
            return func();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ExecuteThreadSafeAsync(Func<Task> task)
    {
        await _semaphore.WaitAsync();
        try
        {
            await task();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<TResult> ExecuteThreadSafeAsync<TResult>(Func<Task<TResult>> task)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await task();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<(ClanScanStateModel General, ClanScanStateModel Patreon)> GetOldestScans()
    {
        return await ExecuteThreadSafe(() =>
        {
            var general = GeneralScanStates.Values.OrderBy(x => x.LastUpdated).First();
            var patreon = PatreonScanStates.Values.OrderBy(x => x.LastUpdated).First();

            return (general, patreon);
        });
    }

    public async Task<ClanScanStateModel?> GetClanScanState(long clanId)
    {
        return await ExecuteThreadSafe(() =>
        {
            if (GeneralScanStates.TryGetValue(clanId, out var state))
            {
                return state;
            }
            
            if (PatreonScanStates.TryGetValue(clanId, out state))
            {
                return state;
            }
            
            return state;
        });
    }
}