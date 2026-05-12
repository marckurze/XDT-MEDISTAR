namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationExecutorResult(
    InterfaceProfileActivationExecutorStatus Status,
    bool Success,
    string Message,
    IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> Preconditions,
    bool ProfileChanged,
    bool Saved,
    bool ProcessingStarted,
    bool RequiresFreshLoad = true,
    bool RequiresSafeUserDefinedStore = true,
    bool RequiresFinalEvaluation = true,
    bool IsValidationOnly = true,
    IReadOnlyList<string>? MissingCapabilities = null,
    bool FreshLoadPerformed = false,
    bool FinalEvaluationPerformed = false,
    bool GuardRechecked = false,
    bool SaveDryRunPerformed = false,
    bool SaveDryRunBlocked = false);
