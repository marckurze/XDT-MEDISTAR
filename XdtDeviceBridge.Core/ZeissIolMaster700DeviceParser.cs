using System.Globalization;
using System.Xml.Linq;

namespace XdtDeviceBridge.Core;

public sealed class ZeissIolMaster700DeviceParser
{
    public const string ParserMode = "ZeissIolMaster700";

    public static bool IsParserMode(string? parserMode)
    {
        return string.Equals(parserMode, ParserMode, StringComparison.OrdinalIgnoreCase);
    }

    public DeviceParseResult ParseFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return ParseText(File.ReadAllText(path), path);
    }

    public DeviceParseResult ParseText(string rawText, string? sourcePath = null)
    {
        var text = rawText ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return CreateError("ZEISS IOLMaster 700-Rohdaten sind leer.", sourcePath);
        }

        XDocument document;
        try
        {
            document = XDocument.Parse(text, LoadOptions.PreserveWhitespace);
        }
        catch (Exception ex) when (ex is System.Xml.XmlException or InvalidOperationException)
        {
            return CreateError($"ZEISS IOLMaster 700-XML konnte nicht gelesen werden: {ex.Message}", sourcePath);
        }

        var measurements = new List<MeasurementValue>();
        var issues = new List<DeviceParseIssue>();
        Add(measurements, "Common/Company", "Company", "ZEISS", null, null, "Common");
        Add(measurements, "Common/ModelName", "ModelName", "IOLMaster 700", null, null, "Common");
        Add(measurements, "Common/PatientId", "Patient ID", ReadDescendantValue(document.Root, "Patient", "Id"), null, null, "Common");

        var right = ReadEye(document, "Od", "R");
        var left = ReadEye(document, "Os", "L");
        AddIolMeasurements(measurements, right, left);
        AddKmMeasurements(measurements, right, left);

        var iolLine = BuildIolLine(right, left);
        var kmLine = BuildKmLine(right, left);
        Add(measurements, "Measure[@Type='IOL']/IOL/MedistarLine", "IOL Biometrie MEDISTAR-Zeile", iolLine, null, null, "IOL");
        Add(measurements, "Measure[@Type='KM']/KM/MedistarLine", "IOLMaster Keratometrie MEDISTAR-Zeile", kmLine, null, null, "KM");

        if (measurements.All(measurement => measurement.SourcePath.StartsWith("Common/", StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add(new DeviceParseIssue(
                DeviceParseIssueSeverity.Warning,
                "ZEISS IOLMaster 700-XML wurde gelesen, aber keine exportierbaren VKT/AL- oder Keratometerwerte erkannt.",
                sourcePath ?? string.Empty,
                null));
        }

        return new DeviceParseResult(measurements, issues);
    }

    private static EyeValues ReadEye(XDocument document, string xmlEyeName, string eye)
    {
        var eyeElement = FindFirst(document, xmlEyeName);
        return new EyeValues(
            eye,
            NormalizeDecimal(ReadDescendantValue(eyeElement, "AxialData", "Acd", "Value"), decimals: 2, trimTrailingZeros: false),
            NormalizeDecimal(ReadDescendantValue(eyeElement, "AxialData", "Al", "Value"), decimals: 2, trimTrailingZeros: false),
            NormalizeDecimal(ReadDescendantValue(eyeElement, "Keratometry", "MaximalRadius"), decimals: 2, trimTrailingZeros: false),
            NormalizeDecimal(ReadDescendantValue(eyeElement, "Keratometry", "MaximalAxis"), decimals: 2, trimTrailingZeros: true),
            NormalizeDecimal(ReadDescendantValue(eyeElement, "Keratometry", "MinimalRadius"), decimals: 2, trimTrailingZeros: false),
            NormalizeDecimal(ReadDescendantValue(eyeElement, "Keratometry", "MinimalAxis"), decimals: 2, trimTrailingZeros: true));
    }

    private static void AddIolMeasurements(List<MeasurementValue> measurements, params EyeValues[] eyes)
    {
        foreach (var values in eyes)
        {
            var prefix = $"Measure[@Type='IOL']/IOL/{values.Eye}";
            Add(measurements, $"{prefix}/VKT", $"IOL {values.Eye} VKT", values.Vkt, "mm", values.Eye, "IOL");
            Add(measurements, $"{prefix}/AL", $"IOL {values.Eye} AL", values.Al, "mm", values.Eye, "IOL");
        }
    }

    private static void AddKmMeasurements(List<MeasurementValue> measurements, params EyeValues[] eyes)
    {
        foreach (var values in eyes)
        {
            var prefix = $"Measure[@Type='KM']/KM/{values.Eye}";
            Add(measurements, $"{prefix}/R1", $"KM {values.Eye} R1", values.R1, "mm", values.Eye, "KM");
            Add(measurements, $"{prefix}/R1Axis", $"KM {values.Eye} R1 Achse", values.R1Axis, "deg", values.Eye, "KM");
            Add(measurements, $"{prefix}/R2", $"KM {values.Eye} R2", values.R2, "mm", values.Eye, "KM");
            Add(measurements, $"{prefix}/R2Axis", $"KM {values.Eye} R2 Achse", values.R2Axis, "deg", values.Eye, "KM");
        }
    }

    private static string? BuildIolLine(params EyeValues[] eyes)
    {
        var parts = eyes
            .Select(BuildIolEyeLine)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
        return parts.Length == 0 ? null : string.Join(" // ", parts);
    }

    private static string? BuildIolEyeLine(EyeValues values)
    {
        var tokens = new List<string>();
        if (!string.IsNullOrWhiteSpace(values.Vkt))
        {
            tokens.Add($"VKT={values.Vkt}");
        }

        if (!string.IsNullOrWhiteSpace(values.Al))
        {
            tokens.Add($"AL={values.Al}");
        }

        return tokens.Count == 0 ? null : $"{values.Eye}: {string.Join(' ', tokens)}";
    }

    private static string? BuildKmLine(params EyeValues[] eyes)
    {
        var parts = eyes
            .Select(BuildKmEyeLine)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
        return parts.Length == 0 ? null : string.Join(" // ", parts);
    }

    private static string? BuildKmEyeLine(EyeValues values)
    {
        var tokens = new List<string>();
        if (!string.IsNullOrWhiteSpace(values.R1) || !string.IsNullOrWhiteSpace(values.R1Axis))
        {
            var token = $"R1={FormatSignedRadius(values.R1)}";
            if (!string.IsNullOrWhiteSpace(values.R1Axis))
            {
                token += $"*{values.R1Axis}";
            }

            tokens.Add(token);
        }

        if (!string.IsNullOrWhiteSpace(values.R2) || !string.IsNullOrWhiteSpace(values.R2Axis))
        {
            var token = $"R2={FormatSignedRadius(values.R2)}";
            if (!string.IsNullOrWhiteSpace(values.R2Axis))
            {
                token += $"*{values.R2Axis}";
            }

            tokens.Add(token);
        }

        return tokens.Count == 0 ? null : $"{values.Eye}: {string.Join(' ', tokens)}";
    }

    private static string FormatSignedRadius(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        return trimmed[0] switch
        {
            '+' => "+ " + trimmed[1..],
            '-' => "- " + trimmed[1..],
            _ => "+ " + trimmed
        };
    }

    private static XElement? FindFirst(XContainer? root, params string[] names)
    {
        if (root is null)
        {
            return null;
        }

        return root.Descendants()
            .FirstOrDefault(element => names.Any(name => element.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase)));
    }

    private static string? ReadDescendantValue(XElement? root, params string[] path)
    {
        if (root is null || path.Length == 0)
        {
            return null;
        }

        XContainer? current = root;
        foreach (var segment in path)
        {
            var next = current.Descendants()
                .FirstOrDefault(element => element.Name.LocalName.Equals(segment, StringComparison.OrdinalIgnoreCase));
            if (next is null)
            {
                return null;
            }

            current = next;
        }

        return (current as XElement)?.Value.Trim();
    }

    private static string? NormalizeDecimal(string? value, int decimals, bool trimTrailingZeros)
    {
        var trimmed = value?.Trim().Replace(',', '.');
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        if (!decimal.TryParse(trimmed, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed))
        {
            return trimmed;
        }

        var result = parsed.ToString("0." + new string('0', decimals), CultureInfo.InvariantCulture);
        return trimTrailingZeros && result.Contains('.', StringComparison.Ordinal)
            ? result.TrimEnd('0').TrimEnd('.')
            : result;
    }

    private static void Add(
        List<MeasurementValue> measurements,
        string sourcePath,
        string displayName,
        string? value,
        string? unit,
        string? eye,
        string? group)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        measurements.Add(new MeasurementValue(sourcePath, displayName, value.Trim(), unit, eye, group));
    }

    private static DeviceParseResult CreateError(string message, string? sourcePath)
    {
        return new DeviceParseResult(
            Array.Empty<MeasurementValue>(),
            new[] { new DeviceParseIssue(DeviceParseIssueSeverity.Error, message, sourcePath ?? string.Empty, null) });
    }

    private sealed record EyeValues(
        string Eye,
        string? Vkt = null,
        string? Al = null,
        string? R1 = null,
        string? R1Axis = null,
        string? R2 = null,
        string? R2Axis = null);
}
