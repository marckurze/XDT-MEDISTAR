using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace XdtDeviceBridge.Core;

public sealed class CanonZeissVisionixDeviceParser
{
    public const string ParserMode = "CanonZeissVisionix";

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
                new[] { new DeviceParseIssue(DeviceParseIssueSeverity.Error, "Canon/ZEISS/Visionix-Rohdaten sind leer.", sourcePath ?? string.Empty, null) });
        }

        var trimmed = text.TrimStart('\uFEFF', '\u0001', '\u0002', '\u0004', '\u0017', '\r', '\n', ' ', '\t');
        if (trimmed.StartsWith("<", StringComparison.Ordinal))
        {
            return ParseXml(text, sourcePath);
        }

        return ParseReferenceText(text, sourcePath);
    }

    private DeviceParseResult ParseXml(string text, string? sourcePath)
    {
        XDocument document;
        try
        {
            document = XDocument.Parse(text, LoadOptions.PreserveWhitespace);
        }
        catch (Exception ex) when (ex is System.Xml.XmlException or InvalidOperationException)
        {
            return new DeviceParseResult(
                Array.Empty<MeasurementValue>(),
                new[] { new DeviceParseIssue(DeviceParseIssueSeverity.Error, $"Canon/ZEISS/Visionix-XML konnte nicht gelesen werden: {ex.Message}", sourcePath ?? string.Empty, null) });
        }

        var model = DetectXmlModel(document, text, sourcePath);
        var company = DetectXmlCompany(document, text, sourcePath, model);
        var measurements = CreateCommonMeasurements(company, model);
        var issues = new List<DeviceParseIssue>();

        if (IsVisionixXml(document, text, sourcePath))
        {
            ParseVisionixOpticXml(document, measurements, model);
        }
        else
        {
            ParseJoiaRef(document, measurements);
            ParseJoiaLm(document, measurements);
            ParseJoiaTm(document, measurements);
            ParseJoiaPachy(document, measurements);
        }

        AddNoExportableWarningIfNeeded(measurements, issues, $"{company} {model}-Daten wurden gelesen, aber keine exportierbaren REF/LM/KM/Tono/Pachy-Werte erkannt.", sourcePath);
        return new DeviceParseResult(measurements, issues);
    }

    private DeviceParseResult ParseReferenceText(string text, string? sourcePath)
    {
        var model = DetectTextModel(text, sourcePath);
        var company = DetectTextCompany(text, sourcePath, model);
        var measurements = CreateCommonMeasurements(company, model);
        var issues = new List<DeviceParseIssue>();

        var refEyes = new Dictionary<string, RefValues>(StringComparer.OrdinalIgnoreCase);
        var lmEyes = new Dictionary<string, LensValues>(StringComparer.OrdinalIgnoreCase);
        var kmEyes = new Dictionary<string, KmValues>(StringComparer.OrdinalIgnoreCase);
        var tonoEyes = new Dictionary<string, SeriesValues>(StringComparer.OrdinalIgnoreCase);
        string? pd = null;
        string? vd = null;
        string? cctRight = null;
        string? cctLeft = null;

        foreach (var rawLine in SplitLines(text))
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            ReadRefLine(line, refEyes, ref pd, ref vd);
            ReadLmLine(line, lmEyes);
            ReadKmLine(line, kmEyes);
            ReadTonoLine(line, tonoEyes);
            ReadCctLine(line, ref cctRight, ref cctLeft);
        }

        AddRefMeasurements(measurements, refEyes, pd, vd);
        AddRefMedistarLines(measurements, refEyes, pd, vd);
        AddLensMeasurements(measurements, lmEyes);
        AddLensMedistarLines(measurements, lmEyes);
        AddKmMeasurements(measurements, kmEyes);
        AddKmMedistarLines(measurements, kmEyes);
        AddTonoMeasurements(measurements, tonoEyes);
        AddTonoMedistarLine(measurements, tonoEyes);
        AddPachyMeasurements(measurements, cctRight, cctLeft);

        AddNoExportableWarningIfNeeded(measurements, issues, $"{company} {model}-Textdaten wurden gelesen, aber keine exportierbaren Werte erkannt.", sourcePath);
        return new DeviceParseResult(measurements, issues);
    }

    private static string DetectXmlCompany(XDocument document, string text, string? sourcePath, string model)
    {
        var common = FindFirst(document, "Common");
        var company = ReadDescendantValue(common ?? document.Root, "Company");
        if (!string.IsNullOrWhiteSpace(company))
        {
            return NormalizeCompany(company);
        }

        var combined = $"{sourcePath ?? string.Empty} {model} {text}";
        if (combined.Contains("ZEISS", StringComparison.OrdinalIgnoreCase) || combined.Contains("VISU", StringComparison.OrdinalIgnoreCase) || combined.Contains("IOL", StringComparison.OrdinalIgnoreCase))
        {
            return "ZEISS";
        }

        if (combined.Contains("Visionix", StringComparison.OrdinalIgnoreCase) || combined.Contains("VX", StringComparison.OrdinalIgnoreCase) || combined.Contains("optic", StringComparison.OrdinalIgnoreCase))
        {
            return "Visionix";
        }

        return "Canon";
    }

    private static string DetectXmlModel(XDocument document, string text, string? sourcePath)
    {
        var common = FindFirst(document, "Common");
        var model = ReadDescendantValue(common ?? document.Root, "ModelName", "Model", "DeviceName");
        if (!string.IsNullOrWhiteSpace(model))
        {
            return model.Trim();
        }

        var combined = $"{sourcePath ?? string.Empty} {text}";
        return DetectModelFromText(combined);
    }

    private static string DetectTextCompany(string text, string? sourcePath, string model)
    {
        var combined = $"{sourcePath ?? string.Empty} {model} {text}";
        if (combined.Contains("ZEISS", StringComparison.OrdinalIgnoreCase) || combined.Contains("VISU", StringComparison.OrdinalIgnoreCase) || combined.Contains("IOL", StringComparison.OrdinalIgnoreCase))
        {
            return "ZEISS";
        }

        if (combined.Contains("Visionix", StringComparison.OrdinalIgnoreCase) || combined.Contains("RETINOMAX", StringComparison.OrdinalIgnoreCase) || combined.Contains("VX", StringComparison.OrdinalIgnoreCase))
        {
            return "Visionix";
        }

        return "Canon";
    }

    private static string DetectTextModel(string text, string? sourcePath)
    {
        foreach (var line in SplitLines(text))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("MODEL=", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed[6..].Trim();
            }
        }

        return DetectModelFromText($"{sourcePath ?? string.Empty} {text}");
    }

    private static string DetectModelFromText(string text)
    {
        var normalized = text.Replace("_", " ", StringComparison.OrdinalIgnoreCase);
        if (normalized.Contains("RK-F2", StringComparison.OrdinalIgnoreCase))
        {
            return "RK-F2";
        }

        if (normalized.Contains("TX-20P", StringComparison.OrdinalIgnoreCase))
        {
            return "TX-20P";
        }

        if (normalized.Contains("VISULENS 550", StringComparison.OrdinalIgnoreCase) || normalized.Contains("VIS550", StringComparison.OrdinalIgnoreCase))
        {
            return "VISULENS 550";
        }

        if (normalized.Contains("VISUPLAN 500", StringComparison.OrdinalIgnoreCase) || normalized.Contains("VISUPLAN500", StringComparison.OrdinalIgnoreCase))
        {
            return "VISUPLAN 500";
        }

        if (normalized.Contains("VISUREF 100", StringComparison.OrdinalIgnoreCase) || normalized.Contains("VISUREF100", StringComparison.OrdinalIgnoreCase))
        {
            return "VISUREF 100";
        }

        if (normalized.Contains("RETINOMAX 5", StringComparison.OrdinalIgnoreCase))
        {
            return "Retinomax 5";
        }

        if (normalized.Contains("VX 650", StringComparison.OrdinalIgnoreCase) || normalized.Contains("VX650", StringComparison.OrdinalIgnoreCase))
        {
            return "VX 650";
        }

        if (normalized.Contains("VX 120", StringComparison.OrdinalIgnoreCase) || normalized.Contains("VX120", StringComparison.OrdinalIgnoreCase))
        {
            return "VX 120";
        }

        return "Canon/ZEISS/Visionix";
    }

    private static string NormalizeCompany(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Equals("Zeiss", StringComparison.OrdinalIgnoreCase) ? "ZEISS" : trimmed;
    }

    private static bool IsVisionixXml(XDocument document, string text, string? sourcePath)
    {
        return document.Root?.Name.LocalName.Equals("optic", StringComparison.OrdinalIgnoreCase) == true
            || text.Contains("<optic", StringComparison.OrdinalIgnoreCase)
            || (sourcePath ?? string.Empty).Contains("VX", StringComparison.OrdinalIgnoreCase);
    }

    private void ParseJoiaRef(XDocument document, List<MeasurementValue> measurements)
    {
        var measure = FindMeasure(document, "REF");
        if (measure is null)
        {
            return;
        }

        var right = ReadRefEye(measure, "R");
        var left = ReadRefEye(measure, "L");
        var pd = NormalizeUnsignedDecimal(ReadDescendantValue(FindFirst(measure, "PD"), "Distance", "PD"));
        var vd = NormalizeUnsignedDecimal(ReadDescendantValue(measure, "VD"));
        AddRefMeasurements(measurements, CreateEyeDictionary(right, left), pd, vd);
        AddRefMedistarLines(measurements, CreateEyeDictionary(right, left), pd, vd);
    }

    private void ParseJoiaLm(XDocument document, List<MeasurementValue> measurements)
    {
        var measure = FindMeasure(document, "LM");
        if (measure is null)
        {
            return;
        }

        var eyes = new Dictionary<string, LensValues>(StringComparer.OrdinalIgnoreCase);
        var right = ReadLensEye(measure, "R");
        var left = ReadLensEye(measure, "L");
        if (right.HasPower)
        {
            eyes["R"] = right;
        }

        if (left.HasPower)
        {
            eyes["L"] = left;
        }

        AddLensMeasurements(measurements, eyes);
        AddLensMedistarLines(measurements, eyes);
    }

    private void ParseJoiaTm(XDocument document, List<MeasurementValue> measurements)
    {
        var measure = FindMeasure(document, "TM");
        if (measure is null)
        {
            return;
        }

        var eyes = new Dictionary<string, SeriesValues>(StringComparer.OrdinalIgnoreCase);
        foreach (var eye in new[] { "R", "L" })
        {
            var eyeElement = FindEyeElement(measure, eye);
            if (eyeElement is null)
            {
                continue;
            }

            var values = FindAll(eyeElement, "List")
                .SelectMany(listElement => FindAll(listElement, "IOP_mmHg"))
                .Select(element => NormalizeUnsignedDecimal(element.Value))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!)
                .ToList();
            var average = NormalizeUnsignedDecimal(ReadDescendantValue(FindFirst(eyeElement, "Average"), "IOP_mmHg"));
            if (values.Count == 0)
            {
                values = FindAll(eyeElement, "IOP_mmHg")
                    .Where(element => !element.Ancestors().Any(ancestor => ancestor.Name.LocalName.Equals("Average", StringComparison.OrdinalIgnoreCase)))
                    .Select(element => NormalizeUnsignedDecimal(element.Value))
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value!)
                    .ToList();
            }

            if (values.Count > 0 || !string.IsNullOrWhiteSpace(average))
            {
                eyes[eye] = new SeriesValues(eye, values, average);
            }
        }

        AddTonoMeasurements(measurements, eyes);
        AddTonoMedistarLine(measurements, eyes);
    }

    private void ParseJoiaPachy(XDocument document, List<MeasurementValue> measurements)
    {
        var measure = FindMeasure(document, "PM") ?? FindMeasure(document, "CCT");
        if (measure is null)
        {
            return;
        }

        var right = ReadPachyEye(measure, "R");
        var left = ReadPachyEye(measure, "L");
        AddPachyMeasurements(measurements, right, left);
    }

    private void ParseVisionixOpticXml(XDocument document, List<MeasurementValue> measurements, string model)
    {
        if (model.Contains("VX 120", StringComparison.OrdinalIgnoreCase))
        {
            ParseVisionixVx120(document, measurements);
            return;
        }

        ParseVisionixVx650(document, measurements);
    }

    private void ParseVisionixVx120(XDocument document, List<MeasurementValue> measurements)
    {
        var measure = FindFirst(document, "measure_REF") ?? FindFirst(document, "measure_LSM") ?? FindFirst(document, "LSM_mesurement");
        if (measure is null)
        {
            return;
        }

        var eyes = new Dictionary<string, RefValues>(StringComparer.OrdinalIgnoreCase);
        var right = ReadVisionixRefEye(measure, "ref_right", "R");
        var left = ReadVisionixRefEye(measure, "ref_left", "L");
        if (right.HasPower)
        {
            eyes["R"] = right;
        }

        if (left.HasPower)
        {
            eyes["L"] = left;
        }

        AddRefMeasurements(measurements, eyes, null, null);
        AddRefMedistarLines(measurements, eyes, null, null);
    }

    private void ParseVisionixVx650(XDocument document, List<MeasurementValue> measurements)
    {
        var measure = FindFirst(document, "measure_WF") ?? FindFirst(document, "objective_mesurement");
        var eyes = new Dictionary<string, RefValues>(StringComparer.OrdinalIgnoreCase);
        if (measure is not null)
        {
            var right = ReadVisionixWfEye(measure, "wf_right", "R");
            var left = ReadVisionixWfEye(measure, "wf_left", "L");
            if (right.HasPower)
            {
                eyes["R"] = right;
            }

            if (left.HasPower)
            {
                eyes["L"] = left;
            }
        }

        var vd = NormalizeUnsignedDecimal(ReadDescendantValue(measure, "VD"));
        AddRefMeasurements(measurements, eyes, null, vd);
        AddRefMedistarLines(measurements, eyes, null, vd);

        var tono = FindFirst(document, "TONO") ?? FindFirst(document, "measure_TONO");
        if (tono is not null)
        {
            var tonoEyes = new Dictionary<string, SeriesValues>(StringComparer.OrdinalIgnoreCase);
            foreach (var (eyeNode, eye) in new[] { ("tono_right", "R"), ("tono_left", "L") })
            {
                var node = FindFirst(tono, eyeNode);
                var average = NormalizeUnsignedDecimal(ReadDescendantValue(node, "average", "Average", "IOP_mmHg"));
                if (!string.IsNullOrWhiteSpace(average))
                {
                    tonoEyes[eye] = new SeriesValues(eye, Array.Empty<string>(), average);
                }
            }

            AddTonoMeasurements(measurements, tonoEyes);
            AddTonoMedistarLine(measurements, tonoEyes);
        }
    }

    private static RefValues ReadRefEye(XElement measure, string eye)
    {
        var eyeElement = FindEyeElement(measure, eye);
        var valueNode = FindFirst(eyeElement, "Median") ?? FindFirst(eyeElement, "List") ?? eyeElement;
        return new RefValues(
            eye,
            NormalizeSignedDecimal(ReadDescendantValue(valueNode, "Sphere", "SPH", "Sph")),
            NormalizeSignedDecimal(ReadDescendantValue(valueNode, "Cylinder", "CYL", "Cyl")),
            NormalizeAxis(ReadDescendantValue(valueNode, "Axis", "AXIS")));
    }

    private static LensValues ReadLensEye(XElement measure, string eye)
    {
        var eyeElement = FindEyeElement(measure, eye);
        var valueNode = FindFirst(eyeElement, "LM") ?? FindFirst(eyeElement, "List") ?? eyeElement;
        return new LensValues(
            eye,
            NormalizeSignedDecimal(ReadDescendantValue(valueNode, "Sphere", "SPH", "Sph")),
            NormalizeSignedDecimal(ReadDescendantValue(valueNode, "Cylinder", "CYL", "Cyl")),
            NormalizeAxis(ReadDescendantValue(valueNode, "Axis", "AXIS")),
            NormalizeSignedDecimal(ReadDescendantValue(valueNode, "ADD", "Add", "Addition")),
            NormalizeSignedDecimal(ReadDescendantValue(valueNode, "ADD2", "Add2")),
            NormalizeUnsignedDecimal(ReadDescendantValue(valueNode, "PrismX", "Horizontal")),
            NormalizeUnsignedDecimal(ReadDescendantValue(valueNode, "PrismY", "Vertical")),
            NormalizeUnsignedDecimal(ReadDescendantValue(valueNode, "PD", "Distance")));
    }

    private static RefValues ReadVisionixRefEye(XElement measure, string nodeName, string eye)
    {
        var node = FindFirst(measure, nodeName);
        return new RefValues(
            eye,
            NormalizeSignedDecimal(ReadDescendantValue(node, "sphere")),
            NormalizeSignedDecimal(ReadDescendantValue(node, "cylinder")),
            NormalizeAxis(ReadDescendantValue(node, "axis")),
            NormalizeSignedDecimal(ReadDescendantValue(node, "addition")));
    }

    private static RefValues ReadVisionixWfEye(XElement measure, string nodeName, string eye)
    {
        var eyeNode = FindFirst(measure, nodeName);
        var near = FindFirst(eyeNode, "Near_Vision_fast");
        var photopic = FindFirst(eyeNode, "Photopic");
        var node = near ?? photopic ?? eyeNode;
        return new RefValues(
            eye,
            NormalizeSignedDecimal(ReadDescendantValue(node, "sphere")),
            NormalizeSignedDecimal(ReadDescendantValue(node, "cylinder")),
            NormalizeAxis(ReadDescendantValue(node, "axis")),
            NormalizeSignedDecimal(ReadDescendantValue(node, "addition")));
    }

    private static string? ReadPachyEye(XElement measure, string eye)
    {
        var eyeElement = FindEyeElement(measure, eye);
        return NormalizePachy(ReadDescendantValue(FindFirst(eyeElement, "Average") ?? eyeElement, "CCT", "CCT_um", "Pachy", "Thickness"));
    }

    private static void ReadRefLine(string line, Dictionary<string, RefValues> eyes, ref string? pd, ref string? vd)
    {
        var match = Regex.Match(line, @"^REF\s+(?<eye>R|L)\s+S=(?<s>[+-]?\s*\d+(?:[\.,]\d+)?)\s+C=(?<c>[+-]?\s*\d+(?:[\.,]\d+)?)\s+A=(?<a>\d{1,3})(?<rest>.*)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            return;
        }

        var eye = match.Groups["eye"].Value.ToUpperInvariant();
        var rest = match.Groups["rest"].Value;
        eyes[eye] = new RefValues(
            eye,
            NormalizeSignedDecimal(match.Groups["s"].Value),
            NormalizeSignedDecimal(match.Groups["c"].Value),
            NormalizeAxis(match.Groups["a"].Value),
            NormalizeSignedDecimal(ReadInlineValue(rest, "ADD")));
        pd ??= NormalizeUnsignedDecimal(ReadInlineValue(rest, "PD"));
        vd ??= NormalizeUnsignedDecimal(ReadInlineValue(rest, "VD"));
    }

    private static void ReadLmLine(string line, Dictionary<string, LensValues> eyes)
    {
        var match = Regex.Match(line, @"^LM\s+(?<eye>R|L)\s+S=(?<s>[+-]?\s*\d+(?:[\.,]\d+)?)\s+C=(?<c>[+-]?\s*\d+(?:[\.,]\d+)?)\s+A=(?<a>\d{1,3})(?<rest>.*)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            return;
        }

        var eye = match.Groups["eye"].Value.ToUpperInvariant();
        var rest = match.Groups["rest"].Value;
        eyes[eye] = new LensValues(
            eye,
            NormalizeSignedDecimal(match.Groups["s"].Value),
            NormalizeSignedDecimal(match.Groups["c"].Value),
            NormalizeAxis(match.Groups["a"].Value),
            NormalizeSignedDecimal(ReadInlineValue(rest, "ADD")),
            NormalizeSignedDecimal(ReadInlineValue(rest, "ADD2")),
            NormalizeUnsignedDecimal(ReadInlineValue(rest, "PRISMX")),
            NormalizeUnsignedDecimal(ReadInlineValue(rest, "PRISMY")),
            NormalizeUnsignedDecimal(ReadInlineValue(rest, "PD")));
    }

    private static void ReadKmLine(string line, Dictionary<string, KmValues> eyes)
    {
        var match = Regex.Match(line, @"^KM\s+(?<eye>R|L)\s+(?<rest>.+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            return;
        }

        var eye = match.Groups["eye"].Value.ToUpperInvariant();
        var rest = match.Groups["rest"].Value;
        eyes[eye] = new KmValues(
            eye,
            NormalizeUnsignedDecimal(ReadInlineValue(rest, "R1")),
            NormalizeUnsignedDecimal(ReadInlineValue(rest, "P1")),
            NormalizeAxis(ReadInlineValue(rest, "A1")),
            NormalizeUnsignedDecimal(ReadInlineValue(rest, "R2")),
            NormalizeUnsignedDecimal(ReadInlineValue(rest, "P2")),
            NormalizeAxis(ReadInlineValue(rest, "A2")),
            NormalizeUnsignedDecimal(ReadInlineValue(rest, "AV")),
            NormalizeUnsignedDecimal(ReadInlineValue(rest, "AP")),
            NormalizeSignedDecimal(ReadInlineValue(rest, "CYL")),
            NormalizeAxis(ReadInlineValue(rest, "CA")));
    }

    private static void ReadTonoLine(string line, Dictionary<string, SeriesValues> eyes)
    {
        var match = Regex.Match(line, @"^TM\s+(?<eye>R|L)\s+(?<values>.+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            return;
        }

        var eye = match.Groups["eye"].Value.ToUpperInvariant();
        var valueText = match.Groups["values"].Value;
        var average = NormalizeUnsignedDecimal(ReadInlineValue(valueText, "AVG"));
        var seriesText = Regex.Replace(valueText, @"\bAVG\s*=\s*[+-]?\s*\d+(?:[\.,]\d+)?", string.Empty, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        var values = Regex.Matches(seriesText, @"(?<![A-Z=])(?<value>\d+(?:[\.,]\d+)?)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
            .Select(matchItem => NormalizeUnsignedDecimal(matchItem.Groups["value"].Value))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToList();

        eyes[eye] = new SeriesValues(eye, values, average);
    }

    private static void ReadCctLine(string line, ref string? cctRight, ref string? cctLeft)
    {
        if (!line.StartsWith("CCT", StringComparison.OrdinalIgnoreCase) && !line.StartsWith("PACHY", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        cctRight = NormalizePachy(ReadInlineValue(line, "R")) ?? cctRight;
        cctLeft = NormalizePachy(ReadInlineValue(line, "L")) ?? cctLeft;
    }

    private static string? ReadInlineValue(string text, string key)
    {
        var match = Regex.Match(text, $@"\b{Regex.Escape(key)}\s*=\s*(?<value>[+-]?\s*\d+(?:[\.,]\d+)?)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return match.Success ? match.Groups["value"].Value.Trim() : null;
    }

    private List<MeasurementValue> CreateCommonMeasurements(string company, string model)
    {
        var measurements = new List<MeasurementValue>();
        Add(measurements, "Common/Company", "Company", company, null, null, "Common");
        Add(measurements, "Common/ModelName", "ModelName", model, null, null, "Common");
        return measurements;
    }

    private static Dictionary<string, RefValues> CreateEyeDictionary(RefValues right, RefValues left)
    {
        var eyes = new Dictionary<string, RefValues>(StringComparer.OrdinalIgnoreCase);
        if (right.HasPower)
        {
            eyes["R"] = right;
        }

        if (left.HasPower)
        {
            eyes["L"] = left;
        }

        return eyes;
    }

    private static void AddRefMeasurements(List<MeasurementValue> measurements, IReadOnlyDictionary<string, RefValues> eyes, string? pd, string? vd)
    {
        foreach (var values in eyes.Values)
        {
            var prefix = $"Measure[@Type='REF']/REF/{values.Eye}";
            Add(measurements, $"{prefix}/Sphere", $"REF {values.Eye} Sphere", values.Sphere, "dpt", values.Eye, "REF");
            Add(measurements, $"{prefix}/Cylinder", $"REF {values.Eye} Cylinder", values.Cylinder, "dpt", values.Eye, "REF");
            Add(measurements, $"{prefix}/Axis", $"REF {values.Eye} Axis", values.Axis, "deg", values.Eye, "REF");
            Add(measurements, $"{prefix}/ADD", $"REF {values.Eye} ADD", values.Add, "dpt", values.Eye, "REF");
        }

        Add(measurements, "Measure[@Type='REF']/REF/PD", "REF PD", pd, "mm", null, "REF");
        Add(measurements, "Measure[@Type='REF']/REF/VD", "REF VD", vd, "mm", null, "REF");
    }

    private void AddRefMedistarLines(List<MeasurementValue> measurements, IReadOnlyDictionary<string, RefValues> eyes, string? pd, string? vd)
    {
        foreach (var eye in new[] { "R", "L" })
        {
            if (!eyes.TryGetValue(eye, out var values))
            {
                continue;
            }

            var line = $"{eye}.:S={_formatter.FormatDiopter(values.Sphere)} Z={_formatter.FormatDiopter(values.Cylinder)}*{_formatter.FormatAxis(values.Axis)}";
            if (!string.IsNullOrWhiteSpace(values.Add))
            {
                line += $"                     A={_formatter.FormatDiopter(values.Add)}";
            }

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

    private static void AddLensMeasurements(List<MeasurementValue> measurements, IReadOnlyDictionary<string, LensValues> eyes)
    {
        foreach (var values in eyes.Values)
        {
            var prefix = $"Measure[@Type='LM']/LM/{values.Eye}";
            Add(measurements, $"{prefix}/Sphere", $"LM {values.Eye} Sphere", values.Sphere, "dpt", values.Eye, "LM");
            Add(measurements, $"{prefix}/Cylinder", $"LM {values.Eye} Cylinder", values.Cylinder, "dpt", values.Eye, "LM");
            Add(measurements, $"{prefix}/Axis", $"LM {values.Eye} Axis", values.Axis, "deg", values.Eye, "LM");
            Add(measurements, $"{prefix}/ADD", $"LM {values.Eye} ADD", values.Add, "dpt", values.Eye, "LM");
            Add(measurements, $"{prefix}/ADD2", $"LM {values.Eye} ADD2", values.Add2, "dpt", values.Eye, "LM");
            Add(measurements, $"{prefix}/PrismX", $"LM {values.Eye} PrismX", values.PrismX, "pdpt", values.Eye, "LM");
            Add(measurements, $"{prefix}/PrismY", $"LM {values.Eye} PrismY", values.PrismY, "pdpt", values.Eye, "LM");
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
            if (!string.IsNullOrWhiteSpace(values.PrismX) || !string.IsNullOrWhiteSpace(values.PrismY))
            {
                line += $" P={_formatter.FormatPrism(values.PrismX)} OUT {_formatter.FormatPrism(values.PrismY)} UP";
            }

            if (!string.IsNullOrWhiteSpace(values.Pd))
            {
                line += $" PD= {_formatter.FormatPd(values.Pd)}";
            }

            if (!string.IsNullOrWhiteSpace(values.Add))
            {
                line += $"                     A={_formatter.FormatDiopter(values.Add)}";
            }

            if (!string.IsNullOrWhiteSpace(values.Add2))
            {
                line += $" A2={_formatter.FormatDiopter(values.Add2)}";
            }

            Add(measurements, $"Measure[@Type='LM']/LM/{eye}/MedistarLine", $"LM {eye} MEDISTAR-Zeile", line, null, eye, "LM");
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
        Add(measurements, "Measure[@Type='KM']/KM/MedistarLine1", "KM MEDISTAR R1/R2-Zeile", BuildKmRadiiLine(eyes), null, null, "KM");
        Add(measurements, "Measure[@Type='KM']/KM/MedistarLine2", "KM MEDISTAR AV/CYL-Zeile", BuildKmAverageLine(eyes), null, null, "KM");
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
                line += $" CYL={values.Cylinder} {FormatAxis(values.CylinderAxis)}";
            }

            if (line.Length > 2)
            {
                parts.Add(line);
            }
        }

        return parts.Count == 0 ? null : string.Join(" // ", parts);
    }

    private static void AddTonoMeasurements(List<MeasurementValue> measurements, IReadOnlyDictionary<string, SeriesValues> eyes)
    {
        foreach (var values in eyes.Values)
        {
            var prefix = $"Measure[@Type='TM']/Tono/{values.Eye}";
            Add(measurements, $"{prefix}/Values", $"Tonometrie {values.Eye} Werte", string.Join(" ", values.Values), "mmHg", values.Eye, "TM");
            Add(measurements, $"{prefix}/Average", $"Tonometrie {values.Eye} Mittelwert", values.Average, "mmHg", values.Eye, "TM");
        }
    }

    private static void AddTonoMedistarLine(List<MeasurementValue> measurements, IReadOnlyDictionary<string, SeriesValues> eyes)
    {
        var parts = new List<string>();
        foreach (var eye in new[] { "R", "L" })
        {
            if (!eyes.TryGetValue(eye, out var values))
            {
                continue;
            }

            var label = eye == "R" ? "R" : "L";
            var rawValues = values.Values.Count == 0 ? values.Average : string.Join(" ", values.Values);
            var average = string.IsNullOrWhiteSpace(values.Average) ? string.Empty : $" [{values.Average}]";
            if (!string.IsNullOrWhiteSpace(rawValues) || !string.IsNullOrWhiteSpace(average))
            {
                parts.Add($"{label} = {rawValues}{average}");
            }
        }

        Add(measurements, "Measure[@Type='TM']/Tono/TonoListLine", "Tonometrie MEDISTAR-Zeile", parts.Count == 0 ? null : $"{string.Join(" // ", parts)} mmHg", null, null, "TM");
    }

    private static void AddPachyMeasurements(List<MeasurementValue> measurements, string? right, string? left)
    {
        Add(measurements, "Measure[@Type='CCT']/Pachy/R/Average", "Pachymetrie R Mittelwert", right, "mm", "R", "CCT");
        Add(measurements, "Measure[@Type='CCT']/Pachy/L/Average", "Pachymetrie L Mittelwert", left, "mm", "L", "CCT");
        var line = BuildPachyLine(right, left);
        Add(measurements, "Measure[@Type='CCT']/Pachy/MedistarLine", "Pachymetrie MEDISTAR-Zeile", line, null, null, "CCT");
    }

    private static string? BuildPachyLine(string? right, string? left)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(right))
        {
            parts.Add($"RA: {right}");
        }

        if (!string.IsNullOrWhiteSpace(left))
        {
            parts.Add($"LA: {left}");
        }

        return parts.Count == 0 ? null : string.Join(" // ", parts);
    }

    private static XElement? FindMeasure(XContainer document, string type)
    {
        return document.Descendants()
            .FirstOrDefault(element =>
                element.Name.LocalName.Equals("Measure", StringComparison.OrdinalIgnoreCase)
                && (element.Name.NamespaceName.Contains(type, StringComparison.OrdinalIgnoreCase)
                    || string.Equals((string?)element.Attribute("type"), type, StringComparison.OrdinalIgnoreCase)
                    || string.Equals((string?)element.Attribute("Type"), type, StringComparison.OrdinalIgnoreCase)
                    || element.Descendants().Any(descendant => descendant.Name.LocalName.Equals(type, StringComparison.OrdinalIgnoreCase))));
    }

    private static XElement? FindEyeElement(XElement? root, string eye)
    {
        if (root is null)
        {
            return null;
        }

        return root.Descendants()
            .FirstOrDefault(element => element.Name.LocalName.Equals(eye, StringComparison.OrdinalIgnoreCase));
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

    private static IEnumerable<XElement> FindAll(XContainer? root, string name)
    {
        return root is null
            ? Array.Empty<XElement>()
            : root.Descendants().Where(element => element.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static string? ReadDescendantValue(XElement? root, params string[] names)
    {
        if (root is null)
        {
            return null;
        }

        return root.Descendants()
            .FirstOrDefault(element => names.Any(name => element.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase)))
            ?.Value
            .Trim();
    }

    private static void AddNoExportableWarningIfNeeded(
        IReadOnlyList<MeasurementValue> measurements,
        List<DeviceParseIssue> issues,
        string message,
        string? sourcePath)
    {
        if (measurements.All(measurement => measurement.SourcePath.StartsWith("Common/", StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add(new DeviceParseIssue(DeviceParseIssueSeverity.Warning, message, sourcePath ?? string.Empty, null));
        }
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

    private static IReadOnlyList<string> SplitLines(string text)
    {
        return text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n');
    }

    private static string? NormalizeSignedDecimal(string? value)
    {
        var normalized = NormalizeNumber(value, 2);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return normalized[0] is '+' or '-' ? normalized : $"+{normalized}";
    }

    private static string? NormalizeUnsignedDecimal(string? value)
    {
        return NormalizeNumber(value, 2, trimTrailingZeros: true);
    }

    private static string? NormalizePachy(string? value)
    {
        var normalized = NormalizeNumber(value, 3);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (decimal.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed)
            && parsed > 10)
        {
            return (parsed / 1000m).ToString("0.000", CultureInfo.InvariantCulture);
        }

        return normalized;
    }

    private static string? NormalizeNumber(string? value, int decimals, bool trimTrailingZeros = false)
    {
        var trimmed = value?.Trim().Replace(" ", string.Empty, StringComparison.Ordinal).Replace(',', '.');
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        if (!decimal.TryParse(trimmed, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed))
        {
            return trimmed;
        }

        var format = "0." + new string('0', decimals);
        var result = parsed.ToString(format, CultureInfo.InvariantCulture);
        if (trimTrailingZeros && result.Contains('.', StringComparison.Ordinal))
        {
            result = result.TrimEnd('0').TrimEnd('.');
        }

        return result;
    }

    private static string? NormalizeAxis(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        return int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var axis)
            ? axis.ToString(CultureInfo.InvariantCulture)
            : trimmed;
    }

    private static string FormatAxis(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().PadLeft(3);
    }

    private sealed record RefValues(string Eye, string? Sphere = null, string? Cylinder = null, string? Axis = null, string? Add = null)
    {
        public bool HasPower => !string.IsNullOrWhiteSpace(Sphere) || !string.IsNullOrWhiteSpace(Cylinder) || !string.IsNullOrWhiteSpace(Axis);
    }

    private sealed record LensValues(
        string Eye,
        string? Sphere = null,
        string? Cylinder = null,
        string? Axis = null,
        string? Add = null,
        string? Add2 = null,
        string? PrismX = null,
        string? PrismY = null,
        string? Pd = null)
    {
        public bool HasPower => !string.IsNullOrWhiteSpace(Sphere) || !string.IsNullOrWhiteSpace(Cylinder) || !string.IsNullOrWhiteSpace(Axis);
    }

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

    private sealed record SeriesValues(string Eye, IReadOnlyList<string> Values, string? Average);
}
