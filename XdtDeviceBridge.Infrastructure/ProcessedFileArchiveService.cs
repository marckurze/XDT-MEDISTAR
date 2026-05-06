using System.Text;
using System.Text.RegularExpressions;

namespace XdtDeviceBridge.Infrastructure;

public sealed class ProcessedFileArchiveService
{
    public ProcessedFileArchiveResult ArchiveProcessedFiles(
        string archiveFolder,
        string interfaceProfileName,
        string aisFilePath,
        string deviceFilePath,
        DateTime processedAtUtc,
        bool moveFiles)
    {
        if (string.IsNullOrWhiteSpace(archiveFolder))
        {
            throw new ArgumentException("Archive folder must not be empty.", nameof(archiveFolder));
        }

        if (string.IsNullOrWhiteSpace(aisFilePath))
        {
            throw new ArgumentException("AIS file path must not be empty.", nameof(aisFilePath));
        }

        if (string.IsNullOrWhiteSpace(deviceFilePath))
        {
            throw new ArgumentException("Device file path must not be empty.", nameof(deviceFilePath));
        }

        var archivedFiles = new List<string>();
        var issues = new List<string>();
        var profileFolderName = SanitizePathSegment(interfaceProfileName);
        var dayFolder = Path.Combine(
            archiveFolder,
            processedAtUtc.ToString("yyyy"),
            processedAtUtc.ToString("MM"),
            processedAtUtc.ToString("dd"),
            profileFolderName);

        ArchiveSingleFile(aisFilePath, Path.Combine(dayFolder, "AIS"), moveFiles, archivedFiles, issues);
        ArchiveSingleFile(deviceFilePath, Path.Combine(dayFolder, "Device"), moveFiles, archivedFiles, issues);

        return new ProcessedFileArchiveResult(
            ArchivedFiles: archivedFiles,
            Issues: issues,
            HasErrors: issues.Count > 0);
    }

    private static void ArchiveSingleFile(
        string sourceFilePath,
        string targetFolder,
        bool moveFile,
        List<string> archivedFiles,
        List<string> issues)
    {
        if (!File.Exists(sourceFilePath))
        {
            issues.Add($"Quelldatei fehlt: {sourceFilePath}");
            return;
        }

        try
        {
            Directory.CreateDirectory(targetFolder);
            var targetFilePath = GetNextAvailableFilePath(targetFolder, Path.GetFileName(sourceFilePath));

            if (moveFile)
            {
                File.Move(sourceFilePath, targetFilePath);
            }
            else
            {
                File.Copy(sourceFilePath, targetFilePath);
            }

            archivedFiles.Add(targetFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            issues.Add($"Archivierung fehlgeschlagen für {sourceFilePath}: {ex.Message}");
        }
    }

    private static string GetNextAvailableFilePath(string folder, string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var candidate = Path.Combine(folder, fileName);
        var suffix = 0;

        while (File.Exists(candidate))
        {
            suffix++;
            candidate = Path.Combine(folder, $"{baseName}_{suffix}{extension}");
        }

        return candidate;
    }

    private static string SanitizePathSegment(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            var isSafeCharacter = char.IsLetterOrDigit(character) || character is '_' or '-' or '.';
            builder.Append(invalidChars.Contains(character) || !isSafeCharacter ? '_' : character);
        }

        var sanitized = Regex.Replace(builder.ToString(), "_{2,}", "_").Trim('_');
        return string.IsNullOrWhiteSpace(sanitized) ? "Schnittstellenprofil" : sanitized;
    }
}
