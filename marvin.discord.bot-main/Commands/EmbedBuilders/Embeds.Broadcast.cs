using System.Text;
using Discord;
using DotNetBungieAPI.HashReferences;
using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.Destiny.Definitions.Collectibles;
using DotNetBungieAPI.Models.Destiny.Definitions.PresentationNodes;
using DotNetBungieAPI.Models.Destiny.Definitions.Records;
using DotNetBungieAPI.Service.Abstractions;
using Marvin.Bot.Models;
using Marvin.DbAccess.EntityFramework.Models.Clans;
using Marvin.DbAccess.EntityFramework.Models.UserBroadcasts;
using Marvin.DbAccess.EntityFramework.Models.Broadcasts;
using Marvin.DbAccess.EntityFramework.Models.ClanBroadcasts;

namespace Marvin.Bot.Commands.EmbedBuilders;

public static partial class Embeds
{
    public static class Broadcast
    {
        public static Embed BuildDestinyUserBroadcast(
            UserBroadcastDbModel destinyUserBroadcast,
            ClanDbModel clanData,
            IBungieClient bungieClient,
            string username)
        {
            var embedBuilder = new EmbedBuilder();

            embedBuilder.WithTitle($"Clan Broadcast - {clanData.ClanName}");

            switch (destinyUserBroadcast.Type)
            {
                case BroadcastType.Collectible:
                    AddCollectibleDataToEmbed(embedBuilder, destinyUserBroadcast, bungieClient, username);
                    break;
                case BroadcastType.Triumph:
                    AddTriumphDataToEmbed(embedBuilder, destinyUserBroadcast, bungieClient, username);
                    break;
                case BroadcastType.Title:
                    AddTitleDataToEmbed(embedBuilder, destinyUserBroadcast, bungieClient, username);
                    break;
                case BroadcastType.GildedTitle:
                    AddTitleGildDataToEmbed(embedBuilder, destinyUserBroadcast, bungieClient, username);
                    break;
            }

            embedBuilder
                .WithFooter(FooterDomain, FooterURL)
                .WithColor((uint)CustomDiscordColors.Gold)
                .WithCurrentTimestamp();

            return embedBuilder.Build();
        }
        
        private static void AddCollectibleDataToEmbed(
            EmbedBuilder embedBuilder,
            UserBroadcastDbModel destinyUserBroadcast,
            IBungieClient bungieClient,
            string username)
        {
            if (bungieClient.TryGetDefinition<DestinyCollectibleDefinition>(
                    (uint)destinyUserBroadcast.DefinitionHash,
                    BungieLocales.EN,
                    out var collectibleDefinition))
            {
                embedBuilder.WithThumbnailUrl(collectibleDefinition.DisplayProperties.Icon.AbsolutePath);

                if (destinyUserBroadcast.AdditionalData is not null &&
                    destinyUserBroadcast.AdditionalData.TryGetValue("completions", out var complString))
                {
                    if (int.TryParse(complString, out var activityCompletions))
                        embedBuilder.WithDescription(
                            $"{username} has obtained [{collectibleDefinition.DisplayProperties.Name}](https://www.light.gg/db/items/{collectibleDefinition.Item.Hash.GetValueOrDefault()}) on their {activityCompletions}th clear");
                    else
                        embedBuilder.WithDescription(
                            $"{username} has obtained [{collectibleDefinition.DisplayProperties.Name}](https://www.light.gg/db/items/{collectibleDefinition.Item.Hash.GetValueOrDefault()})");
                }
                else
                {
                    embedBuilder.WithDescription(
                        $"{username} has obtained [{collectibleDefinition.DisplayProperties.Name}](https://www.light.gg/db/items/{collectibleDefinition.Item.Hash.GetValueOrDefault()})");
                }
            }
        }
        
