using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationPlanRequest(
    InterfaceProfileDefinition? Profile,
    InterfaceProfileActivationEvaluationResult? EvaluationResult,
    InterfaceProfileActivationGuardResult? GuardResult,
    InterfaceProfileActivationWarningConfirmationResult? WarningConfirmationResult,
    bool WarningsAccepted = false,
    string Context = "PreviewOnly");
