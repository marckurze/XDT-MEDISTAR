using System.Globalization;

namespace XdtDeviceBridge.Infrastructure;

public static class AttachmentImportFileFingerprint
{
    public static string Create(AttachmentImportFileCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        return Create(candidate.FullPath, candidate.SizeBytes, candidate.LastWriteTimeUtc);
    }

    public static string Create(string fullPath, long sizeBytes, DateTime lastWriteTimeUtc)
    {
        return string.Join(
            "|",
            NormalizePath(fullPath),
            sizeBytes.ToString(CultureInfo.InvariantCulture),
            lastWriteTimeUtc.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture));
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
