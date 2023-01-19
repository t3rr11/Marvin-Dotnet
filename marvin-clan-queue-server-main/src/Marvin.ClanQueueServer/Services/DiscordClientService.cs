using Discord.WebSocket;
using Marvin.ClanQueueServer.Options;
using Microsoft.Extensions.Options;

namespace Marvin.ClanQueueServer.Services;

public class DiscordClientService
{
    private readonly IOptions<DiscordOptions> _discordOptions;
    private readonly DiscordShardedClient _discordShardedClient;

    public bool IsReady { get; private set; }

    public DiscordClientService(
        IOptions<DiscordOptions> discordOptions,
        DiscordShardedClient discordShardedClient)
    {
        _discordOptions = discordOptions;
        _discordShardedClient = discordShardedClient;
    }

    public void SetReady()
    {
        IsReady = true;
    }
    
    public SocketTextChannel? GetAlertChannel()
    {
        if (!IsReady)
            return null;
        
        var options = _discordOptions.Value;

        var guild = _discordShardedClient.GetGuild(options.AlertServerId);

        return guild?.GetTextChannel(options.AlertChannelId);
    }
}