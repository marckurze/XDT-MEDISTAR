namespace XdtDeviceBridge.Infrastructure;

public sealed record DirectoryFileSnapshot(
    string FilePath,
    string FileName,
    long FileSizeBytes,
    DateTime LastWriteTimeUtc,
    string Extension);
