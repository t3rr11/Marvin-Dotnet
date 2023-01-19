namespace Marvin.Pipelines;

public class PipeLineExecutionException : Exception
{
    public PipeLineExecutionException(IPipeLineAction action, Exception exception) :
        base($"Failed to execute action {action.ActionStepName}", exception)
    {
        Action = action;
    }

    public IPipeLineAction Action { get; }
}