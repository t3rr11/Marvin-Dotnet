using DotNetBungieAPI.Models.Destiny.Definitions.Progressions;

namespace Marvin.Bot.Extensions;

public static class DestinyDefinitionExtensions
{
    public static int GetTotalProgressionValue(this DestinyProgressionDefinition progressionDefinition)
    {
        return progressionDefinition.Steps.Sum(x => x.ProgressTotal);
    }
}