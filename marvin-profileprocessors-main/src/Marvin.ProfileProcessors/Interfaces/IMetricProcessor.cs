using DotNetBungieAPI.Models.Destiny.Responses;
using Marvin.DbAccess.Models.User;

namespace Marvin.ProfileProcessors.Interfaces;

public interface IMetricProcessor
{
    void UpdateMetrics(
        IEnumerable<uint> trackedMetrics,
        DestinyProfileResponse destinyProfileResponse,
        DestinyProfile destinyProfile);
}