        private static void AddTriumphDataToEmbed(
            EmbedBuilder embedBuilder,
            UserBroadcastDbModel destinyUserBroadcast,
            IBungieClient bungieClient,
            string username)
        {
            if (bungieClient.TryGetDefinition<DestinyRecordDefinition>(
                    (uint)destinyUserBroadcast.DefinitionHash,
                    BungieLocales.EN,
                    out var recordDefinition))
            {
                embedBuilder.WithThumbnailUrl(recordDefinition.DisplayProperties.Icon.AbsolutePath);

                embedBuilder.WithDescription(
                    $"{username} has completed triumph: {recordDefinition.DisplayProperties.Name}");

                if (!string.IsNullOrEmpty(recordDefinition.DisplayProperties.Description))
                    embedBuilder.AddField("How to complete:", recordDefinition.DisplayProperties.Description);
            }
        }
        
        private static void AddTitleDataToEmbed(
            EmbedBuilder embedBuilder,
            UserBroadcastDbModel destinyUserBroadcast,
            IBungieClient bungieClient,
            string username)
        {
            if (bungieClient.TryGetDefinition<DestinyRecordDefinition>(
                    (uint)destinyUserBroadcast.DefinitionHash,
                    BungieLocales.EN,
                    out var recordDefinition))
            {
                if (recordDefinition.DisplayProperties.Icon.HasValue)
                {
                    embedBuilder.WithThumbnailUrl(recordDefinition.DisplayProperties.Icon.AbsolutePath);
                }
                else
                {
                    var recordTitleNode = bungieClient
                        .Repository
                        .GetAll<DestinyPresentationNodeDefinition>()
                        .FirstOrDefault(x => x.CompletionRecord.Hash == (uint)destinyUserBroadcast.DefinitionHash);
                    if (recordTitleNode is not null)
                        embedBuilder.WithThumbnailUrl(recordTitleNode.DisplayProperties.Icon.AbsolutePath);
                }

                embedBuilder.WithDescription(
                    $"{username} has obtained title: **{recordDefinition.TitleInfo.TitlesByGenderHash[DefinitionHashes.Genders.Masculine]}**");
            }
        }
        
        private static void AddTitleGildDataToEmbed(
            EmbedBuilder embedBuilder,
            UserBroadcastDbModel destinyUserBroadcast,
            IBungieClient bungieClient,
            string username)
        {
            if (bungieClient.TryGetDefinition<DestinyRecordDefinition>(
                    (uint)destinyUserBroadcast.DefinitionHash,
                    BungieLocales.EN,
                    out _))
            {
                var titleHash = uint.Parse(destinyUserBroadcast.AdditionalData["parentTitleHash"]);
                var gildedCount = int.Parse(destinyUserBroadcast.AdditionalData["gildedCount"]);

                if (bungieClient.TryGetDefinition<DestinyRecordDefinition>(
                        titleHash,
                        BungieLocales.EN,
                        out var titleRecordDefinition))
                {
                    if (titleRecordDefinition.DisplayProperties.Icon.HasValue)
                    {
                        embedBuilder.WithThumbnailUrl(titleRecordDefinition.DisplayProperties.Icon.AbsolutePath);
                    }
                    else
                    {
                        var recordTitleNode = bungieClient
                            .Repository
                            .GetAll<DestinyPresentationNodeDefinition>()
                            .FirstOrDefault(x => x.CompletionRecord.Hash == titleHash);
                        if (recordTitleNode is not null)
                            embedBuilder.WithThumbnailUrl(recordTitleNode.DisplayProperties.Icon.AbsolutePath);
                    }

                    embedBuilder.WithDescription(
                        $"{username} has gilded title: **{titleRecordDefinition.TitleInfo.TitlesByGenderHash[DefinitionHashes.Genders.Masculine]}** {gildedCount} times!");
                }
            }
        }
        
