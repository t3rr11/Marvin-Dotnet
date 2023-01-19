namespace Marvin.Pipelines;

public class PipelineAction : IPipeLineAction
{
    private readonly Func<PipelineBag, bool> _action;

    internal PipelineAction(
        Func<PipelineBag, bool> action,
        string actionStepName)
    {
        _action = action;
        ActionStepName = actionStepName;
    }

    public string ActionStepName { get; }

    public Task<bool> Run(PipelineBag bag, CancellationToken cancellationToken)
    {
        var result = _action(bag);
        return Task.FromResult(result);
    }
}