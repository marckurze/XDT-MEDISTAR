using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileManualProcessingResult(
    bool Success,
    string? ExportFilePath,
    string? ExportContent,
    ProcessingPipelineResult? PipelineResult,
    ProcessedFileArchiveResult? ArchiveResult,
    IReadOnlyList<string> Messages);
