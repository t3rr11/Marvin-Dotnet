using DotNetBungieAPI.Models.Destiny.Components;
using DotNetBungieAPI.Models.Destiny.Responses;
using Marvin.DbAccess.Models.User;

namespace Marvin.ClanScannerServer.Services.ProfileProcessors.Interfaces;

public interface IProgressionProcessor
{
    void UpdateProgressions(
        IEnumerable<uint> trackedProgressions,
        DestinyProfile destinyProfile,
        DestinyProfileResponse destinyProfileResponse,
        DestinyCharacterComponent character);

}