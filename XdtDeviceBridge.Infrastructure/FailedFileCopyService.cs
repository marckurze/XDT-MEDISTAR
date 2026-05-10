using System.Text;
using System.Text.RegularExpressions;

namespace XdtDeviceBridge.Infrastructure;

public sealed class FailedFileCopyService
{
    public FailedFileCopyResult CopyFailedFiles(
        string errorFolder,
        string interfaceProfileName,
        string aisFilePath,
        string deviceFilePath,
        DateTime failedAtUtc,
        string failureReason)
    {
        return TransferFailedFiles(
            errorFolder,
            interfaceProfileName,
            aisFilePath,
            deviceFilePath,
            failedAtUtc,
            failureReason,
            moveFiles: false);
    }

    public FailedFileCopyResult MoveFailedFiles(
        string errorFolder,
        string interfaceProfileName,
        string aisFilePath,
        string deviceFilePath,
        DateTime failedAtUtc,
        string failureReason)
    {
        return TransferFailedFiles(
            errorFolder,
            interfaceProfileName,
            aisFilePath,
            deviceFilePath,
            failedAtUtc,
            failureReason,
            moveFiles: true);
    }

    private static FailedFileCopyResult TransferFailedFiles(
        string errorFolder,
        string interfaceProfileName,
        string aisFilePath,
        string deviceFilePath,
        DateTime failedAtUtc,
        string failureReason,
        bool moveFiles)
    {
        if (string.IsNullOrWhiteSpace(errorFolder))
        {
            throw new ArgumentException("Error folder must not be empty.", nameof(errorFolder));
        }

        if (string.IsNullOrWhiteSpace(aisFilePath))
        {
            throw new ArgumentException("AIS file path must not be empty.", nameof(aisFilePath));
        }

        if (string.IsNullOrWhiteSpace(deviceFilePath))
        {
            throw new ArgumentException("Device file path must not be empty.", nameof(deviceFilePath));
        }

        var copiedFiles = new List<string>();
        var issues = new List<string>();
        var profileFolderName = SanitizePathSegment(interfaceProfileName);
        var dayFolder = Path.Combine(
            errorFolder,
            failedAtUtc.ToString("yyyy"),
            failedAtUtc.ToString("MM"),
            failedAtUtc.ToString("dd"),
            profileFolderName);

        Directory.CreateDirectory(dayFolder);
        TransferSingleFile(aisFilePath, Path.Combine(dayFolder, "AIS"), copiedFiles, issues, moveFiles);
        TransferSingleFile(deviceFilePath, Path.Combine(dayFolder, "Device"), copiedFiles, issues, moveFiles);
        WriteErrorFile(
            dayFolder,
            interfaceProfileName,
            aisFilePath,
            deviceFilePath,
            failedAtUtc,
            failureReason,
            copiedFiles,
            issues);

        return new FailedFileCopyResult(
            CopiedFiles: copiedFiles,
            Issues: issues,
            HasErrors: issues.Count > 0);
    }

    private static void TransferSingleFile(
        string sourceFilePath,
        string targetFolder,
        List<string> copiedFiles,
        List<string> issues,
        bool moveFile)
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

            copiedFiles.Add(targetFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            issues.Add($"Fehlerablage fehlgeschlagen für {sourceFilePath}: {ex.Message}");
        }
    }

    private static void WriteErrorFile(
        string dayFolder,
        string interfaceProfileName,
        string aisFilePath,
        string deviceFilePath,
        DateTime failedAtUtc,
        string failureReason,
        List<string> copiedFiles,
        List<string> issues)
    {
        try
        {
            var errorFilePath = GetNextAvailableFilePath(dayFolder, "error.txt");
            var content = new StringBuilder()
                .AppendLine($"Zeitpunkt UTC: {failedAtUtc:O}")
                .AppendLine($"Schnittstellenprofil: {interfaceProfileName}")
                .AppendLine($"AIS-Datei: {aisFilePath}")
                .AppendLine($"Gerätedatei: {deviceFilePath}")
                .AppendLine("Fehlergrund:")
                .AppendLine(failureReason ?? string.Empty)
                .ToString();

            File.WriteAllText(errorFilePath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            copiedFiles.Add(errorFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            issues.Add($"error.txt konnte nicht geschrieben werden: {ex.Message}");
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
