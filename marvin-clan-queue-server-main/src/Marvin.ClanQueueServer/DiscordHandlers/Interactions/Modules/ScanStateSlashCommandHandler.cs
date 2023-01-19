using System.Text;
using Discord.Interactions;
using Discord.WebSocket;
using Marvin.ClanQueueServer.Models;
using Marvin.ClanQueueServer.Services;

namespace Marvin.ClanQueueServer.DiscordHandlers.Interactions.Modules;

[Group("scan-states", "commands related to clan scan states")]
public class ScanStateSlashCommandHandler : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    private readonly ClanScanningTrackerService _clanScanningTrackerService;

    public ScanStateSlashCommandHandler(ClanScanningTrackerService clanScanningTrackerService)
    {
        _clanScanningTrackerService = clanScanningTrackerService;
    }

    [SlashCommand("oldest", "Gets 2 oldest clans")]
    public async Task ReportOldest()
    {
        var oldestScans = await _clanScanningTrackerService.GetOldestScans();

        var embed = EmbedBuilding.CreateSimpleEmbed(
            "Oldest scans",
            $"**General**:\n{FormatClanState(oldestScans.General)}\n**Patreon**:\n{FormatClanState(oldestScans.Patreon)}");

        await Context.Interaction.RespondAsync(embed: embed);
    }

    [SlashCommand("lookup-clan", "Gets 2 oldest clans")]
    public async Task ReportStatusOfClan(long clanId)
    {
        var clanState = await _clanScanningTrackerService.GetClanScanState(clanId);
        
        var embed = EmbedBuilding.CreateSimpleEmbed(
            "Oldest scans",
            FormatClanState(clanState));

        await Context.Interaction.RespondAsync(embed: embed);
    }

    private string FormatClanState(ClanScanStateModel? clanScanStateModel)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"> Id: {clanScanStateModel?.ClanId}");
        sb.AppendLine($"> State: {clanScanStateModel?.State}");
        sb.AppendLine($"> Last updated: {clanScanStateModel?.LastUpdated}");
        sb.AppendLine($"> Last scan started: {clanScanStateModel?.LastScanStarted}");
        sb.AppendLine($"> Assigned scanner id: {clanScanStateModel?.AssignedScannerId}");
        sb.AppendLine($"> Scanning by: {clanScanStateModel?.ScanningBy}");
        
        return sb.ToString();
    }
}