using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record BuilderTestExportResult(
    bool Success,
    string? ExportFilePath,
    string? AttachmentTargetPath,
    string? AttachmentTargetFileName,
    string ExportContent,
    IReadOnlyList<ExportFieldRecord> ExportRecords,
    IReadOnlyList<string> Issues);
