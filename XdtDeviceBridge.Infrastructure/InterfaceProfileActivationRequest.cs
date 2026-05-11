using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationRequest(
    InterfaceProfileDefinition? Profile,
    InterfaceProfileActivationEvaluationResult? EvaluationResult,
    bool WarningsAccepted = false,
    string Context = "PreviewOnly");
