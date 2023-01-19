using DotNetBungieAPI.Models.Destiny.Responses;
using Marvin.DbAccess.Models.Guild;
using Marvin.DbAccess.Models.User;

namespace Marvin.ClanScannerServer.Services.ProfileProcessors.Interfaces;

public interface IRecordProcessor
{
    void UpdateRecords(
        DestinyProfile destinyProfile,
        IEnumerable<uint> trackedProfileRecords,
        DestinyProfileResponse destinyProfileResponse);

    void UpdateAndReportRecords(
        Dictionary<ulong, GuildBroadcastsConfig> broadcastsConfigs,
        DestinyProfile destinyProfile,
        List<(uint titleHash, uint? gildingHash)> titlesHashes,
        IEnumerable<uint> trackedProfileRecords,
        DestinyProfileResponse destinyProfileResponse,
        CancellationToken cancellationToken);

}