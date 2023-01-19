using Microsoft.AspNetCore.SignalR;

namespace Marvin.ClanQueueServer.Hubs.Filters;

public class HubErrorFilter : IHubFilter
{
    private readonly ILogger<HubErrorFilter> _logger;

    public HubErrorFilter(ILogger<HubErrorFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            return await next(invocationContext);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Encountered error while executing hub method {HubMethodName}",
                invocationContext.HubMethodName);
            throw;
        }
    }
}