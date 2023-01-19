using DotNetBungieAPI.Service.Abstractions;

namespace Marvin.ClanScannerServer.Services.Hosted;

public class DefinitionsLoaderService : IHostedService
{
    private readonly IBungieClient _bungieClient;
    private readonly ILogger<DefinitionsLoaderService> _logger;

    public DefinitionsLoaderService(
        IBungieClient bungieClient,
        ILogger<DefinitionsLoaderService> logger)
    {
        _bungieClient = bungieClient;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _bungieClient.DefinitionProvider.Initialize();
            await _bungieClient.DefinitionProvider.ReadToRepository(_bungieClient.Repository);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to initialize definitions");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}