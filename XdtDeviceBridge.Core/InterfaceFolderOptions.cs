namespace XdtDeviceBridge.Core;

public sealed record InterfaceFolderOptions(
    string AisImportFolder,
    string DeviceImportFolder,
    string ExportFolder,
    string ArchiveFolder,
    string ErrorFolder,
    bool ClearAisImportFolderBeforeProcessing,
    bool ClearDeviceImportFolderBeforeProcessing,
    bool ClearExportFolderAfterSuccessfulTransfer,
    bool ArchiveProcessedFiles,
    bool MoveFailedFilesToErrorFolder,
    ArchiveProcessedFileMode ArchiveProcessedFileMode = ArchiveProcessedFileMode.Copy,
    int? ArchiveRetentionDays = null,
    string AttachmentImportFolder = "",
    string AttachmentExportFolder = "");
