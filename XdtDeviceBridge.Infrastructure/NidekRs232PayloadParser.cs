using System.Globalization;
using System.Text.RegularExpressions;

namespace XdtDeviceBridge.Infrastructure;

public sealed class NidekRs232PayloadParser
{
    private static readonly Regex LmRefractionRegex = new(
        @"^(?<eye>[ RL]{2})(?<sphere>[+-]\d{2}\.\d{2})(?<cylinder>[+-]\d{2}\.\d{2})(?<axis>\d{3})$",
        RegexOptions.Compiled);

    private static readonly Regex NumericRegex = new(
        @"[+-]?\d+(?:\.\d+)?",
        RegexOptions.Compiled);

    private static readonly Regex ThreeOrFourDigitRegex = new(
        @"\b\d{3,4}\b",
        RegexOptions.Compiled);

    public NidekRs232ParsedPayload Parse(NidekRs232Frame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var rawSegments = frame.Segments
            .Select((segment, index) => new NidekRs232Segment(index, segment, DetermineSegmentCode(segment)))
            .ToList();
        var warnings = new List<string>(frame.Warnings);
        var errors = new List<NidekRs232ErrorSegment>();
        var refractions = new List<NidekRs232RefractionMeasurement>();
        var tonometry = new List<NidekRs232TonometryMeasurement>();
        var pachymetry = new List<NidekRs232PachymetryMeasurement>();
        string? manufacturer = null;
        string? model = null;
        string? patientOrPrintNumber = null;
        string? patientId = null;
        DateTime? measurementDateTime = null;

        foreach (var segment in frame.Segments)
        {
            if (TryReadIdentity(segment, out var parsedManufacturer, out var parsedModel))
            {
                manufacturer = parsedManufacturer;
                model = parsedModel;
                continue;
            }

            if (segment.StartsWith("NO", StringComparison.Ordinal))
            {
                patientOrPrintNumber = segment[2..].Trim();
                continue;
            }

            if (segment.StartsWith("IP", StringComparison.Ordinal))
            {
                patientId = segment[2..].Trim();
                continue;
            }

            if (segment.StartsWith("DA", StringComparison.Ordinal))
            {
                measurementDateTime = ParseNidekDateTime(segment[2..].Trim(), warnings);
            }
        }

        switch (frame.Header)
        {
            case "DLM":
                ParseLensmeterSegments(frame.Segments, refractions, errors, warnings);
                break;
            case "DNT":
                ParseTonometrySegments(frame.Segments, tonometry, errors, warnings);
                break;
            case "DPM":
                ParsePachymetrySegments(frame.Segments, pachymetry, errors, warnings);
                break;
            default:
                warnings.Add($"NIDEK-RS232-Header '{frame.Header}' ist noch keinem produktiven Payload-Parser zugeordnet.");
                break;
        }

        var candidates = CreateMedistarCandidates(refractions, tonometry, pachymetry);
        return new NidekRs232ParsedPayload(
            DeviceFamily: ResolveDeviceFamily(frame.Header),
            DeviceCode: frame.DeviceCode,
            Manufacturer: manufacturer,
            Model: model,
            PatientOrPrintNumber: string.IsNullOrWhiteSpace(patientOrPrintNumber) ? null : patientOrPrintNumber,
            PatientId: string.IsNullOrWhiteSpace(patientId) ? null : patientId,
            MeasurementDateTime: measurementDateTime,
            RawSegments: rawSegments,
            RefractionMeasurements: refractions,
            TonometryMeasurements: tonometry,
            PachymetryMeasurements: pachymetry,
            Errors: errors,
            MedistarCandidates: candidates,
            Warnings: warnings);
    }

