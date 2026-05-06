namespace XdtDeviceBridge.Infrastructure;

public sealed record DirectorySnapshotResult(
    string DirectoryPath,
    bool Exists,
    IReadOnlyList<DirectoryFileSnapshot> Files,
    string? Message);
