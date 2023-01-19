namespace Marvin.Bot.Services.Interfaces;

public interface ISystemsStatusService
{
    bool DiscordBotIsReady { get; }
    bool DefinitionsLoaded { get; }
}