    private static void ParseLensmeterSegments(
        IReadOnlyList<string> segments,
        List<NidekRs232RefractionMeasurement> refractions,
        List<NidekRs232ErrorSegment> errors,
        List<string> warnings)
    {
        foreach (var segment in segments)
        {
            var refractionMatch = LmRefractionRegex.Match(segment);
            if (refractionMatch.Success)
            {
                refractions.Add(new NidekRs232RefractionMeasurement(
                    ParseEyeCode(refractionMatch.Groups["eye"].Value),
                    ParseDecimal(refractionMatch.Groups["sphere"].Value),
                    ParseDecimal(refractionMatch.Groups["cylinder"].Value),
                    ParseInt(refractionMatch.Groups["axis"].Value),
                    Add: null,
                    NearSphere: null,
                    PdTotal: null,
                    PdRight: null,
                    PdLeft: null,
                    SourceSegment: segment));
                continue;
            }

            if (segment.StartsWith("IE", StringComparison.Ordinal) || segment.StartsWith("DE", StringComparison.Ordinal))
            {
                errors.Add(new NidekRs232ErrorSegment(segment[..2], segment));
                continue;
            }

            if (TryParseLmAddSegment(segment, out var addEye, out var addValue))
            {
                ApplyLmAdd(refractions, addEye, addValue, warnings);
                continue;
            }

            if (TryParseLmNearSegment(segment, out var nearEye, out var nearValue))
            {
                ApplyLmNearSphere(refractions, nearEye, nearValue, warnings);
                continue;
            }

            if (TryParseLmPdSegment(segment, out var pdTotal, out var pdRight, out var pdLeft))
            {
                ApplyLmPd(refractions, pdTotal, pdRight, pdLeft, warnings);
            }
        }
    }

    private static void ParseTonometrySegments(
        IReadOnlyList<string> segments,
        List<NidekRs232TonometryMeasurement> tonometry,
        List<NidekRs232ErrorSegment> errors,
        List<string> warnings)
    {
        foreach (var segment in segments)
        {
            if (TryParseEyeCount(segment, out var eye, out var count) && (eye == NidekRs232Eye.Right || eye == NidekRs232Eye.Left))
            {
                foreach (var errorCode in FindErrorCodes(segment, "APL", "OVR", "ERR"))
                {
                    errors.Add(new NidekRs232ErrorSegment(errorCode, segment, eye));
                }

                if (TryParseAveragePair(segment, out var averageMmHg, out var averageKpa))
                {
                    var valuePart = SegmentBeforeAverage(segment[3..]);
                    var values = NumericRegex.Matches(valuePart)
                        .Select(match => ParseDecimal(match.Value))
                        .Where(value => value.HasValue)
                        .Select(value => value!.Value)
                        .ToList();
                    tonometry.Add(new NidekRs232TonometryMeasurement(
                        eye,
                        count,
                        values,
                        Array.Empty<decimal>(),
                        averageMmHg,
                        averageKpa,
                        segment));
                }
                else if (!ContainsOnlyKnownError(segment))
                {
                    warnings.Add($"NT-Segment '{segment}' enthält keine sichere AV-Durchschnittsangabe.");
                }
            }
        }
    }

    private static void ParsePachymetrySegments(
        IReadOnlyList<string> segments,
        List<NidekRs232PachymetryMeasurement> pachymetry,
        List<NidekRs232ErrorSegment> errors,
        List<string> warnings)
    {
        foreach (var segment in segments)
        {
            if (TryParseEyeCount(segment, out var eye, out var count) && (eye == NidekRs232Eye.Right || eye == NidekRs232Eye.Left))
            {
                foreach (var errorCode in FindErrorCodes(segment, "BLK", "ALM", "ERR"))
                {
                    errors.Add(new NidekRs232ErrorSegment(errorCode, segment, eye));
                }

                var averageIndex = segment.IndexOf("AV", StringComparison.Ordinal);
                if (averageIndex >= 0)
                {
                    var valuePart = segment[3..averageIndex];
                    var values = ThreeOrFourDigitRegex.Matches(valuePart)
                        .Select(match => ParseInt(match.Value))
                        .Where(value => value.HasValue)
                        .Select(value => value!.Value)
                        .ToList();
                    var averageText = segment[(averageIndex + 2)..].Trim();
                    var averageMatch = ThreeOrFourDigitRegex.Match(averageText);
                    pachymetry.Add(new NidekRs232PachymetryMeasurement(
                        eye,
                        count,
                        values,
                        averageMatch.Success ? ParseInt(averageMatch.Value) : null,
                        segment));
                }
                else if (!ContainsOnlyKnownError(segment))
                {
                    warnings.Add($"PM-Segment '{segment}' enthält keine sichere AV-Durchschnittsangabe.");
                }
            }
        }
    }

