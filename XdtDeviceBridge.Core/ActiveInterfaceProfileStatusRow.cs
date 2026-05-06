namespace XdtDeviceBridge.Core;

public sealed record ActiveInterfaceProfileStatusRow(
    string Name,
    string AisName,
    string DeviceName,
    string ExportProfileName,
    string AisImportFolder,
    string DeviceImportFolder,
    string ExportFolder,
    string LicenseStatus,
    string FolderStatus,
    string ProcessingStatus);
