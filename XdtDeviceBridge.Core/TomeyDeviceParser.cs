using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace XdtDeviceBridge.Core;

public sealed class TomeyDeviceParser
{
    public const string ParserMode = "Tomey";

    private static readonly Regex SignedNumberRegex = new(
        @"[+-]\s*\d+(?:[\.,]\d+)?",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

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
            return CreateError("TOMEY-Rohdaten sind leer.", sourcePath);
        }

        var trimmed = text.TrimStart('\uFEFF', '\u0001', '\u0002', '\u0004', '\u0017', '\r', '\n', ' ', '\t');
        if (trimmed.StartsWith("<", StringComparison.Ordinal))
        {
            return ParseXml(text, sourcePath);
        }

        if (text.Contains("[POWER_R]", StringComparison.OrdinalIgnoreCase)
            || text.Contains("[POWER_L]", StringComparison.OrdinalIgnoreCase))
        {
            return ParseTlCsv(text, sourcePath);
        }

        if (LooksLikeCf2000(text))
        {
            return ParseCf2000(text, sourcePath);
        }

        return new DeviceParseResult(
            Array.Empty<MeasurementValue>(),
            new[]
            {
                new DeviceParseIssue(
                    DeviceParseIssueSeverity.Error,
                    "TOMEY-Rohdaten konnten keinem unterstützten TOMEY-Format zugeordnet werden.",
                    sourcePath ?? string.Empty,
                    null)
            });
    }

    private DeviceParseResult ParseCf2000(string rawText, string? sourcePath)
    {
        var measurements = CreateCommonMeasurements("CF-2000");
        var issues = new List<DeviceParseIssue>();
        var lines = SplitControlLines(rawText);
        var eyes = new Dictionary<string, LensValues>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            if (line.Length < 2)
            {
                continue;
            }

            var token = line[..2].ToUpperInvariant();
            var payload = line.Length > 2 ? line[2..] : string.Empty;
            switch (token)
            {
                case "LR":
                    SetLensPower(eyes, "R", payload);
                    break;
                case "LL":
                    SetLensPower(eyes, "L", payload);
                    break;
                case "AR":
                    SetLensAdd(eyes, "R", payload);
                    break;
                case "AL":
                    SetLensAdd(eyes, "L", payload);
                    break;
                case "PR":
                    SetLensPrism(eyes, "R", payload);
                    break;
                case "PL":
                    SetLensPrism(eyes, "L", payload);
                    break;
            }
        }

        AddLensMeasurements(measurements, eyes);
        AddLensMedistarLines(measurements, eyes);
        AddNoExportableWarningIfNeeded(measurements, issues, "TOMEY CF-2000-Rohdaten wurden gelesen, aber keine exportierbaren Lensmeter-Werte erkannt.");
        return new DeviceParseResult(measurements, issues);
    }

    private DeviceParseResult ParseTlCsv(string rawText, string? sourcePath)
    {
        var model = DetectModelFromPath(sourcePath, "TL-2000C");
        var measurements = CreateCommonMeasurements(model);
        var issues = new List<DeviceParseIssue>();
        var tokens = ReadTokenLines(rawText);
        var eyes = new Dictionary<string, LensValues>(StringComparer.OrdinalIgnoreCase);

        ReadTlPower(tokens, eyes, "R");
        ReadTlPower(tokens, eyes, "L");
        ReadTlAdd(tokens, eyes, "R");
        ReadTlAdd(tokens, eyes, "L");
        ReadTlPrism(tokens, eyes, "R");
        ReadTlPrism(tokens, eyes, "L");
        ReadTlPd(tokens, eyes);

        AddLensMeasurements(measurements, eyes);
        AddLensMedistarLines(measurements, eyes);
        AddNoExportableWarningIfNeeded(measurements, issues, $"TOMEY {model}-Rohdaten wurden gelesen, aber keine exportierbaren Lensmeter-Werte erkannt.");
        return new DeviceParseResult(measurements, issues);
    }

    private DeviceParseResult ParseXml(string rawText, string? sourcePath)
    {
        try
        {
            var document = XDocument.Parse(rawText, LoadOptions.PreserveWhitespace);
            if (ContainsElement(document, "Measurement") && ContainsElement(document, "Eye"))
            {
                return ParseTop1000Xml(document, sourcePath);
            }

            return ParseMr6000Xml(document, sourcePath);
        }
        catch (Exception ex) when (ex is System.Xml.XmlException or InvalidOperationException)
        {
            return new DeviceParseResult(
                Array.Empty<MeasurementValue>(),
                new[]
                {
                    new DeviceParseIssue(
                        DeviceParseIssueSeverity.Error,
                        $"TOMEY-XML konnte nicht gelesen werden: {ex.Message}",
                        sourcePath ?? string.Empty,
                        null)
                });
        }
    }

    private DeviceParseResult ParseMr6000Xml(XDocument document, string? sourcePath)
    {
        var model = ReadCommonValue(document, "ModelName") ?? DetectModelFromPath(sourcePath, "MR-6000");
        var measurements = CreateCommonMeasurements(model);
        var issues = new List<DeviceParseIssue>();

        ParseMrRef(document, measurements);
        ParseMrKm(document, measurements);
        ParseMrTono(document, measurements);
        ParseMrPachy(document, measurements);

        AddNoExportableWarningIfNeeded(measurements, issues, $"TOMEY {model}-XML wurde gelesen, aber keine exportierbaren REF/KM/Tono/Pachy-Werte erkannt.");
        return new DeviceParseResult(measurements, issues);
    }

    private DeviceParseResult ParseTop1000Xml(XDocument document, string? sourcePath)
    {
        var measurements = CreateCommonMeasurements(DetectModelFromPath(sourcePath, "TOP-1000"));
        var issues = new List<DeviceParseIssue>();
        var right = ReadTopEye(document, "OD");
        var left = ReadTopEye(document, "OS");

        AddSeriesMeasurements(measurements, "TM", "Tono", "Tonometrie", "R", right.IopValues, right.IopAverage, "mmHg");
        AddSeriesMeasurements(measurements, "TM", "Tono", "Tonometrie", "L", left.IopValues, left.IopAverage, "mmHg");
        AddOptional(measurements, "Measure[@Type='TM']/Tono/R/Corrected", "Tonometrie R korrigiert", right.CorrectedIop, "mmHg", "R", "TM");
        AddOptional(measurements, "Measure[@Type='TM']/Tono/L/Corrected", "Tonometrie L korrigiert", left.CorrectedIop, "mmHg", "L", "TM");
        AddOptional(measurements, "Measure[@Type='CCT']/Pachy/R/Average", "Pachymetrie R Mittelwert", right.Cct, "um", "R", "CCT");
        AddOptional(measurements, "Measure[@Type='CCT']/Pachy/L/Average", "Pachymetrie L Mittelwert", left.Cct, "um", "L", "CCT");

        var tonoLine = BuildTonoLine(right.IopValues, right.IopAverage, left.IopValues, left.IopAverage);
        AddOptional(measurements, "Measure[@Type='TM']/Tono/TonoListLine", "MEDISTAR Tonometrie-Zeile", tonoLine, null, null, "TM");

        var correctedLine = BuildCorrectedTonoLine(right, left);
        AddOptional(measurements, "Measure[@Type='TM']/Tono/CorrectedLine", "MEDISTAR Tonometrie-Korrektur-Zeile", correctedLine, null, null, "TM");

        var pachyLine = BuildPachyLine(right.Cct, left.Cct);
        AddOptional(measurements, "Measure[@Type='CCT']/Pachy/MedistarLine", "MEDISTAR Pachymetrie-Zeile", pachyLine, null, null, "CCT");

        AddNoExportableWarningIfNeeded(measurements, issues, "TOMEY TOP-1000-XML wurde gelesen, aber keine exportierbaren Tono/Pachy-Werte erkannt.");
        return new DeviceParseResult(measurements, issues);
    }

    private void ParseMrRef(XContainer document, List<MeasurementValue> measurements)
    {
        var measure = FindMeasure(document, "REF");
        if (measure is null)
        {
            return;
        }

        var right = ReadRefEye(measure, "R");
        var left = ReadRefEye(measure, "L");
        var vd = ReadFirstDescendantValue(measure, "VD");
        var pd = ReadFirstDescendantValue(measure, "Distance");

        AddRefMeasurements(measurements, "R", right, vd, pd);
        AddRefMeasurements(measurements, "L", left, vd, null);
        AddOptional(measurements, "Measure[@Type='REF']/REF/R/MedistarLine", "MEDISTAR REF R-Zeile", BuildRefLine("R", right, pd, vd), null, "R", "REF");
        AddOptional(measurements, "Measure[@Type='REF']/REF/L/MedistarLine", "MEDISTAR REF L-Zeile", BuildRefLine("L", left, null, null), null, "L", "REF");
    }

    private void ParseMrKm(XContainer document, List<MeasurementValue> measurements)
    {
        var measure = FindMeasure(document, "KM");
        if (measure is null)
        {
            return;
        }

        var right = ReadKmEye(measure, "R");
        var left = ReadKmEye(measure, "L");
        AddKmMeasurements(measurements, "R", right);
        AddKmMeasurements(measurements, "L", left);
        AddOptional(measurements, "Measure[@Type='KM']/KM/MedistarLine1", "MEDISTAR KM R1/R2-Zeile", BuildKmRadiiLine(right, left), null, null, "KM");
        AddOptional(measurements, "Measure[@Type='KM']/KM/MedistarLine2", "MEDISTAR KM AV/CYL-Zeile", BuildKmAverageLine(right, left), null, null, "KM");
    }

    private void ParseMrTono(XContainer document, List<MeasurementValue> measurements)
    {
        var tmElement = FindMeasure(document, "TM")?
            .Descendants()
            .FirstOrDefault(element => IsLocalName(element, "TM"));
        if (tmElement is null)
        {
            return;
        }

        var right = ReadMrTonoEye(tmElement, "R");
        var left = ReadMrTonoEye(tmElement, "L");
        AddSeriesMeasurements(measurements, "TM", "Tono", "Tonometrie", "R", right.IopValues, right.IopAverage, "mmHg");
        AddSeriesMeasurements(measurements, "TM", "Tono", "Tonometrie", "L", left.IopValues, left.IopAverage, "mmHg");
        AddOptional(measurements, "Measure[@Type='TM']/Tono/TonoListLine", "MEDISTAR Tonometrie-Zeile", BuildTonoLine(right.IopValues, right.IopAverage, left.IopValues, left.IopAverage), null, null, "TM");
        AddOptional(measurements, "Measure[@Type='TM']/Tono/CorrectedLine", "MEDISTAR Tonometrie-Korrektur-Zeile", BuildCorrectedTonoLine(right, left), null, null, "TM");
    }

    private void ParseMrPachy(XContainer document, List<MeasurementValue> measurements)
    {
        var pmElement = FindMeasure(document, "TM")?
            .Descendants()
            .FirstOrDefault(element => IsLocalName(element, "PM"));
        if (pmElement is null)
        {
            return;
        }

        var rightValues = ReadEyeList(pmElement, "R", "CCT");
        var leftValues = ReadEyeList(pmElement, "L", "CCT");
        var rightAverage = ReadEyeAverage(pmElement, "R", "CCT");
        var leftAverage = ReadEyeAverage(pmElement, "L", "CCT");

        AddSeriesMeasurements(measurements, "CCT", "Pachy", "Pachymetrie", "R", rightValues, rightAverage, "um");
        AddSeriesMeasurements(measurements, "CCT", "Pachy", "Pachymetrie", "L", leftValues, leftAverage, "um");
        AddOptional(measurements, "Measure[@Type='CCT']/Pachy/MedistarLine", "MEDISTAR Pachymetrie-Zeile", BuildPachyLine(rightAverage ?? rightValues.FirstOrDefault(), leftAverage ?? leftValues.FirstOrDefault()), null, null, "CCT");
    }

    private static IReadOnlyList<string> SplitControlLines(string rawText)
    {
        return rawText
            .Replace('\u0001', '\r')
            .Replace('\u0002', '\r')
            .Replace('\u0004', '\r')
            .Replace('\u0017', '\r')
            .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToArray();
    }

    private static Dictionary<string, string[]> ReadTokenLines(string rawText)
    {
        var result = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var rawLine in rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            var parts = line.Split(',').Select(part => part.Trim()).ToArray();
            if (parts.Length > 0)
            {
                result[parts[0]] = parts.Skip(1).ToArray();
            }
        }

        return result;
    }

    private static bool LooksLikeCf2000(string rawText)
    {
        return SplitControlLines(rawText).Any(line =>
            line.StartsWith("LR", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("LL", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("AR", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("AL", StringComparison.OrdinalIgnoreCase));
    }

    private static void SetLensPower(Dictionary<string, LensValues> eyes, string eye, string payload)
    {
        var values = ParseFixedPower(payload);
        eyes[eye] = GetLensValues(eyes, eye) with
        {
            Eye = eye,
            Sphere = values.Sphere,
            Cylinder = values.Cylinder,
            Axis = values.Axis
        };
    }

    private static void SetLensAdd(Dictionary<string, LensValues> eyes, string eye, string payload)
    {
        var addValues = SignedNumberRegex.Matches(payload).Select(match => NormalizeSignedDecimal(match.Value)).ToArray();
        eyes[eye] = GetLensValues(eyes, eye) with
        {
            Eye = eye,
            Add = addValues.ElementAtOrDefault(0),
            Add2 = addValues.ElementAtOrDefault(1)
        };
    }

    private static void SetLensPrism(Dictionary<string, LensValues> eyes, string eye, string payload)
    {
        var prismValues = SignedNumberRegex.Matches(payload).Select(match => NormalizeSignedDecimal(match.Value)).ToArray();
        eyes[eye] = GetLensValues(eyes, eye) with
        {
            Eye = eye,
            PrismHorizontal = prismValues.ElementAtOrDefault(0),
            PrismVertical = prismValues.ElementAtOrDefault(1)
        };
    }

    private static void ReadTlPower(IReadOnlyDictionary<string, string[]> tokens, Dictionary<string, LensValues> eyes, string eye)
    {
        if (!tokens.TryGetValue($"[POWER_{eye}]", out var values))
        {
            return;
        }

        eyes[eye] = GetLensValues(eyes, eye) with
        {
            Eye = eye,
            Sphere = NormalizeSignedDecimal(values.ElementAtOrDefault(0)),
            Cylinder = NormalizeSignedDecimal(values.ElementAtOrDefault(1)),
            Axis = NormalizeAxis(values.ElementAtOrDefault(2))
        };
    }

    private static void ReadTlAdd(IReadOnlyDictionary<string, string[]> tokens, Dictionary<string, LensValues> eyes, string eye)
    {
        if (!tokens.TryGetValue($"[ADD_{eye}]", out var values))
        {
            return;
        }

        eyes[eye] = GetLensValues(eyes, eye) with
        {
            Eye = eye,
            Add = NormalizeSignedDecimal(values.ElementAtOrDefault(0)),
            Add2 = NormalizeSignedDecimal(values.ElementAtOrDefault(1))
        };
    }

    private static void ReadTlPrism(IReadOnlyDictionary<string, string[]> tokens, Dictionary<string, LensValues> eyes, string eye)
    {
        if (!tokens.TryGetValue($"[PRISM_SEL_{eye}]", out var selected)
            || !string.Equals(selected.ElementAtOrDefault(0), "1", StringComparison.OrdinalIgnoreCase)
            || !tokens.TryGetValue($"[PRISM_{eye}]", out var values))
        {
            return;
        }

        eyes[eye] = GetLensValues(eyes, eye) with
        {
            Eye = eye,
            PrismHorizontal = NormalizeSignedDecimal(values.ElementAtOrDefault(0)),
            PrismVertical = NormalizeSignedDecimal(values.ElementAtOrDefault(1))
        };
    }

    private static void ReadTlPd(IReadOnlyDictionary<string, string[]> tokens, Dictionary<string, LensValues> eyes)
    {
        if (!tokens.TryGetValue("[PD]", out var values))
        {
            return;
        }

        var pd = NormalizeUnsignedDecimal(values.ElementAtOrDefault(0));
        if (string.IsNullOrWhiteSpace(pd))
        {
            return;
        }

        foreach (var eye in new[] { "R", "L" })
        {
            eyes[eye] = GetLensValues(eyes, eye) with { Eye = eye, Pd = pd };
        }
    }

    private static LensValues ParseFixedPower(string payload)
    {
        var text = payload.Trim();
        if (text.Length >= 15)
        {
            return new LensValues(
                Eye: string.Empty,
                Sphere: NormalizeSignedDecimal(text[..6]),
                Cylinder: NormalizeSignedDecimal(text.Substring(6, 6)),
                Axis: NormalizeAxis(text.Substring(12, Math.Min(3, text.Length - 12))),
                Add: null,
                Add2: null,
                Pd: null,
                PrismHorizontal: null,
                PrismVertical: null);
        }

        var values = SignedNumberRegex.Matches(text).Select(match => NormalizeSignedDecimal(match.Value)).ToArray();
        var axisMatch = Regex.Match(text, @"(?<axis>\d{1,3})\s*$", RegexOptions.CultureInvariant);
        return new LensValues(
            Eye: string.Empty,
            Sphere: values.ElementAtOrDefault(0),
            Cylinder: values.ElementAtOrDefault(1),
            Axis: axisMatch.Success ? NormalizeAxis(axisMatch.Groups["axis"].Value) : null,
            Add: null,
            Add2: null,
            Pd: null,
            PrismHorizontal: null,
            PrismVertical: null);
    }

    private static LensValues GetLensValues(IReadOnlyDictionary<string, LensValues> eyes, string eye)
    {
        return eyes.TryGetValue(eye, out var values) ? values : new LensValues(Eye: eye);
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
            AddOptional(measurements, $"{prefix}/Sphere", $"LM {eye} Sphere", values.Sphere, "dpt", eye, "LM");
            AddOptional(measurements, $"{prefix}/Cylinder", $"LM {eye} Cylinder", values.Cylinder, "dpt", eye, "LM");
            AddOptional(measurements, $"{prefix}/Axis", $"LM {eye} Axis", values.Axis, "deg", eye, "LM");
            AddOptional(measurements, $"{prefix}/ADD", $"LM {eye} ADD", values.Add, "dpt", eye, "LM");
            AddOptional(measurements, $"{prefix}/ADD2", $"LM {eye} ADD2", values.Add2, "dpt", eye, "LM");
            AddOptional(measurements, $"{prefix}/PD", $"LM {eye} PD", values.Pd, "mm", eye, "LM");
            AddOptional(measurements, $"{prefix}/PrismHorizontal", $"LM {eye} Prisma horizontal", values.PrismHorizontal, "prism dpt", eye, "LM");
            AddOptional(measurements, $"{prefix}/PrismVertical", $"LM {eye} Prisma vertikal", values.PrismVertical, "prism dpt", eye, "LM");
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

            AddOptional(measurements, $"Measure[@Type='LM']/LM/{eye}/MedistarLine", $"MEDISTAR LM {eye}-Zeile", BuildLensLine(eye, values), null, eye, "LM");
        }
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
        var prism = BuildPrismSegment(values.PrismHorizontal, values.PrismVertical);
        if (!string.IsNullOrWhiteSpace(prism))
        {
            line += $" {prism}";
        }

        if (!string.IsNullOrWhiteSpace(values.Pd) && eye == "R")
        {
            line += $" PD= {_formatter.FormatPd(values.Pd)}";
        }

        if (!string.IsNullOrWhiteSpace(values.Add))
        {
            line += $" A={_formatter.FormatDiopter(values.Add)}";
        }

        if (!string.IsNullOrWhiteSpace(values.Add2))
        {
            line += $" A2={_formatter.FormatDiopter(values.Add2)}";
        }

        return line;
    }

    private static string? BuildPrismSegment(string? horizontal, string? vertical)
    {
        var parts = new List<string>();
        if (TryParseDecimal(horizontal, out var horizontalValue) && horizontalValue != 0)
        {
            parts.Add($"P={Math.Abs(horizontalValue).ToString("0.00", CultureInfo.InvariantCulture)} {(horizontalValue > 0 ? "OUT" : "IN")}");
        }

        if (TryParseDecimal(vertical, out var verticalValue) && verticalValue != 0)
        {
            parts.Add($"{Math.Abs(verticalValue).ToString("0.00", CultureInfo.InvariantCulture)} {(verticalValue > 0 ? "UP" : "DOWN")}");
        }

        return parts.Count == 0 ? null : string.Join(' ', parts);
    }

    private void AddRefMeasurements(List<MeasurementValue> measurements, string eye, RefValues values, string? vd, string? pd)
    {
        var prefix = $"Measure[@Type='REF']/REF/{eye}";
        AddOptional(measurements, $"{prefix}/Sphere", $"REF {eye} Sphere", values.Sphere, "dpt", eye, "REF");
        AddOptional(measurements, $"{prefix}/Cylinder", $"REF {eye} Cylinder", values.Cylinder, "dpt", eye, "REF");
        AddOptional(measurements, $"{prefix}/Axis", $"REF {eye} Axis", values.Axis, "deg", eye, "REF");
        AddOptional(measurements, $"{prefix}/PD", $"REF {eye} PD", pd, "mm", eye, "REF");
        AddOptional(measurements, "Measure[@Type='REF']/REF/VD", "REF VD", vd, "mm", null, "REF");
    }

    private void AddKmMeasurements(List<MeasurementValue> measurements, string eye, KmValues values)
    {
        var prefix = $"Measure[@Type='KM']/KM/{eye}";
        AddOptional(measurements, $"{prefix}/R1/Radius", $"KM {eye} R1 Radius", values.R1Radius, "mm", eye, "KM");
        AddOptional(measurements, $"{prefix}/R1/Power", $"KM {eye} R1 Power", values.R1Power, "dpt", eye, "KM");
        AddOptional(measurements, $"{prefix}/R1/Axis", $"KM {eye} R1 Axis", values.R1Axis, "deg", eye, "KM");
        AddOptional(measurements, $"{prefix}/R2/Radius", $"KM {eye} R2 Radius", values.R2Radius, "mm", eye, "KM");
        AddOptional(measurements, $"{prefix}/R2/Power", $"KM {eye} R2 Power", values.R2Power, "dpt", eye, "KM");
        AddOptional(measurements, $"{prefix}/R2/Axis", $"KM {eye} R2 Axis", values.R2Axis, "deg", eye, "KM");
        AddOptional(measurements, $"{prefix}/Average/Radius", $"KM {eye} AV Radius", values.AverageRadius, "mm", eye, "KM");
        AddOptional(measurements, $"{prefix}/Average/Power", $"KM {eye} AV Power", values.AveragePower, "dpt", eye, "KM");
        AddOptional(measurements, $"{prefix}/Cylinder/Power", $"KM {eye} CYL", values.CylinderPower, "dpt", eye, "KM");
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

    private static string? BuildKmRadiiLine(KmValues right, KmValues left)
    {
        var parts = new List<string>();
        var rightPart = BuildKmRadiiEye("R", right);
        var leftPart = BuildKmRadiiEye("L", left);
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

    private static string? BuildKmRadiiEye(string eye, KmValues values)
    {
        if (string.IsNullOrWhiteSpace(values.R1Radius) && string.IsNullOrWhiteSpace(values.R2Radius))
        {
            return null;
        }

        return $"{eye}: R1={values.R1Radius} {values.R1Power} *{FormatAxisForInline(values.R1Axis)} R2={values.R2Radius} {values.R2Power} *{FormatAxisForInline(values.R2Axis)}";
    }

    private static string? BuildKmAverageLine(KmValues right, KmValues left)
    {
        var parts = new List<string>();
        var rightPart = BuildKmAverageEye("R", right);
        var leftPart = BuildKmAverageEye("L", left);
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

    private static string? BuildKmAverageEye(string eye, KmValues values)
    {
        if (string.IsNullOrWhiteSpace(values.AverageRadius) && string.IsNullOrWhiteSpace(values.CylinderPower))
        {
            return null;
        }

        return $"{eye}: AV={values.AverageRadius} {values.AveragePower} CYL={NormalizeSignedDecimal(values.CylinderPower)} {FormatAxisForInline(values.R1Axis)}";
    }

    private static string? BuildTonoLine(IReadOnlyList<string> rightValues, string? rightAverage, IReadOnlyList<string> leftValues, string? leftAverage)
    {
        var parts = new List<string>();
        var right = BuildTonoEye("R", rightValues, rightAverage);
        var left = BuildTonoEye("L", leftValues, leftAverage);
        if (!string.IsNullOrWhiteSpace(right))
        {
            parts.Add(right);
        }

        if (!string.IsNullOrWhiteSpace(left))
        {
            parts.Add(left);
        }

        return parts.Count == 0 ? null : $"{string.Join(" // ", parts)} mmHg";
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

    private static string? BuildCorrectedTonoLine(TonoValues right, TonoValues left)
    {
        var parts = new List<string>();
        var rightPart = BuildCorrectedTonoEye("PR", right);
        var leftPart = BuildCorrectedTonoEye("PL", left);
        if (!string.IsNullOrWhiteSpace(rightPart))
        {
            parts.Add(rightPart);
        }

        if (!string.IsNullOrWhiteSpace(leftPart))
        {
            parts.Add(leftPart);
        }

        return parts.Count == 0 ? null : string.Join("  ", parts);
    }

    private static string? BuildCorrectedTonoEye(string eye, TonoValues values)
    {
        if (string.IsNullOrWhiteSpace(values.MeasuredIop)
            && string.IsNullOrWhiteSpace(values.CorrectedIop)
            && string.IsNullOrWhiteSpace(values.Cct))
        {
            return null;
        }

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(values.MeasuredIop))
        {
            parts.Add($"Gemessen = {FormatOneDecimal(values.MeasuredIop)} mmHg");
        }

        if (!string.IsNullOrWhiteSpace(values.CorrectedIop))
        {
            parts.Add($"Korrigiert = {FormatOneDecimal(values.CorrectedIop)} mmHg");
        }

        if (!string.IsNullOrWhiteSpace(values.Cct))
        {
            parts.Add($"CCT = {FormatPlainNumber(values.Cct)}um");
        }

        return $"{eye}: {string.Join("; ", parts)}";
    }

    private static string? BuildPachyLine(string? right, string? left)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(right))
        {
            parts.Add($"RA: {FormatPachyMillimeters(right)}");
        }

        if (!string.IsNullOrWhiteSpace(left))
        {
            parts.Add($"LA: {FormatPachyMillimeters(left)}");
        }

        return parts.Count == 0 ? null : string.Join(" // ", parts);
    }

    private static RefValues ReadRefEye(XElement measure, string eye)
    {
        var eyeElement = FindEyeElement(measure, "REF", eye);
        var median = eyeElement?.Descendants().FirstOrDefault(element => IsLocalName(element, "Median")) ?? eyeElement;
        return new RefValues(
            NormalizeSignedDecimal(ReadFirstDescendantValue(median, "Sphere")),
            NormalizeSignedDecimal(ReadFirstDescendantValue(median, "Cylinder")),
            NormalizeAxis(ReadFirstDescendantValue(median, "Axis")));
    }

    private static KmValues ReadKmEye(XElement measure, string eye)
    {
        var eyeElement = FindEyeElement(measure, "KM", eye);
        var median = eyeElement?.Descendants().FirstOrDefault(element => IsLocalName(element, "Median")) ?? eyeElement;
        return new KmValues(
            ReadChildValue(ReadChild(median, "R1"), "Radius"),
            ReadChildValue(ReadChild(median, "R1"), "Power"),
            NormalizeAxis(ReadChildValue(ReadChild(median, "R1"), "Axis")),
            ReadChildValue(ReadChild(median, "R2"), "Radius"),
            ReadChildValue(ReadChild(median, "R2"), "Power"),
            NormalizeAxis(ReadChildValue(ReadChild(median, "R2"), "Axis")),
            ReadChildValue(ReadChild(median, "Average"), "Radius"),
            ReadChildValue(ReadChild(median, "Average"), "Power"),
            ReadChildValue(ReadChild(median, "Cylinder"), "Power"));
    }

    private static TonoValues ReadMrTonoEye(XElement tmElement, string eye)
    {
        var values = ReadEyeList(tmElement, eye, "IOP_mmHg");
        var average = ReadEyeAverage(tmElement, eye, "IOP_mmHg");
        var formulaEye = tmElement
            .Descendants()
            .Where(element => IsLocalName(element, eye))
            .LastOrDefault(element => element.Ancestors().Any(ancestor => IsLocalName(ancestor, "CorrectedIOP")));

        return new TonoValues(
            values,
            average,
            ReadFirstDescendantValue(formulaEye, "Measured"),
            ReadFirstDescendantValue(formulaEye, "Corrected"),
            ReadFirstDescendantValue(formulaEye, "CCT"));
    }

    private static TonoValues ReadTopEye(XContainer document, string eyeType)
    {
        var eyeElement = document
            .Descendants()
            .FirstOrDefault(element => IsLocalName(element, "Eye")
                && string.Equals(AttributeValue(element, "type"), eyeType, StringComparison.OrdinalIgnoreCase));
        var values = eyeElement?
            .Elements()
            .Where(element => IsLocalName(element, "IOP"))
            .Select(element => element.Value.Trim())
            .Where(value => value.Length > 0)
            .ToArray()
            ?? Array.Empty<string>();

        return new TonoValues(
            values,
            ReadChildValue(eyeElement, "IOPAvg"),
            ReadChildValue(eyeElement, "IOPAvg"),
            ReadChildValue(eyeElement, "CIOP"),
            ReadChildValue(eyeElement, "CCT"));
    }

    private static IReadOnlyList<string> ReadEyeList(XElement parent, string eye, string valueLocalName)
    {
        var eyeElement = FindEyeElement(parent, null, eye);
        var list = eyeElement?.Descendants().FirstOrDefault(element => IsLocalName(element, "List"));
        return list?
            .Descendants()
            .Where(element => IsLocalName(element, valueLocalName))
            .Select(element => element.Value.Trim())
            .Where(value => value.Length > 0)
            .ToArray()
            ?? Array.Empty<string>();
    }

    private static string? ReadEyeAverage(XElement parent, string eye, string valueLocalName)
    {
        var eyeElement = FindEyeElement(parent, null, eye);
        var average = eyeElement?.Descendants().FirstOrDefault(element => IsLocalName(element, "Average"));
        return ReadFirstDescendantValue(average, valueLocalName);
    }

    private static XElement? FindMeasure(XContainer document, string type)
    {
        return document
            .Descendants()
            .FirstOrDefault(element => IsLocalName(element, "Measure")
                && string.Equals(AttributeValue(element, "type"), type, StringComparison.OrdinalIgnoreCase));
    }

    private static XElement? FindEyeElement(XElement? parent, string? groupLocalName, string eye)
    {
        var scope = parent;
        if (!string.IsNullOrWhiteSpace(groupLocalName))
        {
            scope = parent?
                .Descendants()
                .FirstOrDefault(element => IsLocalName(element, groupLocalName));
        }

        return scope?
            .Descendants()
            .FirstOrDefault(element => IsLocalName(element, eye));
    }

    private static string? ReadCommonValue(XContainer document, string name)
    {
        var common = document.Descendants().FirstOrDefault(element => IsLocalName(element, "Common"));
        return ReadChildValue(common, name);
    }

    private static string? ReadChildValue(XElement? parent, string name)
    {
        return parent?
            .Elements()
            .FirstOrDefault(element => IsLocalName(element, name))
            ?.Value
            .Trim();
    }

    private static XElement? ReadChild(XElement? parent, string name)
    {
        return parent?
            .Elements()
            .FirstOrDefault(element => IsLocalName(element, name));
    }

    private static string? ReadFirstDescendantValue(XElement? parent, string name)
    {
        return parent?
            .Descendants()
            .FirstOrDefault(element => IsLocalName(element, name))
            ?.Value
            .Trim();
    }

    private static bool ContainsElement(XContainer document, string localName)
    {
        return document.Descendants().Any(element => IsLocalName(element, localName));
    }

    private static bool IsLocalName(XElement element, string name)
    {
        return string.Equals(element.Name.LocalName, name, StringComparison.OrdinalIgnoreCase);
    }

    private static string? AttributeValue(XElement element, string name)
    {
        return element
            .Attributes()
            .FirstOrDefault(attribute => string.Equals(attribute.Name.LocalName, name, StringComparison.OrdinalIgnoreCase))
            ?.Value;
    }

    private static List<MeasurementValue> CreateCommonMeasurements(string model)
    {
        return new List<MeasurementValue>
        {
            new("Common/Company", "Company", "TOMEY", null, null, "Common"),
            new("Common/ModelName", "ModelName", model, null, null, "Common")
        };
    }

    private static void AddSeriesMeasurements(
        List<MeasurementValue> measurements,
        string measureType,
        string groupPath,
        string groupDisplayName,
        string eye,
        IReadOnlyList<string> values,
        string? average,
        string unit)
    {
        for (var index = 0; index < values.Count; index++)
        {
            AddOptional(measurements, $"Measure[@Type='{measureType}']/{groupPath}/{eye}/Value{index + 1}", $"{groupDisplayName} {eye} Wert {index + 1}", values[index], unit, eye, measureType);
        }

        AddOptional(measurements, $"Measure[@Type='{measureType}']/{groupPath}/{eye}/Average", $"{groupDisplayName} {eye} Mittelwert", average, unit, eye, measureType);
    }

    private static void AddNoExportableWarningIfNeeded(
        IReadOnlyCollection<MeasurementValue> measurements,
        List<DeviceParseIssue> issues,
        string message)
    {
        if (measurements.Any(measurement => !measurement.SourcePath.StartsWith("Common/", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        issues.Add(new DeviceParseIssue(DeviceParseIssueSeverity.Warning, message, string.Empty, null));
    }

    private static void AddOptional(
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

    private static DeviceParseResult CreateError(string message, string? sourcePath)
    {
        return new DeviceParseResult(
            Array.Empty<MeasurementValue>(),
            new[] { new DeviceParseIssue(DeviceParseIssueSeverity.Error, message, sourcePath ?? string.Empty, null) });
    }

    private static string DetectModelFromPath(string? sourcePath, string fallback)
    {
        var fileName = Path.GetFileName(sourcePath ?? string.Empty);
        foreach (var model in new[] { "CF-2000", "TL-2000C", "TL-6000", "TL-7000", "MR-6000", "TOP-1000" })
        {
            if (fileName.Contains(model, StringComparison.OrdinalIgnoreCase)
                || fileName.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase).Contains(model.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase))
            {
                return model;
            }
        }

        return fallback;
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
        if (normalized.Length == 0)
        {
            return null;
        }

        return int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var axis)
            ? axis.ToString(CultureInfo.InvariantCulture)
            : normalized;
    }

    private static string FormatAxisForInline(string? value)
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

    private static bool TryParseDecimal(string? value, out decimal number)
    {
        return decimal.TryParse(
            value?.Trim().Replace(" ", string.Empty).Replace(',', '.'),
            NumberStyles.Number | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out number);
    }

    private sealed record LensValues(
        string Eye = "",
        string? Sphere = null,
        string? Cylinder = null,
        string? Axis = null,
        string? Add = null,
        string? Add2 = null,
        string? Pd = null,
        string? PrismHorizontal = null,
        string? PrismVertical = null);

    private sealed record RefValues(string? Sphere, string? Cylinder, string? Axis);

    private sealed record KmValues(
        string? R1Radius,
        string? R1Power,
        string? R1Axis,
        string? R2Radius,
        string? R2Power,
        string? R2Axis,
        string? AverageRadius,
        string? AveragePower,
        string? CylinderPower);

    private sealed record TonoValues(
        IReadOnlyList<string> IopValues,
        string? IopAverage,
        string? MeasuredIop,
        string? CorrectedIop,
        string? Cct);
}
