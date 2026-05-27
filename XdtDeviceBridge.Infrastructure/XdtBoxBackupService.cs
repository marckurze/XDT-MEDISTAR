using System.IO.Compression;
using System.Text.Json;

namespace XdtDeviceBridge.Infrastructure;

public sealed class XdtBoxBackupService : IXdtBoxBackupService
{
    public const string BackupFormatVersion = "1";
    public const string ProductCode = "XDTBOX";
    public const string ManifestEntryName = "manifest.json";
    public const string HardwareMigrationNotice = "Bei Hardwaretausch bitte neue Lizenz anfordern. Karenzzeit 7 Tage ab Umzug der Hardware.";

    private const string DeviceImagesFolderName = "DeviceImages";
    private const string DeviceImageOverridesFileName = "device-image-overrides.json";
    private const string SignedLicenseFileName = "license.xdtboxlic";
    private const string LicenseCustomerDataFileName = "license-customer-data.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public XdtBoxBackupResult CreateBackup(
        AppDataPaths paths,
        string backupFilePath,
        string appVersion,
        string sourceInstallationId,
        bool includeLicenseFile = true)
    {
        ArgumentNullException.ThrowIfNull(paths);

        if (string.IsNullOrWhiteSpace(backupFilePath))
        {
            return Failure<XdtBoxBackupResult>("Sicherungsdatei fehlt.");
        }

        var messages = new List<string>();
        var includedAreas = new List<string>();
        var normalizedBackupPath = Path.GetFullPath(backupFilePath);
        var backupDirectory = Path.GetDirectoryName(normalizedBackupPath);
        if (!string.IsNullOrWhiteSpace(backupDirectory))
        {
            Directory.CreateDirectory(backupDirectory);
        }

        if (File.Exists(normalizedBackupPath))
        {
            File.Delete(normalizedBackupPath);
        }

        var includesLicenseFile = includeLicenseFile && File.Exists(GetSignedLicenseFilePath(paths));
        var manifest = new XdtBoxBackupManifest(
            BackupFormatVersion,
            ProductCode,
            string.IsNullOrWhiteSpace(appVersion) ? "unbekannt" : appVersion.Trim(),
            DateTime.UtcNow,
            string.IsNullOrWhiteSpace(sourceInstallationId) ? "unbekannt" : sourceInstallationId.Trim(),
            includedAreas,
            includesLicenseFile,
            HardwareMigrationNotice);

        try
        {
            using var archive = ZipFile.Open(normalizedBackupPath, ZipArchiveMode.Create);

            if (Directory.Exists(paths.ProfilesFolder))
            {
                AddDirectory(archive, paths.ProfilesFolder, "profiles", messages);
                includedAreas.Add("Profile");
            }

            if (Directory.Exists(paths.TemplatePackagesFolder))
            {
                AddDirectory(archive, paths.TemplatePackagesFolder, "template-packages", messages);
                includedAreas.Add("Templatepakete");
            }

            var deviceImagesFolder = GetDeviceImagesFolder(paths);
            if (Directory.Exists(deviceImagesFolder))
            {
                AddDirectory(archive, deviceImagesFolder, "device-images", messages);
                includedAreas.Add("Gerätebilder");
            }

            AddFileIfExists(archive, GetDeviceImageOverridesFilePath(paths), "settings/device-image-overrides.json", includedAreas, "Gerätebild-Overrides");
            AddFileIfExists(archive, GetFloatingWindowStateFilePath(paths), "settings/ui/floating-interface-windows.json", includedAreas, "UI-Einstellungen");
            AddFileIfExists(archive, GetAppSettingsFilePath(paths), "settings/ui/app-settings.json", includedAreas, "App-Einstellungen");
            AddFileIfExists(archive, GetLicenseCustomerDataFilePath(paths), "license-customer/license-customer-data.json", includedAreas, "Lizenz-Kundendaten");
            AddFileIfExists(archive, paths.DeviceGracePeriodsFile, "license/device-grace-periods.json", includedAreas, "Lizenz-Karenzzeiten");

            if (includesLicenseFile)
            {
                AddFile(archive, GetSignedLicenseFilePath(paths), "license/license.xdtboxlic");
            }

            WriteManifest(archive, manifest with { IncludedAreas = includedAreas.Distinct(StringComparer.OrdinalIgnoreCase).ToArray() });
            messages.Add("Sicherung wurde erstellt. Es wurden keine Patientendaten oder Messdateien gesichert.");
            return new XdtBoxBackupResult(true, normalizedBackupPath, manifest with { IncludedAreas = includedAreas.Distinct(StringComparer.OrdinalIgnoreCase).ToArray() }, messages);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or NotSupportedException)
        {
            TryDeleteFile(normalizedBackupPath);
            return new XdtBoxBackupResult(false, null, null, new[] { $"Sicherung konnte nicht erstellt werden: {ex.Message}" });
        }
    }

    public XdtBoxRestorePreview PreviewRestore(string backupFilePath)
    {
        if (string.IsNullOrWhiteSpace(backupFilePath))
        {
            return new XdtBoxRestorePreview(false, null, new[] { "Sicherungsdatei fehlt." });
        }

        try
        {
            using var archive = ZipFile.OpenRead(backupFilePath);
            var manifest = ReadManifest(archive);
            var validation = ValidateManifest(manifest);
            return validation.Count == 0
                ? new XdtBoxRestorePreview(true, manifest, new[] { "Sicherung ist lesbar." })
                : new XdtBoxRestorePreview(false, manifest, validation);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or JsonException or NotSupportedException)
        {
            return new XdtBoxRestorePreview(false, null, new[] { $"Sicherung konnte nicht gelesen werden: {ex.Message}" });
        }
    }

    public XdtBoxRestoreResult RestoreBackup(
        AppDataPaths paths,
        string backupFilePath,
        bool isMonitoringRunning)
    {
        ArgumentNullException.ThrowIfNull(paths);

        if (isMonitoringRunning)
        {
            return new XdtBoxRestoreResult(false, null, new[] { "Wiederherstellung nicht möglich: Bitte stoppen Sie zuerst die Überwachung." });
        }

        try
        {
            using var archive = ZipFile.OpenRead(backupFilePath);
            var manifest = ReadManifest(archive);
            var validation = ValidateManifest(manifest);
            if (validation.Count > 0)
            {
                return new XdtBoxRestoreResult(false, manifest, validation);
            }

            RestoreDirectory(archive, "profiles/", paths.ProfilesFolder);
            RestoreDirectory(archive, "template-packages/", paths.TemplatePackagesFolder);
            RestoreDirectory(archive, "device-images/", GetDeviceImagesFolder(paths));
            RestoreFile(archive, "settings/device-image-overrides.json", GetDeviceImageOverridesFilePath(paths));
            RestoreFile(archive, "settings/ui/floating-interface-windows.json", GetFloatingWindowStateFilePath(paths));
            RestoreFile(archive, "settings/ui/app-settings.json", GetAppSettingsFilePath(paths));
            RestoreFile(archive, "license-customer/license-customer-data.json", GetLicenseCustomerDataFilePath(paths));
            RestoreFile(archive, "license/device-grace-periods.json", paths.DeviceGracePeriodsFile);
            RestoreFile(archive, "license/license.xdtboxlic", GetSignedLicenseFilePath(paths));

            return new XdtBoxRestoreResult(true, manifest, new[]
            {
                "Sicherung wurde wiederhergestellt.",
                HardwareMigrationNotice,
                "Eine wiederhergestellte Lizenz kann auf neuer Hardware ungültig sein."
            });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or JsonException or NotSupportedException)
        {
            return new XdtBoxRestoreResult(false, null, new[] { $"Wiederherstellung fehlgeschlagen: {ex.Message}" });
        }
    }

    private static XdtBoxBackupResult Failure<T>(string message)
    {
        return new XdtBoxBackupResult(false, null, null, new[] { message });
    }

    private static void WriteManifest(ZipArchive archive, XdtBoxBackupManifest manifest)
    {
        var entry = archive.CreateEntry(ManifestEntryName, CompressionLevel.Optimal);
        using var stream = entry.Open();
        JsonSerializer.Serialize(stream, manifest, JsonOptions);
    }

    private static XdtBoxBackupManifest ReadManifest(ZipArchive archive)
    {
        var entry = archive.GetEntry(ManifestEntryName)
            ?? throw new InvalidDataException("Sicherung enthält kein Manifest.");
        using var stream = entry.Open();
        return JsonSerializer.Deserialize<XdtBoxBackupManifest>(stream, JsonOptions)
            ?? throw new InvalidDataException("Manifest ist leer oder ungültig.");
    }

    private static IReadOnlyList<string> ValidateManifest(XdtBoxBackupManifest manifest)
    {
        var messages = new List<string>();
        if (!string.Equals(manifest.ProductCode, ProductCode, StringComparison.Ordinal))
        {
            messages.Add("Sicherung ist nicht für XDTBox ausgestellt.");
        }

        if (!string.Equals(manifest.BackupFormatVersion, BackupFormatVersion, StringComparison.Ordinal))
        {
            messages.Add($"Sicherungsformat wird nicht unterstützt: {manifest.BackupFormatVersion}.");
        }

        return messages;
    }

    private static void AddDirectory(ZipArchive archive, string sourceFolder, string entryRoot, List<string> messages)
    {
        var normalizedSource = Path.GetFullPath(sourceFolder);
        foreach (var file in Directory.EnumerateFiles(normalizedSource, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(normalizedSource, file).Replace(Path.DirectorySeparatorChar, '/');
            AddFile(archive, file, $"{entryRoot.TrimEnd('/')}/{relativePath}");
        }

        messages.Add($"{entryRoot} gesichert.");
    }

    private static void AddFileIfExists(
        ZipArchive archive,
        string filePath,
        string entryName,
        List<string> includedAreas,
        string areaName)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        AddFile(archive, filePath, entryName);
        includedAreas.Add(areaName);
    }

    private static void AddFile(ZipArchive archive, string filePath, string entryName)
    {
        archive.CreateEntryFromFile(filePath, entryName.Replace('\\', '/'), CompressionLevel.Optimal);
    }

    private static void RestoreDirectory(ZipArchive archive, string entryPrefix, string targetFolder)
    {
        var entries = archive.Entries
            .Where(entry => entry.FullName.StartsWith(entryPrefix, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(entry.Name))
            .ToArray();
        if (entries.Length == 0)
        {
            return;
        }

        if (Directory.Exists(targetFolder))
        {
            Directory.Delete(targetFolder, recursive: true);
        }

        foreach (var entry in entries)
        {
            var relativePath = entry.FullName[entryPrefix.Length..];
            ExtractEntry(entry, Path.Combine(targetFolder, relativePath), targetFolder);
        }
    }

    private static void RestoreFile(ZipArchive archive, string entryName, string targetPath)
    {
        var entry = archive.GetEntry(entryName);
        if (entry is null)
        {
            return;
        }

        ExtractEntry(entry, targetPath, Path.GetDirectoryName(targetPath) ?? string.Empty);
    }

    private static void ExtractEntry(ZipArchiveEntry entry, string targetPath, string allowedRootFolder)
    {
        var normalizedTargetPath = Path.GetFullPath(targetPath);
        var normalizedRoot = Path.GetFullPath(allowedRootFolder);
        var normalizedRootWithSeparator = normalizedRoot.EndsWith(Path.DirectorySeparatorChar)
            ? normalizedRoot
            : normalizedRoot + Path.DirectorySeparatorChar;
        if (!normalizedTargetPath.StartsWith(normalizedRootWithSeparator, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(normalizedTargetPath, normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Sicherungsdatei enthält einen ungültigen Pfad.");
        }

        var targetDirectory = Path.GetDirectoryName(normalizedTargetPath)
            ?? throw new InvalidDataException("Zielpfad ist ungültig.");
        Directory.CreateDirectory(targetDirectory);

        entry.ExtractToFile(normalizedTargetPath, overwrite: true);
    }

    private static string GetDeviceImagesFolder(AppDataPaths paths)
    {
        return Path.Combine(paths.BaseFolder, DeviceImagesFolderName);
    }

    private static string GetDeviceImageOverridesFilePath(AppDataPaths paths)
    {
        return Path.Combine(paths.BaseFolder, DeviceImageOverridesFileName);
    }

    private static string GetFloatingWindowStateFilePath(AppDataPaths paths)
    {
        return Path.Combine(paths.BaseFolder, "ui", "floating-interface-windows.json");
    }

    private static string GetAppSettingsFilePath(AppDataPaths paths)
    {
        return Path.Combine(paths.BaseFolder, "ui", "app-settings.json");
    }

    private static string GetLicenseCustomerDataFilePath(AppDataPaths paths)
    {
        return Path.Combine(paths.LicensesFolder, LicenseCustomerDataFileName);
    }

    private static string GetSignedLicenseFilePath(AppDataPaths paths)
    {
        return Path.Combine(paths.LicensesFolder, SignedLicenseFileName);
    }

    private static void TryDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            // Best-effort cleanup only.
        }
    }
}
