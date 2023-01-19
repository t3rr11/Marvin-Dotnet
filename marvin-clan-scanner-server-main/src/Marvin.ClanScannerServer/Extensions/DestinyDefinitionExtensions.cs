using DotNetBungieAPI.Models.Destiny.Definitions.Progressions;
using DotNetBungieAPI.Models.Destiny.Definitions.SeasonPasses;
using DotNetBungieAPI.Models.Destiny.Definitions.Seasons;
using DotNetBungieAPI.Service.Abstractions;

namespace Marvin.ClanScannerServer.Extensions;

public static class DestinyDefinitionExtensions
{
    public static string GetReadableNameForProgression(
        this DestinyProgressionDefinition progressionDefinition,
        IBungieClient bungieClient)
    {
        var seasonPasses = bungieClient.Repository.GetAll<DestinySeasonPassDefinition>();

        var seasonPassDefinition = seasonPasses
            .FirstOrDefault(x =>
                x.RewardProgression == progressionDefinition.Hash ||
                x.PrestigeProgression == progressionDefinition.Hash);

        if (seasonPassDefinition is not null)
        {
            var seasons = bungieClient.Repository.GetAll<DestinySeasonDefinition>();

            var seasonDef = seasons
                .FirstOrDefault(x =>
                    x.SeasonPass == seasonPassDefinition.Hash &&
                    x.DisplayProperties.Name != "Season of [Redacted]");

            if (seasonDef is null)
            {
                return "Something is invalid with this definition";
            }

            if (seasonPassDefinition.RewardProgression == progressionDefinition.Hash)
                return $"Season {seasonDef.SeasonNumber} Rank";

            if (seasonPassDefinition.PrestigeProgression == progressionDefinition.Hash)
                return $"Season {seasonDef.SeasonNumber} Overflow Rank";
        }

        return progressionDefinition.DisplayProperties.Name;
    }

    public static string GetReadableDescriptionForProgression(
        this DestinyProgressionDefinition progressionDefinition,
        IBungieClient bungieClient)
    {
        var seasonPasses = bungieClient.Repository.GetAll<DestinySeasonPassDefinition>();

        var seasonPassDefinition = seasonPasses
            .FirstOrDefault(x =>
                x.RewardProgression == progressionDefinition.Hash ||
                x.PrestigeProgression == progressionDefinition.Hash);

        if (seasonPassDefinition is not null)
        {
            var seasonDef = bungieClient
                .Repository
                .GetAll<DestinySeasonDefinition>()
                .FirstOrDefault(x =>
                    x.SeasonPass == seasonPassDefinition.Hash &&
                    x.DisplayProperties.Name != "Season of [Redacted]");
            
            if (seasonDef is null)
            {
                return "Something is invalid with this definition";
            }

            if (seasonPassDefinition.RewardProgression == progressionDefinition.Hash)
                return seasonDef.DisplayProperties.Name;

            if (seasonPassDefinition.PrestigeProgression == progressionDefinition.Hash)
                return seasonDef.DisplayProperties.Name;
        }

        return progressionDefinition.DisplayProperties.Description;
    }
}