    private static IReadOnlyList<NidekRs232MedistarCandidate> CreateMedistarCandidates(
        IReadOnlyList<NidekRs232RefractionMeasurement> refractions,
        IReadOnlyList<NidekRs232TonometryMeasurement> tonometry,
        IReadOnlyList<NidekRs232PachymetryMeasurement> pachymetry)
    {
        var candidates = new List<NidekRs232MedistarCandidate>();
        foreach (var measurement in refractions.Where(HasRefractionValues))
        {
            candidates.Add(new NidekRs232MedistarCandidate(
                "6228",
                "NIDEK-RS232 LM Lensmeter/Refraction",
                FormatRefractionPreview(measurement)));
        }

        foreach (var measurement in tonometry.Where(value => value.ValuesMmHg.Count > 0 || value.AverageMmHg.HasValue))
        {
            candidates.Add(new NidekRs232MedistarCandidate(
                "6205",
                "NIDEK-RS232 NT Tonometrie",
                FormatTonometryPreview(measurement)));
        }

        foreach (var measurement in pachymetry.Where(value => value.ValuesMicrometer.Count > 0 || value.AverageMicrometer.HasValue))
        {
            candidates.Add(new NidekRs232MedistarCandidate(
                "6220",
                "NIDEK-RS232 PM Pachymetrie",
                FormatPachymetryPreview(measurement)));
        }

        return candidates;
    }

    private static bool HasRefractionValues(NidekRs232RefractionMeasurement measurement)
    {
        return measurement.Sphere.HasValue || measurement.Cylinder.HasValue || measurement.Axis.HasValue;
    }

    private static string FormatRefractionPreview(NidekRs232RefractionMeasurement measurement)
    {
        var eye = measurement.Eye switch
        {
            NidekRs232Eye.Right => "R.",
            NidekRs232Eye.Left => "L.",
            NidekRs232Eye.Single => "S.",
            _ => "?."
        };
        var parts = new List<string> { $"{eye}:S={FormatDiopter(measurement.Sphere)}" };
        if (measurement.Cylinder.HasValue)
        {
            parts.Add($"Z={FormatDiopter(measurement.Cylinder)}*{(measurement.Axis ?? 0).ToString(CultureInfo.InvariantCulture).PadLeft(3)}");
        }

        if (measurement.Add.HasValue)
        {
            parts.Add($"A={FormatDiopter(measurement.Add)}");
        }

        if (measurement.PdTotal.HasValue)
        {
            parts.Add($"PD={FormatDecimal(measurement.PdTotal)}");
        }

        return string.Join(' ', parts);
    }

    private static string FormatTonometryPreview(NidekRs232TonometryMeasurement measurement)
    {
        var eye = measurement.Eye == NidekRs232Eye.Right ? "R" : "L";
        var values = measurement.ValuesMmHg.Count == 0
            ? string.Empty
            : string.Join(' ', measurement.ValuesMmHg.Select(value => value.ToString("0.#", CultureInfo.InvariantCulture)));
        var average = measurement.AverageMmHg.HasValue
            ? $" [{measurement.AverageMmHg.Value.ToString("0.0", CultureInfo.InvariantCulture)}]"
            : string.Empty;
        return $"{eye} = {values}{average} mmHg".Replace("  ", " ", StringComparison.Ordinal).Trim();
    }

    private static string FormatPachymetryPreview(NidekRs232PachymetryMeasurement measurement)
    {
        var eye = measurement.Eye == NidekRs232Eye.Right ? "R" : "L";
        var values = measurement.ValuesMicrometer.Count == 0
            ? string.Empty
            : string.Join(' ', measurement.ValuesMicrometer);
        var average = measurement.AverageMicrometer.HasValue
            ? $" [{measurement.AverageMicrometer.Value}]"
            : string.Empty;
        return $"{eye} = {values}{average} um".Replace("  ", " ", StringComparison.Ordinal).Trim();
    }

    private static string FormatDiopter(decimal? value)
    {
        if (!value.HasValue)
        {
            return string.Empty;
        }

        var sign = value.Value < 0 ? "-" : "+";
        return $"{sign} {Math.Abs(value.Value).ToString("0.00", CultureInfo.InvariantCulture)}";
    }

