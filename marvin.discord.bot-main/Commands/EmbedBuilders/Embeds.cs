using Discord;

namespace Marvin.Bot.Commands.EmbedBuilders;

public static partial class Embeds
{
    public static EmbedBuilder GetPrefilledEmbed()
    {
        return new EmbedBuilder()
            .WithCurrentTimestamp()
            .WithFooter(FooterDomain, FooterURL);
    }
    
    public static Embed GetBasicEmbed(string title, string description)
    {
        return new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithCurrentTimestamp()
            .WithFooter(FooterDomain, FooterURL)
            .Build();
    }

    private const string FooterDomain = "Marvin.gg";

    private const string FooterURL = "https://guardianstats.com/images/icons/logo.png";
}