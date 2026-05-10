namespace XdtDeviceBridge.Core;

public sealed record InterfaceMonitoringCardDisplay(
    string InterfaceProfileId,
    string InterfaceProfileName,
    IReadOnlyList<ExpectedInputDisplayItem> ExpectedInputs,
    string AttachmentImportFolder,
    string AttachmentExportFolder,
    string AttachmentConfigurationStatus);
