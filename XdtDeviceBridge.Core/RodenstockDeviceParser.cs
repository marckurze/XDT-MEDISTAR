using System.Globalization;
using System.Text.RegularExpressions;

namespace XdtDeviceBridge.Core;

public sealed class RodenstockDeviceParser
{
    public const string ParserMode = "RodenstockText";

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
                new[] { new DeviceParseIssue(DeviceParseIssueSeverity.Error, "Rodenstock-Rohdaten sind leer.", sourcePath ?? string.Empty, null) });
        }

        var measurements = new List<MeasurementValue>();
        var issues = new List<DeviceParseIssue>();
        Add(measurements, "Common/Company", "Company", "Rodenstock", null, null, "Common");
        Add(measurements, "Common/ModelName", "ModelName", DetectModel(text, sourcePath), null, null, "Common");

        var refEyes = new Dictionary<string, RefValues>(StringComparer.OrdinalIgnoreCase);
        var kmEyes = new Dictionary<string, KmValues>(StringComparer.OrdinalIgnoreCase);
        string? pd = null;
        string? vd = null;

        foreach (var row in ReadTokenRows(text))
        {
            switch (row.Token.ToUpperInvariant())
            {
                case "[VD]":
                    vd = NormalizeUnsignedDecimalTwo(row.Values.ElementAtOrDefault(0));
                    break;
                case "[PD]":
                    pd = NormalizeUnsignedDecimal(row.Values.ElementAtOrDefault(0));
                    break;
                case "[POWER_R]":
                    refEyes["R"] = ReadRefValues("R", row.Values);
                    break;
                case "[POWER_L]":
                    refEyes["L"] = ReadRefValues("L", row.Values);
                    break;
                case "[K1_R]":
                    kmEyes["R"] = EnsureKm(kmEyes, "R") with { R1Radius = NormalizeUnsignedDecimalTwo(row.Values.ElementAtOrDefault(0)), R1Power = NormalizeUnsignedDecimalTwo(row.Values.ElementAtOrDefault(1)), R1Axis = NormalizeAxis(row.Values.ElementAtOrDefault(2)) };
                    break;
                case "[K2_R]":
                    kmEyes["R"] = EnsureKm(kmEyes, "R") with { R2Radius = NormalizeUnsignedDecimalTwo(row.Values.ElementAtOrDefault(0)), R2Power = NormalizeUnsignedDecimalTwo(row.Values.ElementAtOrDefault(1)), R2Axis = NormalizeAxis(row.Values.ElementAtOrDefault(2)) };
                    break;
                case "[AV_R]":
                    kmEyes["R"] = EnsureKm(kmEyes, "R") with { AverageRadius = NormalizeUnsignedDecimalTwo(row.Values.ElementAtOrDefault(0)), AveragePower = NormalizeUnsignedDecimalTwo(row.Values.ElementAtOrDefault(1)) };
                    break;
                case "[CYL_R]":
                    kmEyes["R"] = EnsureKm(kmEyes, "R") with { Cylinder = NormalizeSignedDecimal(row.Values.ElementAtOrDefault(0)), CylinderAxis = NormalizeAxis(row.Values.ElementAtOrDefault(1)) };
                    break;
                case "[K1_L]":
                    kmEyes["L"] = EnsureKm(kmEyes, "L") with { R1Radius = NormalizeUnsignedDecimalTwo(row.Values.ElementAtOrDefault(0)), R1Power = NormalizeUnsignedDecimalTwo(row.Values.ElementAtOrDefault(1)), R1Axis = NormalizeAxis(row.Values.ElementAtOrDefault(2)) };
                    break;
                case "[K2_L]":
                    kmEyes["L"] = EnsureKm(kmEyes, "L") with { R2Radius = NormalizeUnsignedDecimalTwo(row.Values.ElementAtOrDefault(0)), R2Power = NormalizeUnsignedDecimalTwo(row.Values.ElementAtOrDefault(1)), R2Axis = NormalizeAxis(row.Values.ElementAtOrDefault(2)) };
                    break;
                case "[AV_L]":
                    kmEyes["L"] = EnsureKm(kmEyes, "L") with { AverageRadius = NormalizeUnsignedDecimalTwo(row.Values.ElementAtOrDefault(0)), AveragePower = NormalizeUnsignedDecimalTwo(row.Values.ElementAtOrDefault(1)) };
                    break;
                case "[CYL_L]":
                    kmEyes["L"] = EnsureKm(kmEyes, "L") with { Cylinder = NormalizeSignedDecimal(row.Values.ElementAtOrDefault(0)), CylinderAxis = NormalizeAxis(row.Values.ElementAtOrDefault(1)) };
                    break;
            }
        }

        AddRefMeasurements(measurements, refEyes, pd, vd);
        AddRefMedistarLines(measurements, refEyes, pd, vd);
        AddKmMeasurements(measurements, kmEyes);
        AddKmMedistarLines(measurements, kmEyes);

        if (measurements.All(measurement => measurement.SourcePath.StartsWith("Common/", StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add(new DeviceParseIssue(
                DeviceParseIssueSeverity.Warning,
                "Rodenstock-Rohdaten wurden gelesen, aber keine exportierbaren CX-800-REF/KM-Werte erkannt.",
                sourcePath ?? string.Empty,
                null));
        }

        return new DeviceParseResult(measurements, issues);
    }

    private static string DetectModel(string text, string? sourcePath)
    {
        var combined = $"{sourcePath ?? string.Empty} {text}".Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);
        return combined.Contains("CX800", StringComparison.OrdinalIgnoreCase) ? "CX 800" : "CX 800";
    }

    private static IReadOnlyList<TokenRow> ReadTokenRows(string text)
    {
        var rows = new List<TokenRow>();
        foreach (var rawLine in text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || !line.StartsWith("[", StringComparison.Ordinal))
            {
                continue;
            }

            var parts = line.Split(',').Select(part => part.Trim()).ToArray();
            rows.Add(new TokenRow(parts[0], parts.Skip(1).ToArray()));
        }

        return rows;
    }

    private static RefValues ReadRefValues(string eye, IReadOnlyList<string> values)
    {
        var offset = values.Count > 0 && values[0].Equals("A", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        return new RefValues(
            eye,
            NormalizeSignedDecimal(values.ElementAtOrDefault(offset)),
            NormalizeSignedDecimal(values.ElementAtOrDefault(offset + 1)),
            NormalizeAxis(values.ElementAtOrDefault(offset + 2)));
    }

    private static KmValues EnsureKm(Dictionary<string, KmValues> eyes, string eye)
    {
        return eyes.TryGetValue(eye, out var values) ? values : new KmValues(eye);
    }

    private static void AddRefMeasurements(
        List<MeasurementValue> measurements,
        IReadOnlyDictionary<string, RefValues> eyes,
        string? pd,
        string? vd)
    {
        foreach (var values in eyes.Values)
        {
            var prefix = $"Measure[@Type='REF']/REF/{values.Eye}";
            Add(measurements, $"{prefix}/Sphere", $"REF {values.Eye} Sphere", values.Sphere, "dpt", values.Eye, "REF");
            Add(measurements, $"{prefix}/Cylinder", $"REF {values.Eye} Cylinder", values.Cylinder, "dpt", values.Eye, "REF");
            Add(measurements, $"{prefix}/Axis", $"REF {values.Eye} Axis", values.Axis, "deg", values.Eye, "REF");
        }

        Add(measurements, "Measure[@Type='REF']/REF/PD", "REF PD", pd, "mm", null, "REF");
        Add(measurements, "Measure[@Type='REF']/REF/VD", "REF VD", vd, "mm", null, "REF");
    }

    private void AddRefMedistarLines(
        List<MeasurementValue> measurements,
        IReadOnlyDictionary<string, RefValues> eyes,
        string? pd,
        string? vd)
    {
        foreach (var eye in new[] { "R", "L" })
        {
            if (!eyes.TryGetValue(eye, out var values))
            {
                continue;
            }

            var line = $"{eye}.:S={_formatter.FormatDiopter(values.Sphere)} Z={_formatter.FormatDiopter(values.Cylinder)}*{_formatter.FormatAxis(values.Axis)}";
            if (eye == "R" && !string.IsNullOrWhiteSpace(pd))
            {
                line += $" PD= {_formatter.FormatPd(pd)}";
            }

            if (eye == "R" && !string.IsNullOrWhiteSpace(vd))
            {
                line += $" VD= {_formatter.FormatRaw(vd)}";
            }

            Add(measurements, $"Measure[@Type='REF']/REF/{eye}/MedistarLine", $"REF {eye} MEDISTAR-Zeile", line, null, eye, "REF");
        }
    }

    private static void AddKmMeasurements(List<MeasurementValue> measurements, IReadOnlyDictionary<string, KmValues> eyes)
    {
        foreach (var values in eyes.Values)
        {
            var prefix = $"Measure[@Type='KM']/KM/{values.Eye}";
            Add(measurements, $"{prefix}/R1/Radius", $"KM {values.Eye} R1 Radius", values.R1Radius, "mm", values.Eye, "KM");
            Add(measurements, $"{prefix}/R1/Power", $"KM {values.Eye} R1 Power", values.R1Power, "dpt", values.Eye, "KM");
            Add(measurements, $"{prefix}/R1/Axis", $"KM {values.Eye} R1 Axis", values.R1Axis, "deg", values.Eye, "KM");
            Add(measurements, $"{prefix}/R2/Radius", $"KM {values.Eye} R2 Radius", values.R2Radius, "mm", values.Eye, "KM");
            Add(measurements, $"{prefix}/R2/Power", $"KM {values.Eye} R2 Power", values.R2Power, "dpt", values.Eye, "KM");
            Add(measurements, $"{prefix}/R2/Axis", $"KM {values.Eye} R2 Axis", values.R2Axis, "deg", values.Eye, "KM");
            Add(measurements, $"{prefix}/AV/Radius", $"KM {values.Eye} AV Radius", values.AverageRadius, "mm", values.Eye, "KM");
            Add(measurements, $"{prefix}/AV/Power", $"KM {values.Eye} AV Power", values.AveragePower, "dpt", values.Eye, "KM");
            Add(measurements, $"{prefix}/Cylinder", $"KM {values.Eye} Cylinder", values.Cylinder, "dpt", values.Eye, "KM");
            Add(measurements, $"{prefix}/CylinderAxis", $"KM {values.Eye} Cylinder Axis", values.CylinderAxis, "deg", values.Eye, "KM");
        }
    }

    private static void AddKmMedistarLines(List<MeasurementValue> measurements, IReadOnlyDictionary<string, KmValues> eyes)
    {
        var radii = BuildKmRadiiLine(eyes);
        var average = BuildKmAverageLine(eyes);
        Add(measurements, "Measure[@Type='KM']/KM/MedistarLine1", "KM MEDISTAR R1/R2-Zeile", radii, null, null, "KM");
        Add(measurements, "Measure[@Type='KM']/KM/MedistarLine2", "KM MEDISTAR AV/CYL-Zeile", average, null, null, "KM");
    }

    private static string? BuildKmRadiiLine(IReadOnlyDictionary<string, KmValues> eyes)
    {
        var parts = new List<string>();
        foreach (var eye in new[] { "R", "L" })
        {
            if (!eyes.TryGetValue(eye, out var values))
            {
                continue;
            }

            var line = $"{eye}:";
            if (!string.IsNullOrWhiteSpace(values.R1Radius) || !string.IsNullOrWhiteSpace(values.R1Power) || !string.IsNullOrWhiteSpace(values.R1Axis))
            {
                line += $" R1={values.R1Radius} {values.R1Power} *{FormatAxis(values.R1Axis)}";
            }

            if (!string.IsNullOrWhiteSpace(values.R2Radius) || !string.IsNullOrWhiteSpace(values.R2Power) || !string.IsNullOrWhiteSpace(values.R2Axis))
            {
                line += $" R2={values.R2Radius} {values.R2Power} *{FormatAxis(values.R2Axis)}";
            }

            if (line.Length > 2)
            {
                parts.Add(line);
            }
        }

        return parts.Count == 0 ? null : string.Join(" // ", parts);
    }

    private static string? BuildKmAverageLine(IReadOnlyDictionary<string, KmValues> eyes)
    {
        var parts = new List<string>();
        foreach (var eye in new[] { "R", "L" })
        {
            if (!eyes.TryGetValue(eye, out var values))
            {
                continue;
            }

            var line = $"{eye}:";
            if (!string.IsNullOrWhiteSpace(values.AverageRadius) || !string.IsNullOrWhiteSpace(values.AveragePower))
            {
                line += $" AV={values.AverageRadius} {values.AveragePower}";
            }

            if (!string.IsNullOrWhiteSpace(values.Cylinder))
            {
                line += $" CYL={values.Cylinder}";
            }

            if (!string.IsNullOrWhiteSpace(values.CylinderAxis))
            {
                line += $" {values.CylinderAxis}";
            }

            if (line.Length > 2)
            {
                parts.Add(line);
            }
        }

        return parts.Count == 0 ? null : string.Join(" // ", parts);
    }

    private static string FormatAxis(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
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

    private static string NormalizeUnsignedDecimalTwo(string? value)
    {
        var normalized = NormalizeNumericText(value);
        if (!decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number))
        {
            return normalized;
        }

        return Math.Abs(number).ToString("0.00", CultureInfo.InvariantCulture);
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

    private sealed record TokenRow(string Token, IReadOnlyList<string> Values);
    private sealed record RefValues(string Eye, string? Sphere = null, string? Cylinder = null, string? Axis = null);
    private sealed record KmValues(
        string Eye,
        string? R1Radius = null,
        string? R1Power = null,
        string? R1Axis = null,
        string? R2Radius = null,
        string? R2Power = null,
        string? R2Axis = null,
        string? AverageRadius = null,
        string? AveragePower = null,
        string? Cylinder = null,
        string? CylinderAxis = null);
}
