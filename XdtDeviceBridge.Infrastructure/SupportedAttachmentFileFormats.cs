namespace XdtDeviceBridge.Infrastructure;

public static class SupportedAttachmentFileFormats
{
    private static readonly IReadOnlySet<string> SupportedExtensions = new HashSet<string>(
        new[]
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png",
            ".tif",
            ".tiff",
            ".dcm",
            ".txt",
            ".xml",
            ".mp4",
            ".mp3",
            ".wav"
        },
        StringComparer.OrdinalIgnoreCase);

    public static IReadOnlySet<string> Extensions => SupportedExtensions;

    public static bool IsSupported(string? extension)
    {
        return !string.IsNullOrWhiteSpace(extension)
            && SupportedExtensions.Contains(Normalize(extension));
    }

    public static string Normalize(string extension)
    {
        var normalized = extension.Trim();
        return normalized.StartsWith(".", StringComparison.Ordinal)
            ? normalized.ToLowerInvariant()
            : $".{normalized.ToLowerInvariant()}";
    }
}