    private static string FormatDecimal(decimal? value)
    {
        return value?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static void ApplyLmAdd(
        List<NidekRs232RefractionMeasurement> refractions,
        NidekRs232Eye eye,
        decimal value,
        List<string> warnings)
    {
        UpdateLmMeasurement(refractions, eye, measurement => measurement with { Add = value }, "ADD", warnings);
    }

    private static void ApplyLmNearSphere(
        List<NidekRs232RefractionMeasurement> refractions,
        NidekRs232Eye eye,
        decimal value,
        List<string> warnings)
    {
        UpdateLmMeasurement(refractions, eye, measurement => measurement with { NearSphere = value }, "NearSphere", warnings);
    }

    private static void ApplyLmPd(
        List<NidekRs232RefractionMeasurement> refractions,
        decimal? pdTotal,
        decimal? pdRight,
        decimal? pdLeft,
        List<string> warnings)
    {
        if (refractions.Count == 0)
        {
            warnings.Add("PD-Segment ohne vorherige LM-Refraktionsmessung wurde roh erhalten.");
            return;
        }

        for (var i = 0; i < refractions.Count; i++)
        {
            var measurement = refractions[i];
            refractions[i] = measurement with
            {
                PdTotal = pdTotal,
                PdRight = pdRight,
                PdLeft = pdLeft
            };
        }
    }

    private static void UpdateLmMeasurement(
        List<NidekRs232RefractionMeasurement> refractions,
        NidekRs232Eye eye,
        Func<NidekRs232RefractionMeasurement, NidekRs232RefractionMeasurement> update,
        string label,
        List<string> warnings)
    {
        var index = refractions.FindIndex(measurement => measurement.Eye == eye || eye == NidekRs232Eye.Single);
        if (index < 0)
        {
            warnings.Add($"{label}-Segment ohne passende LM-Refraktionsmessung wurde roh erhalten.");
            return;
        }

        refractions[index] = update(refractions[index]);
    }

    private static bool TryParseLmAddSegment(string segment, out NidekRs232Eye eye, out decimal value)
    {
        eye = NidekRs232Eye.Unknown;
        value = 0;
        if (!segment.StartsWith('A'))
        {
            return false;
        }

        var prefixLength = segment.StartsWith("AR", StringComparison.Ordinal) || segment.StartsWith("AL", StringComparison.Ordinal)
            ? 2
            : 1;
        eye = prefixLength == 2
            ? segment[1] == 'R' ? NidekRs232Eye.Right : NidekRs232Eye.Left
            : NidekRs232Eye.Single;
        return TryParseDecimal(segment[prefixLength..], out value);
    }

    private static bool TryParseLmNearSegment(string segment, out NidekRs232Eye eye, out decimal value)
    {
        eye = NidekRs232Eye.Unknown;
        value = 0;
        if (!segment.StartsWith('N') || segment.StartsWith("NO", StringComparison.Ordinal))
        {
            return false;
        }

        var prefixLength = segment.StartsWith("NR", StringComparison.Ordinal) || segment.StartsWith("NL", StringComparison.Ordinal)
            ? 2
            : 1;
        eye = prefixLength == 2
            ? segment[1] == 'R' ? NidekRs232Eye.Right : NidekRs232Eye.Left
            : NidekRs232Eye.Single;
        return TryParseDecimal(segment[prefixLength..], out value);
    }

    private static bool TryParseLmPdSegment(string segment, out decimal? total, out decimal? right, out decimal? left)
    {
        total = null;
        right = null;
        left = null;
        if (!segment.StartsWith("PD", StringComparison.Ordinal))
        {
            return false;
        }

        var payload = segment[2..].Trim();
        var compactMatch = Regex.Match(payload, @"^(?<total>\d{2}\.\d)(?<right>\d{2}\.\d)(?<left>\d{2}\.\d)$");
        if (compactMatch.Success)
        {
            total = ParseDecimal(compactMatch.Groups["total"].Value);
            right = ParseDecimal(compactMatch.Groups["right"].Value);
            left = ParseDecimal(compactMatch.Groups["left"].Value);
            return true;
        }

        var values = NumericRegex.Matches(payload)
            .Select(match => ParseDecimal(match.Value))
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();
        if (values.Count == 0)
        {
            return true;
        }

        total = values.Count > 0 ? values[0] : null;
        right = values.Count > 1 ? values[1] : null;
        left = values.Count > 2 ? values[2] : null;
        return true;
    }

    private static bool TryParseEyeCount(string segment, out NidekRs232Eye eye, out int? count)
    {
        eye = NidekRs232Eye.Unknown;
        count = null;
        if (segment.Length < 3 || (segment[0] is not ('R' or 'L')) || !char.IsDigit(segment[1]) || !char.IsDigit(segment[2]))
        {
            return false;
        }

        eye = segment[0] == 'R' ? NidekRs232Eye.Right : NidekRs232Eye.Left;
        count = ParseInt(segment.Substring(1, 2));
        return true;
    }

    private static bool TryParseAveragePair(string segment, out decimal? averageMmHg, out decimal? averageKpa)
    {
        averageMmHg = null;
        averageKpa = null;
        var averageIndex = segment.IndexOf("AV", StringComparison.Ordinal);
        if (averageIndex < 0)
        {
            return false;
        }

        var averageText = segment[(averageIndex + 2)..];
        var values = NumericRegex.Matches(averageText)
            .Select(match => ParseDecimal(match.Value))
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();
        if (values.Count == 0)
        {
            return false;
        }

        averageMmHg = values[0];
        averageKpa = values.Count > 1 ? values[1] : null;
        return true;
    }

    private static string SegmentBeforeAverage(string segment)
    {
        var averageIndex = segment.IndexOf("AV", StringComparison.Ordinal);
        return averageIndex < 0 ? segment : segment[..averageIndex];
    }

    private static IEnumerable<string> FindErrorCodes(string segment, params string[] knownCodes)
    {
        return knownCodes.Where(code => segment.Contains(code, StringComparison.Ordinal));
    }

    private static bool ContainsOnlyKnownError(string segment)
    {
        return segment.Contains("ERR", StringComparison.Ordinal)
            || segment.Contains("OVR", StringComparison.Ordinal)
            || segment.Contains("APL", StringComparison.Ordinal)
            || segment.Contains("BLK", StringComparison.Ordinal)
            || segment.Contains("ALM", StringComparison.Ordinal);
    }

    private static string DetermineSegmentCode(string segment)
    {
        if (string.IsNullOrEmpty(segment))
        {
            return string.Empty;
        }

        if (segment.StartsWith("ID", StringComparison.Ordinal)
            || segment.StartsWith("NO", StringComparison.Ordinal)
            || segment.StartsWith("IP", StringComparison.Ordinal)
            || segment.StartsWith("DA", StringComparison.Ordinal)
            || segment.StartsWith("PD", StringComparison.Ordinal)
            || segment.StartsWith("AR", StringComparison.Ordinal)
            || segment.StartsWith("AL", StringComparison.Ordinal)
            || segment.StartsWith("NR", StringComparison.Ordinal)
            || segment.StartsWith("NL", StringComparison.Ordinal))
        {
            return segment[..2];
        }

        return char.IsLetter(segment[0]) ? segment[0].ToString() : string.Empty;
    }

    private static bool TryReadIdentity(string segment, out string? manufacturer, out string? model)
    {
        manufacturer = null;
        model = null;
        if (!segment.StartsWith("ID", StringComparison.Ordinal))
        {
            return false;
        }

        var value = segment[2..].Trim();
        var slashIndex = value.IndexOf('/');
        if (slashIndex < 0)
        {
            manufacturer = value;
            return true;
        }

        manufacturer = value[..slashIndex];
        model = value[(slashIndex + 1)..];
        return true;
    }

    private static DateTime? ParseNidekDateTime(string value, List<string> warnings)
    {
        if (DateTime.TryParseExact(
            value,
            "yyyy.MM.dd.HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed))
        {
            return parsed;
        }

        if (DateTime.TryParseExact(
            value,
            "yyyy.MM.dd.HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out parsed))
        {
            return parsed;
        }

        warnings.Add($"NIDEK-RS232-DA-Segment '{value}' konnte nicht sicher als Datum/Zeit gelesen werden.");
        return null;
    }

    private static string ResolveDeviceFamily(string header)
    {
        return header switch
        {
            "DLM" => "LM",
            "DNT" => "NT",
            "DPM" => "PM",
            _ => "Unknown"
        };
    }

    private static NidekRs232Eye ParseEyeCode(string eyeCode)
    {
        return eyeCode switch
        {
            " R" => NidekRs232Eye.Right,
            " L" => NidekRs232Eye.Left,
            "  " => NidekRs232Eye.Single,
            _ => NidekRs232Eye.Unknown
        };
    }

    private static decimal? ParseDecimal(string value)
    {
        return TryParseDecimal(value, out var parsed) ? parsed : null;
    }

    private static bool TryParseDecimal(string value, out decimal parsed)
    {
        return decimal.TryParse(value.Trim(), NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out parsed);
    }

    private static int? ParseInt(string value)
    {
        return int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
}
