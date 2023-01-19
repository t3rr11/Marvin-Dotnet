using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Marvin.ClanScannerServer.Options;
using Marvin.ClanScannerServer.Services.ScanningStrategies;
using Marvin.DbAccess.Models.Clan;
using Marvin.Hub.Messaging;
using Marvin.Hub.Messaging.Interfaces;
using Marvin.Hub.Messaging.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace Marvin.ClanScannerServer.Services;

public class ClanQueueHubConnection : IClanQueueHub
{
    private readonly IOptions<ClanQueueServerOptions> _clanQueueServerOptions;
    private readonly ILogger<ClanQueueHubConnection> _logger;
    private readonly HubConnection _connection;

    public bool IsConnected { get; set; }

    /// <summary>
    ///     Sent when received server events that signals to remove clans from queue
    /// </summary>
    public Func<List<long>, Task>? OnClanScansAborted;

    public Func<Task>? ManifestUpdated;

    public Func<Task>? OnReconnected;

    public ClanQueueHubConnection(
        IOptions<ClanQueueServerOptions> clanQueueServerOptions,
        ILogger<ClanQueueHubConnection> logger)
    {
        _clanQueueServerOptions = clanQueueServerOptions;
        _logger = logger;

        _connection = new HubConnectionBuilder()
            .WithUrl($"{_clanQueueServerOptions.Value.Address}/clanQueueHub", options =>
            {
                options.WebSocketConfiguration = conf =>
                {
                    conf.RemoteCertificateValidationCallback = (message, cert, chain, errors) => { return true; };
                };
                options.HttpMessageHandlerFactory = factory => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
                };
            })
            .Build();

        _connection.Closed += OnConnectionClosed;

        _connection.On<List<long>>(HubMessages.ServerEvents.ClanScanAborted, async (clanIds) =>
        {
            if (OnClanScansAborted != null)
            {
                await OnClanScansAborted(clanIds);
            }
        });

        _connection.On(
            HubMessages.ServerEvents.ManifestUpdated,
            async () =>
            {
                if (ManifestUpdated != null)
                {
                    await ManifestUpdated();
                }
            });
    }

    public async Task WaitUntilConnectedAsync()
    {
        var continueAttempts = true;

        if (IsConnected)
            return;

        while (continueAttempts)
        {
            try
            {
                _logger.LogInformation("Attempting to connect to queue server...");
                await _connection.StartAsync();
                _logger.LogInformation("Connected to queue server!");
                continueAttempts = false;
                IsConnected = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to queue server... Attempting again in 5s");
                await Task.Delay(5000);
            }
        }
    }

    public async Task ClearConnection()
    {
        await _connection.StopAsync();
    }

    public async Task<ServerRunMode> RegisterServerIdAsync()
    {
        return await _connection.InvokeAsync<ServerRunMode>(HubMessages.ServerMethods.RegisterServerId);
    }

    public async Task DeregisterServerIdAsync()
    {
        await _connection.InvokeAsync(HubMessages.ServerMethods.DeregisterServerId);
    }

    public async Task<IEnumerable<long>> GetClansForScanningAsync()
    {
        return await _connection.InvokeAsync<IEnumerable<long>>(HubMessages.ServerMethods.GetClansForScanning);
    }

    public async Task<IEnumerable<FirstTimeScanEntry>> GetClansForFirstTimeScanningAsync()
    {
        return await _connection.InvokeAsync<IEnumerable<FirstTimeScanEntry>>(HubMessages.ServerMethods
            .GetClansForFirstTimeScanning);
    }

    public async Task SuccessfullyScannedClansAsync(long[] clanIds)
    {
        await _connection.InvokeAsync(HubMessages.ServerMethods.SuccessfullyScannedClans, clanIds);
    }

    public async Task StartedClanScanAsync(long clanId)
    {
        await _connection.InvokeAsync(HubMessages.ServerMethods.StartedClanScan, clanId);
    }

    public async Task FirstTimeScanDoneAsync(long clanId)
    {
        await _connection.InvokeAsync(HubMessages.ServerMethods.FirstTimeScanDone, clanId);
    }

    private async Task OnConnectionClosed(Exception? exception)
    {
        IsConnected = false;
        
        _ = WaitUntilConnectedAsync();
        if (OnReconnected != null)
        {
            await OnReconnected();
        }

        IsConnected = true;
    }
}