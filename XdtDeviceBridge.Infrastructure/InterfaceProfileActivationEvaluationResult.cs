namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationEvaluationResult(
    InterfaceProfileActivationStatus ActivationStatus,
    bool CanActivate,
    IReadOnlyList<InterfaceProfileActivationCheckResult> Checks)
{
    public IReadOnlyList<InterfaceProfileActivationCheckResult> Blockers =>
        Checks
            .Where(check => check.Severity == InterfaceProfileActivationSeverity.Blocker)
            .ToList();

    public IReadOnlyList<InterfaceProfileActivationCheckResult> Warnings =>
        Checks
            .Where(check => check.Severity == InterfaceProfileActivationSeverity.Warning)
            .ToList();

    public IReadOnlyList<InterfaceProfileActivationCheckResult> Infos =>
        Checks
            .Where(check => check.Severity == InterfaceProfileActivationSeverity.Info)
            .ToList();
}
