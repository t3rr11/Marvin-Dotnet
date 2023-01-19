namespace Marvin.Pipelines;

public interface IPipeLineAction
{
    string ActionStepName { get; }
    Task<bool> Run(PipelineBag bag, CancellationToken cancellationToken);
}