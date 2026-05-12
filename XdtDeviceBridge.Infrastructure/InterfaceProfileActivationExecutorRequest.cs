using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationExecutorRequest(
    InterfaceProfileDefinition? Profile,
    InterfaceProfileActivationEvaluationResult? EvaluationResult,
    InterfaceProfileActivationGuardResult? GuardResult,
    string Context = "FutureActivation",
    string InterfaceProfileId = "",
    string InterfaceProfileName = "",
    InterfaceProfileActivationExecutorOperationMode OperationMode =
        InterfaceProfileActivationExecutorOperationMode.ValidateOnly,
    string Source = "ActivationPreview",
    DateTimeOffset? RequestedAtUtc = null)
{
    public string EffectiveInterfaceProfileId =>
        string.IsNullOrWhiteSpace(InterfaceProfileId)
            ? Profile?.Metadata.Id ?? string.Empty
            : InterfaceProfileId;

    public string EffectiveInterfaceProfileName =>
        string.IsNullOrWhiteSpace(InterfaceProfileName)
            ? Profile?.Metadata.Name ?? string.Empty
            : InterfaceProfileName;
}