        public static Embed BuildDestinyClanBroadcast(
            ClanBroadcastDbModel clanBroadcast,
            ClanDbModel clanData)
        {
            var embedBuilder = new EmbedBuilder();

            embedBuilder.WithTitle($"Clan Broadcast - {clanData.ClanName}");

            switch (clanBroadcast.Type)
            {
                case BroadcastType.ClanLevel:
                    AddClanLevelChangesInfo(embedBuilder, clanBroadcast);
                    break;
                case BroadcastType.ClanName:
                    embedBuilder.WithDescription(
                        $"Clan name was changed from \"{clanBroadcast.OldValue}\" to \"{clanBroadcast.NewValue}\"");
                    break;
                case BroadcastType.ClanCallSign:
                    embedBuilder.WithDescription(
                        $"Clan callsign was changed from \"{clanBroadcast.OldValue}\" to \"{clanBroadcast.NewValue}\"");
                    break;
                case BroadcastType.ClanScanFinished:
                    embedBuilder.WithDescription("Clan scanning was finished!");
                    break;
            }

            embedBuilder
                .WithFooter(FooterDomain, FooterURL)
                .WithColor((uint)CustomDiscordColors.Primary)
                .WithCurrentTimestamp();

            return embedBuilder.Build();
        }
        
        private static void AddClanLevelChangesInfo(
            EmbedBuilder embedBuilder,
            ClanBroadcastDbModel clanBroadcast)
        {
            embedBuilder.WithDescription(
                $"Clan level increased from {clanBroadcast.OldValue} to {clanBroadcast.NewValue}");
            switch (int.Parse(clanBroadcast.NewValue))
            {
                case 2:
                    embedBuilder.AddField("Bonuses", "Increased public event rewards.");
                    break;
                case 3:
                    embedBuilder.AddField("Bonuses",
                        "Increased public event rewards.\nCompleting weekly Hawthorne bounties rewards mod components.");
                    break;
                case 4:
                    embedBuilder.AddField("Bonuses",
                        "Increased public event rewards.\nCompleting weekly Hawthorne bounties rewards mod components.\nCompleting clan vendor challenges rewards enhancement cores.");
                    break;
                case 5:
                    embedBuilder.AddField("Bonuses",
                        "Increased public event rewards.\nCompleting weekly Hawthorne bounties rewards mod components.\nCompleting clan vendor challenges rewards enhancement cores.\nEarn a bonus trials token when winning trials matches with clanmates.");
                    break;
                case 6:
                    embedBuilder.AddField("Bonuses",
                        "Increased public event rewards.\nCompleting weekly Hawthorne bounties rewards mod components.\nCompleting clan vendor challenges rewards enhancement cores.\nEarn a bonus trials token when winning trials matches with clanmates.\nUnlocked an additional weekly bounty from Hawthorne.");
                    break;
            }
        }
        
        public static Embed BuildDestinyUserGroupedBroadcast(
            IEnumerable<UserBroadcastDbModel> destinyUserBroadcasts,
            BroadcastType broadcastType,
            uint definitionHash,
            Dictionary<long, ClanDbModel> clansData,
            IBungieClient bungieClient,
            Dictionary<long, string> usernames)
        {
            var embedBuilder = new EmbedBuilder();

            embedBuilder.WithTitle("Clans Broadcast");

            switch (broadcastType)
            {
                case BroadcastType.Collectible:
                    AddGroupCollectibleDataToEmbed(
                        embedBuilder,
                        destinyUserBroadcasts,
                        definitionHash,
                        clansData,
                        bungieClient,
                        usernames);
                    break;
                case BroadcastType.Triumph:
                    AddGroupTriumphDataToEmbed(
                        embedBuilder,
                        destinyUserBroadcasts,
                        definitionHash,
                        clansData,
                        bungieClient,
                        usernames);
                    break;
                case BroadcastType.Title:
                    AddGroupTitleDataToEmbed(
                        embedBuilder,
                        destinyUserBroadcasts,
                        definitionHash,
                        clansData,
                        bungieClient,
                        usernames);
                    break;
                case BroadcastType.GildedTitle:
                    var broadcast = destinyUserBroadcasts.First();
                    if (broadcast.AdditionalData is not null &&
                        broadcast.AdditionalData.TryGetValue("parentTitleHash", out var parentTitleHashUnparsed) &&
                        uint.TryParse(parentTitleHashUnparsed, out var parentTitleHash))
                        AddGroupTitleGildingDataToEmbed(
                            embedBuilder,
                            destinyUserBroadcasts,
                            definitionHash,
                            parentTitleHash,
                            clansData,
                            bungieClient,
                            usernames);
                    break;
            }

            embedBuilder
                .WithFooter(FooterDomain, FooterURL)
                .WithColor((uint)CustomDiscordColors.Gold)
                .WithCurrentTimestamp();

            return embedBuilder.Build();
        }
        
