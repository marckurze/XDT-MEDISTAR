namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationExecutorResult(
    InterfaceProfileActivationExecutorStatus Status,
    bool Success,
    string Message,
    IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> Preconditions,
    IReadOnlyList<InterfaceProfileActivationPlanStep> ExecutedSteps,
    IReadOnlyList<InterfaceProfileActivationPlanStep> NotExecutedSteps,
    bool ProfileChanged,
    bool Saved,
    bool ProcessingStarted);
