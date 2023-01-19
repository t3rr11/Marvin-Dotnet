using Discord.WebSocket;
using Marvin.ClanQueueServer.Hubs;
using Marvin.ClanQueueServer.Services.Interfaces;
using Marvin.HostedServices.Extensions;
using Marvin.Hub.Messaging;
using Marvin.Hub.Messaging.Models;
using Microsoft.AspNetCore.SignalR;

namespace Marvin.ClanQueueServer.Services.Hosted;

public class HostedClanTracker : PeriodicBackgroundService
{
    private const double ClanScanTimeoutSeconds = 5.0;

    private readonly ILogger<HostedClanTracker> _logger;
    private readonly ClanScanningTrackerService _clanScanningTrackerService;
    private readonly IHubContext<ClanQueueHub> _clanQueueHub;
    private readonly DiscordClientService _discordClientService;

    public HostedClanTracker(
        ILogger<HostedClanTracker> logger,
        ClanScanningTrackerService clanScanningTrackerService,
        IHubContext<ClanQueueHub> clanQueueHub,
        IBungieNetHealthCheck bungieNetHealthCheck,
        DiscordClientService discordClientService,
        IBungieNetManifestUpdater bungieNetManifestUpdater) : base(logger)
    {
        _logger = logger;
        _clanScanningTrackerService = clanScanningTrackerService;
        _clanQueueHub = clanQueueHub;
        _discordClientService = discordClientService;

        _clanScanningTrackerService.OnClanTrackingStopped += OnClanTrackingStopped;
        bungieNetHealthCheck.StatusChanged += BungieNetHealthCheckOnStatusChanged;
        bungieNetManifestUpdater.ManifestUpdateStarted += BungieNetManifestUpdaterOnManifestUpdateStarted;
    }

    private async Task BungieNetManifestUpdaterOnManifestUpdateStarted()
    {
        await _clanQueueHub.Clients.All.SendAsync(HubMessages.ServerEvents.AbortAllClanScans);
    }

    private async Task BungieNetHealthCheckOnStatusChanged(bool status)
    {
        if (status is false)
        {
            await _clanQueueHub.Clients.All.SendAsync(HubMessages.ServerEvents.AbortAllClanScans);
            var alertChannel = _discordClientService.GetAlertChannel();
            if (alertChannel is not null)
            {
                await alertChannel.SendMessageAsync("Bungie.net API went down");
            }
        }
        else
        {
            var alertChannel = _discordClientService.GetAlertChannel();
            if (alertChannel is not null)
            {
                await alertChannel.SendMessageAsync("Bungie.net API is live!");
            }
        }
    }

    protected override async Task BeforeExecutionAsync(CancellationToken stoppingToken)
    {
        ChangeTimerSafe(TimeSpan.FromSeconds(5));
    }

    protected override async Task OnTimerExecuted(CancellationToken cancellationToken)
    {
        await _clanScanningTrackerService.ActualiseFromDb(cancellationToken);
        var currentTime = DateTime.UtcNow;
        await CheckOutdatedGeneralScans(currentTime, cancellationToken);
        await CheckOutdatedPatreonScans(currentTime, cancellationToken);
        await CheckOutdatedFirstTimeScans(currentTime, cancellationToken);
    }

    private async Task CheckOutdatedGeneralScans(DateTime currentTime, CancellationToken cancellationToken)
    {
        var outdatedScans = _clanScanningTrackerService
            .GeneralScanStates
            .Where(x =>
                x.Value.LastScanStarted.HasValue &&
                (currentTime - x.Value.LastScanStarted.Value).TotalMinutes >= ClanScanTimeoutSeconds);

        if (outdatedScans.Any())
        {
            var groupedByScannerId = outdatedScans
                .GroupBy(x => x.Value.ScanningBy)
                .ToDictionary(x => x.Key!, x => x.Select(x => x.Key).ToList());

            foreach (var (scannerId, clans) in groupedByScannerId)
            {
                await _clanQueueHub
                    .Clients
                    .Client(scannerId)
                    .SendAsync(HubMessages.ServerEvents.ClanScanAborted, clans, cancellationToken: cancellationToken);
            }
        }
    }

    private async Task CheckOutdatedPatreonScans(DateTime currentTime, CancellationToken cancellationToken)
    {
        var outdatedScans = _clanScanningTrackerService
            .PatreonScanStates
            .Where(x =>
                x.Value.LastScanStarted.HasValue &&
                (currentTime - x.Value.LastScanStarted.Value).TotalMinutes >= ClanScanTimeoutSeconds);

        if (outdatedScans.Any())
        {
            var groupedByScannerId = outdatedScans
                .GroupBy(x => x.Value.ScanningBy)
                .ToDictionary(x => x.Key!, x => x.Select(x => x.Key).ToList());

            foreach (var (scannerId, clans) in groupedByScannerId)
            {
                await _clanQueueHub
                    .Clients
                    .Client(scannerId)
                    .SendAsync(HubMessages.ServerEvents.ClanScanAborted, clans, cancellationToken: cancellationToken);
            }
        }
    }

    private async Task CheckOutdatedFirstTimeScans(DateTime currentTime, CancellationToken cancellationToken)
    {
        foreach (var trackedFirstTimeScan in _clanScanningTrackerService
                     .FirstTimeScanStates
                     .Where(x =>
                         x.Value.IsOutdatedOrInvalid(currentTime, _clanScanningTrackerService)))
        {
            trackedFirstTimeScan.Value.AssignedScannerId =
                _clanScanningTrackerService.GetRandomScannerId(ServerRunMode.General);
        }
    }


    private async Task OnClanTrackingStopped(
        long clanId,
        string? scannerId)
    {
        if (scannerId is not null)
        {
            await _clanQueueHub.Clients.Client(scannerId).SendAsync(HubMessages.ServerEvents.ClanScanAborted, clanId);
        }

        _logger.LogInformation("Stopping scan for clan {ClanId}", clanId);
    }
}