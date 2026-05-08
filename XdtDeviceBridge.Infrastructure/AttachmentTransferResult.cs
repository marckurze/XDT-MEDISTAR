using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record AttachmentTransferResult(
    bool Success,
    string SourcePath,
    string? TargetPath,
    string? FileName,
    AttachmentTransferMode TransferMode,
    string? ErrorMessage,
    bool WasCopied,
    bool WasMoved);
