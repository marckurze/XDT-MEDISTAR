namespace XdtDeviceBridge.Infrastructure;

public sealed class ArchiveRetentionCleanupService
{
    private readonly FolderSafetyValidator _folderSafetyValidator;

    public ArchiveRetentionCleanupService()
        : this(new FolderSafetyValidator())
    {
    }

    public ArchiveRetentionCleanupService(FolderSafetyValidator folderSafetyValidator)
    {
        _folderSafetyValidator = folderSafetyValidator ?? throw new ArgumentNullException(nameof(folderSafetyValidator));
    }

    public ArchiveRetentionCleanupPreview PreviewCleanup(string archiveFolder, int retentionDays, DateTime nowUtc)
    {
        ValidateArguments(archiveFolder, retentionDays);

        var fullArchiveFolder = Path.GetFullPath(archiveFolder);
        var deleteFilesOlderThanUtc = nowUtc.ToUniversalTime().AddDays(-retentionDays);
        var issues = new List<string>();
        var filesEligibleForDeletion = new List<string>();

        if (!Directory.Exists(fullArchiveFolder))
        {
            issues.Add("Archivordner existiert nicht.");
            return new ArchiveRetentionCleanupPreview(
                ArchiveFolder: fullArchiveFolder,
                RetentionDays: retentionDays,
                DeleteFilesOlderThanUtc: deleteFilesOlderThanUtc,
                FilesEligibleForDeletion: filesEligibleForDeletion,
                Issues: issues,
                HasErrors: false);
        }

        try
        {
            foreach (var filePath in Directory.EnumerateFiles(fullArchiveFolder, "*", SearchOption.AllDirectories))
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.LastWriteTimeUtc < deleteFilesOlderThanUtc)
                {
                    filesEligibleForDeletion.Add(fileInfo.FullName);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            issues.Add($"Archivordner konnte nicht vollständig geprüft werden: {ex.Message}");
        }

        filesEligibleForDeletion.Sort(StringComparer.OrdinalIgnoreCase);
        return new ArchiveRetentionCleanupPreview(
            ArchiveFolder: fullArchiveFolder,
            RetentionDays: retentionDays,
            DeleteFilesOlderThanUtc: deleteFilesOlderThanUtc,
            FilesEligibleForDeletion: filesEligibleForDeletion,
            Issues: issues,
            HasErrors: issues.Count > 0);
    }

    public ArchiveRetentionCleanupPreview DeleteExpiredArchiveFiles(
        string archiveFolder,
        int retentionDays,
        DateTime nowUtc,
        bool dryRun)
    {
        ValidateArguments(archiveFolder, retentionDays);

        var safetyResult = _folderSafetyValidator.ValidateFolderForCleanup(archiveFolder);
        if (safetyResult.HasErrors)
        {
            var rejectedArchiveFolder = Path.GetFullPath(archiveFolder);
            return new ArchiveRetentionCleanupPreview(
                ArchiveFolder: rejectedArchiveFolder,
                RetentionDays: retentionDays,
                DeleteFilesOlderThanUtc: nowUtc.ToUniversalTime().AddDays(-retentionDays),
                FilesEligibleForDeletion: Array.Empty<string>(),
                Issues: safetyResult.Issues.Select(issue => issue.Message).ToList(),
                HasErrors: true);
        }

        var preview = PreviewCleanup(archiveFolder, retentionDays, nowUtc);
        if (dryRun || preview.HasErrors)
        {
            return preview;
        }

        var issues = preview.Issues.ToList();
        var fullArchiveFolder = Path.GetFullPath(archiveFolder);
        foreach (var filePath in preview.FilesEligibleForDeletion)
        {
            try
            {
                if (!IsSameOrChildPath(filePath, fullArchiveFolder))
                {
                    issues.Add($"Datei liegt nicht im Archivordner und wurde nicht gelöscht: {filePath}");
                    continue;
                }

                File.Delete(filePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                issues.Add($"Archivdatei konnte nicht gelöscht werden: {filePath}: {ex.Message}");
            }
        }

        return preview with
        {
            Issues = issues,
            HasErrors = issues.Count > 0
        };
    }

    private static void ValidateArguments(string archiveFolder, int retentionDays)
    {
        if (string.IsNullOrWhiteSpace(archiveFolder))
        {
            throw new ArgumentException("Archive folder must not be empty.", nameof(archiveFolder));
        }

        if (retentionDays <= 0)
        {
            throw new ArgumentException("Retention days must be greater than 0.", nameof(retentionDays));
        }
    }

    private static bool IsSameOrChildPath(string path, string parentPath)
    {
        var normalizedPath = NormalizeForComparison(path);
        var normalizedParent = NormalizeForComparison(parentPath);

        return string.Equals(normalizedPath, normalizedParent, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(
                normalizedParent + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(
                normalizedParent + Path.AltDirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeForComparison(string path)
    {
        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
