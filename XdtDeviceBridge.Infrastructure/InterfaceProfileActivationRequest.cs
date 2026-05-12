using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationRequest(
    InterfaceProfileDefinition? Profile,
    InterfaceProfileActivationEvaluationResult? EvaluationResult,
    string Context = "PreviewOnly");
