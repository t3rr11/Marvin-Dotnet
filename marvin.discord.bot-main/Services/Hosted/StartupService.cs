using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DotNetBungieAPI.Service.Abstractions;
using Marvin.Bot.Models;
using Marvin.Bot.Options;
using Marvin.Bot.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Marvin.Bot.Services.Hosted
{
    public class StartupService : BackgroundService, ISystemsStatusService
    {
        private readonly DiscordShardedClient _client;
        private readonly IBungieClient _bungieClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly InteractionService _interactionService;
        private readonly IOptions<DiscordOptions> _discordBotOptions;
        private readonly ILogger<StartupService> _logger;

        private TaskCompletionSource<bool> _taskCompletionSource;
        private int _shardsReady;
        
        private bool _discordBotIsReady;
        private bool _definitionsLoaded;

        public StartupService(
            DiscordShardedClient client,
            IBungieClient bungieClient,
            InteractionService interactionService,
            IOptions<DiscordOptions> discordBotOptions,
            IServiceProvider serviceProvider,
            ILogger<StartupService> logger
        )
        {
            _client = client;
            _bungieClient = bungieClient;
            _interactionService = interactionService;
            _discordBotOptions = discordBotOptions;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await LoadDestinyDefinitions();
            _definitionsLoaded = true;
            
            await LoadDiscordBot();
            _discordBotIsReady = true;
        }

        private async Task LoadDestinyDefinitions()
        {
            // Wrap this in a try catch, to avoid crashes by catching exceptions.
            try
            {
                // Load up the definitions from PostgresDB
                await _bungieClient.DefinitionProvider.Initialize();
                await _bungieClient.DefinitionProvider.ReadToRepository(_bungieClient.Repository);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to initialize definitions");
                throw;
            }
        }

        private void PrepareClientAwaiter()
        {
            _taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _shardsReady = 0;
            _client.ShardReady += OnShardReady;
        }

        private async Task LoadDiscordBot()
        {
            // Wrap this in a try catch, to avoid crashes by catching exceptions.
            try
            {
                PrepareClientAwaiter();

                // This is basically a way to hook these functions onto these discord client events. (Kinda neat)
                _client.Log += Log;
                _client.SlashCommandExecuted += SlashCommandExecuted;
                _client.ButtonExecuted += ButtonExecuted;
                _client.AutocompleteExecuted += AutocompleteExecuted;
                _client.SelectMenuExecuted += SelectMenuExecuted;

                // Login and start the bot, this will trigger the ClientReady function as defined above.
                await _client.LoginAsync(TokenType.Bot, _discordBotOptions.Value.BotToken);
                await _client.StartAsync();
                await WaitForReadyAsync(default);
                await RegisterInteractions();
            }
            catch (Exception ex)
            {
                // Catch exceptions duh...
                _logger.LogError(ex, "Discord Service Failure");
                throw;
            }
        }

        private Task OnShardReady(DiscordSocketClient _)
        {
            _shardsReady++;
            if (_shardsReady == _client.Shards.Count)
            {
                _taskCompletionSource!.TrySetResult(true);
                _client.ShardReady -= OnShardReady;
            }

            return Task.CompletedTask;
        }
        
        private Task WaitForReadyAsync(CancellationToken cancellationToken)
        {
            if (_taskCompletionSource is null)
                throw new InvalidOperationException("The sharded client has not been registered correctly.");

            if (_taskCompletionSource.Task.IsCompleted)
                return _taskCompletionSource.Task;

            var registration = cancellationToken.Register(
                state => { ((TaskCompletionSource<bool>)state!).TrySetResult(false!); },
                _taskCompletionSource);

            return _taskCompletionSource.Task.ContinueWith(_ => registration.DisposeAsync(), cancellationToken);
        }

        private async Task SlashCommandExecuted(SocketSlashCommand socketSlashCommand)
        {
            // This will take the command that was excecuted and go return the context of the command (In lamens terms, it finds the command and responds)
            var socketInteractionContext = new ShardedInteractionContext<SocketSlashCommand>(
                _client,
                socketSlashCommand);
            await _interactionService.ExecuteCommandAsync(socketInteractionContext, _serviceProvider);
        }

        private async Task ButtonExecuted(SocketMessageComponent buttonExecutedComponent)
        {
            // This will take the button event that was excecuted and find the trigger for it.
            var socketInteractionContext = new ShardedInteractionContext<SocketMessageComponent>(
                _client,
                buttonExecutedComponent);
            await _interactionService.ExecuteCommandAsync(socketInteractionContext, _serviceProvider);
        }

        private async Task AutocompleteExecuted(SocketAutocompleteInteraction socketAutocompleteInteraction)
        {
            // This will take the autocomplete event that was excecuted and find the trigger for it.
            var socketInteractionContext = new ShardedInteractionContext<SocketAutocompleteInteraction>(
                _client,
                socketAutocompleteInteraction);
            await _interactionService.ExecuteCommandAsync(socketInteractionContext, _serviceProvider);
        }

        private async Task SelectMenuExecuted(SocketMessageComponent selectMenuComponent)
        {
            // This will take the select menu event that was excecuted and find the trigger for it.
            var socketInteractionContext = new ShardedInteractionContext<SocketMessageComponent>(
                _client,
                selectMenuComponent);
            await _interactionService.ExecuteCommandAsync(socketInteractionContext, _serviceProvider);
        }

        /// <summary>
        ///     // The discord client is ready to be actioned on. So how we do things like register interactions and such...
        /// </summary>
        private async Task ClientReady()
        {
            await RegisterInteractions();
        }

        private async Task RegisterInteractions()
        {
            try
            {
                // This does some magic and finds all references of [SlashCommand("name", "description")] in the project and links them to the interaction service.
                await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

                // This registers all the above found SlashCommands to this specific guild, for testing.
                await _interactionService.RegisterCommandsToGuildAsync(886500502060302357, false);
            }
            catch (Exception ex)
            {
                // Catch exceptions duh...
                _logger.LogError(ex, "Failed to register discord interactions");
            }
        }

        private Task Log(LogMessage msg)
        {
            // Log events fired from discord, I actually wish Discord.JS had something like this, that would have been nice.
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical("Log from Discord.NET client: {MessageText}", msg.Message);
                    break;
                case LogSeverity.Error:
                    _logger.LogError(msg.Exception, "Log from Discord.NET client: {MessageText}", msg.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning("Log from Discord.NET client: {MessageText}", msg.Message);
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation("Log from Discord.NET client: {MessageText}", msg.Message);
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug("Log from Discord.NET client: {MessageText}", msg.Message);
                    break;
            }

            return Task.CompletedTask;
        }

        public bool DiscordBotIsReady => _discordBotIsReady;

        public bool DefinitionsLoaded => _definitionsLoaded;
    }
}