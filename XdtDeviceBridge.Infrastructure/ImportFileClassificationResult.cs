namespace XdtDeviceBridge.Infrastructure;

public sealed record ImportFileClassificationResult(
    string FilePath,
    string FileName,
    string Extension,
    ImportFileKind Kind,
    string Message);
