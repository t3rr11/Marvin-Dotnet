using DotNetBungieAPI.Models.GroupsV2;
using DotNetBungieAPI.Models.Queries;
using DotNetBungieAPI.Models.User;
using DotNetBungieAPI.Models;
using Marvin.Bot.Models;

namespace Marvin.Bot.Services.Interfaces;

public interface IUserSearchService
{
    ValueTask<List<UserInfoCard>> SearchUserCachedAsync(string input);
    ValueTask<GroupMembership?> GetClanForMemberCachedAsync(BungieMembershipType bungieMembershipType, long destinyMembershipId);
    ValueTask<DestinyLinkedProfilesResponse> GetDestinyLinkedAccountsCachedAsync(long bungieMembershipId);
    ValueTask<MembershipWithClanSearchResult> GetUserMembershipsWithClanData(long bungieNetMembershipId);
}
