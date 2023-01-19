using System.Numerics;
using System.Text;
using Discord;
using DotNetBungieAPI.HashReferences;
using DotNetBungieAPI.Models.Destiny.Definitions.Progressions;
using DotNetBungieAPI.Models.Destiny.Definitions.Seasons;
using Marvin.Bot.Models.Leaderboards;

namespace Marvin.Bot.Commands.EmbedBuilders;

public static partial class Embeds
{
    public static class Leaderboards
    {
        public static Dictionary<uint, string> ProgressionNameOverloads { get; } = new()
        {
            { DefinitionHashes.Progressions.Savvy, "Season of Plunder Reputation" },
            { DefinitionHashes.Progressions.ReputationReward, "Season of the Seraph Reputation" },
            { DefinitionHashes.Progressions.Reputation, "Season of the Risen Reputation" },
            { DefinitionHashes.Progressions.CrownInfluence, "Season of the Haunted Reputation" },
            { DefinitionHashes.Progressions.CrucibleRank, "Valor" },
            { DefinitionHashes.Progressions.TrialsRank, "Trials" },
            { DefinitionHashes.Progressions.GambitRank, "Infamy" },
            { DefinitionHashes.Progressions.IronBannerRank, "Iron Banner" },
            { DefinitionHashes.Progressions.CompetitiveDivision, "Competitive Division" }
        };

        public static Embed CreateRankingLeaderboard<TEntry, TKey>(
            int supposedAmount,
            string rankingsName,
            string subtitle,
            string emptySubtitle,
            List<TEntry> leaderboard,
            Func<TEntry, TKey> keyGetter,
            Func<TEntry, object>[] valueGetter,
            TKey? currentKey)
        {
            var eb = GetPrefilledEmbed();
            eb.WithTitle(rankingsName);

            if (leaderboard.Count == 0)
            {
                eb.WithDescription(emptySubtitle);
                return eb.Build();
            }

            var amountIndentation = supposedAmount.ToString().Length;
            var paddingBuffer = new int[valueGetter.Length];

            for (var i = 0; i < valueGetter.Length; i++)
            {
                var value = valueGetter[i](leaderboard[0]);

                if (i == valueGetter.Length - 1)
                {
                    paddingBuffer[i] = 0;
                }
                else
                {
                    paddingBuffer[i] = value.ToString().Length;
                }
            }

            var sb = new StringBuilder();

            sb.AppendLine($"```{subtitle}\n");

            var cutLeaderboard = leaderboard.Take(supposedAmount).ToArray();

            var valueBuffer = new object[valueGetter.Length];

            for (var i = 0; i < cutLeaderboard.Length; i++)
            {
                var entry = cutLeaderboard[i];

                var paddedAmount = (i + 1).ToString().PadLeft(amountIndentation);

                for (var j = 0; j < valueGetter.Length; j++)
                {
                    valueBuffer[j] = valueGetter[j](entry);
                }

                var mainText = string.Join(
                    " | ",
                    valueBuffer.Select((x, inc) => x.ToString().PadLeft(paddingBuffer[inc])));

                sb.AppendLine($"{paddedAmount}: {mainText}");
            }

            if (currentKey is not null)
            {
                var indexOfUser = leaderboard.FindIndex(x => keyGetter(x).Equals(currentKey));

                if (indexOfUser != -1)
                {
                    var user = leaderboard[indexOfUser];
                    var paddedAmount = (indexOfUser + 1).ToString().PadLeft(amountIndentation);

                    for (var j = 0; j < valueGetter.Length; j++)
                    {
                        valueBuffer[j] = valueGetter[j](user);
                    }

                    var mainText = string.Join(
                        " | ",
                        valueBuffer.Select((x, inc) => x.ToString().PadLeft(paddingBuffer[inc])));

                    sb.AppendLine($"\n{paddedAmount}: {mainText}");
                }
            }

            sb.Append("```");

            eb.WithDescription(sb.ToString());

            return eb.Build();
        }
    }
}