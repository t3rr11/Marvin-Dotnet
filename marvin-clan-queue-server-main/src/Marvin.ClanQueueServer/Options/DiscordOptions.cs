namespace Marvin.ClanQueueServer.Options;

public class DiscordOptions
{
    public string BotToken { get; set; }
    public ulong AlertServerId { get; set; }
    public ulong AlertChannelId { get; set; }
}