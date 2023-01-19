using DotNetBungieAPI.HashReferences;
using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.Destiny.Definitions.PresentationNodes;
using DotNetBungieAPI.Models.Destiny.Definitions.Records;
using DotNetBungieAPI.Service.Abstractions;
using Marvin.ClanQueueServer.Models.ManifestValidation;

namespace Marvin.ClanQueueServer.Services;

public class BungieNetManifestValidator
{
    private readonly IBungieClient _bungieClient;

    public BungieNetManifestValidator(
        IBungieClient bungieClient)
    {
        _bungieClient = bungieClient;
    }

    public async Task<ManifestValidationReport> ValidateManifest()
    {
        var report = new ManifestValidationReport();

        await CheckSeals(report);

        return report;
    }

    private async Task CheckSeals(ManifestValidationReport report)
    {
        await CheckPresentationNodeSeals(report);
        await CheckPresentationNodeLegacySeals(report);
    }

    private async Task CheckPresentationNodeSeals(
        ManifestValidationReport report)
    {
        DestinyPresentationNodeDefinition? sealsNode = null;
        try
        {
            sealsNode = await LoadPresentationNodeAsync(DefinitionHashes.PresentationNodes.Seals);
        }
        catch (Exception _)
        {
            report.Errors.Add(new ManifestValidationError()
            {
                Message = "[Seals presentation node]: missing in DB"
            });
        }

        if (sealsNode is null)
            return;

        foreach (var sealPresentationNode in sealsNode.Children.PresentationNodes)
        {
            var nodeHash = sealPresentationNode.PresentationNode.Hash.GetValueOrDefault();

            if (nodeHash is 0)
            {
                report.Warnings.Add(new ManifestValidationWarning()
                {
                    Message = "[Seals presentation node]: Contains empty node"
                });
                continue;
            }

            var sealPresentationNodeDefinition = await LoadPresentationNodeAsync(nodeHash);

            if (sealPresentationNodeDefinition.Redacted)
            {
                report.Warnings.Add(new ManifestValidationWarning()
                {
                    Message = $"[Seals presentation node]: Presentation node {nodeHash} is redacted"
                });
                continue;
            }

            var recordHash = sealPresentationNodeDefinition.CompletionRecord.Hash.GetValueOrDefault();

            if (recordHash is 0)
            {
                report.Warnings.Add(new ManifestValidationWarning()
                {
                    Message = $"[Seals presentation node]: Presentation node {nodeHash} is missing linked record"
                });
                continue;
            }

            var sealRecordDefinition = await LoadRecordAsync(recordHash);

            if (sealRecordDefinition.Redacted)
            {
                report.Warnings.Add(new ManifestValidationWarning()
                {
                    Message = $"[Seals presentation node]: Presentation node {nodeHash} record {recordHash} is redacted"
                });
                continue;
            }

            if (sealRecordDefinition.TitleInfo.GildingTrackingRecord.HasValidHash)
            {
                var gildRecordHash = sealRecordDefinition.TitleInfo.GildingTrackingRecord.Hash.GetValueOrDefault();
                var titleGildRecordDefinition = await LoadRecordAsync(gildRecordHash);

                if (titleGildRecordDefinition.Redacted)
                {
                    report.Warnings.Add(new ManifestValidationWarning()
                    {
                        Message =
                            $"[Seals presentation node]: Presentation node {nodeHash} record {recordHash} gilding record {gildRecordHash} is redacted"
                    });
                    continue;
                }
            }
        }
        
        report.Successes.Add(new ManifestValidationSuccess()
        {
            Message = "[Seals presentation node]: finished check"
        });
    }
    
    private async Task CheckPresentationNodeLegacySeals(
        ManifestValidationReport report)
    {
        DestinyPresentationNodeDefinition? legacy = null;
        try
        {
            legacy = await LoadPresentationNodeAsync(DefinitionHashes.PresentationNodes.Seals);
        }
        catch (Exception _)
        {
            report.Errors.Add(new ManifestValidationError()
            {
                Message = "[Legacy seals presentation node]: missing in DB"
            });
        }

        if (legacy is null)
            return;

        foreach (var sealPresentationNode in legacy.Children.PresentationNodes)
        {
            var nodeHash = sealPresentationNode.PresentationNode.Hash.GetValueOrDefault();

            if (nodeHash is 0)
            {
                report.Warnings.Add(new ManifestValidationWarning()
                {
                    Message = "[Legacy seals presentation node]: Contains empty node"
                });
                continue;
            }

            var sealPresentationNodeDefinition = await LoadPresentationNodeAsync(nodeHash);

            if (sealPresentationNodeDefinition.Redacted)
            {
                report.Warnings.Add(new ManifestValidationWarning()
                {
                    Message = $"[Legacy seals presentation node]: Presentation node {nodeHash} is redacted"
                });
                continue;
            }

            var recordHash = sealPresentationNodeDefinition.CompletionRecord.Hash.GetValueOrDefault();

            if (recordHash is 0)
            {
                report.Warnings.Add(new ManifestValidationWarning()
                {
                    Message = $"[Legacy seals presentation node]: Presentation node {nodeHash} is missing linked record"
                });
                continue;
            }

            var sealRecordDefinition = await LoadRecordAsync(recordHash);

            if (sealRecordDefinition.Redacted)
            {
                report.Warnings.Add(new ManifestValidationWarning()
                {
                    Message = $"[Legacy seals presentation node]: Presentation node {nodeHash} record {recordHash} is redacted"
                });
                continue;
            }

            if (sealRecordDefinition.TitleInfo.GildingTrackingRecord.HasValidHash)
            {
                var gildRecordHash = sealRecordDefinition.TitleInfo.GildingTrackingRecord.Hash.GetValueOrDefault();
                var titleGildRecordDefinition = await LoadRecordAsync(gildRecordHash);

                if (titleGildRecordDefinition.Redacted)
                {
                    report.Warnings.Add(new ManifestValidationWarning()
                    {
                        Message =
                            $"[Legacy seals presentation node]: Presentation node {nodeHash} record {recordHash} gilding record {gildRecordHash} is redacted"
                    });
                    continue;
                }
            }
        }
        
        report.Successes.Add(new ManifestValidationSuccess()
        {
            Message = "[Legacy seals presentation node]: finished check"
        });
    }

    private async Task<DestinyRecordDefinition> LoadRecordAsync(uint hash)
    {
        return await _bungieClient
            .DefinitionProvider
            .LoadDefinition<DestinyRecordDefinition>(hash, BungieLocales.EN);
    }

    private async Task<DestinyPresentationNodeDefinition> LoadPresentationNodeAsync(uint hash)
    {
        return await _bungieClient
            .DefinitionProvider
            .LoadDefinition<DestinyPresentationNodeDefinition>(hash, BungieLocales.EN);
    }
}