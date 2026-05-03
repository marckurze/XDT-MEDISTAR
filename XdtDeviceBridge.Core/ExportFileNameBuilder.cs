using System.Text;
using System.Text.RegularExpressions;

namespace XdtDeviceBridge.Core;

public sealed class ExportFileNameBuilder
{
    public string Build(DeviceProfile profile, PatientData? patient, DateTime timestamp)
    {
        var pattern = profile.ExportFileNamePattern;

        if (string.IsNullOrWhiteSpace(pattern))
        {
            pattern = "EXPORT_{yyyyMMdd_HHmmss}.XDT";
        }

        var fileName = pattern
            .Replace("{DeviceName}", profile.Name ?? string.Empty)
            .Replace("{PatientNumber}", patient?.PatientNumber ?? string.Empty)
            .Replace("{LastName}", patient?.LastName ?? string.Empty)
            .Replace("{FirstName}", patient?.FirstName ?? string.Empty)
            .Replace("{yyyyMMdd_HHmmss}", timestamp.ToString("yyyyMMdd_HHmmss"))
            .Replace("{yyyyMMdd}", timestamp.ToString("yyyyMMdd"))
            .Replace("{HHmmss}", timestamp.ToString("HHmmss"));

        fileName = SanitizeFileName(fileName);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = $"EXPORT_{timestamp:yyyyMMdd_HHmmss}.XDT";
        }

        if (!Path.HasExtension(fileName))
        {
            fileName += ".XDT";
        }

        return fileName;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();

        var builder = new StringBuilder(fileName.Length);

        foreach (var character in fileName)
        {
            if (invalidChars.Contains(character) || char.IsWhiteSpace(character))
            {
                builder.Append('_');
            }
            else
            {
                builder.Append(character);
            }
        }

        var sanitized = builder.ToString();

        sanitized = Regex.Replace(sanitized, "_{2,}", "_");
        sanitized = sanitized.Trim('_');

        return sanitized;
    }
}