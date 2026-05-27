namespace XdtDeviceBridge.Infrastructure;

public sealed record XdtBoxBackupManifest(
    string BackupFormatVersion,
    string ProductCode,
    string AppVersion,
    DateTime CreatedAtUtc,
    string SourceInstallationId,
    IReadOnlyList<string> IncludedAreas,
    bool IncludesLicenseFile,
    string HardwareMigrationNotice);

public sealed record XdtBoxBackupResult(
    bool Success,
    string? BackupFilePath,
    XdtBoxBackupManifest? Manifest,
    IReadOnlyList<string> Messages);

public sealed record XdtBoxRestorePreview(
    bool Success,
    XdtBoxBackupManifest? Manifest,
    IReadOnlyList<string> Messages);

public sealed record XdtBoxRestoreResult(
    bool Success,
    XdtBoxBackupManifest? Manifest,
    IReadOnlyList<string> Messages);
