using DotNetBungieAPI.Models.Common;

namespace Marvin.ClanQueueServer.Services.Interfaces;

public interface IBungieNetHealthCheck
{
    bool IsLive { get; }
    event Func<bool, Task> StatusChanged;
    CoreSettingsConfiguration? LatestSettingsResponse { get; }
    DateTime? LatestSettingsResponseDate { get; }
}