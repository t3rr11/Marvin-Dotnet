using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Marvin.ClanQueueServer.Options;
using Microsoft.Extensions.Options;

namespace Marvin.ClanQueueServer.Services.Hosted;

public class DiscordStartupService : BackgroundService
{
    private readonly DiscordShardedClient _discordShardedClient;
    private readonly IOptions<DiscordOptions> _discordBotOptions;
    private readonly InteractionService _interactionService;
    private readonly CommandService _commandService;
    private readonly IServiceProvider _serviceProvider;
    private readonly DiscordClientService _discordClientService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private TaskCompletionSource<object> _taskCompletionSource;
    private int _shardsReady;

    public DiscordStartupService(
        DiscordShardedClient discordShardedClient,
        IOptions<DiscordOptions> discordBotOptions,
        InteractionService interactionService,
        CommandService commandService,
        IServiceProvider serviceProvider,
        DiscordClientService discordClientService,
        IWebHostEnvironment webHostEnvironment)
    {
        _discordShardedClient = discordShardedClient;
        _discordBotOptions = discordBotOptions;
        _interactionService = interactionService;
        _commandService = commandService;
        _serviceProvider = serviceProvider;
        _discordClientService = discordClientService;
        _webHostEnvironment = webHostEnvironment;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //_discordShardedClient.InteractionCreated += OnDiscordInteractionCreated;
        _discordShardedClient.SlashCommandExecuted += OnDiscordSlashCommandExecuted;
        _discordShardedClient.MessageReceived += OnDiscordMessageReceived;
        PrepareClientAwaiter();
        await _discordShardedClient.LoginAsync(TokenType.Bot, _discordBotOptions.Value.BotToken);
        await _discordShardedClient.StartAsync();
        await WaitForReadyAsync(stoppingToken);

        _discordClientService.SetReady();
        
        var alertChannel = _discordClientService.GetAlertChannel();

        if (alertChannel is not null)
        {

            var embed = new EmbedBuilder()
                .WithTitle("Bot is online!")
                .AddField("Environment",
                    $"App name: {_webHostEnvironment.ApplicationName}\nEnv name: {_webHostEnvironment.EnvironmentName}")
                .Build();

            await alertChannel.SendMessageAsync(embed: embed);
        }

        // load text commands
        //await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        // load interactions
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

        // register your commands here
        await _interactionService.RegisterCommandsToGuildAsync(_discordBotOptions.Value.AlertServerId);
    }

    private async Task OnDiscordSlashCommandExecuted(SocketSlashCommand socketSlashCommand)
    {
        var shardedInteractionContext = new ShardedInteractionContext<SocketSlashCommand>(_discordShardedClient, socketSlashCommand);
        var result = await _interactionService.ExecuteCommandAsync(shardedInteractionContext, _serviceProvider);
    }
    
    // private async Task OnDiscordInteractionCreated(SocketInteraction socketInteraction)
    // {
    //     var shardedInteractionContext = new ShardedInteractionContext<SocketSlashCommand>(_discordShardedClient, socketInteraction);
    //     var result = await _interactionService.ExecuteCommandAsync(shardedInteractionContext, _serviceProvider);
    // }

    private async Task OnDiscordMessageReceived(SocketMessage socketMessage)
    {
        if (socketMessage is not SocketUserMessage socketUserMessage)
            return;

        var argPos = 0;
        if (socketUserMessage.HasCharPrefix('!', ref argPos))
            return;
        if (socketUserMessage.Author.IsBot)
            return;

        var context = new ShardedCommandContext(_discordShardedClient, socketUserMessage);
        await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
    }

    private void PrepareClientAwaiter()
    {
        _taskCompletionSource = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        _shardsReady = 0;

        _discordShardedClient.ShardReady += OnShardReady;
    }

    private Task OnShardReady(DiscordSocketClient _)
    {
        _shardsReady++;
        if (_shardsReady == _discordShardedClient.Shards.Count)
        {
            _taskCompletionSource!.TrySetResult(null!);
            _discordShardedClient.ShardReady -= OnShardReady;
        }

        return Task.CompletedTask;
    }

    private Task WaitForReadyAsync(CancellationToken cancellationToken)
    {
        if (_taskCompletionSource is null)
            throw new InvalidOperationException(
                "The sharded client has not been registered correctly. Did you use ConfigureDiscordShardedHost on your HostBuilder?");

        if (_taskCompletionSource.Task.IsCompleted)
            return _taskCompletionSource.Task;

        var registration = cancellationToken.Register(
            state => { ((TaskCompletionSource<object>)state!).TrySetResult(null!); },
            _taskCompletionSource);

        return _taskCompletionSource.Task.ContinueWith(_ => registration.DisposeAsync(), cancellationToken);
    }
}