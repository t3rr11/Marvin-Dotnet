using Marvin.ClanScannerServer.Options;
using Marvin.ClanScannerServer.Services.ScanningStrategies;
using Marvin.Hub.Messaging.Models;
using Microsoft.Extensions.Options;

namespace Marvin.ClanScannerServer.Services.Hosted;

public class SignalRClientBootstrap : IHostedService
{
    private readonly ClanQueueHubConnection _clanQueueHubConnection;
    private readonly ILogger<SignalRClientBootstrap> _logger;
    private readonly CurrentScanningStrategyHolder _scanningStrategyHolder;

    public SignalRClientBootstrap(
        ClanQueueHubConnection clanQueueHubConnection,
        ILogger<SignalRClientBootstrap> logger,
        CurrentScanningStrategyHolder scanningStrategyHolder)
    {
        _clanQueueHubConnection = clanQueueHubConnection;
        _logger = logger;
        _scanningStrategyHolder = scanningStrategyHolder;
        _clanQueueHubConnection.OnReconnected += OnReconnectedAsync;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _clanQueueHubConnection.WaitUntilConnectedAsync();
            var scanMode = await _clanQueueHubConnection.RegisterServerIdAsync();
            _scanningStrategyHolder.CurrentScanningStrategy = scanMode switch
            {
                ServerRunMode.General => _scanningStrategyHolder.GeneralScanningStrategy,
                ServerRunMode.Patreon => _scanningStrategyHolder.PatreonScanningStrategy
            };
            await _scanningStrategyHolder.CurrentScanningStrategy.ForceClearQueueAndCancelAllScans();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to start SignalR client");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _clanQueueHubConnection.DeregisterServerIdAsync();
        await _clanQueueHubConnection.ClearConnection();
    }

    private async Task OnReconnectedAsync()
    {
        var scanMode = await _clanQueueHubConnection.RegisterServerIdAsync();
        _scanningStrategyHolder.CurrentScanningStrategy = scanMode switch
        {
            ServerRunMode.General => _scanningStrategyHolder.GeneralScanningStrategy,
            ServerRunMode.Patreon => _scanningStrategyHolder.PatreonScanningStrategy
        };
        await _scanningStrategyHolder.CurrentScanningStrategy.ForceClearQueueAndCancelAllScans();
    }
}