using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Marvin.DbAccess.Models.User;
using Marvin.DbAccess.Services.Interfaces;
using Marvin.Bot.Services.Interfaces;

namespace Marvin.Bot.Commands.Register;

public class Events : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    public const string RegisterBungieUserSelectMenu = "register_select_user";
    public const string RegisterDestinyUserSelectMenu = "register_select_membership";
    private readonly IUserSearchService _userSearchService;
    private readonly IUserAccountDbAccess _userAccountDbAccess;

    public Events(IUserSearchService userSearchService, IUserAccountDbAccess userAccountDbAccess)
    {
        _userSearchService = userSearchService;
        _userAccountDbAccess = userAccountDbAccess;
    }

    [ComponentInteraction(RegisterBungieUserSelectMenu)]
    public async Task OnRegisterBungieUserSelectMenu()
    {
        try
        {
            var selection = Context.Interaction.Data.Values.FirstOrDefault()!;
            var userData = await _userSearchService.GetUserMembershipsWithClanData(long.Parse(selection));
            var membership = userData.MembershipsWithClan.First()!;
            var userName =
                $"{membership.DestinyProfile.DisplayName}#{membership.DestinyProfile.BungieGlobalDisplayNameCode:D4}";
            var userAccount = new UserAccountDbModel()
            {
                DiscordId = Context.User.Id.ToString(),
                CreatedAt = DateTime.UtcNow,
                MembershipId = membership.DestinyProfile.MembershipId,
                Platform = membership.DestinyProfile.MembershipType,
                Username = userName
            };
            await _userAccountDbAccess.UpsertUserAccountAsync(userAccount, default);
            await Context.Interaction.UpdateAsync(msg =>
            {
                msg.Embeds = new Embed[] { Embeds.Register.SuccessfullyRegistered(userName) };
                msg.Components = null;
            });
        }
        catch (Exception ex)
        {
            // TODO: Add generic error embed response on failure.
            Console.WriteLine(ex.ToString());
        }
    }
}