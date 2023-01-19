using System.Diagnostics;

namespace Marvin.ClanScannerServer.Services.Scanning;

#if DEBUG
[DebuggerDisplay("{StepName}")]
#endif
public class ScannerStepInfo<TInput, TContext>
{
    public Func<TInput, TContext, ScanContext, CancellationToken, ValueTask<bool>> Delegate { get; init; }
    public string StepName { get; init; }
    public bool ExecuteAfterErrors { get; init; }
}