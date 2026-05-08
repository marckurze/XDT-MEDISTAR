namespace XdtDeviceBridge.Infrastructure;

public sealed record AttachmentExternalLinkDiagnosticResult(
    bool Success,
    string Message,
    AttachmentExternalLinkPreparationResult? PreparationResult);
