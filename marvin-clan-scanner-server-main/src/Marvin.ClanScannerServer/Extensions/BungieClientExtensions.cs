using DotNetBungieAPI.Extensions;
using DotNetBungieAPI.HashReferences;
using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.Destiny.Definitions.PresentationNodes;
using DotNetBungieAPI.Service.Abstractions;

namespace Marvin.ClanScannerServer.Extensions;

public static class BungieClientExtensions
{
    public static List<(uint titleHash, uint? gildingHash)> GetTitleAndGildRecordHashes(this IBungieClient bungieClient)
    {
        var hashes = new List<(uint titleHash, uint? gildingHash)>();

        AddTitleRecordHashesFromPresentationNode(
            bungieClient,
            hashes,
            DefinitionHashes.PresentationNodes.LegacySeals);

        AddTitleRecordHashesFromPresentationNode(
            bungieClient,
            hashes,
            DefinitionHashes.PresentationNodes.Seals);

        return hashes;
    }

    private static void AddTitleRecordHashesFromPresentationNode(
        IBungieClient bungieClient,
        List<(uint titleHash, uint? gildingHash)> hashes,
        uint presentationNodeHash)
    {
        if (!bungieClient.TryGetDefinition<DestinyPresentationNodeDefinition>(
                presentationNodeHash,
                BungieLocales.EN, out var sealsPresentationNodeDefinition))
            return;

        foreach (var nodeSealEntry in sealsPresentationNodeDefinition.Children.PresentationNodes)
        {
            uint titleHash = 0;
            uint? gildingHash = null;

            if (!nodeSealEntry.PresentationNode.TryGetDefinition(out var sealDefinition))
                continue;

            if (sealDefinition.Redacted)
                continue;

            titleHash = sealDefinition.CompletionRecord.Hash.GetValueOrDefault();

            if (!sealDefinition.CompletionRecord.TryGetDefinition(out var sealRecordDefinition))
            {
                hashes.Add((titleHash, gildingHash));
                continue;
            }

            if (sealRecordDefinition.TitleInfo is null)
                continue;

            if (!sealRecordDefinition.TitleInfo.GildingTrackingRecord.HasValidHash)
            {
                hashes.Add((titleHash, gildingHash));
                continue;
            }

            gildingHash = sealRecordDefinition.TitleInfo.GildingTrackingRecord.Hash.GetValueOrDefault();
            hashes.Add((titleHash, gildingHash));
        }
    }
}