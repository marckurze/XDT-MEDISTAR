namespace XdtDeviceBridge.Infrastructure;

public static class ImportFileFingerprint
{
    public static string Create(PendingImportFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        var normalizedPath = NormalizePath(file.FilePath);
        try
        {
            var info = new FileInfo(normalizedPath);
            if (info.Exists)
            {
                return string.Join(
                    "|",
                    normalizedPath,
                    info.LastWriteTimeUtc.Ticks.ToString(),
                    info.Length.ToString());
            }
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or UnauthorizedAccessException)
        {
            // Fall back to scanner metadata. The fingerprint remains bounded to this scan's file version.
        }

        return string.Join(
            "|",
            normalizedPath,
            file.DetectedAtUtc.ToUniversalTime().Ticks.ToString(),
            "unknown");
    }

    public static string CreateObservedFileVersion(PendingImportFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        var normalizedPath = NormalizePath(file.FilePath);
        try
        {
            var info = new FileInfo(normalizedPath);
            if (info.Exists)
            {
                return string.Join(
                    "|",
                    normalizedPath,
                    info.LastWriteTimeUtc.Ticks.ToString(),
                    info.CreationTimeUtc.Ticks.ToString(),
                    info.LastAccessTimeUtc.Ticks.ToString(),
                    info.Length.ToString());
            }
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or UnauthorizedAccessException)
        {
            // Fall back to the normal import fingerprint if extended file metadata cannot be read.
        }

        return Create(file);
    }

    private static string NormalizePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return string.Empty;
        }

        try
        {
            return Path.GetFullPath(filePath.Trim());
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return filePath.Trim();
        }
    }
}
