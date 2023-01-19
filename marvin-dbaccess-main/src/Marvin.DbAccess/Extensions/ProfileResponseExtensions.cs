using DotNetBungieAPI.Models.Destiny.Components;

namespace Marvin.DbAccess.Extensions;

public static class ProfileResponseExtensions
{
    public static DestinyCharacterComponent GetMostPlayedCharacterSafe(
        this DictionaryComponentResponseOfint64AndDestinyCharacterComponent component)
    {
        long longestPlayedId = 0;
        long longestPlayed = -1;
        foreach (var (key, value) in component.Data)
            if (value.MinutesPlayedTotal > longestPlayed)
            {
                longestPlayed = value.MinutesPlayedTotal;
                longestPlayedId = key;
            }

        return component.Data[longestPlayedId];
    }
}