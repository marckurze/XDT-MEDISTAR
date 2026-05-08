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
    string AttachmentExportFolder = "",
    string? AttachmentFileNameTemplate = AttachmentFileNameBuilder.DefaultTemplate,
    AttachmentTransferMode AttachmentTransferMode = AttachmentTransferMode.Move,
    string AttachmentExternalLinkDocumentName = "Datei",
    string AttachmentExternalLinkFileFormat = "{ExtensionUpperWithoutDot}",
    string AttachmentExternalLinkDescription = "",
    string AttachmentExternalLinkPathTemplate = "{Attachment.TargetFullPath}",
    bool IsAttachmentProcessingEnabled = false);
