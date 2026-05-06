namespace XdtDeviceBridge.Infrastructure;

public sealed record FileStabilityResult(
    string FilePath,
    bool Exists,
    bool IsReadable,
    bool IsStable,
    long? FileSizeBytes,
    string Message);
