using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Marvin.Bot.Commands.EmbedBuilders;

namespace Marvin.Bot.Commands.Help;

public class Commands : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    public Commands() { }

    [SlashCommand("help", "Need help? We Gotcha!")]
    public async Task Help()
    {
        await Context.Interaction.RespondAsync(embed: Embeds.Help.Setup, components: Components.components);
    }
}
