namespace Marvin.Pipelines;

public class PipelineActionMetrics
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    public ulong TimesExecuted { get; set; }
    public double AverageExecutionTime { get; set; }

    public async ValueTask IncrementValues(
        long executionMs, 
        CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        if (TimesExecuted > 1000000)
        {
            TimesExecuted = 1;
        }

        var totalTimeSpent = AverageExecutionTime * TimesExecuted;
        TimesExecuted++;
        AverageExecutionTime = (totalTimeSpent + executionMs) / TimesExecuted;
        
        _semaphore.Release();
    }
}