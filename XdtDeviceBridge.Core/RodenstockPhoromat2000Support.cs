using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace XdtDeviceBridge.Core;

public static class RodenstockPhoromat2000Constants
{
    public const string ParserMode = "RodenstockPhoromat2000";
    public const string DeviceOutputFormat = "Rodenstock Phoromat 2000 RS232";
    public const string DefaultFileNameTemplate = "Rodenstock_Phoromat_2000_Send.txt";
}

public static class RodenstockPhoromat2000ControlChars
{
    public const byte SOH = 0x01;
    public const byte STX = 0x02;
    public const byte EOT = 0x04;
    public const byte ETB = 0x17;
    public const byte LF = 0x0A;
    public const byte CR = 0x0D;
}

public sealed record RodenstockPhoromat2000FrameParseResult(
    string Payload,
    bool StartMarkerFound,
    bool EndMarkerFound,
    bool UsedLfOnlyEndMarker,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Errors)
{
    public bool Success => Errors.Count == 0;
}

public sealed class RodenstockPhoromat2000FrameParser
{
    private const string ReceiveStartMarker = "\u0001*PC_RCV_S\u0004";
    private const string ReceiveEndMarkerCrLf = "\u0001*PC_RCV_E\u0004\r\n";
    private const string ReceiveEndMarkerLf = "\u0001*PC_RCV_E\u0004\n";
    private const string ReceiveEndMarkerRaw = "\u0001*PC_RCV_E\u0004";

    public RodenstockPhoromat2000FrameParseResult ExtractPayload(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        return ExtractPayload(Encoding.ASCII.GetString(bytes));
    }

    public RodenstockPhoromat2000FrameParseResult ExtractPayload(string text)
    {
        var raw = text ?? string.Empty;
        var warnings = new List<string>();
        var errors = new List<string>();
        var startIndex = raw.IndexOf(ReceiveStartMarker, StringComparison.Ordinal);
        var startFound = startIndex >= 0;
        var payloadStart = startFound ? startIndex + ReceiveStartMarker.Length : 0;

        if (!startFound)
        {
            warnings.Add("Rodenstock Phoromat 2000: Empfangs-Startmarker fehlt; Rohdaten werden als Nutzdaten gelesen.");
        }

        var end = FindEndMarker(raw, payloadStart);
        var endFound = end.Index >= 0;
        if (!endFound)
        {
            if (startFound)
            {
                errors.Add("Rodenstock Phoromat 2000: Empfangs-Endmarker fehlt.");
            }
            else
            {
                warnings.Add("Rodenstock Phoromat 2000: Empfangs-Endmarker fehlt; Rohdaten werden vollstaendig gelesen.");
            }
        }

        var payloadEnd = endFound ? end.Index : raw.Length;
        if (payloadEnd < payloadStart)
        {
            errors.Add("Rodenstock Phoromat 2000: Empfangsmarker stehen in ungueltiger Reihenfolge.");
            payloadEnd = payloadStart;
        }

        var payload = raw[payloadStart..payloadEnd].Trim('\r', '\n');
        return new RodenstockPhoromat2000FrameParseResult(
            payload,
            startFound,
            endFound,
            end.UsedLfOnly,
            warnings,
            errors);
    }

    private static (int Index, bool UsedLfOnly) FindEndMarker(string raw, int startIndex)
    {
        var crlf = raw.IndexOf(ReceiveEndMarkerCrLf, startIndex, StringComparison.Ordinal);
        var lf = raw.IndexOf(ReceiveEndMarkerLf, startIndex, StringComparison.Ordinal);
        var rawMarker = raw.IndexOf(ReceiveEndMarkerRaw, startIndex, StringComparison.Ordinal);

        var candidates = new[]
        {
            (Index: crlf, UsedLfOnly: false),
            (Index: lf, UsedLfOnly: true),
            (Index: rawMarker, UsedLfOnly: false)
        }
        .Where(candidate => candidate.Index >= 0)
        .OrderBy(candidate => candidate.Index)
        .ToArray();

        return candidates.FirstOrDefault((-1, false));
    }
}

public sealed class RodenstockPhoromat2000Parser
{
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private readonly RodenstockPhoromat2000FrameParser _frameParser = new();
    private readonly MedistarResultFormatter _formatter = new();

    public static bool IsParserMode(string? parserMode)
    {
        return string.Equals(parserMode, RodenstockPhoromat2000Constants.ParserMode, StringComparison.OrdinalIgnoreCase);
    }

