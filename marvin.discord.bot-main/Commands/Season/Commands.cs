using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.Destiny.Definitions.Seasons;
using DotNetBungieAPI.Service.Abstractions;

namespace Marvin.Bot.Commands.Season;

public class Commands : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    private readonly IBungieClient _bungieClient;

    public Commands(IBungieClient bungieClient)
    {
        _bungieClient = bungieClient;
    }

    [SlashCommand("season", "How long till next season?")]
    public async Task Season()
    {
        var settings = await _bungieClient.ApiAccess.Misc.GetCommonSettings();

        if (_bungieClient.Repository.TryGetDestinyDefinition<DestinySeasonDefinition>(
                settings.Response.Destiny2CoreSettings.СurrentSeason.Hash.Value, BungieLocales.EN, out var season))
        {
            var timestamp = (long)(season.EndDate.Value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)))
                .TotalSeconds;
            var ts = season.EndDate.Value.Subtract(DateTime.Now);

            var testEmbed = new EmbedBuilder()
                .WithTitle($"Season {season.SeasonNumber} - {season.DisplayProperties.Name}")
                .WithDescription(
                    $"""
                        Season {season.SeasonNumber} ends in {ts.ToString("d' Days 'h' Hours 'm' Minutes")}
                        Season {season.SeasonNumber + 1} starts on: <t:{timestamp}:f>
                    """)
                .WithImageUrl($"{season.BackgroundImagePath.AbsolutePath}")
                .Build();

            await Context.Interaction.RespondAsync(embed: testEmbed);
        }
        else
        {
            await Context.Interaction.RespondAsync(text: "Failed");
        }
    }
}