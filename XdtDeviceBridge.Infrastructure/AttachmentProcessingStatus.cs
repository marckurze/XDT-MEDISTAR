using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record AttachmentProcessingStatus(
    bool WasAttempted,
    bool WasSkipped,
    bool Success,
    AttachmentProcessingStatusReason Reason,
    string Message,
    string? SourcePath,
    string? TargetPath,
    string? TargetFileName,
    IReadOnlyList<ExportFieldRecord> PreparedFields);
