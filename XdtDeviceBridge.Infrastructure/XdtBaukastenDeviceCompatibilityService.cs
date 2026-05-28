using System.Text.RegularExpressions;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class XdtBaukastenDeviceCompatibilityService
{
    private readonly XmlDeviceParser _parser;

    public XdtBaukastenDeviceCompatibilityService()
        : this(new XmlDeviceParser())
    {
    }

    public XdtBaukastenDeviceCompatibilityService(XmlDeviceParser parser)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    public XdtBaukastenDeviceCompatibilityResult Evaluate(
        DeviceProfileDefinition? deviceProfile,
        string? deviceFilePath)
    {
        if (deviceProfile is null)
        {
            return XdtBaukastenDeviceCompatibilityResult.Compatible(Array.Empty<MeasurementValue>());
        }

        if (string.IsNullOrWhiteSpace(deviceFilePath) || !File.Exists(deviceFilePath))
        {
            return XdtBaukastenDeviceCompatibilityResult.MissingFixture("Bitte zuerst eine Gerätetestdatei laden.");
        }

        try
        {
            var parseResult = _parser.ParseFile(deviceFilePath);
            if (parseResult.HasErrors)
            {
                return XdtBaukastenDeviceCompatibilityResult.Malformed(
                    "Gerätedatei konnte nicht als unterstützte XML-Datei gelesen werden.",
                    parseResult.Measurements);
            }

            return Evaluate(deviceProfile, parseResult.Measurements);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return XdtBaukastenDeviceCompatibilityResult.Malformed(
                $"Gerätedatei konnte nicht gelesen werden: {ex.Message}",
                Array.Empty<MeasurementValue>());
        }
    }

    public XdtBaukastenDeviceCompatibilityResult Evaluate(
        DeviceProfileDefinition? deviceProfile,
        IReadOnlyList<MeasurementValue> measurements)
    {
        if (deviceProfile is null)
        {
            return XdtBaukastenDeviceCompatibilityResult.Compatible(measurements);
        }

        if (measurements.Count == 0)
        {
            return XdtBaukastenDeviceCompatibilityResult.Unknown(
                "Gerätedatei enthält keine erkennbaren Messwerte.",
                measurements);
        }

        var parsedModelName = FindModelName(measurements);
        if (!string.IsNullOrWhiteSpace(parsedModelName))
        {
            var aliases = CreateModelAliases(deviceProfile);
            if (aliases.Contains(NormalizeModelName(parsedModelName)))
            {
                return XdtBaukastenDeviceCompatibilityResult.Compatible(measurements);
            }

            if (!deviceProfile.Metadata.IsBuiltIn)
            {
                return XdtBaukastenDeviceCompatibilityResult.Compatible(measurements);
            }

            return XdtBaukastenDeviceCompatibilityResult.Incompatible(
                "Die geladene Gerätedatei passt nicht zum aktuell gewählten Geräteprofil. Bitte passende Gerätedatei laden.",
                measurements);
        }

        var profileSourcePaths = deviceProfile.Measurements
            .Where(measurement => !IsCommonMeasurement(measurement.SourcePath))
            .Select(measurement => measurement.SourcePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (measurements.Any(measurement =>
            !IsCommonMeasurement(measurement.SourcePath)
            && profileSourcePaths.Contains(measurement.SourcePath)))
        {
            return XdtBaukastenDeviceCompatibilityResult.Compatible(measurements);
        }

        return XdtBaukastenDeviceCompatibilityResult.Incompatible(
            "Die geladene Gerätedatei passt nicht zum aktuell gewählten Geräteprofil. Bitte passende Gerätedatei laden.",
            measurements);
    }

    public bool IsCompatibleWithDeviceProfile(DeviceProfileDefinition? deviceProfile, IReadOnlyList<MeasurementValue> measurements)
    {
        return Evaluate(deviceProfile, measurements).IsCompatible;
    }

    private static string? FindModelName(IReadOnlyList<MeasurementValue> measurements)
    {
        return measurements.FirstOrDefault(measurement =>
                string.Equals(measurement.SourcePath, "Common/ModelName", StringComparison.OrdinalIgnoreCase))?.Value
            ?? measurements.FirstOrDefault(measurement =>
                string.Equals(measurement.SourcePath, "ModelName", StringComparison.OrdinalIgnoreCase))?.Value;
    }

    private static HashSet<string> CreateModelAliases(DeviceProfileDefinition deviceProfile)
    {
        var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddAliases(aliases, deviceProfile.Model);
        AddAliases(aliases, deviceProfile.Metadata.Name);
        AddAliases(aliases, deviceProfile.Metadata.Product);

        var normalizedProfileText = NormalizeModelName(string.Join(" ", new[]
        {
            deviceProfile.Model,
            deviceProfile.Metadata.Name,
            deviceProfile.Metadata.Product
        }));

        AddKnownAliases(aliases, normalizedProfileText);
        return aliases;
    }

    private static void AddAliases(HashSet<string> aliases, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        AddAlias(aliases, value);
        foreach (var part in Regex.Split(value, @"[/,;|+]"))
        {
            AddAlias(aliases, part);
        }
    }

    private static void AddKnownAliases(HashSet<string> aliases, string normalizedProfileText)
    {
        AddAliasIfContains(aliases, normalizedProfileText, "ARK1S", "ARK1S");
        AddAliasIfContains(aliases, normalizedProfileText, "AR360", "AR360", "AR360A");
        AddAliasIfContains(aliases, normalizedProfileText, "LM7", "LM7", "LM7P");
        AddAliasIfContains(aliases, normalizedProfileText, "NT530P", "NT530P", "NT530");
        AddAliasIfContains(aliases, normalizedProfileText, "RT6100", "RT6100");
        AddAliasIfContains(aliases, normalizedProfileText, "CL300", "CL300");
        AddAliasIfContains(aliases, normalizedProfileText, "SOLOS", "SOLOS");
        AddAliasIfContains(aliases, normalizedProfileText, "KR800", "KR800S", "KR800");
        AddAliasIfContains(aliases, normalizedProfileText, "KR1", "KR1");
        AddAliasIfContains(aliases, normalizedProfileText, "TRK2P", "TRK2P", "TRK2");
        AddAliasIfContains(aliases, normalizedProfileText, "CT1P", "CT1P");
        AddAliasIfContains(aliases, normalizedProfileText, "CT800A", "CT800A", "CT800");
        AddAliasIfContains(aliases, normalizedProfileText, "CV5000", "CV5000", "CV5000S");
    }

    private static void AddAliasIfContains(HashSet<string> aliases, string normalizedProfileText, string marker, params string[] values)
    {
        if (!normalizedProfileText.Contains(marker, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        foreach (var value in values)
        {
            aliases.Add(value);
        }
    }

    private static void AddAlias(HashSet<string> aliases, string value)
    {
        var normalized = NormalizeModelName(value);
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            aliases.Add(normalized);
        }
    }

    private static bool IsCommonMeasurement(string sourcePath)
    {
        return sourcePath.StartsWith("Common/", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourcePath, "Company", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourcePath, "ModelName", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourcePath, "MachineNo", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourcePath, "ROMVersion", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourcePath, "Version", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourcePath, "Date", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourcePath, "Time", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeModelName(string value)
    {
        return Regex.Replace(value, "[^A-Za-z0-9]", string.Empty).ToUpperInvariant();
    }
}

public sealed record XdtBaukastenDeviceCompatibilityResult(
    XdtBaukastenDeviceCompatibilityStatus Status,
    IReadOnlyList<MeasurementValue> Measurements,
    string Message)
{
    public bool IsCompatible => Status == XdtBaukastenDeviceCompatibilityStatus.Compatible
        || Status == XdtBaukastenDeviceCompatibilityStatus.ParserSupportedButNoExportableValues;

    public static XdtBaukastenDeviceCompatibilityResult Compatible(IReadOnlyList<MeasurementValue> measurements)
    {
        return new(
            XdtBaukastenDeviceCompatibilityStatus.Compatible,
            measurements,
            "Gerätedatei passt zum aktuell gewählten Geräteprofil.");
    }

    public static XdtBaukastenDeviceCompatibilityResult Incompatible(string message, IReadOnlyList<MeasurementValue> measurements)
    {
        return new(XdtBaukastenDeviceCompatibilityStatus.Incompatible, measurements, message);
    }

    public static XdtBaukastenDeviceCompatibilityResult Unknown(string message, IReadOnlyList<MeasurementValue> measurements)
    {
        return new(XdtBaukastenDeviceCompatibilityStatus.Unknown, measurements, message);
    }

    public static XdtBaukastenDeviceCompatibilityResult Malformed(string message, IReadOnlyList<MeasurementValue> measurements)
    {
        return new(XdtBaukastenDeviceCompatibilityStatus.Malformed, measurements, message);
    }

    public static XdtBaukastenDeviceCompatibilityResult MissingFixture(string message)
    {
        return new(XdtBaukastenDeviceCompatibilityStatus.MissingFixture, Array.Empty<MeasurementValue>(), message);
    }
}

public enum XdtBaukastenDeviceCompatibilityStatus
{
    Compatible,
    Incompatible,
    Unknown,
    Malformed,
    MissingFixture,
    ParserSupportedButNoExportableValues
}
