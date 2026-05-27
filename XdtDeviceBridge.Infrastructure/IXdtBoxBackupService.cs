namespace XdtDeviceBridge.Infrastructure;

public interface IXdtBoxBackupService
{
    XdtBoxBackupResult CreateBackup(
        AppDataPaths paths,
        string backupFilePath,
        string appVersion,
        string sourceInstallationId,
        bool includeLicenseFile = true);

    XdtBoxRestorePreview PreviewRestore(string backupFilePath);

    XdtBoxRestoreResult RestoreBackup(
        AppDataPaths paths,
        string backupFilePath,
        bool isMonitoringRunning);
}
