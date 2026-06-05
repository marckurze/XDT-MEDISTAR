using System.Globalization;
using System.Text.RegularExpressions;

namespace XdtDeviceBridge.Core;

public sealed class HuvitzTextDeviceParser
{
    public const string ParserMode = "HuvitzText";

    private static readonly Regex RefKmRegex = new(
        @"(?<id>S-[RK]-[RL])\s+(?<first>[+-]\d+(?:[\.,]\d+)?)\s+(?<second>[+-]\d+(?:[\.,]\d+)?)\s+(?<axis>[+-]?\d{1,3})(?:\s+(?<pd>\d+(?:[\.,]\d+)?))?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly string[] KnownModels =
    {
        "HDR-7000",
        "HDR-9000",
        "HLM-1",
        "HLM-7000P",
        "HLM-9000",
        "HNT-1P",
        "HRK-8000A",
        "HRK-9000A",
        "HTR-1A"
    };

    private readonly MedistarResultFormatter _formatter = new();

    public static bool IsParserMode(string? parserMode)
    {
        return string.Equals(parserMode, ParserMode, StringComparison.OrdinalIgnoreCase);
    }

    public DeviceParseResult ParseFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return ParseText(File.ReadAllText(path));
    }

    public DeviceParseResult ParseText(string rawText)
    {
        var normalizedText = rawText ?? string.Empty;
        var measurements = new List<MeasurementValue>();
        var issues = new List<DeviceParseIssue>();

        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return new DeviceParseResult(
                Array.Empty<MeasurementValue>(),
                new[]
                {
                    new DeviceParseIssue(DeviceParseIssueSeverity.Error, "Huvitz-Rohdaten sind leer.", string.Empty, null)
                });
        }

        AddMeasurement(measurements, "Common/Company", "Company", "Huvitz", null, null, "Common");
        var model = DetectModel(normalizedText);
        if (!string.IsNullOrWhiteSpace(model))
        {
            AddMeasurement(measurements, "Common/ModelName", "ModelName", model, null, null, "Common");
        }

        var refByEye = new Dictionary<string, RefValues>(StringComparer.OrdinalIgnoreCase);
        var kmByEye = new Dictionary<string, KmValues>(StringComparer.OrdinalIgnoreCase);
        ParseRefKmBlocks(normalizedText, measurements, refByEye, kmByEye);

        AddRefPreparedLines(measurements, refByEye);
        AddKmPreparedLines(measurements, kmByEye);

        ParseTonoBlocks(normalizedText, measurements);
        ParsePachyBlocks(normalizedText, measurements);

        var exportableMeasurements = measurements
            .Count(measurement => !measurement.SourcePath.StartsWith("Common/", StringComparison.OrdinalIgnoreCase));
        if (exportableMeasurements == 0)
        {
            issues.Add(new DeviceParseIssue(
                DeviceParseIssueSeverity.Warning,
                "Huvitz-Rohdaten wurden gelesen, aber keine exportierbaren REF/KM/Tono/Pachy-Werte erkannt.",
                string.Empty,
                null));
        }

