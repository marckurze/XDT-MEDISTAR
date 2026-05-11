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
    string? Comment = null,
    string InterfaceProfileId = "",
    string InterfaceProfileName = "",
    InterfaceProfileActivationExecutorOperationMode OperationMode =
        InterfaceProfileActivationExecutorOperationMode.ValidateOnly,
    string Source = "ActivationPreview",
    DateTimeOffset? RequestedAtUtc = null,
    DateTimeOffset? PreviewCreatedAtUtc = null,
    string? ExpectedConfigurationFingerprint = null,
    InterfaceProfileActivationStatus? ExpectedEvaluationStatus = null,
    InterfaceProfileActivationPlanStatus? ExpectedActivationPlanStatus = null,
    string? WarningConfirmationToken = null,
    IReadOnlyList<string>? ConfirmedWarningCodes = null)
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
