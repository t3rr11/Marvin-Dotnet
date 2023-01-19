using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Marvin.ClanQueueServer.Services;

namespace Marvin.ClanQueueServer.DiscordHandlers.Interactions.Modules;

public class ManifestValidationSlashCommandHandler : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    private readonly BungieNetManifestValidator _validator;

    public ManifestValidationSlashCommandHandler(BungieNetManifestValidator validator)
    {
        _validator = validator;
    }

    [SlashCommand("validate-manifest", "validates current loaded manifest")]
    public async Task ValidateManifestAsync()
    {
        await Context.Interaction.DeferAsync();

        var report = await _validator.ValidateManifest();

        var embed = new EmbedBuilder().WithTitle("Manifest validation result");

        if (report.Successes.Count > 0)
        {
            embed.AddField(
                "Successes", 
                $"```{String.Join("\n", report.Successes.Select(x => x.Message))}```");
        }
        
        if (report.Warnings.Count > 0)
        {
            embed.AddField(
                "Warnings", 
                $"```{String.Join("\n", report.Warnings.Select(x => x.Message))}```");
        }
        
        if (report.Errors.Count > 0)
        {
            embed.AddField(
                "Errors", 
                $"```{String.Join("\n", report.Errors.Select(x => x.Message))}```");
        }

        await Context.Interaction.FollowupAsync(embed: embed.Build());
    }
}