using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationWarningConfirmationRequest(
    InterfaceProfileDefinition? Profile,
    InterfaceProfileActivationEvaluationResult? EvaluationResult,
    InterfaceProfileActivationGuardResult? GuardResult = null,
    DateTimeOffset? EvaluatedAt = null,
    string Source = "PreviewOnly");
