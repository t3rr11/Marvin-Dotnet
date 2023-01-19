using Discord.Interactions;
using Discord.WebSocket;
using Marvin.Bot.Commands.EmbedBuilders;

namespace Marvin.Bot.Commands.Clan;

[Group("clan", "Setup or manage the clan(s) associated with this server")]
public class Commands : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    public Commands()
    {
    }

    [SlashCommand("help", "Helpful information to get you started.")]
    public async Task Help()
    {
        await Context.Interaction.RespondAsync(embed: Embeds.Clan.Help);
    }

    [SlashCommand("list", "Show a list of clans linked to this server.")]
    public async Task List()
    {
        await Context.Interaction.RespondAsync(embed: Embeds.Clan.Help);
    }

    [SlashCommand("setup", "Link your clan to this server.")]
    public async Task Setup()
    {
        await Context.Interaction.RespondAsync(embed: Embeds.Clan.Help);
    }

    [SlashCommand("info", "See information about the clans on this server.")]
    public async Task Info()
    {
        await Context.Interaction.RespondAsync(embed: Embeds.Clan.Help);
    }

    [SlashCommand("activity", "See what clannies are up to.")]
    public async Task Activity()
    {
        await Context.Interaction.RespondAsync(embed: Embeds.Clan.Help);
    }

    [SlashCommand("add", "Link another clan to this discord.")]
    public async Task Add(
        [Summary(description: "Use `/clan help` to learn how to get this ID value for this command")] long id)
    {
        await Context.Interaction.RespondAsync(embed: Embeds.Clan.Help);
    }
}