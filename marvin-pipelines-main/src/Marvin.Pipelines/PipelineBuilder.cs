namespace Marvin.Pipelines;

public class PipelineBuilder
{
    private readonly List<IPipeLineAction> _actions;
    private readonly List<IPipeLineAction> _finalizeActions;

    public PipelineBuilder()
    {
        _actions = new List<IPipeLineAction>();
        _finalizeActions = new List<IPipeLineAction>();
    }

    public PipelineBuilder AddAction(
        Func<PipelineBag, bool> action,
        string actionStepName)
    {
        _actions.Add(new PipelineAction(action, actionStepName));
        return this;
    }

    public PipelineBuilder AddAsyncAction(
        Func<PipelineBag, CancellationToken, Task<bool>> action,
        string actionStepName)
    {
        _actions.Add(new AsyncPipeLineAction(action, actionStepName));
        return this;
    }

    public PipelineBuilder AddFinalizeAction(
        Func<PipelineBag, bool> action,
        string actionStepName)
    {
        _finalizeActions.Add(new PipelineAction(action, actionStepName));
        return this;
    }

    public PipelineBuilder AddFinalizeAsyncAction(
        Func<PipelineBag, CancellationToken, Task<bool>> action,
        string actionStepName)
    {
        _finalizeActions.Add(new AsyncPipeLineAction(action, actionStepName));
        return this;
    }

    public Pipeline Build(
        string pipelineName,
        Action<PipelineBag> configureBag)
    {
        var bag = new PipelineBag();
        configureBag(bag);
        return new Pipeline(bag, _actions, _finalizeActions)
        {
            Name = pipelineName
        };
    }
}