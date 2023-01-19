using DotNetBungieAPI.Models.Destiny.Responses;
using Marvin.DbAccess.Models.User;

namespace Marvin.ClanScannerServer.Services.ProfileProcessors.Interfaces;

public interface IMetricProcessor
{
    void UpdateMetrics(
        List<uint> trackedMetrics,
        DestinyProfileResponse destinyProfileResponse,
        DestinyProfile destinyProfile);

}