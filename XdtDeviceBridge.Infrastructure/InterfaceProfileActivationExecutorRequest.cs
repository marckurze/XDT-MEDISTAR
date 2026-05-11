using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationExecutorRequest(
    InterfaceProfileDefinition? Profile,
    InterfaceProfileActivationPlan? ActivationPlan,
    InterfaceProfileActivationEvaluationResult? EvaluationResult,
    InterfaceProfileActivationGuardResult? GuardResult,
    InterfaceProfileActivationWarningConfirmationResult? WarningConfirmationResult,
    bool WarningsAccepted,
    string Context = "FutureActivation",
    string RequestedBy = "",
    DateTimeOffset? RequestedAt = null,
    string? Comment = null);
