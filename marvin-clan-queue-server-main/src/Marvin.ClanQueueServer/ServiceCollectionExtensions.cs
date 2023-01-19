using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Marvin.ClanQueueServer.Options;
using Marvin.ClanQueueServer.Services.Hosted;
using Microsoft.Extensions.Options;

namespace Marvin.ClanQueueServer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHostedServiceWithInterface<THostedServiceInterface, THostedService>(
        this IServiceCollection serviceCollection)
        where THostedService : class, THostedServiceInterface, IHostedService
        where THostedServiceInterface : class
    {
        serviceCollection.AddSingleton<THostedServiceInterface, THostedService>();
        
        serviceCollection.AddSingleton<IHostedService>(x => (THostedService) x.GetRequiredService<THostedServiceInterface>());

        return serviceCollection;
    }
    
    public static IServiceCollection AddDiscord(
        this IServiceCollection serviceCollection,
        Action<DiscordSocketConfig> configureClient,
        Action<InteractionServiceConfig> configureInteractionService,
        Action<CommandServiceConfig> configureTextCommands,
        IConfiguration configuration)
    {
        var discordSocketConfig = new DiscordSocketConfig();
        configureClient(discordSocketConfig);
        var discordClient = new DiscordShardedClient(discordSocketConfig);

        var interactionServiceConfig = new InteractionServiceConfig();
        configureInteractionService(interactionServiceConfig);
        var interactionService = new InteractionService(discordClient, interactionServiceConfig);

        var commandServiceConfig = new CommandServiceConfig();
        configureTextCommands(commandServiceConfig);
        var textCommandService = new CommandService(commandServiceConfig);

        return serviceCollection
            .Configure<DiscordOptions>(configuration.GetSection("DiscordOptions"))
            .AddHostedService<DiscordStartupService>()
            .AddSingleton(discordClient)
            .AddSingleton(interactionService)
            .AddSingleton(textCommandService);
    }
}