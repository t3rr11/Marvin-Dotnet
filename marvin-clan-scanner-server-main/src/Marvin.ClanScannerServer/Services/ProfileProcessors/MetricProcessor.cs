using System.Runtime.InteropServices;
using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Service.Abstractions;
using Marvin.ClanScannerServer.Services.ProfileProcessors.Interfaces;
using Marvin.DbAccess.Models.User;

namespace Marvin.ClanScannerServer.Services.ProfileProcessors;

public class MetricProcessor : IMetricProcessor
{
    private readonly IBungieClient _bungieClient;

    public MetricProcessor(IBungieClient bungieClient)
    {
        _bungieClient = bungieClient;
    }

    public void UpdateMetrics(
        List<uint> trackedMetrics,
        DestinyProfileResponse destinyProfileResponse,
        DestinyProfile destinyProfile)
    {
        if (destinyProfileResponse.Metrics.Data is null)
            return;

        foreach (var metricHash in trackedMetrics)
        {
            if (!destinyProfileResponse.Metrics.Data.Metrics.TryGetValue(metricHash, out var metricComponent))
                continue;

            if (destinyProfile.Metrics.TryGetValue(metricHash, out var userMetricData))
            {
                userMetricData.Progress = metricComponent.ObjectiveProgress.Progress.GetValueOrDefault();
            }
            else
            {
                // for some reason objective progress can be null sometimes (start of season 20 as example)
                if (metricComponent.ObjectiveProgress is null)
                    continue;

                var newUserMetricData = new UserMetricData
                {
                    Progress = metricComponent.ObjectiveProgress.Progress.GetValueOrDefault()
                };
                destinyProfile.Metrics.Add(metricHash, newUserMetricData);
            }
        }
    }
}