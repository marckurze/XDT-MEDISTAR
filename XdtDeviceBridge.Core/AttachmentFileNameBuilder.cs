using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace XdtDeviceBridge.Core;

public sealed class AttachmentFileNameBuilder
{
    public const string DefaultTemplate = "{Ais.PatientNumber}_{Date:ddMMyyyy}_{Time:HHmmss}{ExtensionUpper}";

    public string Build(
        string? template,
        PatientData? patient,
        DateTime timestamp,
        string? originalExtension)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            template = DefaultTemplate;
        }

        var normalizedOriginalExtension = NormalizeExtension(originalExtension, upperCase: false);
        var normalizedUpperExtension = NormalizeExtension(originalExtension, upperCase: true);

        var fileName = template
            .Replace("{Ais.PatientNumber}", GetSafeValue(patient?.PatientNumber), StringComparison.Ordinal)
            .Replace("{Ais.LastName}", GetSafeValue(patient?.LastName), StringComparison.Ordinal)
            .Replace("{Ais.FirstName}", GetSafeValue(patient?.FirstName), StringComparison.Ordinal)
            .Replace("{Ais.BirthDate:ddMMyyyy}", FormatBirthDate(patient?.BirthDate), StringComparison.Ordinal)
            .Replace("{Date:ddMMyyyy}", timestamp.ToString("ddMMyyyy", CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{Date:yyyyMMdd}", timestamp.ToString("yyyyMMdd", CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{Time:HHmmss}", timestamp.ToString("HHmmss", CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{OriginalExtension}", normalizedOriginalExtension, StringComparison.Ordinal)
            .Replace("{ExtensionUpper}", normalizedUpperExtension, StringComparison.Ordinal);

        fileName = SanitizeFileName(fileName);

        if (string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(fileName)))
        {
            fileName = $"ATTACHMENT_{timestamp:yyyyMMdd_HHmmss}{normalizedUpperExtension}";
        }

        return fileName;
    }

    public string BuildUniqueFileName(string targetFolder, string desiredFileName)
    {
        if (string.IsNullOrWhiteSpace(targetFolder))
        {
            throw new ArgumentException("Target folder must not be empty.", nameof(targetFolder));
        }

        if (string.IsNullOrWhiteSpace(desiredFileName))
        {
            throw new ArgumentException("Desired file name must not be empty.", nameof(desiredFileName));
        }

        var safeFileName = SanitizeFileName(desiredFileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            throw new ArgumentException("Desired file name did not contain a usable file name.", nameof(desiredFileName));
        }

        if (!File.Exists(Path.Combine(targetFolder, safeFileName)))
        {
            return safeFileName;
        }

        var extension = Path.GetExtension(safeFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(safeFileName);

        for (var index = 1; index < int.MaxValue; index++)
        {
            var candidate = $"{fileNameWithoutExtension}_{index:000}{extension}";
            if (!File.Exists(Path.Combine(targetFolder, candidate)))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Could not create a unique attachment file name.");
    }

    private static string GetSafeValue(string? value)
    {
        return SanitizeFileNamePart(value ?? string.Empty);
    }

    private static string FormatBirthDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        var supportedFormats = new[] { "ddMMyyyy", "yyyyMMdd", "dd.MM.yyyy", "yyyy-MM-dd" };
        return DateTime.TryParseExact(
            trimmed,
            supportedFormats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? parsed.ToString("ddMMyyyy", CultureInfo.InvariantCulture)
            : GetSafeValue(trimmed);
    }

    private static string NormalizeExtension(string? extension, bool upperCase)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        var normalized = extension.Trim();
        var pathExtension = Path.GetExtension(normalized);
        if (!string.IsNullOrWhiteSpace(pathExtension))
        {
            normalized = pathExtension;
        }

        if (!normalized.StartsWith(".", StringComparison.Ordinal))
        {
            normalized = "." + normalized;
        }

        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            if (character == '.')
            {
                builder.Append(character);
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
        }

        normalized = builder.ToString();
        if (normalized == ".")
        {
            return string.Empty;
        }

        return upperCase ? normalized.ToUpperInvariant() : normalized.ToLowerInvariant();
    }

    private static string SanitizeFileName(string fileName)
    {
        string safeFileName;
        try
        {
            safeFileName = Path.GetFileName(fileName);
        }
        catch (ArgumentException)
        {
            safeFileName = fileName;
        }

        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            safeFileName = fileName;
        }

        safeFileName = SanitizeFileNamePart(safeFileName);
        safeFileName = Regex.Replace(safeFileName, "_{2,}", "_");
        return safeFileName.Trim(' ', '_', '.');
    }

    private static string SanitizeFileNamePart(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var character in value.Trim())
        {
            if (invalidChars.Contains(character)
                || character == Path.DirectorySeparatorChar
                || character == Path.AltDirectorySeparatorChar
                || char.IsControl(character)
                || char.IsWhiteSpace(character))
            {
                builder.Append('_');
            }
            else
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }
}
