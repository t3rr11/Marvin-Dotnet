using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Marvin.ClanQueueServer.Hubs;
using Marvin.ClanQueueServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace Marvin.ClanQueueServer.DiscordHandlers.Interactions.Modules;

public class ConnectedScannersSlashCommandHandler : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    private readonly ClanScanningTrackerService _clanScanningTrackerService;

    public ConnectedScannersSlashCommandHandler(
        ClanScanningTrackerService clanScanningTrackerService)
    {
        _clanScanningTrackerService = clanScanningTrackerService;
    }

    [SlashCommand("report-scanner-states", "Reports a detailed message on what scanners are doing")]
    public async Task ReportStates()
    {
        await Context.Interaction.DeferAsync();
        var report = await _clanScanningTrackerService.CollectScanStatesReport();
        
        var embed = new EmbedBuilder().WithTitle("Scanner states report");

        if (report.CurrentScanners.Count == 0)
        {
            embed.AddField("No scanners connected :(", "Looks like no scanners connected to this hub yet");
        }
        else
        {
            foreach (var (scannerId, clanTrackerDetails) in report.CurrentScanners)
            {
                embed.AddField(
                    $"{scannerId} | {clanTrackerDetails.ServerRunMode} | {clanTrackerDetails.ClanAmount}",
                    string.Join("\n", clanTrackerDetails.ClansAmountPerState.Select(x => $"{x.Key}: {x.Value}")));
            }
        }

        await Context.Interaction.FollowupAsync(embed: embed.Build());
    }
}