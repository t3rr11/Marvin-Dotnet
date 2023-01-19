namespace Marvin.Pipelines;

public class AsyncPipeLineAction : IPipeLineAction
{
    private readonly Func<PipelineBag, CancellationToken, Task<bool>> _action;

    internal AsyncPipeLineAction(
        Func<PipelineBag, CancellationToken, Task<bool>> action,
        string actionStepName)
    {
        _action = action;
        ActionStepName = actionStepName;
    }

    public string ActionStepName { get; }

    public async Task<bool> Run(PipelineBag bag, CancellationToken cancellationToken)
    {
        return await _action(bag, cancellationToken);
    }
}