        return new DeviceParseResult(measurements, issues);
    }

    private void ParseRefKmBlocks(
        string rawText,
        List<MeasurementValue> measurements,
        Dictionary<string, RefValues> refByEye,
        Dictionary<string, KmValues> kmByEye)
    {
        foreach (Match match in RefKmRegex.Matches(rawText))
        {
            var id = match.Groups["id"].Value.ToUpperInvariant();
            var eye = id.EndsWith("-R", StringComparison.OrdinalIgnoreCase) ? "R" : "L";
            var first = NormalizeSignedDecimal(match.Groups["first"].Value);
            var second = NormalizeSignedDecimal(match.Groups["second"].Value);
            var axis = NormalizeAxis(match.Groups["axis"].Value);
            var optionalPd = match.Groups["pd"].Success ? NormalizeUnsignedDecimal(match.Groups["pd"].Value) : null;

            if (id.StartsWith("S-R-", StringComparison.OrdinalIgnoreCase))
            {
                var values = new RefValues(eye, first, second, axis, optionalPd);
                refByEye[eye] = values;
                AddRefMeasurements(measurements, values);
            }
            else if (id.StartsWith("S-K-", StringComparison.OrdinalIgnoreCase))
            {
                var values = new KmValues(eye, NormalizeUnsignedDecimal(first), NormalizeUnsignedDecimal(second), axis);
                kmByEye[eye] = values;
                AddKmMeasurements(measurements, values);
            }
        }
    }

    private void AddRefMeasurements(List<MeasurementValue> measurements, RefValues values)
    {
        var prefix = $"Measure[@Type='REF']/REF/{values.Eye}";
        AddMeasurement(measurements, $"{prefix}/Sphere", $"{values.Eye} Sphere", values.Sphere, "dpt", values.Eye, "REF");
        AddMeasurement(measurements, $"{prefix}/Cylinder", $"{values.Eye} Cylinder", values.Cylinder, "dpt", values.Eye, "REF");
        AddMeasurement(measurements, $"{prefix}/Axis", $"{values.Eye} Axis", values.Axis, "deg", values.Eye, "REF");
        if (!string.IsNullOrWhiteSpace(values.Pd))
        {
            AddMeasurement(measurements, $"{prefix}/PD", $"{values.Eye} PD", values.Pd, "mm", values.Eye, "REF");
        }
    }

    private void AddKmMeasurements(List<MeasurementValue> measurements, KmValues values)
    {
        var prefix = $"Measure[@Type='KM']/KM/{values.Eye}";
        AddMeasurement(measurements, $"{prefix}/Radius1", $"{values.Eye} R1", values.Radius1, "mm", values.Eye, "KM");
        AddMeasurement(measurements, $"{prefix}/Radius2", $"{values.Eye} R2", values.Radius2, "mm", values.Eye, "KM");
        AddMeasurement(measurements, $"{prefix}/Axis", $"{values.Eye} Axis", values.Axis, "deg", values.Eye, "KM");
    }

    private void AddRefPreparedLines(List<MeasurementValue> measurements, IReadOnlyDictionary<string, RefValues> refByEye)
    {
        foreach (var eye in new[] { "R", "L" })
        {
            if (!refByEye.TryGetValue(eye, out var values))
            {
                continue;
            }

            var line = $"{eye}.:S={_formatter.FormatDiopter(values.Sphere)} Z={_formatter.FormatDiopter(values.Cylinder)}*{_formatter.FormatAxis(values.Axis)}";
            if (!string.IsNullOrWhiteSpace(values.Pd))
            {
                line += $" PD= {_formatter.FormatPd(values.Pd)}";
            }

            AddMeasurement(
                measurements,
                $"Measure[@Type='REF']/REF/{eye}/MedistarLine",
                $"{eye} MEDISTAR Huvitz REF-Zeile",
                line,
                null,
                eye,
                "REF");
        }
    }

    private void AddKmPreparedLines(List<MeasurementValue> measurements, IReadOnlyDictionary<string, KmValues> kmByEye)
    {
        var parts = new List<string>();
        foreach (var eye in new[] { "R", "L" })
        {
            if (!kmByEye.TryGetValue(eye, out var values))
            {
                continue;
            }

            parts.Add($"{eye}: R1={values.Radius1} R2={values.Radius2} *{_formatter.FormatAxis(values.Axis)}");
        }

        if (parts.Count > 0)
        {
            AddMeasurement(
                measurements,
                "Measure[@Type='KM']/KM/MedistarLine1",
                "MEDISTAR Huvitz KM R1/R2-Zeile",
                string.Join(" // ", parts),
                null,
                null,
                "KM");
        }
    }

    private void ParseTonoBlocks(string rawText, List<MeasurementValue> measurements)
    {
        var right = ReadEyeSeries(rawText, "T", "R");
        var left = ReadEyeSeries(rawText, "T", "L");

        AddEyeSeriesMeasurements(measurements, "TM", "Tono", "Tonometrie", "R", right, "mmHg");
        AddEyeSeriesMeasurements(measurements, "TM", "Tono", "Tonometrie", "L", left, "mmHg");

        var line = BuildTonoLine(right, left);
        if (!string.IsNullOrWhiteSpace(line))
        {
            AddMeasurement(
                measurements,
                "Measure[@Type='TM']/Tono/TonoListLine",
                "MEDISTAR Huvitz Tonometrie-Zeile",
                line,
                null,
                null,
                "TM");
        }
    }

    private void ParsePachyBlocks(string rawText, List<MeasurementValue> measurements)
    {
        var right = ReadEyeSeries(rawText, "P", "R");
        var left = ReadEyeSeries(rawText, "P", "L");

        AddEyeSeriesMeasurements(measurements, "CCT", "Pachy", "Pachymetrie", "R", right, "um");
        AddEyeSeriesMeasurements(measurements, "CCT", "Pachy", "Pachymetrie", "L", left, "um");

        var line = BuildPachyLine(right, left);
        if (!string.IsNullOrWhiteSpace(line))
        {
            AddMeasurement(
                measurements,
                "Measure[@Type='CCT']/Pachy/MedistarLine",
                "MEDISTAR Huvitz Pachymetrie-Zeile",
                line,
                null,
                null,
                "CCT");
        }
    }

    private static EyeSeries ReadEyeSeries(string rawText, string prefix, string eye)
    {
        return new EyeSeries(
            ReadTokenValue(rawText, $"{prefix}-{eye}01"),
            ReadTokenValue(rawText, $"{prefix}-{eye}02"),
            ReadTokenValue(rawText, $"{prefix}-{eye}03"),
            ReadTokenValue(rawText, $"{prefix}-{eye}-A"));
    }

    private static void AddEyeSeriesMeasurements(
        List<MeasurementValue> measurements,
        string measureType,
        string groupPath,
        string groupDisplayName,
        string eye,
        EyeSeries values,
        string unit)
    {
        var prefix = $"Measure[@Type='{measureType}']/{groupPath}/{eye}";
        AddSeriesValue(measurements, $"{prefix}/Value1", $"{groupDisplayName} {eye} Wert 1", values.Value1, unit, eye, measureType);
        AddSeriesValue(measurements, $"{prefix}/Value2", $"{groupDisplayName} {eye} Wert 2", values.Value2, unit, eye, measureType);
        AddSeriesValue(measurements, $"{prefix}/Value3", $"{groupDisplayName} {eye} Wert 3", values.Value3, unit, eye, measureType);
        AddSeriesValue(measurements, $"{prefix}/Average", $"{groupDisplayName} {eye} Mittelwert", values.Average, unit, eye, measureType);
    }

    private static void AddSeriesValue(
        List<MeasurementValue> measurements,
        string sourcePath,
        string displayName,
        string? value,
        string unit,
        string eye,
        string group)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        AddMeasurement(measurements, sourcePath, displayName, NormalizeUnsignedDecimal(value), unit, eye, group);
    }

    private static string? BuildTonoLine(EyeSeries right, EyeSeries left)
    {
        var parts = new List<string>();
        var rightPart = BuildTonoEyeSegment("R", right);
        var leftPart = BuildTonoEyeSegment("L", left);
        if (!string.IsNullOrWhiteSpace(rightPart))
        {
            parts.Add(rightPart);
        }

        if (!string.IsNullOrWhiteSpace(leftPart))
        {
            parts.Add(leftPart);
        }

        return parts.Count == 0 ? null : $"{string.Join(" // ", parts)} mmHg";
    }

    private static string? BuildTonoEyeSegment(string eye, EyeSeries values)
    {
        var valueParts = new[] { values.Value1, values.Value2, values.Value3 }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => FormatPlainNumber(value!))
            .ToList();
        var average = string.IsNullOrWhiteSpace(values.Average) ? string.Empty : $"[{FormatOneDecimal(values.Average)}]";
        if (valueParts.Count == 0 && string.IsNullOrWhiteSpace(average))
        {
            return null;
        }

        return $"{eye} = {string.Join(" ", valueParts.Concat(new[] { average }.Where(part => !string.IsNullOrWhiteSpace(part))))}";
    }

    private static string? BuildPachyLine(EyeSeries right, EyeSeries left)
    {
        var parts = new List<string>();
        var rightPart = BuildPachyEyeSegment("RA", right);
        var leftPart = BuildPachyEyeSegment("LA", left);
        if (!string.IsNullOrWhiteSpace(rightPart))
        {
            parts.Add(rightPart);
        }

        if (!string.IsNullOrWhiteSpace(leftPart))
        {
            parts.Add(leftPart);
        }

        return parts.Count == 0 ? null : string.Join(" // ", parts);
    }

    private static string? BuildPachyEyeSegment(string label, EyeSeries values)
    {
        var selected = values.Average
            ?? values.Value1
            ?? values.Value2
            ?? values.Value3;
        return string.IsNullOrWhiteSpace(selected)
            ? null
            : $"{label}: {FormatPachyMillimeters(selected)}";
    }

    private static string? ReadTokenValue(string rawText, string token)
    {
        var match = Regex.Match(
            rawText,
            $@"(?<![A-Za-z0-9-]){Regex.Escape(token)}\s*[:=]?\s*(?<value>[+-]?\d+(?:[\.,]\d+)?)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return match.Success ? match.Groups["value"].Value : null;
    }

    private static string? DetectModel(string rawText)
    {
        return KnownModels.FirstOrDefault(model => rawText.Contains(model, StringComparison.OrdinalIgnoreCase));
    }

    private static void AddMeasurement(
        List<MeasurementValue> measurements,
        string sourcePath,
        string displayName,
        string value,
        string? unit,
        string? eye,
        string group)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        measurements.Add(new MeasurementValue(sourcePath, displayName, value, unit, eye, group));
    }

    private static string NormalizeSignedDecimal(string value)
    {
        var normalized = value.Trim().Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number))
        {
            return normalized;
        }

        var sign = number < 0 ? "-" : "+";
        return sign + Math.Abs(number).ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string NormalizeUnsignedDecimal(string value)
    {
        var normalized = value.Trim().Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number))
        {
            return normalized;
        }

        return Math.Abs(number).ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string NormalizeAxis(string value)
    {
        var normalized = value.Trim().TrimStart('+');
        return int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var axis)
            ? axis.ToString(CultureInfo.InvariantCulture)
            : normalized;
    }

    private static string FormatPlainNumber(string value)
    {
        var normalized = value.Trim().Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number))
        {
            return normalized;
        }

        return number % 1 == 0
            ? number.ToString("0", CultureInfo.InvariantCulture)
            : number.ToString("0.#", CultureInfo.InvariantCulture);
    }

    private static string FormatOneDecimal(string value)
    {
        var normalized = value.Trim().Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number))
        {
            return normalized;
        }

        return number.ToString("0.0", CultureInfo.InvariantCulture);
    }

    private static string FormatPachyMillimeters(string value)
    {
        var normalized = value.Trim().Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number))
        {
            return normalized;
        }

        if (Math.Abs(number) > 10)
        {
            number /= 1000m;
        }

        return number.ToString("0.000", CultureInfo.InvariantCulture);
    }

    private sealed record RefValues(string Eye, string Sphere, string Cylinder, string Axis, string? Pd);
    private sealed record KmValues(string Eye, string Radius1, string Radius2, string Axis);
    private sealed record EyeSeries(string? Value1, string? Value2, string? Value3, string? Average);
}
