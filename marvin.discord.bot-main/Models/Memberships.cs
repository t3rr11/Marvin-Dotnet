using DotNetBungieAPI.Models.GroupsV2;
using DotNetBungieAPI.Models.User;

namespace Marvin.Bot.Models;

public class MembershipWithClan
{
    public DestinyProfileUserInfoCard DestinyProfile { get; init; }
    public GroupMembership? ClanData { get; init; }
}

public class MembershipWithClanSearchResult
{
    public UserInfoCard BungieNetMembership { get; init; }
    public List<MembershipWithClan> MembershipsWithClan { get; init; }
}