namespace XdtDeviceBridge.Infrastructure;

public sealed class XdtBoxBackupPathService
{
    public string GetDefaultBackupFolder()
    {
        var preferredFolder = Path.Combine(
            Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\",
            "XDTBox",
            "Backup");

        try
        {
            var root = Path.GetPathRoot(preferredFolder);
            if (!string.IsNullOrWhiteSpace(root) && Directory.Exists(root))
            {
                return preferredFolder;
            }
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            // Fall back to the user's documents folder below.
        }

        var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(documentsFolder, "XDTBox", "Backup");
    }

    public string CreateDefaultBackupFilePath(string? targetFolder = null, DateTime? timestamp = null)
    {
        var folder = string.IsNullOrWhiteSpace(targetFolder)
            ? GetDefaultBackupFolder()
            : targetFolder.Trim();
        var effectiveTimestamp = timestamp ?? DateTime.Now;
        return Path.Combine(folder, $"XDTBox_Backup_{effectiveTimestamp:yyyyMMdd_HHmmss}.xdtboxbackup");
    }
}
