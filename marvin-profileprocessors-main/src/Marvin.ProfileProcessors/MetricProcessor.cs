using DotNetBungieAPI.Clients;
using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Service.Abstractions;
using Marvin.DbAccess.Models.User;
using Marvin.ProfileProcessors.Interfaces;

namespace Marvin.ProfileProcessors;

public class MetricProcessor : IMetricProcessor
{
    private readonly IBungieClient _bungieClient;

    public MetricProcessor(IBungieClient bungieClient)
    {
        _bungieClient = bungieClient;
    }

    public void UpdateMetrics(
        IEnumerable<uint> trackedMetrics,
        DestinyProfileResponse destinyProfileResponse,
        DestinyProfile destinyProfile)
    {
        if (destinyProfileResponse.Metrics.Data is null)
            return;

        foreach (var metricHash in trackedMetrics)
            if (destinyProfileResponse.Metrics.Data.Metrics.TryGetValue(metricHash, out var metricComponent))
            {
                if (destinyProfile.Metrics.TryGetValue(metricHash, out var userMetricData))
                {
                    userMetricData.Progress = metricComponent.ObjectiveProgress.Progress.GetValueOrDefault();
                }
                else
                {
                    var newUserMetricData = new UserMetricData
                    {
                        Progress = metricComponent.ObjectiveProgress.Progress.GetValueOrDefault()
                    };
                    destinyProfile.Metrics.Add(metricHash, newUserMetricData);
                }
            }
    }
}