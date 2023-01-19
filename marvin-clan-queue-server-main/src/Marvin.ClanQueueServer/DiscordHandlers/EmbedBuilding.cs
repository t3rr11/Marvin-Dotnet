using Discord;

namespace Marvin.ClanQueueServer.DiscordHandlers;

public static class EmbedBuilding
{
    public static Embed CreateSimpleEmbed(string title, string description)
    {
        return new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .Build();
    }
}