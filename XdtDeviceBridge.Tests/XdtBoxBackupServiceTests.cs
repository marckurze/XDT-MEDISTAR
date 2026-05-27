using System.IO.Compression;
using System.Text.Json;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class XdtBoxBackupServiceTests
{
    private readonly XdtBoxBackupService _service = new();
    private readonly AppDataPathProvider _pathProvider = new();

    [Fact]
    public void CreateBackup_ShouldWriteManifestAndConfigurationFiles()
    {
        using var temp = new TempFolder();
        var paths = _pathProvider.GetPaths(Path.Combine(temp.Path, "source"));
        SeedConfiguration(paths);
        var importFolder = Path.Combine(temp.Path, "patient-import");
        Directory.CreateDirectory(importFolder);
        File.WriteAllText(Path.Combine(importFolder, "patient.gdt"), "patient");

        var backupPath = Path.Combine(temp.Path, "XDTBox_Backup_20260527_120000.xdtboxbackup");
        var result = _service.CreateBackup(paths, backupPath, "1.2.3", "installation-1");

        Assert.True(result.Success);
        Assert.True(File.Exists(backupPath));
        Assert.NotNull(result.Manifest);
        Assert.Equal("XDTBOX", result.Manifest!.ProductCode);
        Assert.True(result.Manifest.IncludesLicenseFile);

        using var archive = ZipFile.OpenRead(backupPath);
        Assert.NotNull(archive.GetEntry("manifest.json"));
        Assert.NotNull(archive.GetEntry("profiles/interfaces/interface-user.json"));
        Assert.NotNull(archive.GetEntry("device-images/device-user.png"));
        Assert.NotNull(archive.GetEntry("settings/device-image-overrides.json"));
        Assert.NotNull(archive.GetEntry("license-customer/license-customer-data.json"));
        Assert.NotNull(archive.GetEntry("license/license.xdtboxlic"));
        Assert.Null(archive.GetEntry("patient-import/patient.gdt"));
    }

    [Fact]
    public void RestoreBackup_ShouldRestoreConfigurationFiles()
    {
        using var temp = new TempFolder();
        var sourcePaths = _pathProvider.GetPaths(Path.Combine(temp.Path, "source"));
        var targetPaths = _pathProvider.GetPaths(Path.Combine(temp.Path, "target"));
        SeedConfiguration(sourcePaths);
        var backupPath = Path.Combine(temp.Path, "backup.xdtboxbackup");
        var backup = _service.CreateBackup(sourcePaths, backupPath, "1.2.3", "installation-1");
        Assert.True(backup.Success);

        var restore = _service.RestoreBackup(targetPaths, backupPath, isMonitoringRunning: false);

        Assert.True(restore.Success);
        Assert.True(File.Exists(Path.Combine(targetPaths.ProfilesFolder, "interfaces", "interface-user.json")));
        Assert.True(File.Exists(Path.Combine(targetPaths.BaseFolder, "DeviceImages", "device-user.png")));
        Assert.True(File.Exists(Path.Combine(targetPaths.BaseFolder, "device-image-overrides.json")));
        Assert.True(File.Exists(Path.Combine(targetPaths.LicensesFolder, "license-customer-data.json")));
        Assert.True(File.Exists(Path.Combine(targetPaths.LicensesFolder, "license.xdtboxlic")));
    }

    [Fact]
    public void RestoreBackup_ShouldRejectActiveMonitoring()
    {
        using var temp = new TempFolder();
        var paths = _pathProvider.GetPaths(Path.Combine(temp.Path, "source"));
        SeedConfiguration(paths);
        var backupPath = Path.Combine(temp.Path, "backup.xdtboxbackup");
        _ = _service.CreateBackup(paths, backupPath, "1.2.3", "installation-1");

        var restore = _service.RestoreBackup(paths, backupPath, isMonitoringRunning: true);

        Assert.False(restore.Success);
        Assert.Contains("Überwachung", string.Join(" ", restore.Messages));
    }

    [Fact]
    public void PreviewRestore_ShouldRejectBackupWithoutManifest()
    {
        using var temp = new TempFolder();
        var backupPath = Path.Combine(temp.Path, "broken.xdtboxbackup");
        using (var archive = ZipFile.Open(backupPath, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry("profiles/interfaces/test.json");
            using var writer = new StreamWriter(entry.Open());
            writer.Write("{}");
        }

        var preview = _service.PreviewRestore(backupPath);

        Assert.False(preview.Success);
        Assert.Contains("Manifest", string.Join(" ", preview.Messages));
    }

    [Fact]
    public void PreviewRestore_ShouldRejectWrongProductCode()
    {
        using var temp = new TempFolder();
        var backupPath = Path.Combine(temp.Path, "wrong-product.xdtboxbackup");
        using (var archive = ZipFile.Open(backupPath, ZipArchiveMode.Create))
        {
            var manifest = new XdtBoxBackupManifest(
                XdtBoxBackupService.BackupFormatVersion,
                "OTHER",
                "1.0",
                DateTime.UtcNow,
                "installation",
                Array.Empty<string>(),
                false,
                XdtBoxBackupService.HardwareMigrationNotice);
            var entry = archive.CreateEntry("manifest.json");
            using var stream = entry.Open();
            JsonSerializer.Serialize(stream, manifest);
        }

        var preview = _service.PreviewRestore(backupPath);

        Assert.False(preview.Success);
        Assert.Contains("XDTBox", string.Join(" ", preview.Messages));
    }

    private static void SeedConfiguration(AppDataPaths paths)
    {
        Directory.CreateDirectory(Path.Combine(paths.ProfilesFolder, "interfaces"));
        File.WriteAllText(
            Path.Combine(paths.ProfilesFolder, "interfaces", "interface-user.json"),
            """{"id":"interface-user","ConnectionKind":"SerialRs232","AisImportFolder":"C:\\XDTBox\\CV5000\\Patient2Box"}""");

        Directory.CreateDirectory(Path.Combine(paths.BaseFolder, "DeviceImages"));
        File.WriteAllText(Path.Combine(paths.BaseFolder, "DeviceImages", "device-user.png"), "image");
        File.WriteAllText(Path.Combine(paths.BaseFolder, "device-image-overrides.json"), """{"device-user":"DeviceImages/device-user.png"}""");

        Directory.CreateDirectory(paths.LicensesFolder);
        File.WriteAllText(Path.Combine(paths.LicensesFolder, "license-customer-data.json"), """{"CustomerName":"Praxis"}""");
        File.WriteAllText(Path.Combine(paths.LicensesFolder, "license.xdtboxlic"), "signed-license");
    }

    private sealed class TempFolder : IDisposable
    {
        public TempFolder()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"xdtbox-backup-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
