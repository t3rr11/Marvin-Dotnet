using DotNetBungieAPI.Models.Destiny.Components;
using DotNetBungieAPI.Models.Destiny.Responses;
using Marvin.DbAccess.Models.User;
using Marvin.ProfileProcessors.Interfaces;

namespace Marvin.ProfileProcessors;

public class ProgressionProcessor : IProgressionProcessor
{
    public void UpdateProgressions(
        IEnumerable<uint> trackedProgressions,
        DestinyProfile destinyProfile,
        DestinyProfileResponse destinyProfileResponse,
        DestinyCharacterComponent character)
    {
        if (!destinyProfileResponse.CharacterProgressions.Data.ContainsKey(character.CharacterId))
            return;

        if (destinyProfileResponse.CharacterProgressions.Data.TryGetValue(
                character.CharacterId,
                out var progressionsComponent))
            foreach (var progressionHash in trackedProgressions)
                if (progressionsComponent.Progressions.TryGetValue(progressionHash, out var progressionComponent))
                {
                    if (destinyProfile.Progressions.TryGetValue(progressionHash, out var userProgressionData))
                    {
                        userProgressionData.Level = progressionComponent.Level;
                        userProgressionData.CurrentResetCount = progressionComponent.CurrentResetCount;
                        userProgressionData.CurrentProgress = progressionComponent.CurrentProgress;
                        userProgressionData.DailyProgress = progressionComponent.DailyProgress;
                        userProgressionData.WeeklyProgress = progressionComponent.WeeklyProgress;
                    }
                    else
                    {
                        var newUserProgressionData = new UserProgressionData
                        {
                            Level = progressionComponent.Level,
                            CurrentResetCount = progressionComponent.CurrentResetCount,
                            CurrentProgress = progressionComponent.CurrentProgress,
                            DailyProgress = progressionComponent.DailyProgress,
                            WeeklyProgress = progressionComponent.WeeklyProgress
                        };

                        destinyProfile.Progressions.Add(progressionHash, newUserProgressionData);
                    }
                }
    }
}