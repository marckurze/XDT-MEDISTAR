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
    bool ProcessingStarted,
    bool WasExecuted = false,
    bool WasPersisted = false,
    bool WasProfileChanged = false,
    bool RequiresFreshLoad = true,
    bool RequiresSafeUserDefinedStore = true,
    bool RequiresFinalReEvaluation = true,
    bool IsValidationOnly = true,
    IReadOnlyList<string>? MissingCapabilities = null);
