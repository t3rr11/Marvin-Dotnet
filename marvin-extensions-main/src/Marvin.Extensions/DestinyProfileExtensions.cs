using DotNetBungieAPI.Models.Destiny.Responses;

namespace Marvin.Extensions;

public static class DestinyProfileExtensions
{
    public static bool HasPublicRecords(this DestinyProfileResponse profileResponse)
    {
        return profileResponse.ProfileRecords.Data is not null;
    }
}