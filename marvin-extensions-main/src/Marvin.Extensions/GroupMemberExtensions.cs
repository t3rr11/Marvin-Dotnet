using DotNetBungieAPI.Models.GroupsV2;

namespace Marvin.Extensions;

public static class GroupMemberExtensions
{
    public static string? GetDisplayName(this GroupMember groupMember)
    {
        if (!string.IsNullOrWhiteSpace(groupMember.DestinyUserInfo?.BungieGlobalDisplayName) &&
            groupMember.DestinyUserInfo?.BungieGlobalDisplayNameCode is not null)
            return
                $"{groupMember.DestinyUserInfo.BungieGlobalDisplayName}#{groupMember.DestinyUserInfo.BungieGlobalDisplayNameCode.Value:D4}";

        if (!string.IsNullOrWhiteSpace(groupMember.BungieNetUserInfo?.BungieGlobalDisplayName) &&
            groupMember.BungieNetUserInfo?.BungieGlobalDisplayNameCode is not null)
            return
                $"{groupMember.BungieNetUserInfo.BungieGlobalDisplayName}#{groupMember.BungieNetUserInfo.BungieGlobalDisplayNameCode.Value:D4}";

        if (!string.IsNullOrWhiteSpace(groupMember.BungieNetUserInfo?.SupplementalDisplayName))
            return groupMember.BungieNetUserInfo.SupplementalDisplayName;

        return groupMember.DestinyUserInfo?.LastSeenDisplayName;
    }
}