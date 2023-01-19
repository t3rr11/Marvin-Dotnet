using Discord.Interactions;
using Discord.WebSocket;
using Marvin.Bot.Services.Interfaces;
using Marvin.DbAccess.Models.User;
using Marvin.DbAccess.Services.Interfaces;

namespace Marvin.Bot.Commands.Register;

public class Commands : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    private readonly IUserSearchService _userSearchService;
    private readonly IUserAccountDbAccess _userAccountDbAccess;

    public Commands(IUserSearchService userSearchService, IUserAccountDbAccess userAccountDbAccess)
    {
        _userSearchService = userSearchService;
        _userAccountDbAccess = userAccountDbAccess;
    }

    [SlashCommand("register", "Use this to link your discord account with your Destiny 2 account")]
    public async Task Register([Summary("name", "Search for your bungie name, usually something along the lines of Marvin#1234")] string lookupValue)
    {
        await Context.Interaction.DeferAsync(true);
        var lookupResult = await _userSearchService.SearchUserCachedAsync(lookupValue);

        switch (lookupResult.Count)
        {
            case 0:
                await Context.Interaction.FollowupAsync(embed: Embeds.Register.NoResults(lookupValue));
                return;
            case 1:
                var bungieProfile = lookupResult.First()!;
                var userData = await _userSearchService.GetUserMembershipsWithClanData(bungieProfile.MembershipId);
                var membership = userData.MembershipsWithClan.First()!;
                var userName = $"{bungieProfile.BungieGlobalDisplayName}#{bungieProfile.BungieGlobalDisplayNameCode:D4}";
                var userAccount = new UserAccountDbModel()
                {
                    DiscordId = Context.User.Id.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    MembershipId = membership.DestinyProfile.MembershipId,
                    Platform = membership.DestinyProfile.MembershipType,
                    Username = userName
                };
                await _userAccountDbAccess.UpsertUserAccountAsync(userAccount, default);
                await Context.Interaction.FollowupAsync(embed: Embeds.Register.SuccessfullyRegistered(userName));
                return;

            case > 25:
                await Context.Interaction.FollowupAsync(embed: Embeds.Register.TooManyResults());
                return;

            default:
                var selectMenu = Components.CreateRegisterBungieUserSelectMenu(lookupResult);
                try
                {
                    await Context.Interaction.FollowupAsync(
                        embed: Embeds.Register.MultipleResults(),
                        components: selectMenu,
                        ephemeral: true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return;
        }
    }
}