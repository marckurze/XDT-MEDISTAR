namespace XdtDeviceBridge.Core;

public sealed record InterfaceMonitoringCardDisplay(
    string InterfaceProfileId,
    string InterfaceProfileName,
    string AisName,
    string DeviceName,
    string ExportProfileName,
    string CurrentStatus,
    string StatusClass,
    string ScanIntervalText,
    string LastScanText,
    string AutomaticProcessingText,
    IReadOnlyList<ExpectedInputDisplayItem> ExpectedInputs,
    IReadOnlyList<InterfaceMonitoringDetailItem> FolderDetails,
    string AttachmentImportFolder,
    string AttachmentExportFolder,
    string AttachmentConfigurationStatus);
