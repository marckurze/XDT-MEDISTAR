using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record AttachmentExternalLinkPreparationResult(
    bool Success,
    string SourcePath,
    string? TargetPath,
    string? TargetFileName,
    AttachmentTransferMode TransferMode,
    ExternalAisLinkFieldSet? ExternalAisLinkFieldSet,
    IReadOnlyList<ExportFieldRecord> ExportFields,
    string? ErrorMessage);
