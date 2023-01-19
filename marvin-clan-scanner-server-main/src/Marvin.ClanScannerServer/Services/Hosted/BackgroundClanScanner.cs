using DotNetBungieAPI.Service.Abstractions;
using Marvin.ClanScannerServer.Services.ScanningStrategies;
using Marvin.HostedServices.Extensions;

namespace Marvin.ClanScannerServer.Services.Hosted;

public class BackgroundClanScanner : PeriodicBackgroundService
{
    private readonly ClanQueueHubConnection _clanQueueHubConnection;
    private readonly CurrentScanningStrategyHolder _currentScanningStrategyHolder;
    private readonly IBungieClient _bungieClient;

    public BackgroundClanScanner(
        ILogger<BackgroundClanScanner> logger,
        ClanQueueHubConnection clanQueueHubConnection,
        CurrentScanningStrategyHolder currentScanningStrategyHolder,
        IBungieClient bungieClient) : base(logger)
    {
        _clanQueueHubConnection = clanQueueHubConnection;
        _currentScanningStrategyHolder = currentScanningStrategyHolder;
        _bungieClient = bungieClient;
    }

    protected override Task BeforeExecutionAsync(CancellationToken stoppingToken)
    {
        ChangeTimerSafe(TimeSpan.FromSeconds(1));

        _clanQueueHubConnection.OnClanScansAborted += OnClanScanAborted;
        
        _clanQueueHubConnection.ManifestUpdated += ManifestUpdated;

        return Task.CompletedTask;
    }

    private async Task ManifestUpdated()
    {
        _bungieClient.Repository.Clear();
        await _bungieClient.DefinitionProvider.Initialize();
        await _bungieClient.DefinitionProvider.ReadToRepository(_bungieClient.Repository);
    }

    protected override async Task OnTimerExecuted(CancellationToken cancellationToken)
    {
        if (_currentScanningStrategyHolder.CurrentScanningStrategy is not null)
        {
            await _currentScanningStrategyHolder.CurrentScanningStrategy.ProcessClansTick(cancellationToken);
        }
    }
    
    private async Task OnClanScanAborted(List<long> clanIds)
    {
        if (_currentScanningStrategyHolder.CurrentScanningStrategy is not null)
        {
            await _currentScanningStrategyHolder.CurrentScanningStrategy.OnClanScanAborted(clanIds);
        }
    }
}