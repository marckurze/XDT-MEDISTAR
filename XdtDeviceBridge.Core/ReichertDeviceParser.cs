using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace XdtDeviceBridge.Core;

public sealed class ReichertDeviceParser
{
    public const string ParserMode = "ReichertText";

    private readonly MedistarResultFormatter _formatter = new();

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
            return new DeviceParseResult(
                Array.Empty<MeasurementValue>(),
                new[] { new DeviceParseIssue(DeviceParseIssueSeverity.Error, "Reichert-Rohdaten sind leer.", sourcePath ?? string.Empty, null) });
        }

        var measurements = new List<MeasurementValue>();
        var issues = new List<DeviceParseIssue>();
        var model = DetectModel(text, sourcePath);
        Add(measurements, "Common/Company", "Company", "Reichert", null, null, "Common");
        Add(measurements, "Common/ModelName", "ModelName", model, null, null, "Common");

        if (model.Equals("LensChek Plus", StringComparison.OrdinalIgnoreCase))
        {
            if (LooksLikeXml(text))
            {
                ParseLensChekXml(text, measurements, issues, sourcePath);
            }
            else
            {
                ParseLensChekSerial(text, measurements);
            }
        }
        else
        {
            Parse7Cr(text, measurements);
        }

        if (measurements.All(measurement => measurement.SourcePath.StartsWith("Common/", StringComparison.OrdinalIgnoreCase))
            && !issues.Any(issue => issue.Severity == DeviceParseIssueSeverity.Error))
        {
            issues.Add(new DeviceParseIssue(
                DeviceParseIssueSeverity.Warning,
                "Reichert-Rohdaten wurden gelesen, aber keine exportierbaren Werte erkannt.",
                sourcePath ?? string.Empty,
                null));
        }

        return new DeviceParseResult(measurements, issues);
    }

    private static string DetectModel(string text, string? sourcePath)
    {
        var combined = $"{sourcePath ?? string.Empty} {text}";
        if (combined.Contains("LensChek", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("RLCHECK", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("Spectacle_Data", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("<R>", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("<L>", StringComparison.OrdinalIgnoreCase))
        {
            return "LensChek Plus";
        }

        if (combined.Contains("7CR", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("REI7CR", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("IOP", StringComparison.OrdinalIgnoreCase))
        {
            return "7CR NCT";
        }

        return "Reichert";
    }

    private static bool LooksLikeXml(string text)
    {
        return text.TrimStart().StartsWith("<", StringComparison.Ordinal);
    }

    private void ParseLensChekXml(
        string text,
        List<MeasurementValue> measurements,
        List<DeviceParseIssue> issues,
        string? sourcePath)
    {
        XDocument document;
        try
        {
            document = XDocument.Parse(text, LoadOptions.PreserveWhitespace);
        }
        catch (Exception ex) when (ex is System.Xml.XmlException or InvalidOperationException)
        {
            issues.Add(new DeviceParseIssue(DeviceParseIssueSeverity.Error, $"Reichert-LensChek-XML konnte nicht gelesen werden: {ex.Message}", sourcePath ?? string.Empty, null));
            return;
        }

        var eyes = new Dictionary<string, LensValues>(StringComparer.OrdinalIgnoreCase);
        foreach (var (eye, elementNames) in new[]
        {
            ("R", new[] { "OD", "R", "Right" }),
            ("L", new[] { "OS", "L", "Left" })
        })
        {
            var eyeElement = document.Descendants()
                .FirstOrDefault(element => elementNames.Any(name => string.Equals(element.Name.LocalName, name, StringComparison.OrdinalIgnoreCase)));
            if (eyeElement is null)
            {
                continue;
            }

            eyes[eye] = new LensValues(
                Eye: eye,
                Sphere: NormalizeSignedDecimal(ReadDescendantValue(eyeElement, "Sphere", "Sph", "SPH")),
                Cylinder: NormalizeSignedDecimal(ReadDescendantValue(eyeElement, "Cylinder", "Cyl", "CYL")),
                Axis: NormalizeAxis(ReadDescendantValue(eyeElement, "Axis", "AX")),
                Add: NormalizeSignedDecimal(ReadDescendantValue(eyeElement, "ADD", "Add", "Addition")),
                Prism: ReadDescendantValue(eyeElement, "Prism", "P"),
                Pd: NormalizeUnsignedDecimal(ReadDescendantValue(eyeElement, "PD", "Distance")));
        }

        AddLensMeasurements(measurements, eyes);
        AddLensMedistarLines(measurements, eyes);
    }

    private void ParseLensChekSerial(string text, List<MeasurementValue> measurements)
    {
        var eyes = new Dictionary<string, LensValues>(StringComparer.OrdinalIgnoreCase);
        string? eye = null;
        foreach (var rawLine in SplitLines(text))
        {
            var line = rawLine.Trim();
            if (line.Equals("<R>", StringComparison.OrdinalIgnoreCase) || line.Equals("R", StringComparison.OrdinalIgnoreCase))
            {
                eye = "R";
                eyes[eye] = new LensValues(eye);
                continue;
            }

            if (line.Equals("<L>", StringComparison.OrdinalIgnoreCase) || line.Equals("L", StringComparison.OrdinalIgnoreCase))
            {
                eye = "L";
                eyes[eye] = new LensValues(eye);
                continue;
            }

            if (eye is null)
            {
                continue;
            }

            var value = ReadColonValue(line);
            if (line.StartsWith("ADD", StringComparison.OrdinalIgnoreCase))
            {
                eyes[eye] = eyes[eye] with { Add = NormalizeSignedDecimal(value) };
            }
            else if (line.StartsWith("S", StringComparison.OrdinalIgnoreCase))
            {
                eyes[eye] = eyes[eye] with { Sphere = NormalizeSignedDecimal(value) };
            }
            else if (line.StartsWith("C", StringComparison.OrdinalIgnoreCase))
            {
                eyes[eye] = eyes[eye] with { Cylinder = NormalizeSignedDecimal(value) };
            }
            else if (line.StartsWith("A ", StringComparison.OrdinalIgnoreCase) || line.StartsWith("A:", StringComparison.OrdinalIgnoreCase))
            {
                eyes[eye] = eyes[eye] with { Axis = NormalizeAxis(value) };
            }
            else if (line.StartsWith("P ", StringComparison.OrdinalIgnoreCase) || line.StartsWith("P:", StringComparison.OrdinalIgnoreCase))
            {
                eyes[eye] = eyes[eye] with { Prism = value.Trim() };
            }
            else if (line.StartsWith("PD", StringComparison.OrdinalIgnoreCase))
            {
                eyes[eye] = eyes[eye] with { Pd = NormalizeUnsignedDecimal(value) };
            }
        }

        AddLensMeasurements(measurements, eyes);
        AddLensMedistarLines(measurements, eyes);
    }

    private void AddLensMeasurements(List<MeasurementValue> measurements, IReadOnlyDictionary<string, LensValues> eyes)
    {
        foreach (var values in eyes.Values)
        {
            var prefix = $"Measure[@Type='LM']/LM/{values.Eye}";
            Add(measurements, $"{prefix}/Sphere", $"LM {values.Eye} Sphere", values.Sphere, "dpt", values.Eye, "LM");
            Add(measurements, $"{prefix}/Cylinder", $"LM {values.Eye} Cylinder", values.Cylinder, "dpt", values.Eye, "LM");
            Add(measurements, $"{prefix}/Axis", $"LM {values.Eye} Axis", values.Axis, "deg", values.Eye, "LM");
            Add(measurements, $"{prefix}/ADD", $"LM {values.Eye} ADD", values.Add, "dpt", values.Eye, "LM");
            Add(measurements, $"{prefix}/Prism", $"LM {values.Eye} Prism", values.Prism, "pdpt", values.Eye, "LM");
            Add(measurements, $"{prefix}/PD", $"LM {values.Eye} PD", values.Pd, "mm", values.Eye, "LM");
        }
    }

    private void AddLensMedistarLines(List<MeasurementValue> measurements, IReadOnlyDictionary<string, LensValues> eyes)
    {
        foreach (var eye in new[] { "R", "L" })
        {
            if (!eyes.TryGetValue(eye, out var values))
            {
                continue;
            }

            var line = $"{eye}.:S={_formatter.FormatDiopter(values.Sphere)} Z={_formatter.FormatDiopter(values.Cylinder)}*{_formatter.FormatAxis(values.Axis)}";
            if (!string.IsNullOrWhiteSpace(values.Prism))
            {
                line += $" P={_formatter.FormatPrism(values.Prism)}";
            }

            if (!string.IsNullOrWhiteSpace(values.Pd))
            {
                line += $" PD= {_formatter.FormatPd(values.Pd)}";
            }

            if (!string.IsNullOrWhiteSpace(values.Add))
            {
                line += $" A={_formatter.FormatDiopter(values.Add)}";
            }

            Add(measurements, $"Measure[@Type='LM']/LM/{eye}/MedistarLine", $"LM {eye} MEDISTAR-Zeile", line, null, eye, "LM");
        }
    }

    private void Parse7Cr(string text, List<MeasurementValue> measurements)
    {
        var right = Read7CrEye(text, "R");
        var left = Read7CrEye(text, "L");
        Add7CrEyeMeasurements(measurements, right);
        Add7CrEyeMeasurements(measurements, left);

        var line = BuildTonoLine(right, left);
        Add(measurements, "Measure[@Type='TM']/Tono/TonoListLine", "Tonometrie MEDISTAR-Zeile", line, null, null, "TM");
    }

    private static TonoValues Read7CrEye(string text, string eye)
    {
        var marker = eye.Equals("R", StringComparison.OrdinalIgnoreCase) ? @"(?:\(R\)|\bR\b)" : @"(?:\(L\)|\bL\b)";
        var match = Regex.Match(
            text,
            $@"(?im)^\s*{marker}\s*[:=]?\s*(?<iop>[+-]?\d+(?:[\.,]\d+)?)?(?:\s+(?<score>[+-]?\d+(?:[\.,]\d+)?))?",
            RegexOptions.CultureInvariant);
        return match.Success
            ? new TonoValues(eye, NormalizeUnsignedDecimal(match.Groups["iop"].Value), NormalizeUnsignedDecimal(match.Groups["score"].Value))
            : new TonoValues(eye);
    }

    private static void Add7CrEyeMeasurements(List<MeasurementValue> measurements, TonoValues values)
    {
        var prefix = $"Measure[@Type='TM']/Tono/{values.Eye}";
        Add(measurements, $"{prefix}/IOP", $"Tonometrie {values.Eye} IOP", values.Iop, "mmHg", values.Eye, "TM");
        Add(measurements, $"{prefix}/Score", $"Tonometrie {values.Eye} Score", values.Score, null, values.Eye, "TM");
    }

    private static string? BuildTonoLine(TonoValues right, TonoValues left)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(right.Iop))
        {
            parts.Add($"R = {right.Iop}");
        }

        if (!string.IsNullOrWhiteSpace(left.Iop))
        {
            parts.Add($"L = {left.Iop}");
        }

        return parts.Count == 0 ? null : $"{string.Join(" // ", parts)} mmHg";
    }

    private static string? ReadDescendantValue(XElement root, params string[] names)
    {
        return root.Descendants()
            .FirstOrDefault(element => names.Any(name => string.Equals(element.Name.LocalName, name, StringComparison.OrdinalIgnoreCase)))
            ?.Value;
    }

    private static IEnumerable<string> SplitLines(string text)
    {
        return text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
    }

    private static string ReadColonValue(string line)
    {
        var index = line.IndexOf(':', StringComparison.Ordinal);
        return index >= 0 ? line[(index + 1)..] : line;
    }

    private static void Add(
        List<MeasurementValue> measurements,
        string sourcePath,
        string displayName,
        string? value,
        string? unit,
        string? eye,
        string group)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        measurements.Add(new MeasurementValue(sourcePath, displayName, value.Trim(), unit, eye, group));
    }

    private static string NormalizeSignedDecimal(string? value)
    {
        var normalized = NormalizeNumericText(value);
        if (!decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number))
        {
            return normalized;
        }

        var sign = number < 0 ? "-" : "+";
        return sign + Math.Abs(number).ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string NormalizeUnsignedDecimal(string? value)
    {
        var normalized = NormalizeNumericText(value);
        if (!decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number))
        {
            return normalized;
        }

        return Math.Abs(number).ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string NormalizeAxis(string? value)
    {
        var normalized = NormalizeNumericText(value).TrimStart('+');
        return int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var axis)
            ? axis.ToString(CultureInfo.InvariantCulture)
            : normalized;
    }

    private static string NormalizeNumericText(string? value)
    {
        var trimmed = value?.Trim().Replace(',', '.') ?? string.Empty;
        var match = Regex.Match(trimmed, @"[+-]?\s*\d+(?:\.\d+)?", RegexOptions.CultureInvariant);
        return match.Success ? match.Value.Replace(" ", string.Empty, StringComparison.Ordinal) : trimmed;
    }

    private sealed record LensValues(
        string Eye,
        string? Sphere = null,
        string? Cylinder = null,
        string? Axis = null,
        string? Add = null,
        string? Prism = null,
        string? Pd = null);

    private sealed record TonoValues(string Eye, string? Iop = null, string? Score = null);
}