        private static void AddGroupCollectibleDataToEmbed(
            EmbedBuilder embedBuilder,
            IEnumerable<UserBroadcastDbModel> destinyUserBroadcasts,
            uint definitionHash,
            Dictionary<long, ClanDbModel> clansData,
            IBungieClient bungieClient,
            Dictionary<long, string> usernames)
        {
            if (bungieClient.TryGetDefinition<DestinyCollectibleDefinition>(
                    definitionHash,
                    BungieLocales.EN,
                    out var collectibleDefinition))
            {
                embedBuilder.WithDescription(
                    $"{usernames.Count} people have obtained [{collectibleDefinition.DisplayProperties.Name}](https://www.light.gg/db/items/{collectibleDefinition.Item.Hash.GetValueOrDefault()})!");
                embedBuilder.WithThumbnailUrl(collectibleDefinition.DisplayProperties.Icon.AbsolutePath);

                var stringBuilder = new StringBuilder();
                foreach (var (clanId, clanData) in clansData)
                {
                    stringBuilder.Clear();

                    foreach (var broadcast in destinyUserBroadcasts)
                    {
                        if (broadcast.ClanId != clanId)
                            continue;

                        if (usernames.TryGetValue(broadcast.MembershipId, out var username))
                        {
                            if (broadcast.AdditionalData is not null &&
                                broadcast.AdditionalData.TryGetValue("completions", out var completionsUnparsed) &&
                                int.TryParse(completionsUnparsed, out var completions))
                                stringBuilder.AppendLine($"{username} - on their {completions} clear");
                            else
                                stringBuilder.AppendLine(username);
                        }
                    }

                    embedBuilder.AddField($"Clan: {clanData.ClanName}", stringBuilder.ToString());
                }
            }
        }
        
        private static void AddGroupTriumphDataToEmbed(
            EmbedBuilder embedBuilder,
            IEnumerable<UserBroadcastDbModel> destinyUserBroadcasts,
            uint definitionHash,
            Dictionary<long, ClanDbModel> clansData,
            IBungieClient bungieClient,
            Dictionary<long, string> usernames)
        {
            if (bungieClient.TryGetDefinition<DestinyRecordDefinition>(
                    definitionHash,
                    BungieLocales.EN,
                    out var recordDefinition))
            {
                embedBuilder.WithDescription(
                    $"{usernames.Count} people have completed triumph: **{recordDefinition.DisplayProperties.Name}**");
                embedBuilder.WithThumbnailUrl(recordDefinition.DisplayProperties.Icon.AbsolutePath);

                var stringBuilder = new StringBuilder();
                foreach (var (clanId, clanData) in clansData)
                {
                    stringBuilder.Clear();

                    foreach (var broadcast in destinyUserBroadcasts)
                    {
                        if (broadcast.ClanId != clanId)
                            continue;

                        if (usernames.TryGetValue(broadcast.MembershipId, out var username))
                            stringBuilder.AppendLine(username);
                    }

                    embedBuilder.AddField($"Clan: {clanData.ClanName}", stringBuilder.ToString());
                }

                if (!string.IsNullOrEmpty(recordDefinition.DisplayProperties.Description))
                    embedBuilder.AddField("How to complete:", recordDefinition.DisplayProperties.Description);
            }
        }
        
