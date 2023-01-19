using DotNetBungieAPI.Models.GroupsV2;
using Marvin.ClanScannerServer.Models;

namespace Marvin.ClanScannerServer.Services.Hosted.Interfaces;

public interface IUserQueue
{
    Task<ClanScanProgress> EnqueueAndWaitForBroadcastedUserScans(
        ClanUpdateContext updateContext,
        List<GroupMember> memberScanTasks,
        CancellationToken cancellationToken);
    
    Task<ClanScanProgress> EnqueueAndWaitForSilentUserScans(
        ClanUpdateContext updateContext,
        List<GroupMember> memberScanTasks,
        CancellationToken cancellationToken);
}