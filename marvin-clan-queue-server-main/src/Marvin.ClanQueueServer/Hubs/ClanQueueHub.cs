using Marvin.ClanQueueServer.DiscordHandlers;
using Marvin.ClanQueueServer.Services;
using Marvin.DbAccess.Models.Clan;
using Marvin.DbAccess.Services.Interfaces;
using Marvin.Hub.Messaging;
using Marvin.Hub.Messaging.Interfaces;
using Marvin.Hub.Messaging.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace Marvin.ClanQueueServer.Hubs;

public class ClanQueueHub : Microsoft.AspNetCore.SignalR.Hub, IClanQueueHub
{
    private readonly ILogger<ClanQueueHub> _logger;
    private readonly ClanScanningTrackerService _clanScanningTrackerService;
    private readonly DiscordClientService _discordClientService;

    public ClanQueueHub(
        ILogger<ClanQueueHub> logger,
        ClanScanningTrackerService clanScanningTrackerService,
        DiscordClientService discordClientService)
    {
        _logger = logger;
        _clanScanningTrackerService = clanScanningTrackerService;
        _discordClientService = discordClientService;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            await _clanScanningTrackerService.RemoveClanScannerAndReassignClans(Context.ConnectionId);
            if (exception is not null)
            {
                _logger.LogError(exception, "{HubConnectionId} disconnected with error", Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation("{HubConnectionId} disconnected", Context.ConnectionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle disconnect with {HubConnectionId}", Context.ConnectionId);
        }

        try
        {
            var alertChannel = _discordClientService.GetAlertChannel();
            if (alertChannel is not null)
            {
                await alertChannel.SendMessageAsync(embed: EmbedBuilding.CreateSimpleEmbed(
                    "Alert",
                    $"Scanner {Context.ConnectionId} disconnected"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to alert discord with disconnect id {HubConnectionId}", Context.ConnectionId);
        }
    }

    [HubMethodName(HubMessages.ServerMethods.RegisterServerId)]
    public async Task<ServerRunMode> RegisterServerIdAsync()
    {
        var scanMode = await _clanScanningTrackerService.AddNewClanScannerAndReassignClans(Context.ConnectionId);
        var scannerName = "Unknown";

        try
        {
            var httpConnectionFeature = Context.Features.Get<IHttpConnectionFeature>();

            if (httpConnectionFeature is not null)
            {
                scannerName = httpConnectionFeature.RemoteIpAddress?.ToString() switch
                {
                    "45.33.42.97" => "Marvin Scanner 1 (Thor)",
                    "50.116.10.125" => "Marvin Scanner 2 (Turbo)",
                    _ => scannerName
                };

                _logger.LogInformation(
                    "New scanner connected with ID = {ID}, Scan Mode to {ScanMode}, IP is {ScannerIp}, Name is {ScannerName}",
                    Context.ConnectionId,
                    scanMode,
                    httpConnectionFeature.RemoteIpAddress?.ToString(),
                    scannerName
                );
            }
            else
            {
                _logger.LogInformation("New scanner connected with ID = {ID}, Scan Mode to {ScanMode}",
                    Context.ConnectionId,
                    scanMode);
            }

            var alertChannel = _discordClientService.GetAlertChannel();
            if (alertChannel is not null)
            {
                await alertChannel.SendMessageAsync(embed: EmbedBuilding.CreateSimpleEmbed(
                    "Alert",
                    $"Scanner {scannerName} ({Context.ConnectionId}) connected as {scanMode}"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while handling logging features");
        }

        return scanMode;
    }

    [HubMethodName(HubMessages.ServerMethods.DeregisterServerId)]
    public async Task DeregisterServerIdAsync()
    {
        await _clanScanningTrackerService.RemoveClanScannerAndReassignClans(Context.ConnectionId);
    }

    [HubMethodName(HubMessages.ServerMethods.GetClansForScanning)]
    public async Task<IEnumerable<long>> GetClansForScanningAsync()
    {
        return await _clanScanningTrackerService.FetchClanIdsForScanner(Context.ConnectionId);
    }

    [HubMethodName(HubMessages.ServerMethods.GetClansForFirstTimeScanning)]
    public async Task<IEnumerable<FirstTimeScanEntry>> GetClansForFirstTimeScanningAsync()
    {
        return await _clanScanningTrackerService.FetchFirstTimeClanIdsForScanner(Context.ConnectionId);
    }

    [HubMethodName(HubMessages.ServerMethods.SuccessfullyScannedClans)]
    public async Task SuccessfullyScannedClansAsync(long[] clanIds)
    {
        await _clanScanningTrackerService.UpdateScannedClans(Context.ConnectionId, clanIds);
    }

    [HubMethodName(HubMessages.ServerMethods.StartedClanScan)]
    public async Task StartedClanScanAsync(long clanId)
    {
        await _clanScanningTrackerService.MarkClanScanStarted(Context.ConnectionId, clanId);
    }

    [HubMethodName(HubMessages.ServerMethods.FirstTimeScanDone)]
    public async Task FirstTimeScanDoneAsync(long clanId)
    {
        await _clanScanningTrackerService.FirstTimeScanFinished(clanId);
    }
}