        private static void AddGroupTitleDataToEmbed(
            EmbedBuilder embedBuilder,
            IEnumerable<UserBroadcastDbModel> destinyUserBroadcasts,
            uint definitionHash,
            Dictionary<long, ClanDbModel> clansData,
            IBungieClient bungieClient,
            Dictionary<long, string> usernames)
        {
            if (bungieClient.TryGetDefinition<DestinyRecordDefinition>(
                    definitionHash,
                    BungieLocales.EN,
                    out var recordDefinition))
            {
                if (recordDefinition.DisplayProperties.Icon.HasValue)
                {
                    embedBuilder.WithThumbnailUrl(recordDefinition.DisplayProperties.Icon.AbsolutePath);
                }
                else
                {
                    var recordTitleNode = bungieClient
                        .Repository
                        .GetAll<DestinyPresentationNodeDefinition>()
                        .FirstOrDefault(x => x.CompletionRecord.Hash == definitionHash);
                    if (recordTitleNode is not null)
                        embedBuilder.WithThumbnailUrl(recordTitleNode.DisplayProperties.Icon.AbsolutePath);
                }

                embedBuilder.WithDescription(
                    $"{usernames.Count} people have obtained title: **{recordDefinition.TitleInfo.TitlesByGenderHash[DefinitionHashes.Genders.Masculine]}**");

                var stringBuilder = new StringBuilder();
                foreach (var (clanId, clanData) in clansData)
                {
                    stringBuilder.Clear();

                    foreach (var broadcast in destinyUserBroadcasts)
                    {
                        if (broadcast.ClanId != clanId)
                            continue;

                        if (usernames.TryGetValue(broadcast.MembershipId, out var username))
                            stringBuilder.AppendLine(username);
                    }

                    embedBuilder.AddField($"Clan: {clanData.ClanName}", stringBuilder.ToString());
                }
            }
        }
        
        private static void AddGroupTitleGildingDataToEmbed(
            EmbedBuilder embedBuilder,
            IEnumerable<UserBroadcastDbModel> destinyUserBroadcasts,
            uint definitionHash,
            uint parentTitleHash,
            Dictionary<long, ClanDbModel> clansData,
            IBungieClient bungieClient,
            Dictionary<long, string> usernames)
        {
            if (bungieClient.TryGetDefinition<DestinyRecordDefinition>(
                    parentTitleHash,
                    BungieLocales.EN,
                    out var recordDefinition))
            {
                if (recordDefinition.DisplayProperties.Icon.HasValue)
                {
                    embedBuilder.WithThumbnailUrl(recordDefinition.DisplayProperties.Icon.AbsolutePath);
                }
                else
                {
                    var recordTitleNode = bungieClient
                        .Repository
                        .GetAll<DestinyPresentationNodeDefinition>()
                        .FirstOrDefault(x => x.CompletionRecord.Hash == parentTitleHash);
                    if (recordTitleNode is not null)
                        embedBuilder.WithThumbnailUrl(recordTitleNode.DisplayProperties.Icon.AbsolutePath);
                }

                embedBuilder.WithDescription(
                    $"{usernames.Count} people have gilded title: **{recordDefinition.TitleInfo.TitlesByGenderHash[DefinitionHashes.Genders.Masculine]}**");

                var stringBuilder = new StringBuilder();
                foreach (var (clanId, clanData) in clansData)
                {
                    stringBuilder.Clear();

                    foreach (var broadcast in destinyUserBroadcasts)
                    {
                        if (broadcast.ClanId != clanId)
                            continue;

                        if (usernames.TryGetValue(broadcast.MembershipId, out var username) &&
                            broadcast.AdditionalData.TryGetValue("gildedCount", out var gildedCountUnparsed) &&
                            int.TryParse(gildedCountUnparsed, out var gildedCount))
                            stringBuilder.AppendLine($"{username} - {gildedCount} times");
                    }

                    embedBuilder.AddField($"Clan: {clanData.ClanName}", stringBuilder.ToString());
                }
            }
        }
    }
}