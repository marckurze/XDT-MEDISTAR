using System.Text.RegularExpressions;

namespace XdtDeviceBridge.Core;

public sealed class ExportFileNameBuilder
{
    private static readonly Regex InvalidCharsRegex = new("[<>:\\"/\\\\|?*]", RegexOptions.Compiled);
    private static readonly Regex MultiUnderscoreRegex = new("_+", RegexOptions.Compiled);

    public string Build(DeviceProfile profile, PatientData? patient, DateTime timestamp)
    {
        var pattern = string.IsNullOrWhiteSpace(profile.ExportFileNamePattern)
            ? "EXPORT_{yyyyMMdd_HHmmss}.XDT"
            : profile.ExportFileNamePattern;

        var fileName = pattern
            .Replace("{DeviceName}", profile.Name ?? string.Empty, StringComparison.Ordinal)
            .Replace("{PatientNumber}", patient?.PatientNumber ?? string.Empty, StringComparison.Ordinal)
            .Replace("{LastName}", patient?.LastName ?? string.Empty, StringComparison.Ordinal)
            .Replace("{FirstName}", patient?.FirstName ?? string.Empty, StringComparison.Ordinal)
            .Replace("{yyyyMMdd_HHmmss}", timestamp.ToString("yyyyMMdd_HHmmss"), StringComparison.Ordinal)
            .Replace("{yyyyMMdd}", timestamp.ToString("yyyyMMdd"), StringComparison.Ordinal)
            .Replace("{HHmmss}", timestamp.ToString("HHmmss"), StringComparison.Ordinal);

        fileName = fileName.Replace(' ', '_');
        fileName = InvalidCharsRegex.Replace(fileName, "_");
        fileName = MultiUnderscoreRegex.Replace(fileName, "_");
        fileName = fileName.Trim('_', '.');

        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = $"EXPORT_{timestamp:yyyyMMdd_HHmmss}";
        }

        if (!fileName.EndsWith(".XDT", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".XDT";
        }

        return fileName;
    }
}
