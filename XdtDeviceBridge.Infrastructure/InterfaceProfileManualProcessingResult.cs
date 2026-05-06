using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileManualProcessingResult(
    bool Success,
    string? ExportFilePath,
    string? ExportContent,
    ProcessingPipelineResult? PipelineResult,
    IReadOnlyList<string> Messages);
