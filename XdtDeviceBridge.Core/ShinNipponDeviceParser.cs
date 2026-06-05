using System.Globalization;
using System.Text.RegularExpressions;

namespace XdtDeviceBridge.Core;

public sealed class ShinNipponDeviceParser
{
    public const string ParserMode = "ShinNipponText";

    private static readonly string[] KnownModels =
    {
        "Accuref K-900",
        "Accuref R-800",
        "DL-1000",
        "DL-800",
        "DL-900",
        "DR-900",
        "NCT-200",
        "SLM-4000"
    };

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
                new[] { new DeviceParseIssue(DeviceParseIssueSeverity.Error, "Shin-Nippon-Rohdaten sind leer.", sourcePath ?? string.Empty, null) });
        }

        var measurements = new List<MeasurementValue>();
        var issues = new List<DeviceParseIssue>();
        var model = DetectModel(text, sourcePath);
        Add(measurements, "Common/Company", "Company", "Shin-Nippon", null, null, "Common");
        Add(measurements, "Common/ModelName", "ModelName", model, null, null, "Common");

        var refEyes = new Dictionary<string, RefValues>(StringComparer.OrdinalIgnoreCase);
        var lensEyes = new Dictionary<string, LensValues>(StringComparer.OrdinalIgnoreCase);
        var kmEyes = new Dictionary<string, KmValues>(StringComparer.OrdinalIgnoreCase);
        var tono = new TonoValues();

        if (LooksLikeLensmeter(text, model))
        {
            ParseLensmeter(text, lensEyes);
            AddLensMeasurements(measurements, lensEyes);
            AddLensMedistarLines(measurements, lensEyes);
        }

        if (LooksLikeNct(text, model))
        {
            ParseTono(text, tono);
            AddTonoMeasurements(measurements, tono);
        }

        if (LooksLikeAccuref(text, model))
        {
            ParseAccurefRef(text, refEyes, out var pd, out var vd);
            AddRefMeasurements(measurements, refEyes, pd, vd);
            AddRefMedistarLines(measurements, refEyes, pd, vd);

            ParseAccurefKeratometry(text, kmEyes);
            AddKmMeasurements(measurements, kmEyes);
            AddKmMedistarLines(measurements, kmEyes);
        }

        if (measurements.All(measurement => measurement.SourcePath.StartsWith("Common/", StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add(new DeviceParseIssue(
                DeviceParseIssueSeverity.Warning,
                "Shin-Nippon-Rohdaten wurden gelesen, aber keine exportierbaren Werte erkannt.",
                sourcePath ?? string.Empty,
                null));
        }

        return new DeviceParseResult(measurements, issues);
    }

    private static bool LooksLikeLensmeter(string text, string model)
    {
        return model.StartsWith("DL-", StringComparison.OrdinalIgnoreCase)
            || model.Equals("SLM-4000", StringComparison.OrdinalIgnoreCase)
            || text.Contains("<R>", StringComparison.OrdinalIgnoreCase)
            || text.Contains("S :", StringComparison.OrdinalIgnoreCase)
            || text.Contains("ADD:", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeNct(string text, string model)
    {
        return model.Equals("NCT-200", StringComparison.OrdinalIgnoreCase)
            || text.Contains("NCT-200", StringComparison.OrdinalIgnoreCase)
            || text.Contains("[mmHg]", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Avg", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeAccuref(string text, string model)
    {
        return model.Contains("Accuref", StringComparison.OrdinalIgnoreCase)
            || Regex.IsMatch(text, @"(?m)^\s*[RL]\s+.*\bS\b.*\bC\b.*\bA\b", RegexOptions.IgnoreCase)
            || Regex.IsMatch(text, @"(?m)^\s*[RL]\s+.*\bR1\b.*\bR2\b", RegexOptions.IgnoreCase)
            || text.Contains("PD=", StringComparison.OrdinalIgnoreCase);
    }

    private static string DetectModel(string text, string? sourcePath)
    {
        var combined = string.Join(" ", new[] { sourcePath ?? string.Empty, text });
        foreach (var model in KnownModels)
        {
            var compact = model.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase).Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);
            var upperCombined = combined.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase).Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);
            if (combined.Contains(model, StringComparison.OrdinalIgnoreCase)
                || upperCombined.Contains(compact, StringComparison.OrdinalIgnoreCase))
            {
                return model;
            }
        }

        return "Shin-Nippon";
    }

    private static void ParseLensmeter(string text, Dictionary<string, LensValues> eyes)
    {
        string? eye = null;
        foreach (var rawLine in SplitLines(text))
        {
            var line = rawLine.Trim();
            if (line.Equals("<R>", StringComparison.OrdinalIgnoreCase))
            {
                eye = "R";
                EnsureLens(eyes, eye);
                continue;
            }

            if (line.Equals("<L>", StringComparison.OrdinalIgnoreCase))
            {
                eye = "L";
                EnsureLens(eyes, eye);
                continue;
            }

            if (eye is null)
            {
                continue;
            }

            var value = ReadColonValue(line);
            if (line.StartsWith("S", StringComparison.OrdinalIgnoreCase))
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
            else if (line.StartsWith("ADD", StringComparison.OrdinalIgnoreCase))
            {
                eyes[eye] = eyes[eye] with { Add = NormalizeSignedDecimal(value) };
            }
            else if (line.StartsWith("PD", StringComparison.OrdinalIgnoreCase))
            {
                eyes[eye] = eyes[eye] with { Pd = NormalizeUnsignedDecimal(value) };
            }
            else if (line.StartsWith("P ", StringComparison.OrdinalIgnoreCase) || line.StartsWith("P:", StringComparison.OrdinalIgnoreCase))
            {
                eyes[eye] = eyes[eye] with { Prism = NormalizePrism(value) };
            }
        }
    }

    private static void ParseAccurefRef(string text, Dictionary<string, RefValues> eyes, out string? pd, out string? vd)
    {
        pd = null;
        vd = null;
        foreach (var rawLine in SplitLines(text))
        {
            var line = rawLine.Trim();
            if (line.StartsWith("PD=", StringComparison.OrdinalIgnoreCase))
            {
                pd = NormalizeUnsignedDecimal(line[3..]);
                continue;
            }

            if (line.StartsWith("T ", StringComparison.OrdinalIgnoreCase))
            {
                vd = NormalizeUnsignedDecimal(ReadNamedValue(line, "VD") ?? ReadTrailingNumber(line));
                continue;
            }

            var eye = ReadLeadingEye(line);
            if (eye is null || line.Contains("R1", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var sphere = NormalizeSignedDecimal(ReadNamedValue(line, "S"));
            var cylinder = NormalizeSignedDecimal(ReadNamedValue(line, "C"));
            var axis = NormalizeAxis(ReadNamedValue(line, "A"));
            if (!string.IsNullOrWhiteSpace(sphere) || !string.IsNullOrWhiteSpace(cylinder) || !string.IsNullOrWhiteSpace(axis))
            {
                eyes[eye] = new RefValues(eye, sphere, cylinder, axis);
            }
        }
    }

    private static void ParseAccurefKeratometry(string text, Dictionary<string, KmValues> eyes)
    {
        foreach (var rawLine in SplitLines(text))
        {
            var line = rawLine.Trim();
            var eye = ReadLeadingEye(line);
            if (eye is null || !line.Contains("R1", StringComparison.OrdinalIgnoreCase) || !line.Contains("R2", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var tokens = Regex.Split(line, @"\s+").Where(token => token.Length > 0).ToArray();
            var radius1 = NormalizeUnsignedDecimal(ReadTokenAfter(tokens, "R1"));
            var radius2 = NormalizeUnsignedDecimal(ReadTokenAfter(tokens, "R2"));
            var axisValues = ReadTokenValuesAfter(tokens, "AX").Select(NormalizeAxis).ToArray();
            var cylinder = NormalizeSignedDecimal(ReadTokenAfter(tokens, "CYL"));
            eyes[eye] = new KmValues(
                eye,
                radius1,
                axisValues.ElementAtOrDefault(0),
                radius2,
                axisValues.ElementAtOrDefault(1),
                cylinder);
        }
    }

    private static void ParseTono(string text, TonoValues values)
    {
        var currentUnit = ReadBracketValue(text, "mmHg") ?? "mmHg";
        values.Unit = currentUnit;

        foreach (var rawLine in SplitLines(text))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.Contains("[", StringComparison.Ordinal))
            {
                continue;
            }

            if (line.Contains("Avg", StringComparison.OrdinalIgnoreCase))
            {
                var numbers = ReadNumbers(line).ToArray();
                if (numbers.Length > 0)
                {
                    values.RightAverage = NormalizeUnsignedDecimal(numbers[0]);
                }

                if (numbers.Length > 1)
                {
                    values.LeftAverage = NormalizeUnsignedDecimal(numbers[1]);
                }

                continue;
            }

            var rowNumbers = ReadNumbers(line).ToArray();
            if (rowNumbers.Length >= 2)
            {
                var rightValue = NormalizeUnsignedDecimal(rowNumbers[0]);
                var leftValue = NormalizeUnsignedDecimal(rowNumbers[1]);
                if (!string.IsNullOrWhiteSpace(rightValue))
                {
                    values.RightValues.Add(rightValue);
                }

                if (!string.IsNullOrWhiteSpace(leftValue))
                {
                    values.LeftValues.Add(leftValue);
                }
            }
        }
    }

    private void AddLensMeasurements(List<MeasurementValue> measurements, IReadOnlyDictionary<string, LensValues> eyes)
    {
        foreach (var eye in new[] { "R", "L" })
        {
            if (!eyes.TryGetValue(eye, out var values))
            {
                continue;
            }

            var prefix = $"Measure[@Type='LM']/LM/{eye}";
            Add(measurements, $"{prefix}/Sphere", $"LM {eye} Sphere", values.Sphere, "dpt", eye, "LM");
            Add(measurements, $"{prefix}/Cylinder", $"LM {eye} Cylinder", values.Cylinder, "dpt", eye, "LM");
            Add(measurements, $"{prefix}/Axis", $"LM {eye} Axis", values.Axis, "deg", eye, "LM");
            Add(measurements, $"{prefix}/ADD", $"LM {eye} ADD", values.Add, "dpt", eye, "LM");
            Add(measurements, $"{prefix}/Prism", $"LM {eye} Prism", values.Prism, "pdpt", eye, "LM");
            Add(measurements, $"{prefix}/PD", $"LM {eye} PD", values.Pd, "mm", eye, "LM");
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

            var line = BuildLensLine(eye, values);
            Add(measurements, $"Measure[@Type='LM']/LM/{eye}/MedistarLine", $"LM {eye} MEDISTAR-Zeile", line, null, eye, "LM");
        }
    }

    private void AddRefMeasurements(List<MeasurementValue> measurements, IReadOnlyDictionary<string, RefValues> eyes, string? pd, string? vd)
    {
        foreach (var eye in new[] { "R", "L" })
        {
            if (!eyes.TryGetValue(eye, out var values))
            {
                continue;
            }

            var prefix = $"Measure[@Type='REF']/REF/{eye}";
            Add(measurements, $"{prefix}/Sphere", $"REF {eye} Sphere", values.Sphere, "dpt", eye, "REF");
            Add(measurements, $"{prefix}/Cylinder", $"REF {eye} Cylinder", values.Cylinder, "dpt", eye, "REF");
            Add(measurements, $"{prefix}/Axis", $"REF {eye} Axis", values.Axis, "deg", eye, "REF");
            Add(measurements, $"{prefix}/PD", $"REF {eye} PD", pd, "mm", eye, "REF");
            Add(measurements, $"{prefix}/VD", $"REF {eye} VD", vd, "mm", eye, "REF");
        }
    }

    private void AddRefMedistarLines(List<MeasurementValue> measurements, IReadOnlyDictionary<string, RefValues> eyes, string? pd, string? vd)
    {
        foreach (var eye in new[] { "R", "L" })
        {
            if (!eyes.TryGetValue(eye, out var values))
            {
                continue;
            }

            var line = BuildRefLine(eye, values, pd, vd);
            Add(measurements, $"Measure[@Type='REF']/REF/{eye}/MedistarLine", $"REF {eye} MEDISTAR-Zeile", line, null, eye, "REF");
        }
    }

    private static void AddKmMeasurements(List<MeasurementValue> measurements, IReadOnlyDictionary<string, KmValues> eyes)
    {
        foreach (var eye in new[] { "R", "L" })
        {
            if (!eyes.TryGetValue(eye, out var values))
            {
                continue;
            }

            var prefix = $"Measure[@Type='KM']/KM/{eye}";
            Add(measurements, $"{prefix}/R1/Radius", $"KM {eye} R1 Radius", values.Radius1, "mm", eye, "KM");
            Add(measurements, $"{prefix}/R1/Axis", $"KM {eye} R1 Axis", values.Radius1Axis, "deg", eye, "KM");
            Add(measurements, $"{prefix}/R2/Radius", $"KM {eye} R2 Radius", values.Radius2, "mm", eye, "KM");
            Add(measurements, $"{prefix}/R2/Axis", $"KM {eye} R2 Axis", values.Radius2Axis, "deg", eye, "KM");
            Add(measurements, $"{prefix}/Cylinder", $"KM {eye} Cylinder", values.Cylinder, "dpt", eye, "KM");
        }
    }

    private static void AddKmMedistarLines(List<MeasurementValue> measurements, IReadOnlyDictionary<string, KmValues> eyes)
    {
        var radiiLine = BuildKmRadiiLine(eyes);
        Add(measurements, "Measure[@Type='KM']/KM/MedistarLine1", "KM MEDISTAR R1/R2-Zeile", radiiLine, null, null, "KM");

        var cylinderLine = BuildKmCylinderLine(eyes);
        Add(measurements, "Measure[@Type='KM']/KM/MedistarLine2", "KM MEDISTAR CYL-Zeile", cylinderLine, null, null, "KM");
    }

    private static void AddTonoMeasurements(List<MeasurementValue> measurements, TonoValues values)
    {
        for (var index = 0; index < values.RightValues.Count; index++)
        {
            Add(measurements, $"Measure[@Type='TM']/Tono/R/Value{index + 1}", $"Tonometrie R Wert {index + 1}", values.RightValues[index], values.Unit, "R", "TM");
        }

        for (var index = 0; index < values.LeftValues.Count; index++)
        {
            Add(measurements, $"Measure[@Type='TM']/Tono/L/Value{index + 1}", $"Tonometrie L Wert {index + 1}", values.LeftValues[index], values.Unit, "L", "TM");
        }

        Add(measurements, "Measure[@Type='TM']/Tono/R/Average", "Tonometrie R Mittelwert", values.RightAverage, values.Unit, "R", "TM");
        Add(measurements, "Measure[@Type='TM']/Tono/L/Average", "Tonometrie L Mittelwert", values.LeftAverage, values.Unit, "L", "TM");
        Add(measurements, "Measure[@Type='TM']/Tono/TonoListLine", "Tonometrie MEDISTAR-Zeile", BuildTonoLine(values), null, null, "TM");
    }

    private string? BuildLensLine(string eye, LensValues values)
    {
        if (string.IsNullOrWhiteSpace(values.Sphere)
            && string.IsNullOrWhiteSpace(values.Cylinder)
            && string.IsNullOrWhiteSpace(values.Axis))
        {
            return null;
        }

        var line = $"{eye}.:S={_formatter.FormatDiopter(values.Sphere)} Z={_formatter.FormatDiopter(values.Cylinder)}*{_formatter.FormatAxis(values.Axis)}";
        if (!string.IsNullOrWhiteSpace(values.Prism))
        {
            line += $" P={values.Prism}";
        }

        if (!string.IsNullOrWhiteSpace(values.Pd))
        {
            line += $" PD= {_formatter.FormatPd(values.Pd)}";
        }

        if (!string.IsNullOrWhiteSpace(values.Add))
        {
            line += $" A={_formatter.FormatDiopter(values.Add)}";
        }

        return line;
    }

    private string? BuildRefLine(string eye, RefValues values, string? pd, string? vd)
    {
        if (string.IsNullOrWhiteSpace(values.Sphere)
            && string.IsNullOrWhiteSpace(values.Cylinder)
            && string.IsNullOrWhiteSpace(values.Axis))
        {
            return null;
        }

        var line = $"{eye}.:S={_formatter.FormatDiopter(values.Sphere)} Z={_formatter.FormatDiopter(values.Cylinder)}*{_formatter.FormatAxis(values.Axis)}";
        if (!string.IsNullOrWhiteSpace(pd))
        {
            line += $" PD= {_formatter.FormatPd(pd)}";
        }

        if (!string.IsNullOrWhiteSpace(vd))
        {
            line += $" VD= {_formatter.FormatRaw(vd)}";
        }

        return line;
    }

    private static string? BuildKmRadiiLine(IReadOnlyDictionary<string, KmValues> eyes)
    {
        var parts = new List<string>();
        foreach (var eye in new[] { "R", "L" })
        {
            if (!eyes.TryGetValue(eye, out var values)
                || string.IsNullOrWhiteSpace(values.Radius1)
                || string.IsNullOrWhiteSpace(values.Radius2))
            {
                continue;
            }

            parts.Add($"{eye}: R1={values.Radius1} *{FormatAxis(values.Radius1Axis)} R2={values.Radius2} *{FormatAxis(values.Radius2Axis)}");
        }

        return parts.Count == 0 ? null : string.Join(" // ", parts);
    }

    private static string? BuildKmCylinderLine(IReadOnlyDictionary<string, KmValues> eyes)
    {
        var parts = new List<string>();
        foreach (var eye in new[] { "R", "L" })
        {
            if (!eyes.TryGetValue(eye, out var values) || string.IsNullOrWhiteSpace(values.Cylinder))
            {
                continue;
            }

            parts.Add($"{eye}: CYL={values.Cylinder} {FormatAxis(values.Radius1Axis)}");
        }

        return parts.Count == 0 ? null : string.Join(" // ", parts);
    }

    private static string? BuildTonoLine(TonoValues values)
    {
        var right = BuildTonoEye("R", values.RightValues, values.RightAverage);
        var left = BuildTonoEye("L", values.LeftValues, values.LeftAverage);
        var parts = new[] { right, left }.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray();
        return parts.Length == 0 ? null : $"{string.Join(" // ", parts)} {values.Unit}";
    }

    private static string? BuildTonoEye(string eye, IReadOnlyList<string> values, string? average)
    {
        var parts = values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(FormatPlainNumber).ToList();
        if (!string.IsNullOrWhiteSpace(average))
        {
            parts.Add($"[{FormatOneDecimal(average)}]");
        }

        return parts.Count == 0 ? null : $"{eye} = {string.Join(" ", parts)}";
    }

    private static IEnumerable<string> SplitLines(string text)
    {
        return text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
    }

    private static string? ReadLeadingEye(string line)
    {
        if (line.Length < 2 || !char.IsWhiteSpace(line[1]))
        {
            return null;
        }

        return char.ToUpperInvariant(line[0]) switch
        {
            'R' => "R",
            'L' => "L",
            _ => null
        };
    }

    private static string? ReadNamedValue(string line, string name)
    {
        var match = Regex.Match(
            line,
            $@"(?<![A-Za-z0-9]){Regex.Escape(name)}\s*[:=]?\s*(?<value>[+-]?\s*\d+(?:[\.,]\d+)?)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return match.Success ? match.Groups["value"].Value : null;
    }

    private static string? ReadTokenAfter(IReadOnlyList<string> tokens, string token)
    {
        for (var index = 0; index < tokens.Count - 1; index++)
        {
            if (string.Equals(tokens[index], token, StringComparison.OrdinalIgnoreCase))
            {
                return tokens[index + 1];
            }
        }

        return null;
    }

    private static IEnumerable<string?> ReadTokenValuesAfter(IReadOnlyList<string> tokens, string token)
    {
        for (var index = 0; index < tokens.Count - 1; index++)
        {
            if (string.Equals(tokens[index], token, StringComparison.OrdinalIgnoreCase))
            {
                yield return tokens[index + 1];
            }
        }
    }

    private static string? ReadTrailingNumber(string line)
    {
        return ReadNumbers(line).LastOrDefault();
    }

    private static string? ReadColonValue(string line)
    {
        var index = line.IndexOf(':', StringComparison.Ordinal);
        return index >= 0 && index < line.Length - 1 ? line[(index + 1)..] : null;
    }

    private static string? ReadBracketValue(string text, string fallback)
    {
        var match = Regex.Match(text, @"\[(?<value>[^\]]+)\]", RegexOptions.CultureInvariant);
        return match.Success ? match.Groups["value"].Value.Trim() : fallback;
    }

    private static IEnumerable<string> ReadNumbers(string text)
    {
        foreach (Match match in Regex.Matches(text, @"[+-]?\s*\d+(?:[\.,]\d+)?", RegexOptions.CultureInvariant))
        {
            yield return match.Value;
        }
    }

    private static string? NormalizeSignedDecimal(string? value)
    {
        var normalized = value?.Trim().Replace(" ", string.Empty).Replace(',', '.') ?? string.Empty;
        if (normalized.Length == 0)
        {
            return null;
        }

        if (!decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number))
        {
            return normalized;
        }

        var sign = number < 0 ? "-" : "+";
        return sign + Math.Abs(number).ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string? NormalizeUnsignedDecimal(string? value)
    {
        var normalized = value?.Trim().Replace(" ", string.Empty).Replace(',', '.') ?? string.Empty;
        if (normalized.Length == 0)
        {
            return null;
        }

        if (!decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number))
        {
            return normalized;
        }

        return Math.Abs(number).ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string? NormalizeAxis(string? value)
    {
        var normalized = value?.Trim().TrimStart('+') ?? string.Empty;
        return normalized.Length == 0
            ? null
            : int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var axis)
                ? axis.ToString(CultureInfo.InvariantCulture)
                : normalized;
    }

    private static string? NormalizePrism(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : Regex.Replace(value.Trim().Replace(',', '.'), @"\s+", " ");
    }

    private static string FormatAxis(string? value)
    {
        return (NormalizeAxis(value) ?? string.Empty).PadLeft(3);
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

    private static void EnsureLens(IDictionary<string, LensValues> eyes, string eye)
    {
        if (!eyes.ContainsKey(eye))
        {
            eyes[eye] = new LensValues(eye);
        }
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

    private sealed record RefValues(string Eye, string? Sphere, string? Cylinder, string? Axis);
    private sealed record LensValues(string Eye, string? Sphere = null, string? Cylinder = null, string? Axis = null, string? Add = null, string? Prism = null, string? Pd = null);
    private sealed record KmValues(string Eye, string? Radius1, string? Radius1Axis, string? Radius2, string? Radius2Axis, string? Cylinder);

    private sealed class TonoValues
    {
        public List<string> RightValues { get; } = new();
        public List<string> LeftValues { get; } = new();
        public string? RightAverage { get; set; }
        public string? LeftAverage { get; set; }
        public string Unit { get; set; } = "mmHg";
    }
}
