using DotNetBungieAPI.Service.Abstractions;
using Marvin.ClanQueueServer.DiscordHandlers;
using Marvin.ClanQueueServer.Hubs;
using Marvin.ClanQueueServer.Services.Interfaces;
using Marvin.HostedServices.Extensions;
using Marvin.Hub.Messaging;
using Microsoft.AspNetCore.SignalR;

namespace Marvin.ClanQueueServer.Services.Hosted;

public class BungieNetManifestUpdaterService : PeriodicBackgroundService, IBungieNetManifestUpdater
{
    private readonly IBungieClient _bungieClient;
    private readonly DiscordClientService _discordClientService;
    private readonly ILogger<BungieNetManifestUpdaterService> _logger;
    private readonly IHubContext<ClanQueueHub> _clanQueueHub;

    public bool IsUpdating { get; private set; }
    public event Func<Task>? ManifestUpdateStarted;

    public BungieNetManifestUpdaterService(
        IBungieClient bungieClient,
        DiscordClientService discordClientService,
        ILogger<BungieNetManifestUpdaterService> logger,
        IHubContext<ClanQueueHub> clanQueueHub) : base(logger)
    {
        _bungieClient = bungieClient;
        _discordClientService = discordClientService;
        _logger = logger;
        _clanQueueHub = clanQueueHub;
    }

    protected override async Task BeforeExecutionAsync(CancellationToken stoppingToken)
    {
        ChangeTimerSafe(TimeSpan.FromSeconds(30));
        await _bungieClient.DefinitionProvider.Initialize();
    }

    protected override async Task OnTimerExecuted(CancellationToken cancellationToken)
    {
        try
        {
            var hasUpdates = await _bungieClient.DefinitionProvider.CheckForUpdates();

            if (hasUpdates)
            {
                IsUpdating = true;
                try
                {
                    var alertChannel = _discordClientService.GetAlertChannel();

                    if (alertChannel is not null)
                    {
                        await alertChannel.SendMessageAsync(
                            embed: EmbedBuilding.CreateSimpleEmbed(
                                "Manifest alert!",
                                "Manifest update detected, starting update"));
                    }

                    var sw = System.Diagnostics.Stopwatch.StartNew();

                    if (ManifestUpdateStarted != null)
                    {
                        await ManifestUpdateStarted();
                    }

                    await _bungieClient.DefinitionProvider.Update();

                    sw.Stop();

                    if (alertChannel is not null)
                    {
                        await alertChannel.SendMessageAsync(
                            embed: EmbedBuilding.CreateSimpleEmbed(
                                "Manifest alert!",
                                $"Manifest update finished, took {sw.ElapsedMilliseconds} ms to update"));
                    }

                    await _clanQueueHub.Clients.All.SendAsync(HubMessages.ServerEvents.ManifestUpdated);
                }
                finally
                {
                    IsUpdating = false;
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Encountered error during manifest update check");
        }
    }
}