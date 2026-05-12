using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationProfileSaveRequest(
    InterfaceProfileDefinition? Profile,
    string Source = "ActivationExecutor",
    DateTimeOffset? RequestedAtUtc = null,
    string? ExpectedConfigurationFingerprint = null,
    bool AllowOverwriteExistingUserDefined = false);
