using DotNetBungieAPI.Models.GroupsV2;

namespace Marvin.ClanScannerServer.Extensions;

public static class GroupMemberExtensions
{
    public static bool ShouldScanClanMember(this GroupMember clanMember)
    {
        if (clanMember.IsOnline)
            return true;
        
        return (DateTime.UtcNow - clanMember.LastOnlineStatusChange.UnixTimeStampToDateTime()).TotalMinutes <= 15;
    }
}