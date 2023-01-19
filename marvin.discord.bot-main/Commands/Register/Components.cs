using Discord;
using DotNetBungieAPI.Models.User;
using DotNetBungieAPI.Models;
using Marvin.Bot.Models;

namespace Marvin.Bot.Commands.Register;

public class Components
{
    public static MessageComponent CreateRegisterBungieUserSelectMenu(
        IEnumerable<UserInfoCard> searchResponseDetails)
    {
        searchResponseDetails = searchResponseDetails.Where(x => x.MembershipId != 0);
        var componentBuilder = new ComponentBuilder();

        var userSelectMenu =
            new SelectMenuBuilder()
                .WithCustomId(Events.RegisterBungieUserSelectMenu)
                .WithMaxValues(1);

        var userSelectMenuOptions = new List<SelectMenuOptionBuilder>();

        foreach (var userSearchResponse in searchResponseDetails)
        {
            try
            {
                var optionBuilder = new SelectMenuOptionBuilder()
                    .WithValue(userSearchResponse.MembershipId.ToString())
                    .WithLabel($"{userSearchResponse.BungieGlobalDisplayName}#{userSearchResponse.BungieGlobalDisplayNameCode:D4}")
                    .WithEmote(Emote.Parse(MembershipTypeToEmoji(userSearchResponse.MembershipType)));
                userSelectMenuOptions.Add(optionBuilder);
            }
            catch (Exception ex)
            {
                // TODO: Add generic error embed response on failure.
                Console.WriteLine(ex.ToString());
            }
        }

        userSelectMenu.WithOptions(userSelectMenuOptions);
        componentBuilder.WithSelectMenu(userSelectMenu);

        return componentBuilder.Build();
    }

    private static string MembershipTypeToEmoji(
        BungieMembershipType bungieMembershipType)
    {
        return bungieMembershipType switch
        {
            BungieMembershipType.TigerXbox => "<:xbl:769837546037182475>",
            BungieMembershipType.TigerPsn => "<:psn:769837546091053056>",
            BungieMembershipType.TigerSteam => "<:steam:769837546179919892>",
            BungieMembershipType.TigerBlizzard => "<:bnet:769837546132733962>",
            BungieMembershipType.TigerStadia => "<:stadia:769837546024730634>",
            BungieMembershipType.TigerEgs => "<:egs:1041969553476948038>",
            _ => string.Empty
        };
    }
}
