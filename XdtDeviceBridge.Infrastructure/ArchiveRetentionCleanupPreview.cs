namespace XdtDeviceBridge.Infrastructure;

public sealed record ArchiveRetentionCleanupPreview(
    string ArchiveFolder,
    int RetentionDays,
    DateTime DeleteFilesOlderThanUtc,
    IReadOnlyList<string> FilesEligibleForDeletion,
    IReadOnlyList<string> Issues,
    bool HasErrors);
