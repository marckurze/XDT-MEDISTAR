using System.Text.RegularExpressions;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class XdtBaukastenDeviceCompatibilityService
{
    private const string WorkbenchMismatchWarning =
        "Hinweis: Die geladene Gerätedatei passt nicht eindeutig zum gewählten Geräteprofil. Die Baukasten-Vorschau wird trotzdem erzeugt. Bitte prüfen Sie Mapping und Ausgabe sorgfältig.";

    private readonly XmlDeviceParser _parser;
    private readonly NidekRtSerialPhoropterParser _rtSerialParser = new();
    private readonly HuvitzTextDeviceParser _huvitzTextParser = new();
    private readonly ShinNipponDeviceParser _shinNipponParser = new();
    private readonly ReichertDeviceParser _reichertParser = new();
    private readonly RodenstockDeviceParser _rodenstockParser = new();
    private readonly TomeyDeviceParser _tomeyParser = new();
    private readonly TomeyEmDeviceParser _tomeyEmParser = new();

    public XdtBaukastenDeviceCompatibilityService()
        : this(new XmlDeviceParser())
    {
    }

    public XdtBaukastenDeviceCompatibilityService(XmlDeviceParser parser)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    public XdtBaukastenDeviceCompatibilityResult Evaluate(DeviceProfileDefinition? deviceProfile, string? deviceFilePath)
    {
        return EvaluateForWorkbench(deviceProfile, deviceFilePath);
    }

    public XdtBaukastenDeviceCompatibilityResult Evaluate(DeviceProfileDefinition? deviceProfile, IReadOnlyList<MeasurementValue> measurements)
    {
        return EvaluateForWorkbench(deviceProfile, measurements);
    }

    public XdtBaukastenDeviceCompatibilityResult EvaluateForWorkbench(
        DeviceProfileDefinition? deviceProfile,
        string? deviceFilePath)
    {
        if (deviceProfile is null)
        {
            return XdtBaukastenDeviceCompatibilityResult.Compatible(Array.Empty<MeasurementValue>());
        }

        if (string.IsNullOrWhiteSpace(deviceFilePath) || !File.Exists(deviceFilePath))
        {
            return XdtBaukastenDeviceCompatibilityResult.NotReadable("Bitte zuerst eine Gerätetestdatei laden.");
        }

        try
        {
            if (NidekRtSerialPhoropterParser.IsParserMode(deviceProfile.ParserMode))
            {
                var serialResult = _rtSerialParser.ParseFile(deviceFilePath);
                if (serialResult.HasErrors)
                {
                    return XdtBaukastenDeviceCompatibilityResult.Malformed(
                        "Die Gerätedatei konnte nicht gelesen oder ausgewertet werden. Bitte prüfen Sie Datei und Format.",
                        serialResult.Measurements,
                        FindCompany(serialResult.Measurements),
                        FindModelName(serialResult.Measurements));
                }

                return EvaluateForWorkbench(deviceProfile, serialResult.Measurements);
            }

            if (HuvitzTextDeviceParser.IsParserMode(deviceProfile.ParserMode))
            {
                var huvitzResult = _huvitzTextParser.ParseFile(deviceFilePath);
                if (huvitzResult.HasErrors)
                {
                    return XdtBaukastenDeviceCompatibilityResult.Malformed(
                        "Die Gerätedatei konnte nicht gelesen oder ausgewertet werden. Bitte prüfen Sie Datei und Format.",
                        huvitzResult.Measurements,
                        FindCompany(huvitzResult.Measurements),
                        FindModelName(huvitzResult.Measurements));
                }

                return EvaluateForWorkbench(deviceProfile, huvitzResult.Measurements);
            }

            if (ShinNipponDeviceParser.IsParserMode(deviceProfile.ParserMode))
            {
                var shinNipponResult = _shinNipponParser.ParseFile(deviceFilePath);
                if (shinNipponResult.HasErrors)
                {
                    return XdtBaukastenDeviceCompatibilityResult.Malformed(
                        "Die GerÃ¤tedatei konnte nicht gelesen oder ausgewertet werden. Bitte prÃ¼fen Sie Datei und Format.",
                        shinNipponResult.Measurements,
                        FindCompany(shinNipponResult.Measurements),
                        FindModelName(shinNipponResult.Measurements));
                }

                return EvaluateForWorkbench(deviceProfile, shinNipponResult.Measurements);
            }

            if (ReichertDeviceParser.IsParserMode(deviceProfile.ParserMode))
            {
                var reichertResult = _reichertParser.ParseFile(deviceFilePath);
                if (reichertResult.HasErrors)
                {
                    return XdtBaukastenDeviceCompatibilityResult.Malformed(
                        "Die GerÃ¤tedatei konnte nicht gelesen oder ausgewertet werden. Bitte prÃ¼fen Sie Datei und Format.",
                        reichertResult.Measurements,
                        FindCompany(reichertResult.Measurements),
                        FindModelName(reichertResult.Measurements));
                }

                return EvaluateForWorkbench(deviceProfile, reichertResult.Measurements);
            }

            if (RodenstockDeviceParser.IsParserMode(deviceProfile.ParserMode))
            {
                var rodenstockResult = _rodenstockParser.ParseFile(deviceFilePath);
                if (rodenstockResult.HasErrors)
                {
                    return XdtBaukastenDeviceCompatibilityResult.Malformed(
                        "Die GerÃ¤tedatei konnte nicht gelesen oder ausgewertet werden. Bitte prÃ¼fen Sie Datei und Format.",
                        rodenstockResult.Measurements,
                        FindCompany(rodenstockResult.Measurements),
                        FindModelName(rodenstockResult.Measurements));
                }

                return EvaluateForWorkbench(deviceProfile, rodenstockResult.Measurements);
            }

            if (TomeyDeviceParser.IsParserMode(deviceProfile.ParserMode))
            {
                var tomeyResult = _tomeyParser.ParseFile(deviceFilePath);
                if (tomeyResult.HasErrors)
                {
                    return XdtBaukastenDeviceCompatibilityResult.Malformed(
                        "Die Gerätedatei konnte nicht gelesen oder ausgewertet werden. Bitte prüfen Sie Datei und Format.",
                        tomeyResult.Measurements,
                        FindCompany(tomeyResult.Measurements),
                        FindModelName(tomeyResult.Measurements));
                }

                return EvaluateForWorkbench(deviceProfile, tomeyResult.Measurements);
            }

            if (TomeyEmDeviceParser.IsParserMode(deviceProfile.ParserMode))
            {
                var tomeyEmResult = _tomeyEmParser.ParseFile(deviceFilePath);
                if (tomeyEmResult.HasErrors)
                {
                    return XdtBaukastenDeviceCompatibilityResult.Malformed(
                        "Die Gerätedatei konnte nicht gelesen oder ausgewertet werden. Bitte prüfen Sie Datei und Format.",
                        tomeyEmResult.Measurements,
                        FindCompany(tomeyEmResult.Measurements),
                        FindModelName(tomeyEmResult.Measurements));
                }

                return EvaluateForWorkbench(deviceProfile, tomeyEmResult.Measurements);
            }

            var parseResult = _parser.ParseFile(deviceFilePath);
            if (parseResult.HasErrors)
            {
                return XdtBaukastenDeviceCompatibilityResult.Malformed(
                    "Die Gerätedatei konnte nicht gelesen oder ausgewertet werden. Bitte prüfen Sie Datei und Format.",
                    parseResult.Measurements,
                    FindCompany(parseResult.Measurements),
                    FindModelName(parseResult.Measurements));
            }

            return EvaluateForWorkbench(deviceProfile, parseResult.Measurements);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return XdtBaukastenDeviceCompatibilityResult.NotReadable(
                $"Gerätedatei konnte nicht gelesen werden: {ex.Message}");
        }
    }

    public XdtBaukastenDeviceCompatibilityResult EvaluateForWorkbench(
        DeviceProfileDefinition? deviceProfile,
        IReadOnlyList<MeasurementValue> measurements)
    {
        var detectedCompany = FindCompany(measurements);
        var detectedModelName = FindModelName(measurements);
        if (deviceProfile is null)
        {
            return XdtBaukastenDeviceCompatibilityResult.Compatible(measurements, detectedCompany, detectedModelName);
        }

        if (measurements.Count == 0)
        {
            return XdtBaukastenDeviceCompatibilityResult.NoExportableValues(
                "Die Gerätedatei wurde gelesen, enthält aber keine exportierbaren Werte für das gewählte Mapping.",
                measurements,
                detectedCompany,
                detectedModelName);
        }

        if (!string.IsNullOrWhiteSpace(detectedModelName))
        {
            var normalizedDetectedModel = NormalizeModelName(detectedModelName);
            var exactModel = NormalizeModelName(deviceProfile.Model);
            var aliases = CreateModelAliases(deviceProfile);
            if (!string.IsNullOrWhiteSpace(exactModel) && string.Equals(normalizedDetectedModel, exactModel, StringComparison.OrdinalIgnoreCase))
            {
                return XdtBaukastenDeviceCompatibilityResult.Compatible(measurements, detectedCompany, detectedModelName);
            }

            if (aliases.Contains(normalizedDetectedModel))
            {
                return XdtBaukastenDeviceCompatibilityResult.ProbablyCompatible(measurements, detectedCompany, detectedModelName);
            }

            return deviceProfile.Metadata.IsBuiltIn
                ? XdtBaukastenDeviceCompatibilityResult.ModelMismatchWarning(WorkbenchMismatchWarning, measurements, detectedCompany, detectedModelName)
                : XdtBaukastenDeviceCompatibilityResult.UnknownButParseable(
                    "Gerätedatei ist lesbar. Das Modell ist für dieses Entwurfsprofil nicht eindeutig bekannt, die Baukasten-Vorschau bleibt verfügbar.",
                    measurements,
                    detectedCompany,
                    detectedModelName);
        }

        if (HasProfilePathOverlap(deviceProfile, measurements))
        {
            return XdtBaukastenDeviceCompatibilityResult.ProbablyCompatible(measurements, detectedCompany, detectedModelName);
        }

        return XdtBaukastenDeviceCompatibilityResult.UnknownButParseable(
            "Gerätedatei ist lesbar, aber keinem bekannten Gerätemodell eindeutig zuzuordnen. Die Baukasten-Vorschau bleibt verfügbar.",
            measurements,
            detectedCompany,
            detectedModelName);
    }

    public XdtBaukastenDeviceCompatibilityResult EvaluateForProduction(
        DeviceProfileDefinition? deviceProfile,
        IReadOnlyList<MeasurementValue> measurements)
    {
        var workbenchResult = EvaluateForWorkbench(deviceProfile, measurements);
        return workbenchResult.Status is XdtBaukastenDeviceCompatibilityStatus.Compatible
                or XdtBaukastenDeviceCompatibilityStatus.ProbablyCompatible
            ? workbenchResult
            : workbenchResult with
            {
                Status = XdtBaukastenDeviceCompatibilityStatus.Incompatible,
                Message = "Die geladene Gerätedatei passt nicht zum aktuell gewählten Geräteprofil."
            };
    }

    public bool IsCompatibleWithDeviceProfile(DeviceProfileDefinition? deviceProfile, IReadOnlyList<MeasurementValue> measurements)
    {
        return EvaluateForWorkbench(deviceProfile, measurements).IsCompatible;
    }

    public bool AllowsWorkbenchPreview(DeviceProfileDefinition? deviceProfile, IReadOnlyList<MeasurementValue> measurements)
    {
        return EvaluateForWorkbench(deviceProfile, measurements).AllowsPreview;
    }

    private static bool HasProfilePathOverlap(DeviceProfileDefinition deviceProfile, IReadOnlyList<MeasurementValue> measurements)
    {
        var profileSourcePaths = deviceProfile.Measurements
            .Where(measurement => !IsCommonMeasurement(measurement.SourcePath))
            .Select(measurement => measurement.SourcePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return measurements.Any(measurement =>
            !IsCommonMeasurement(measurement.SourcePath)
            && profileSourcePaths.Contains(measurement.SourcePath));
    }

    private static string? FindCompany(IReadOnlyList<MeasurementValue> measurements)
    {
        return measurements.FirstOrDefault(measurement =>
                string.Equals(measurement.SourcePath, "Common/Company", StringComparison.OrdinalIgnoreCase))?.Value
            ?? measurements.FirstOrDefault(measurement =>
                string.Equals(measurement.SourcePath, "Company", StringComparison.OrdinalIgnoreCase))?.Value;
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
        if (normalizedProfileText.Contains("AR1S", StringComparison.OrdinalIgnoreCase))
        {
            AddAliases(aliases, "AR-1S");
        }
        else
        {
            AddAliasIfContains(aliases, normalizedProfileText, "AR1", "AR-1", "AR1");
        }

        AddAliasIfContains(aliases, normalizedProfileText, "AR310A", "AR-310A", "AR310A");
        AddAliasIfContains(aliases, normalizedProfileText, "AR360", "AR360", "AR360A");
        AddAliasIfContains(aliases, normalizedProfileText, "LM1800PD", "LM-1800PD", "LM1800PD");
        AddAliasIfContains(aliases, normalizedProfileText, "LM7", "LM7", "LM7P");
        AddAliasIfContains(aliases, normalizedProfileText, "NT1P", "NT-1P", "NT1P");
        AddAliasIfContains(aliases, normalizedProfileText, "NT1E", "NT-1E", "NT1E");
        if (!normalizedProfileText.Contains("NT1P", StringComparison.OrdinalIgnoreCase)
            && !normalizedProfileText.Contains("NT1E", StringComparison.OrdinalIgnoreCase))
        {
            AddAliasIfContains(aliases, normalizedProfileText, "NT1", "NT-1", "NT1");
        }

        AddAliasIfContains(aliases, normalizedProfileText, "NT510", "NT-510", "NT510");
        AddAliasIfContains(aliases, normalizedProfileText, "NT530P", "NT530P", "NT-530P", "NT530", "NT-530");
        AddAliasIfContains(aliases, normalizedProfileText, "NT530", "NT530", "NT-530");
        AddAliasIfContains(aliases, normalizedProfileText, "RT6100", "RT6100");
        AddAliasIfContains(aliases, normalizedProfileText, "RT2100", "RT2100");
        AddAliasIfContains(aliases, normalizedProfileText, "RT3100", "RT3100");
        AddAliasIfContains(aliases, normalizedProfileText, "RT5100", "RT5100");
        AddAliasIfContains(aliases, normalizedProfileText, "CL300", "CL300", "CL300PDL");
        AddAliasIfContains(aliases, normalizedProfileText, "SOLOS", "SOLOS");
        AddAliasIfContains(aliases, normalizedProfileText, "KR800", "KR800S", "KR800");
        AddAliasIfContains(aliases, normalizedProfileText, "RM800", "RM800");
        AddAliasIfContains(aliases, normalizedProfileText, "KR1", "KR1");
        AddAliasIfContains(aliases, normalizedProfileText, "TRK2P", "TRK2P", "TRK2");
        AddAliasIfContains(aliases, normalizedProfileText, "TRK3", "TRK3", "TRK3OMNIA");
        AddAliasIfContains(aliases, normalizedProfileText, "CT1P", "CT1P");
        AddAliasIfContains(aliases, normalizedProfileText, "CT800A", "CT800A", "CT800");
        AddAliasIfContains(aliases, normalizedProfileText, "CV5000", "CV5000", "CV5000S");
        AddAliasIfContains(aliases, normalizedProfileText, "HRK8000A", "HRK8000A", "HRK-8000A");
        AddAliasIfContains(aliases, normalizedProfileText, "HRK9000A", "HRK9000A", "HRK-9000A");
        AddAliasIfContains(aliases, normalizedProfileText, "HNT1P", "HNT1P", "HNT-1P");
        AddAliasIfContains(aliases, normalizedProfileText, "HTR1A", "HTR1A", "HTR-1A");
        AddAliasIfContains(aliases, normalizedProfileText, "ACCUREFR800", "ACCUREFR800", "ACCUREF R800", "ACCUREF R-800");
        AddAliasIfContains(aliases, normalizedProfileText, "ACCUREFK900", "ACCUREFK900", "ACCUREF K900", "ACCUREF K-900");
        AddAliasIfContains(aliases, normalizedProfileText, "DL1000", "DL1000", "DL-1000");
        AddAliasIfContains(aliases, normalizedProfileText, "DL800", "DL800", "DL-800");
        AddAliasIfContains(aliases, normalizedProfileText, "DL900", "DL900", "DL-900");
        AddAliasIfContains(aliases, normalizedProfileText, "NCT200", "NCT200", "NCT-200");
        AddAliasIfContains(aliases, normalizedProfileText, "SLM4000", "SLM4000", "SLM-4000");
        AddAliasIfContains(aliases, normalizedProfileText, "7CR", "7CR", "7CRNCT");
        AddAliasIfContains(aliases, normalizedProfileText, "LENSCHEKPLUS", "LensChek Plus", "LensChek", "LCHECKP");
        AddAliasIfContains(aliases, normalizedProfileText, "CX800", "CX800", "CX-800", "CX 800");
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
    string Message,
    string? DetectedCompany = null,
    string? DetectedModelName = null)
{
    public bool IsCompatible => Status == XdtBaukastenDeviceCompatibilityStatus.Compatible
        || Status == XdtBaukastenDeviceCompatibilityStatus.ProbablyCompatible;

    public bool AllowsPreview => Status is XdtBaukastenDeviceCompatibilityStatus.Compatible
        or XdtBaukastenDeviceCompatibilityStatus.ProbablyCompatible
        or XdtBaukastenDeviceCompatibilityStatus.ModelMismatchWarning
        or XdtBaukastenDeviceCompatibilityStatus.UnknownButParseable
        or XdtBaukastenDeviceCompatibilityStatus.IncompatibleButPreviewAllowed;

    public bool IsWarning => Status is XdtBaukastenDeviceCompatibilityStatus.ModelMismatchWarning
        or XdtBaukastenDeviceCompatibilityStatus.UnknownButParseable
        or XdtBaukastenDeviceCompatibilityStatus.IncompatibleButPreviewAllowed;

    public static XdtBaukastenDeviceCompatibilityResult Compatible(
        IReadOnlyList<MeasurementValue> measurements,
        string? detectedCompany = null,
        string? detectedModelName = null)
    {
        return new(
            XdtBaukastenDeviceCompatibilityStatus.Compatible,
            measurements,
            "Gerätedatei passt zum aktuell gewählten Geräteprofil.",
            detectedCompany,
            detectedModelName);
    }

    public static XdtBaukastenDeviceCompatibilityResult ProbablyCompatible(
        IReadOnlyList<MeasurementValue> measurements,
        string? detectedCompany = null,
        string? detectedModelName = null)
    {
        return new(
            XdtBaukastenDeviceCompatibilityStatus.ProbablyCompatible,
            measurements,
            "Gerätedatei passt wahrscheinlich zum gewählten Geräteprofil.",
            detectedCompany,
            detectedModelName);
    }

    public static XdtBaukastenDeviceCompatibilityResult ModelMismatchWarning(
        string message,
        IReadOnlyList<MeasurementValue> measurements,
        string? detectedCompany,
        string? detectedModelName)
    {
        return new(XdtBaukastenDeviceCompatibilityStatus.ModelMismatchWarning, measurements, message, detectedCompany, detectedModelName);
    }

    public static XdtBaukastenDeviceCompatibilityResult UnknownButParseable(
        string message,
        IReadOnlyList<MeasurementValue> measurements,
        string? detectedCompany,
        string? detectedModelName)
    {
        return new(XdtBaukastenDeviceCompatibilityStatus.UnknownButParseable, measurements, message, detectedCompany, detectedModelName);
    }

    public static XdtBaukastenDeviceCompatibilityResult Malformed(
        string message,
        IReadOnlyList<MeasurementValue> measurements,
        string? detectedCompany = null,
        string? detectedModelName = null)
    {
        return new(XdtBaukastenDeviceCompatibilityStatus.Malformed, measurements, message, detectedCompany, detectedModelName);
    }

    public static XdtBaukastenDeviceCompatibilityResult NotReadable(string message)
    {
        return new(XdtBaukastenDeviceCompatibilityStatus.NotReadable, Array.Empty<MeasurementValue>(), message);
    }

    public static XdtBaukastenDeviceCompatibilityResult NoExportableValues(
        string message,
        IReadOnlyList<MeasurementValue> measurements,
        string? detectedCompany,
        string? detectedModelName)
    {
        return new(XdtBaukastenDeviceCompatibilityStatus.NoExportableValues, measurements, message, detectedCompany, detectedModelName);
    }
}

public enum XdtBaukastenDeviceCompatibilityStatus
{
    Compatible,
    ProbablyCompatible,
    ModelMismatchWarning,
    UnknownButParseable,
    IncompatibleButPreviewAllowed,
    Incompatible,
    Malformed,
    NotReadable,
    NoExportableValues
}
