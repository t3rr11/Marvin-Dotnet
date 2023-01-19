using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Marvin.Pipelines;

public class Pipeline
{
    public ConcurrentDictionary<string, PipelineActionMetrics> ActionMetrics { get; }

    private readonly PipelineBag _sharedBag;

    private readonly IPipeLineAction[] _actions;

    private readonly IPipeLineAction[] _finalizeActions;

    internal Pipeline(
        PipelineBag bag,
        IEnumerable<IPipeLineAction> actions,
        IEnumerable<IPipeLineAction> finalizeActions)
    {
        _sharedBag = bag;
        _actions = actions.ToArray();
        _finalizeActions = finalizeActions.ToArray();
        ActionMetrics = new ConcurrentDictionary<string, PipelineActionMetrics>();
        foreach (var action in _actions)
        {
            ActionMetrics.TryAdd(action.ActionStepName, new PipelineActionMetrics());
        }

        foreach (var action in _finalizeActions)
        {
            ActionMetrics.TryAdd(action.ActionStepName, new PipelineActionMetrics());
        }
    }

    public string Name { get; set; }

    public async Task RunPipeLine(
        Action<PipelineBag> configureAction,
        ILogger logger,
        object context,
        CancellationToken cancellationToken)
    {
        var bag = _sharedBag.Copy();
        configureAction(bag);
        var globalActionTimer = Stopwatch.StartNew();
        var localActionTimer = Stopwatch.StartNew();
        for (var i = 0; i < _actions.Length; i++)
        {
            var action = _actions[i];
            try
            {
                localActionTimer.Restart();
                var result = await action.Run(bag, cancellationToken);
                var actionMetrics = ActionMetrics[action.ActionStepName];
                await actionMetrics.IncrementValues(
                    localActionTimer.ElapsedMilliseconds, 
                    cancellationToken);
                if (result == false)
                    break;
            }
            catch (Exception exception)
            {
                var ex = new PipeLineExecutionException(action, exception);
                logger.LogError(
                    ex,
                    "Failed to execute action [{Name}].[{Action}], Context = {@Context}",
                    Name,
                    action.ActionStepName,
                    context);
            }
        }

        for (var i = 0; i < _finalizeActions.Length; i++)
        {
            var action = _finalizeActions[i];
            try
            {
                localActionTimer.Restart();
                await action.Run(bag, cancellationToken);
                var actionMetrics = ActionMetrics[action.ActionStepName];
                await actionMetrics.IncrementValues(
                    localActionTimer.ElapsedMilliseconds, 
                    cancellationToken);
            }
            catch (BagTypeMissingException bagTypeMissingException)
            {
                logger.LogDebug(bagTypeMissingException.Message);
            }
            catch (Exception exception)
            {
                var ex = new PipeLineExecutionException(action, exception);
                logger.LogError(ex, "Failed to execute finalize action [{Name}].[{Action}], Context = {@Context}",
                    Name,
                    action.ActionStepName,
                    context);
            }
        }
    }
}