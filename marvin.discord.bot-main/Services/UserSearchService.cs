using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.GroupsV2;
using DotNetBungieAPI.Models.Queries;
using DotNetBungieAPI.Models.Requests;
using DotNetBungieAPI.Models.User;
using DotNetBungieAPI.Service.Abstractions;
using Marvin.Bot.Models;
using Marvin.Bot.Services.Interfaces;
using Marvin.DbAccess.Services.Interfaces;
using Marvin.MemoryCache;

namespace Marvin.Bot.Services;

public class UserSearchService : IUserSearchService
{
    private readonly IBungieClient _bungieClient;
    private readonly ICacheProvider _cacheProvider;
    private readonly IDestinyProfileDbAccess _destinyProfileDbAccess;

    public UserSearchService(
        IBungieClient bungieClient,
        ICacheProvider cacheProvider,
        IDestinyProfileDbAccess destinyProfileDbAccess)
    {
        _bungieClient = bungieClient;
        _cacheProvider = cacheProvider;
        _destinyProfileDbAccess = destinyProfileDbAccess;
    }

    public async ValueTask<List<UserInfoCard>> SearchUserCachedAsync(string input)
    {
        var userInfoCards = new List<UserInfoCard>();

        if (input.Contains("#"))
        {
            var searchInput = input.Split('#');
            var firstResponse = await _bungieClient
                .ApiAccess
                .Destiny2
                .SearchDestinyPlayerByBungieName(BungieMembershipType.All, new ExactSearchRequest()
                {
                    DisplayName = searchInput[0],
                    DisplayNameCode = short.Parse(searchInput[1])
                });
            userInfoCards.AddRange(firstResponse.Response);
        }

        var secondResponse = await _bungieClient
            .ApiAccess
            .User
            .SearchByGlobalNamePost(new UserSearchPrefixRequest(input));
        secondResponse.Response.SearchResults.ToList().ForEach(result =>
        {
            userInfoCards.AddRange(result.DestinyMemberships);
        });

        return userInfoCards;
    }

    public async ValueTask<DestinyLinkedProfilesResponse> GetDestinyLinkedAccountsCachedAsync(long bungieMembershipId)
    {
        return await _cacheProvider.GetAsync($"{nameof(GetDestinyLinkedAccountsCachedAsync)}_{bungieMembershipId}",
            async () =>
            {
                var response = await _bungieClient
                    .ApiAccess
                    .Destiny2
                    .GetLinkedProfiles(
                        membershipType: BungieMembershipType.All,
                        membershipId: bungieMembershipId,
                        getAllMemberships: true
                    );
                return response.Response;
            },
            TimeSpan.FromMinutes(5));
    }

    public async ValueTask<GroupMembership?> GetClanForMemberCachedAsync(BungieMembershipType bungieMembershipType,
        long destinyMembershipId)
    {
        return await _cacheProvider.GetAsync(
            $"{nameof(GetClanForMemberCachedAsync)}_{(int)bungieMembershipType}_{destinyMembershipId}", async () =>
            {
                var response = await _bungieClient
                    .ApiAccess
                    .GroupV2
                    .GetGroupsForMember(
                        bungieMembershipType,
                        destinyMembershipId,
                        GroupsForMemberFilter.All,
                        GroupType.Clan
                    );
                return response.Response.Results.FirstOrDefault();
            },
            TimeSpan.FromMinutes(5));
    }

    public async ValueTask<MembershipWithClanSearchResult> GetUserMembershipsWithClanData(long bungieNetMembershipId)
    {
        var linkedProfiles = await GetDestinyLinkedAccountsCachedAsync(bungieNetMembershipId);

        var membershipsWithClan = new List<MembershipWithClan>();
        foreach (var destinyProfile in linkedProfiles.Profiles)
        {
            var clanData = await GetClanForMemberCachedAsync(
                destinyProfile.MembershipType,
                destinyProfile.MembershipId);

            membershipsWithClan.Add(new MembershipWithClan()
            {
                DestinyProfile = destinyProfile,
                ClanData = clanData
            });
        }

        return new MembershipWithClanSearchResult()
        {
            BungieNetMembership = linkedProfiles.BungieNetMembership,
            MembershipsWithClan = membershipsWithClan
        };
    }
}