    public DeviceParseResult ParseFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return ParseBytes(File.ReadAllBytes(path), path);
    }

    public DeviceParseResult ParseText(string text, string? sourcePath = null)
    {
        return ParsePayloadOrFrame(text ?? string.Empty, sourcePath);
    }

    public DeviceParseResult ParseBytes(byte[] bytes, string? sourcePath = null)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        return ParsePayloadOrFrame(Encoding.ASCII.GetString(bytes), sourcePath);
    }

    private DeviceParseResult ParsePayloadOrFrame(string text, string? sourcePath)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new DeviceParseResult(
                Array.Empty<MeasurementValue>(),
                new[] { new DeviceParseIssue(DeviceParseIssueSeverity.Error, "Rodenstock Phoromat 2000: Rohdaten sind leer.", sourcePath ?? string.Empty, null) });
        }

        var frame = _frameParser.ExtractPayload(text);
        var measurements = new List<MeasurementValue>();
        var issues = new List<DeviceParseIssue>();
        issues.AddRange(frame.Warnings.Select(warning => new DeviceParseIssue(DeviceParseIssueSeverity.Warning, warning, sourcePath ?? string.Empty, null)));
        issues.AddRange(frame.Errors.Select(error => new DeviceParseIssue(DeviceParseIssueSeverity.Error, error, sourcePath ?? string.Empty, null)));

        if (!frame.Success)
        {
            return new DeviceParseResult(measurements, issues);
        }

        Add(measurements, "Common/Company", "Company", "Rodenstock", null, null, "Common");
        Add(measurements, "Common/ModelName", "ModelName", "Phoromat 2000", null, null, "Common");

        var data = ParseSegments(frame.Payload);
        AddFnMeasurements(measurements, data);
        AddTopLevelMeasurements(measurements, data);
        AddMedistarLines(measurements, data, issues, sourcePath);

        if (measurements.All(measurement => measurement.SourcePath.StartsWith("Common/", StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add(new DeviceParseIssue(
                DeviceParseIssueSeverity.Warning,
                "Rodenstock Phoromat 2000: Rohdaten wurden gelesen, aber keine exportierbaren FN-/WD-Werte erkannt.",
                sourcePath ?? string.Empty,
                null));
        }

        return new DeviceParseResult(measurements, issues);
    }

    private static RodenstockPhoromat2000ParsedFrame ParseSegments(string payload)
    {
        var parsed = new RodenstockPhoromat2000ParsedFrame();
        string? currentGroup = null;
        foreach (var segment in SplitSegments(payload))
        {
            var line = NormalizeSegment(segment);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split('|');
            var tag = parts[0].Trim().TrimStart('*').Trim();
            var values = parts.Skip(1).ToList();
            while (values.Count > 0 && string.IsNullOrWhiteSpace(values[^1]))
            {
                values.RemoveAt(values.Count - 1);
            }

            if (IsGroupHeader(tag, values.Count))
            {
                currentGroup = tag.ToUpperInvariant();
                continue;
            }

            switch (tag.ToUpperInvariant())
            {
                case "PD":
                    AssignPd(parsed, values);
                    break;
                case "WD":
                    parsed.WorkingDistance = NormalizeUnsignedDecimal(values.ElementAtOrDefault(0));
                    break;
                case "SP" when IsFn(currentGroup):
                    AssignEyeValues(parsed.Fn, values, (eye, value) => eye.Sphere = NormalizeSignedDecimal(value));
                    break;
                case "CY" when IsFn(currentGroup):
                    AssignEyeValues(parsed.Fn, values, (eye, value) => eye.Cylinder = NormalizeSignedDecimal(value));
                    break;
                case "AX" when IsFn(currentGroup):
                    AssignEyeValues(parsed.Fn, values, (eye, value) => eye.Axis = NormalizeAxis(value));
                    break;
                case "AD" when IsFn(currentGroup):
                    AssignEyeValues(parsed.Fn, values, (eye, value) => eye.Add = NormalizeSignedDecimal(value));
                    break;
                case "PH" when IsFn(currentGroup):
                    AssignEyeValues(parsed.Fn, values, (eye, value) => eye.PrismHorizontal = NormalizePrism(value));
                    break;
                case "PV" when IsFn(currentGroup):
                    AssignEyeValues(parsed.Fn, values, (eye, value) => eye.PrismVertical = NormalizePrism(value));
                    break;
                case "VA" when IsFn(currentGroup):
                    AssignEyeValues(parsed.Fn, values, (eye, value) => eye.VisualAcuity = NormalizeRaw(value));
                    break;
            }
        }

        return parsed;
    }

    private static IEnumerable<string> SplitSegments(string payload)
    {
        return (payload ?? string.Empty).Split((char)RodenstockPhoromat2000ControlChars.ETB);
    }

    private static string NormalizeSegment(string segment)
    {
        var cleaned = segment
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Trim('\n', ' ', '\t');

        if (cleaned.Length > 0 && cleaned[0] == (char)RodenstockPhoromat2000ControlChars.STX)
        {
            cleaned = cleaned[1..];
        }

        return WhitespaceRegex.Replace(cleaned.Trim(), " ");
    }

    private static bool IsGroupHeader(string tag, int valueCount)
    {
        if (valueCount > 0)
        {
            return false;
        }

        return tag.Equals("LM", StringComparison.OrdinalIgnoreCase)
            || tag.Equals("AR", StringComparison.OrdinalIgnoreCase)
            || tag.Equals("SJ", StringComparison.OrdinalIgnoreCase)
            || tag.Equals("FN", StringComparison.OrdinalIgnoreCase)
            || tag.Equals("KM", StringComparison.OrdinalIgnoreCase)
            || tag.Equals("AV", StringComparison.OrdinalIgnoreCase)
            || tag.Equals("TIME", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFn(string? currentGroup)
    {
        return string.Equals(currentGroup, "FN", StringComparison.OrdinalIgnoreCase);
    }

    private static void AssignEyeValues(
        RodenstockPhoromat2000EyePair eyes,
        IReadOnlyList<string> values,
        Action<RodenstockPhoromat2000EyeValues, string?> assign)
    {
        if (values.Count > 0)
        {
            assign(eyes.Left, values[0]);
        }

        if (values.Count > 1)
        {
            assign(eyes.Right, values[1]);
        }
    }

    private static void AssignPd(RodenstockPhoromat2000ParsedFrame parsed, IReadOnlyList<string> values)
    {
        if (values.Count == 1)
        {
            parsed.PdBoth = NormalizeUnsignedDecimal(values[0]);
            return;
        }

        if (values.Count > 0)
        {
            parsed.PdLeft = NormalizeUnsignedDecimal(values[0]);
        }

        if (values.Count > 1)
        {
            parsed.PdRight = NormalizeUnsignedDecimal(values[1]);
        }
    }

    private void AddFnMeasurements(List<MeasurementValue> measurements, RodenstockPhoromat2000ParsedFrame data)
    {
        AddEyeMeasurements(measurements, "R", data.Fn.Right);
        AddEyeMeasurements(measurements, "L", data.Fn.Left);
    }

    private static void AddEyeMeasurements(List<MeasurementValue> measurements, string eye, RodenstockPhoromat2000EyeValues values)
    {
        var prefix = $"Measure[@Type='PHOROMAT2000']/FN/{eye}";
        Add(measurements, $"{prefix}/SP", $"FN {eye} SP", values.Sphere, "dpt", eye, "FN");
        Add(measurements, $"{prefix}/CY", $"FN {eye} CY", values.Cylinder, "dpt", eye, "FN");
        Add(measurements, $"{prefix}/AX", $"FN {eye} AX", values.Axis, "deg", eye, "FN");
        Add(measurements, $"{prefix}/AD", $"FN {eye} AD", values.Add, "dpt", eye, "FN");
        Add(measurements, $"{prefix}/PH", $"FN {eye} PH", values.PrismHorizontal, "pdpt", eye, "FN");
        Add(measurements, $"{prefix}/PV", $"FN {eye} PV", values.PrismVertical, "pdpt", eye, "FN");
        Add(measurements, $"{prefix}/VA", $"FN {eye} VA", values.VisualAcuity, null, eye, "FN");
    }

    private static void AddTopLevelMeasurements(List<MeasurementValue> measurements, RodenstockPhoromat2000ParsedFrame data)
    {
        Add(measurements, "Measure[@Type='PHOROMAT2000']/PD", "PD", data.PdBoth, "mm", null, "PD");
        Add(measurements, "Measure[@Type='PHOROMAT2000']/PD/R", "PD R", data.PdRight, "mm", "R", "PD");
        Add(measurements, "Measure[@Type='PHOROMAT2000']/PD/L", "PD L", data.PdLeft, "mm", "L", "PD");
        Add(measurements, "Measure[@Type='PHOROMAT2000']/WD", "WD", data.WorkingDistance, "cm", null, "WD");
    }

    private void AddMedistarLines(
        List<MeasurementValue> measurements,
        RodenstockPhoromat2000ParsedFrame data,
        List<DeviceParseIssue> issues,
        string? sourcePath)
    {
        AddMedistarLine(measurements, data, "R", data.Fn.Right, data.PdRight ?? data.PdBoth, issues, sourcePath);
        AddMedistarLine(measurements, data, "L", data.Fn.Left, data.PdLeft, issues, sourcePath);

        if (!string.IsNullOrWhiteSpace(data.WorkingDistance))
        {
            Add(measurements, "Measure[@Type='PHOROMAT2000']/WD/MedistarLine", "WD MEDISTAR-Zeile", $"WD= {data.WorkingDistance}", null, null, "WD");
        }
    }

    private void AddMedistarLine(
        List<MeasurementValue> measurements,
        RodenstockPhoromat2000ParsedFrame data,
        string eye,
        RodenstockPhoromat2000EyeValues values,
        string? pd,
        List<DeviceParseIssue> issues,
        string? sourcePath)
    {
        if (values.HasAnyFnValue && !values.HasCompleteRefraction)
        {
            issues.Add(new DeviceParseIssue(
                DeviceParseIssueSeverity.Warning,
                $"Rodenstock Phoromat 2000: FN-{eye} enthaelt keine vollstaendige SP/CY/AX-Basis; MEDISTAR-6228-Zeile wird nicht erzeugt.",
                sourcePath ?? string.Empty,
                null));
        }

        if (!values.HasCompleteRefraction)
        {
            return;
        }

        var line = $"{eye}.:S={_formatter.FormatDiopter(values.Sphere)} Z={_formatter.FormatDiopter(values.Cylinder)}*{_formatter.FormatAxis(values.Axis)}";
        var prism = BuildPrism(values);
        if (!string.IsNullOrWhiteSpace(prism))
        {
            line += $" P= {prism}";
        }

        if (!string.IsNullOrWhiteSpace(pd))
        {
            line += $" PD= {_formatter.FormatPd(pd)}";
        }

        if (!string.IsNullOrWhiteSpace(values.Add))
        {
            line += $" A={_formatter.FormatDiopter(values.Add)}";
        }

        Add(measurements, $"Measure[@Type='PHOROMAT2000']/FN/{eye}/MedistarLine", $"FN {eye} MEDISTAR-Zeile", line, null, eye, "FN");
    }

    private static string? BuildPrism(RodenstockPhoromat2000EyeValues values)
    {
        var parts = new[] { values.PrismHorizontal, values.PrismVertical }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .ToArray();
        return parts.Length == 0 ? null : string.Join(" ", parts);
    }

    private static string? NormalizeRaw(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static string? NormalizeSignedDecimal(string? value)
    {
        var normalized = NormalizeNumericText(value);
        if (string.IsNullOrWhiteSpace(normalized))
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
        var normalized = NormalizeNumericText(value);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number)
            ? Math.Abs(number).ToString("0.##", CultureInfo.InvariantCulture)
            : normalized;
    }

    private static string? NormalizeAxis(string? value)
    {
        var normalized = NormalizeNumericText(value);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var axis)
            ? axis.ToString(CultureInfo.InvariantCulture)
            : normalized;
    }

    private static string? NormalizePrism(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        var parts = trimmed.Replace(',', '.')
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2 && IsPrismDirection(parts[0]))
        {
            return $"{NormalizeUnsignedDecimal(parts[1])} {MapPrismDirection(parts[0])}".Trim();
        }

        return trimmed.Replace(',', '.');
    }

    private static bool IsPrismDirection(string value)
    {
        return value.Equals("BI", StringComparison.OrdinalIgnoreCase)
            || value.Equals("BO", StringComparison.OrdinalIgnoreCase)
            || value.Equals("BU", StringComparison.OrdinalIgnoreCase)
            || value.Equals("BD", StringComparison.OrdinalIgnoreCase);
    }

    private static string MapPrismDirection(string value)
    {
        return value.ToUpperInvariant() switch
        {
            "BI" => "I",
            "BO" => "O",
            "BU" => "U",
            "BD" => "D",
            _ => value
        };
    }

    private static string NormalizeNumericText(string? value)
    {
        return (value ?? string.Empty)
            .Trim()
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace(',', '.');
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
}

public sealed record RodenstockPhoromat2000OutputResult(
    bool Success,
    byte[] Bytes,
    string VisibleContent,
    string HexDump,
    IReadOnlyList<string> Warnings,
    string? ErrorMessage);

public sealed class RodenstockPhoromat2000OutputWriter
{
    public const string DeviceOutputFormat = RodenstockPhoromat2000Constants.DeviceOutputFormat;
    public const string DefaultFileNameTemplate = RodenstockPhoromat2000Constants.DefaultFileNameTemplate;

    public RodenstockPhoromat2000OutputResult BuildFrame(
        PatientData patient,
        IReadOnlyList<AisHistoricalMeasurementRecord> selectedMeasurements,
        DateTimeOffset? timestamp = null)
    {
        ArgumentNullException.ThrowIfNull(patient);
        ArgumentNullException.ThrowIfNull(selectedMeasurements);

        var selected = selectedMeasurements.ToArray();
        var warnings = new List<string>();
        var hasPayloadData = selected.Any(record =>
            record.SourceKind is AisHistoricalMeasurementSourceKind.Lensmeter
                or AisHistoricalMeasurementSourceKind.Autorefraction
                or AisHistoricalMeasurementSourceKind.Phoropter
            || !string.IsNullOrWhiteSpace(record.Pd)
            || !string.IsNullOrWhiteSpace(record.WorkingDistance));

        if (!hasPayloadData)
        {
            return new RodenstockPhoromat2000OutputResult(
                false,
                Array.Empty<byte>(),
                string.Empty,
                string.Empty,
                warnings,
                "Keine exportierbaren LM-/AR-/FN-/PD-/WD-Daten fuer Rodenstock Phoromat 2000 vorhanden.");
        }

        var bytes = new List<byte>();
        AddAscii(bytes, "\u0001*PC_SND_S\u0004\n");
        AddSegment(bytes, "*Phoromat 2000|000000001|0");
        AddPdSegment(bytes, selected);
        AddWdSegment(bytes, selected);
        AddRefractionGroup(bytes, "*LM", selected.FirstOrDefault(record => record.SourceKind == AisHistoricalMeasurementSourceKind.Lensmeter));
        AddRefractionGroup(bytes, "*AR", selected.FirstOrDefault(record => record.SourceKind == AisHistoricalMeasurementSourceKind.Autorefraction));
        AddSegment(bytes, "*SJ");
        AddRefractionGroup(bytes, "*FN", selected.FirstOrDefault(record => record.SourceKind == AisHistoricalMeasurementSourceKind.Phoropter));
        AddSegment(bytes, "*KM");
        AddSegment(bytes, "*AV");
        AddSegment(bytes, "*TIME|" + FormatTimestamp(timestamp ?? DateTimeOffset.Now) + "|");
        AddAscii(bytes, "\u0001*PC_SND_E\u0004\n");

        return new RodenstockPhoromat2000OutputResult(
            true,
            bytes.ToArray(),
            ToVisibleText(bytes),
            ToHexDump(bytes),
            warnings,
            null);
    }

    private static void AddPdSegment(List<byte> bytes, IReadOnlyList<AisHistoricalMeasurementRecord> selected)
    {
        var pd = selected.FirstOrDefault(record => !string.IsNullOrWhiteSpace(record.Pd))?.Pd;
        if (string.IsNullOrWhiteSpace(pd))
        {
            return;
        }

        var normalized = FormatUnsignedProtocolNumber(pd, "0.##");
        AddSegment(bytes, $"*PD|{normalized}|{normalized}|");
    }

    private static void AddWdSegment(List<byte> bytes, IReadOnlyList<AisHistoricalMeasurementRecord> selected)
    {
        var wd = selected.FirstOrDefault(record => !string.IsNullOrWhiteSpace(record.WorkingDistance))?.WorkingDistance;
        if (string.IsNullOrWhiteSpace(wd))
        {
            return;
        }

        AddSegment(bytes, $"*WD|{FormatUnsignedProtocolNumber(wd, "0.##")}|");
    }

    private static void AddRefractionGroup(List<byte> bytes, string groupTag, AisHistoricalMeasurementRecord? record)
    {
        AddSegment(bytes, groupTag);
        if (record is null)
        {
            return;
        }

        AddEyeFieldSegment(bytes, "*SP", record, eye => eye?.Sphere, FormatSignedProtocolNumber);
        AddEyeFieldSegment(bytes, "*CY", record, eye => eye?.Cylinder, FormatSignedProtocolNumber);
        AddEyeFieldSegment(bytes, "*AX", record, eye => eye?.Axis, FormatAxisProtocolValue);
        AddEyeFieldSegment(bytes, "*AD", record, eye => eye?.Add, FormatSignedProtocolNumber);
    }

    private static void AddEyeFieldSegment(
        List<byte> bytes,
        string tag,
        AisHistoricalMeasurementRecord record,
        Func<AisHistoricalEyeRefraction?, string?> selector,
        Func<string?, string> formatter)
    {
        var left = formatter(selector(record.LeftEye));
        var right = formatter(selector(record.RightEye));
        if (string.IsNullOrWhiteSpace(left) && string.IsNullOrWhiteSpace(right))
        {
            return;
        }

        AddSegment(bytes, $"{tag}|{left}|{right}|");
    }

    private static void AddSegment(List<byte> bytes, string text)
    {
        bytes.Add(RodenstockPhoromat2000ControlChars.STX);
        AddAscii(bytes, text);
        bytes.Add(RodenstockPhoromat2000ControlChars.ETB);
        bytes.Add(RodenstockPhoromat2000ControlChars.LF);
    }

    private static void AddAscii(List<byte> bytes, string text)
    {
        bytes.AddRange(Encoding.ASCII.GetBytes(text));
    }

    private static string FormatSignedProtocolNumber(string? value)
    {
        var number = NormalizeDecimal(value);
        return number?.ToString("+0.00;-0.00;0.00", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static string FormatUnsignedProtocolNumber(string? value, string format)
    {
        var number = NormalizeDecimal(value);
        return number?.ToString(format, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static string FormatAxisProtocolValue(string? value)
    {
        return int.TryParse(value?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var axis)
            ? axis.ToString("000", CultureInfo.InvariantCulture)
            : string.Empty;
    }

    private static decimal? NormalizeDecimal(string? value)
    {
        var normalized = value?.Trim().Replace(" ", string.Empty, StringComparison.Ordinal).Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number)
            ? number
            : null;
    }

    private static string FormatTimestamp(DateTimeOffset timestamp)
    {
        return "00" + timestamp.ToString("yy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private static string ToVisibleText(IReadOnlyList<byte> bytes)
    {
        var builder = new StringBuilder();
        foreach (var value in bytes)
        {
            builder.Append(value switch
            {
                RodenstockPhoromat2000ControlChars.SOH => "<SOH>",
                RodenstockPhoromat2000ControlChars.STX => "<STX>",
                RodenstockPhoromat2000ControlChars.EOT => "<EOT>",
                RodenstockPhoromat2000ControlChars.ETB => "<ETB>",
                RodenstockPhoromat2000ControlChars.LF => "<LF>" + Environment.NewLine,
                RodenstockPhoromat2000ControlChars.CR => "<CR>",
                _ => ((char)value).ToString()
            });
        }

        return builder.ToString().TrimEnd();
    }

    private static string ToHexDump(IEnumerable<byte> bytes)
    {
        return string.Join(" ", bytes.Select(value => value.ToString("X2", CultureInfo.InvariantCulture)));
    }
}

internal sealed class RodenstockPhoromat2000ParsedFrame
{
    public RodenstockPhoromat2000EyePair Fn { get; } = new();
    public string? PdBoth { get; set; }
    public string? PdRight { get; set; }
    public string? PdLeft { get; set; }
    public string? WorkingDistance { get; set; }
}

internal sealed class RodenstockPhoromat2000EyePair
{
    public RodenstockPhoromat2000EyeValues Right { get; } = new();
    public RodenstockPhoromat2000EyeValues Left { get; } = new();
}

internal sealed class RodenstockPhoromat2000EyeValues
{
    public string? Sphere { get; set; }
    public string? Cylinder { get; set; }
    public string? Axis { get; set; }
    public string? Add { get; set; }
    public string? PrismHorizontal { get; set; }
    public string? PrismVertical { get; set; }
    public string? VisualAcuity { get; set; }

    public bool HasCompleteRefraction =>
        !string.IsNullOrWhiteSpace(Sphere)
        && !string.IsNullOrWhiteSpace(Cylinder)
        && !string.IsNullOrWhiteSpace(Axis);

    public bool HasAnyFnValue =>
        !string.IsNullOrWhiteSpace(Sphere)
        || !string.IsNullOrWhiteSpace(Cylinder)
        || !string.IsNullOrWhiteSpace(Axis)
        || !string.IsNullOrWhiteSpace(Add)
        || !string.IsNullOrWhiteSpace(PrismHorizontal)
        || !string.IsNullOrWhiteSpace(PrismVertical)
        || !string.IsNullOrWhiteSpace(VisualAcuity);
}
