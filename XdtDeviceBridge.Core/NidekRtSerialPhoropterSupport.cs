using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace XdtDeviceBridge.Core;

public static class NidekRtSerialPhoropterConstants
{
    public const string ParserMode = "NidekRtSerialPhoropter";
}

public static class NidekRtSerialControlChars
{
    public const byte SH = 0x01;
    public const byte SX = 0x02;
    public const byte ET = 0x04;
    public const byte CR = 0x0D;
    public const byte EB = 0x17;
}

public enum NidekRtSerialPhoropterModel
{
    Unknown,
    Rt2100,
    Rt3100,
    Rt5100
}

public enum NidekRtSerialDataSource
{
    Unknown,
    Lensmeter,
    Autorefraction,
    Wavefront,
    Refractor,
    Keratometry,
    Tonometry
}

public enum NidekRtSerialRefractionKind
{
    Lensmeter,
    Objective,
    Wavefront,
    Subjective,
    Final
}

public sealed record NidekRtSerialEyeRefraction(
    NidekRtSerialRefractionKind Kind,
    string Eye,
    string SourceCode,
    bool IsNight,
    decimal? Sphere,
    decimal? Cylinder,
    int? Axis,
    decimal? Add,
    decimal? Pd,
    decimal? VisualAcuity,
    decimal? WorkingDistance,
    string RawLine)
{
    public bool HasSphericalCylinderAxis => Sphere is not null && Cylinder is not null && Axis is not null;
}

public sealed record NidekRtSerialPhoropterParseResult(
    NidekRtSerialPhoropterModel Model,
    string? PatientId,
    DateOnly? MeasurementDate,
    string? SystemNo,
    IReadOnlyList<NidekRtSerialDataSource> Sources,
    IReadOnlyList<NidekRtSerialEyeRefraction> Refractions,
    IReadOnlyList<string> RawLines,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Errors)
{
    public bool HasErrors => Errors.Count > 0;
}

public sealed record NidekRtSerialPhoropterOutputResult(
    bool Success,
    byte[] Bytes,
    string VisibleContent,
    string HexDump,
    IReadOnlyList<string> Warnings,
    string? ErrorMessage);

public sealed class NidekRtSerialPhoropterParser
{
    private static readonly Regex HeaderModelRegex = new(
        @"NIDEK[_ ]?RT-?(?<model>2100|3100|5100)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex HeaderDateRegex = new(
        @"DA(?:Y)?(?<year>\d{4})/(?<month>\d{2})/(?<day>\d{2})(?:_(?<system>[A-Za-z0-9]+))?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex IdLineRegex = new(
        @"^(?:ID)?(?<id>[A-Za-z0-9][A-Za-z0-9 ]{0,19})$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex RefractionLineRegex = new(
        @"^(?<eye>[RL])(?<code>[A-Za-z]{0,2})(?<sph>[ +-]\d{2}\.\d{2})(?<cyl>[ +-]\d{2}\.\d{2})(?<axis>[ 0-9]{3})",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex CodeFirstRefractionLineRegex = new(
        @"^(?<code>[A-Za-z]{1,2})(?<eye>[RL])(?<sph>[ +-]?\s*\d{1,2}\.\d{2})(?<cyl>[ +-]?\s*\d{1,2}\.\d{2})(?<axis>[ 0-9]{3})",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex AddLineRegex = new(
        @"^(?<eye>[RL])(?<code>[aA])(?<add>[ +-]\d{2}\.\d{2})",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex CodeFirstAddLineRegex = new(
        @"^(?<code>[aA])(?<eye>[RL])(?<add>[ +-]?\s*\d{1,2}\.\d{2})",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex PdLineRegex = new(
        @"^(?<code>pD|PD)(?<values>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex VisualAcuityLineRegex = new(
        @"^(?<code>[vV])(?<eye>[RL])(?<va>.*)$|^(?<eyeLegacy>[RL])(?<codeLegacy>[vV])(?<vaLegacy>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex WorkingDistanceLineRegex = new(
        @"^(?<code>[wW]D)(?<wd>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static bool IsParserMode(string? parserMode)
    {
        return string.Equals(parserMode, NidekRtSerialPhoropterConstants.ParserMode, StringComparison.OrdinalIgnoreCase)
            || string.Equals(parserMode, "NIDEK RT Serial Phoropter", StringComparison.OrdinalIgnoreCase)
            || string.Equals(parserMode, "NidekRtSerial", StringComparison.OrdinalIgnoreCase);
    }

    public DeviceParseResult ParseFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must not be empty.", nameof(path));
        }

        return ParseDeviceFile(path);
    }

    public DeviceParseResult ParseDeviceFile(string path)
    {
        var bytes = File.ReadAllBytes(path);
        return ToDeviceParseResult(Parse(bytes), path);
    }

    public DeviceParseResult ParseDeviceText(string content, string sourcePath = "")
    {
        return ToDeviceParseResult(Parse(content), sourcePath);
    }

    public NidekRtSerialPhoropterParseResult Parse(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        return Parse(Encoding.ASCII.GetString(bytes));
    }

    public NidekRtSerialPhoropterParseResult Parse(string content)
    {
        var warnings = new List<string>();
        var errors = new List<string>();
        var refractions = new List<NidekRtSerialEyeRefraction>();
        var sources = new List<NidekRtSerialDataSource>();
        var lines = NormalizeLines(content).ToArray();
        if (lines.Length == 0)
        {
            errors.Add("NIDEK-RT-RS232-Daten sind leer.");
        }

        var model = NidekRtSerialPhoropterModel.Unknown;
        string? patientId = null;
        DateOnly? date = null;
        string? systemNo = null;
        var currentSource = NidekRtSerialDataSource.Unknown;
        var currentNight = false;
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line is "SH" or "SX" or "EB" or "ET")
            {
                continue;
            }

            model = model == NidekRtSerialPhoropterModel.Unknown ? DetectModel(line) : model;
            if (TryReadDate(line, out var detectedDate, out var detectedSystemNo))
            {
                date = detectedDate;
                systemNo ??= detectedSystemNo;
            }

            if (patientId is null && currentSource == NidekRtSerialDataSource.Unknown && TryReadPatientId(line, out var detectedId))
            {
                patientId = detectedId;
            }

            if (TryReadSource(line, out var source, out var night))
            {
                currentSource = source;
                currentNight = night;
                if (!sources.Contains(source))
                {
                    sources.Add(source);
                }

                continue;
            }

            if (currentSource == NidekRtSerialDataSource.Unknown)
            {
                continue;
            }

            if (TryReadRefraction(line, currentSource, currentNight, out var measurement))
            {
                UpsertRefraction(refractions, measurement);
                continue;
            }

            if (TryReadAdd(line, currentSource, currentNight, refractions, out var addWarning))
            {
                if (!string.IsNullOrWhiteSpace(addWarning))
                {
                    warnings.Add(addWarning);
                }

                continue;
            }

            if (TryReadPd(line, currentSource, currentNight, refractions, out addWarning))
            {
                if (!string.IsNullOrWhiteSpace(addWarning))
                {
                    warnings.Add(addWarning);
                }

                continue;
            }

            if (TryReadVisualAcuity(line, currentSource, currentNight, refractions, out addWarning))
            {
                if (!string.IsNullOrWhiteSpace(addWarning))
                {
                    warnings.Add(addWarning);
                }

                continue;
            }

            if (TryReadWorkingDistance(line, currentSource, currentNight, refractions, out addWarning))
            {
                if (!string.IsNullOrWhiteSpace(addWarning))
                {
                    warnings.Add(addWarning);
                }
            }
        }

        if (refractions.Count == 0 && errors.Count == 0)
        {
            warnings.Add("Keine Refraktionswerte in den NIDEK-RT-RS232-Daten erkannt.");
        }

        return new NidekRtSerialPhoropterParseResult(
            model,
            patientId,
            date,
            systemNo,
            sources,
            refractions,
            lines,
            warnings,
            errors);
    }

    private static DeviceParseResult ToDeviceParseResult(
        NidekRtSerialPhoropterParseResult result,
        string sourcePath)
    {
        var measurements = new List<MeasurementValue>();
        AddCommonMeasurements(result, measurements);
        foreach (var measurement in result.Refractions)
        {
            AddRefractionMeasurements(measurement, measurements);
        }

        AddMedistarLines(result.Refractions, measurements);

        var issues = result.Errors
            .Select(error => new DeviceParseIssue(DeviceParseIssueSeverity.Error, error, sourcePath, null))
            .Concat(result.Warnings.Select(warning => new DeviceParseIssue(DeviceParseIssueSeverity.Warning, warning, sourcePath, null)))
            .ToArray();

        return new DeviceParseResult(measurements, issues);
    }

    private static void AddCommonMeasurements(
        NidekRtSerialPhoropterParseResult result,
        List<MeasurementValue> measurements)
    {
        AddMeasurement(measurements, "Common/Company", "Company", "NIDEK", null, null, "Common");
        AddMeasurement(measurements, "Common/ModelName", "ModelName", FormatModel(result.Model), null, null, "Common");
        if (result.Sources.Count > 0)
        {
            AddMeasurement(measurements, "Measure[@Type='RTSERIAL']/Source", "RT Serial Datenquelle", string.Join(", ", result.Sources.Select(FormatSource)), null, null, "RTSERIAL");
        }

        if (!string.IsNullOrWhiteSpace(result.PatientId))
        {
            AddMeasurement(measurements, "Common/Patient/ID", "Patient ID", result.PatientId, null, null, "Common");
        }

        if (result.MeasurementDate is not null)
        {
            AddMeasurement(measurements, "Common/Date", "Date", result.MeasurementDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), null, null, "Common");
        }

        if (!string.IsNullOrWhiteSpace(result.SystemNo))
        {
            AddMeasurement(measurements, "Common/SystemNo", "SystemNo", result.SystemNo, null, null, "Common");
        }
    }

    private static void AddRefractionMeasurements(
        NidekRtSerialEyeRefraction measurement,
        List<MeasurementValue> measurements)
    {
        var path = $"Measure[@Type='RTSERIAL']/{GetPathSegment(measurement.Kind)}/{measurement.Eye}";
        AddMeasurement(measurements, $"{path}/Sphere", $"{measurement.Kind} {measurement.Eye} Sphere", FormatDecimal(measurement.Sphere), "dpt", measurement.Eye, "RTSERIAL");
        AddMeasurement(measurements, $"{path}/Cylinder", $"{measurement.Kind} {measurement.Eye} Cylinder", FormatDecimal(measurement.Cylinder), "dpt", measurement.Eye, "RTSERIAL");
        AddMeasurement(measurements, $"{path}/Axis", $"{measurement.Kind} {measurement.Eye} Axis", measurement.Axis?.ToString(CultureInfo.InvariantCulture), "deg", measurement.Eye, "RTSERIAL");
        AddMeasurement(measurements, $"{path}/ADD", $"{measurement.Kind} {measurement.Eye} ADD", FormatDecimal(measurement.Add), "dpt", measurement.Eye, "RTSERIAL");
        AddMeasurement(measurements, $"{path}/PD", $"{measurement.Kind} {measurement.Eye} PD", FormatPdMeasurement(measurement.Pd), "mm", measurement.Eye, "RTSERIAL");
        AddMeasurement(measurements, $"{path}/VA", $"{measurement.Kind} {measurement.Eye} VA", FormatPlainDecimal(measurement.VisualAcuity), null, measurement.Eye, "RTSERIAL");
        AddMeasurement(measurements, $"{path}/WorkingDistance", $"{measurement.Kind} {measurement.Eye} WD", FormatPlainDecimal(measurement.WorkingDistance), "cm", measurement.Eye, "RTSERIAL");
        AddMeasurement(measurements, $"{path}/RawLine", $"{measurement.Kind} {measurement.Eye} Rohzeile", measurement.RawLine, null, measurement.Eye, "RTSERIAL");
    }

    private static void AddMedistarLines(
        IReadOnlyList<NidekRtSerialEyeRefraction> refractions,
        List<MeasurementValue> measurements)
    {
        AddKindMedistarLines(
            refractions,
            measurements,
            NidekRtSerialRefractionKind.Final,
            "Final",
            "Phoropter finaler Verordnungswert");
        AddKindMedistarLines(
            refractions,
            measurements,
            NidekRtSerialRefractionKind.Subjective,
            "Subjective",
            "Phoropter Maximalwert (Vollkorrektion)");
    }

    private static void AddKindMedistarLines(
        IReadOnlyList<NidekRtSerialEyeRefraction> refractions,
        List<MeasurementValue> measurements,
        NidekRtSerialRefractionKind kind,
        string segment,
        string header)
    {
        var candidates = refractions
            .Where(measurement => measurement.Kind == kind && measurement.HasSphericalCylinderAxis)
            .ToArray();
        if (candidates.Length == 0)
        {
            return;
        }

        AddMeasurement(
            measurements,
            $"Measure[@Type='RTSERIAL']/{segment}/HeaderLine",
            $"{segment} MEDISTAR-Header",
            header,
            null,
            null,
            "RTSERIAL");

        foreach (var candidate in candidates.Where(candidate => candidate.Eye is "R" or "L"))
        {
            AddMeasurement(
                measurements,
                $"Measure[@Type='RTSERIAL']/{segment}/{candidate.Eye}/MedistarLine",
                $"{segment} {candidate.Eye} MEDISTAR-Zeile",
                BuildMedistarLine(candidate),
                null,
                candidate.Eye,
                "RTSERIAL");
        }
    }

    private static string BuildMedistarLine(NidekRtSerialEyeRefraction measurement)
    {
        var formatter = new MedistarResultFormatter();
        var builder = new StringBuilder();
        builder.Append(measurement.Eye);
        builder.Append(".:S=");
        builder.Append(formatter.FormatDiopter(FormatDecimal(measurement.Sphere)));
        builder.Append(" Z=");
        builder.Append(formatter.FormatDiopter(FormatDecimal(measurement.Cylinder)));
        builder.Append('*');
        builder.Append(formatter.FormatAxis(measurement.Axis?.ToString(CultureInfo.InvariantCulture)));

        if (measurement.Add is not null)
        {
            builder.Append(" A=");
            builder.Append(formatter.FormatDiopter(FormatDecimal(measurement.Add)));
        }

        if (measurement.Pd is not null)
        {
            builder.Append(" PD= ");
            builder.Append(formatter.FormatPd(FormatPlainDecimal(measurement.Pd)));
        }

        return builder.ToString();
    }

    private static void AddMeasurement(
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

    private static IEnumerable<string> NormalizeLines(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            yield break;
        }

        var normalized = content
            .Replace(((char)NidekRtSerialControlChars.SH).ToString(), "\nSH ", StringComparison.Ordinal)
            .Replace(((char)NidekRtSerialControlChars.SX).ToString(), "\nSX ", StringComparison.Ordinal)
            .Replace(((char)NidekRtSerialControlChars.EB).ToString(), "\nEB\n", StringComparison.Ordinal)
            .Replace(((char)NidekRtSerialControlChars.ET).ToString(), "\nET\n", StringComparison.Ordinal)
            .Replace(((char)NidekRtSerialControlChars.CR).ToString(), "\n", StringComparison.Ordinal)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');

        foreach (var part in normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var line = part.Trim();
            if (line.StartsWith("SH ", StringComparison.Ordinal))
            {
                line = line[3..].Trim();
            }
            else if (line.StartsWith("SX ", StringComparison.Ordinal))
            {
                line = line[3..].Trim();
            }

            if (line.Length > 0)
            {
                yield return line;
            }
        }
    }

    private static NidekRtSerialPhoropterModel DetectModel(string line)
    {
        var match = HeaderModelRegex.Match(line);
        if (!match.Success)
        {
            return NidekRtSerialPhoropterModel.Unknown;
        }

        return match.Groups["model"].Value switch
        {
            "2100" => NidekRtSerialPhoropterModel.Rt2100,
            "3100" => NidekRtSerialPhoropterModel.Rt3100,
            "5100" => NidekRtSerialPhoropterModel.Rt5100,
            _ => NidekRtSerialPhoropterModel.Unknown
        };
    }

    private static bool TryReadDate(string line, out DateOnly? date, out string? systemNo)
    {
        date = null;
        systemNo = null;
        var match = HeaderDateRegex.Match(line);
        if (!match.Success)
        {
            return false;
        }

        if (DateOnly.TryParseExact(
                $"{match.Groups["year"].Value}-{match.Groups["month"].Value}-{match.Groups["day"].Value}",
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            date = parsed;
            systemNo = match.Groups["system"].Success ? match.Groups["system"].Value : null;
            return true;
        }

        return false;
    }

    private static bool TryReadPatientId(string line, out string? patientId)
    {
        patientId = null;
        if (line.StartsWith("NIDEK", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("DA", StringComparison.OrdinalIgnoreCase)
            || TryReadSource(line, out _, out _))
        {
            return false;
        }

        var match = IdLineRegex.Match(line);
        if (!match.Success)
        {
            return false;
        }

        var value = match.Groups["id"].Value.Trim();
        if (value.Length == 0)
        {
            return false;
        }

        patientId = value;
        return true;
    }

    private static bool TryReadSource(string line, out NidekRtSerialDataSource source, out bool isNight)
    {
        source = NidekRtSerialDataSource.Unknown;
        isNight = false;
        var value = line.Trim();
        if (value == "@")
        {
            return false;
        }

        if (value.StartsWith('@'))
        {
            value = value[1..].Trim();
        }

        isNight = value.Length > 0 && value.All(character => !char.IsLetter(character) || char.IsLower(character));
        switch (value.ToUpperInvariant())
        {
            case "LM":
                source = NidekRtSerialDataSource.Lensmeter;
                return true;
            case "RM":
            case "AR":
                source = NidekRtSerialDataSource.Autorefraction;
                return true;
            case "WF":
                source = NidekRtSerialDataSource.Wavefront;
                return true;
            case "RT":
                source = NidekRtSerialDataSource.Refractor;
                return true;
            case "KM":
                source = NidekRtSerialDataSource.Keratometry;
                return true;
            case "NT":
                source = NidekRtSerialDataSource.Tonometry;
                return true;
            default:
                return false;
        }
    }

    private static bool TryReadRefraction(
        string line,
        NidekRtSerialDataSource source,
        bool isNight,
        out NidekRtSerialEyeRefraction measurement)
    {
        measurement = default!;
        var match = RefractionLineRegex.Match(line);
        if (!match.Success)
        {
            match = CodeFirstRefractionLineRegex.Match(line);
            if (!match.Success)
            {
                return false;
            }
        }

        var code = match.Groups["code"].Value;
        var kind = CreateKind(source, code);
        if (kind is null)
        {
            return false;
        }

        measurement = new NidekRtSerialEyeRefraction(
            kind.Value,
            match.Groups["eye"].Value.ToUpperInvariant(),
            code,
            isNight || code.Any(char.IsLower),
            ParseSignedDecimal(match.Groups["sph"].Value),
            ParseSignedDecimal(match.Groups["cyl"].Value),
            ParseAxis(match.Groups["axis"].Value),
            Add: null,
            Pd: null,
            VisualAcuity: null,
            WorkingDistance: null,
            line);
        return true;
    }

    private static bool TryReadAdd(
        string line,
        NidekRtSerialDataSource source,
        bool isNight,
        List<NidekRtSerialEyeRefraction> refractions,
        out string? warning)
    {
        warning = null;
        var match = AddLineRegex.Match(line);
        if (!match.Success)
        {
            match = CodeFirstAddLineRegex.Match(line);
            if (!match.Success)
            {
                return false;
            }
        }

        var kind = CreateKind(source, match.Groups["code"].Value);
        if (kind is null)
        {
            return false;
        }

        var eye = match.Groups["eye"].Value.ToUpperInvariant();
        var current = refractions.LastOrDefault(refraction => refraction.Kind == kind && refraction.Eye == eye);
        if (current is null)
        {
            warning = $"ADD-Zeile ohne passende SCA-Zeile ignoriert: {line}";
            return true;
        }

        refractions[refractions.IndexOf(current)] = current with
        {
            Add = ParseSignedDecimal(match.Groups["add"].Value),
            IsNight = current.IsNight || isNight
        };
        return true;
    }

    private static bool TryReadPd(
        string line,
        NidekRtSerialDataSource source,
        bool isNight,
        List<NidekRtSerialEyeRefraction> refractions,
        out string? warning)
    {
        warning = null;
        var match = PdLineRegex.Match(line);
        if (!match.Success)
        {
            return false;
        }

        var kind = CreateKind(source, match.Groups["code"].Value);
        if (kind is null)
        {
            return false;
        }

        var pd = ParsePdValue(match.Groups["values"].Value);
        if (pd is null)
        {
            warning = $"PD-Zeile ohne lesbaren PD-Wert ignoriert: {line}";
            return true;
        }

        foreach (var current in refractions.Where(refraction => refraction.Kind == kind).ToArray())
        {
            var index = refractions.IndexOf(current);
            refractions[index] = current with
            {
                Pd = pd,
                IsNight = current.IsNight || isNight
            };
        }

        return true;
    }

    private static bool TryReadVisualAcuity(
        string line,
        NidekRtSerialDataSource source,
        bool isNight,
        List<NidekRtSerialEyeRefraction> refractions,
        out string? warning)
    {
        warning = null;
        var match = VisualAcuityLineRegex.Match(line);
        if (!match.Success)
        {
            return false;
        }

        var code = match.Groups["code"].Success ? match.Groups["code"].Value : match.Groups["codeLegacy"].Value;
        var eye = (match.Groups["eye"].Success ? match.Groups["eye"].Value : match.Groups["eyeLegacy"].Value).ToUpperInvariant();
        var valueText = match.Groups["va"].Success ? match.Groups["va"].Value : match.Groups["vaLegacy"].Value;
        var kind = CreateKind(source, code);
        if (kind is null)
        {
            return false;
        }

        var va = ParseCompactDecimal(valueText);
        var current = refractions.LastOrDefault(refraction => refraction.Kind == kind && refraction.Eye == eye);
        if (current is null)
        {
            warning = $"VA-Zeile ohne passende SCA-Zeile ignoriert: {line}";
            return true;
        }

        refractions[refractions.IndexOf(current)] = current with
        {
            VisualAcuity = va,
            IsNight = current.IsNight || isNight
        };
        return true;
    }

    private static bool TryReadWorkingDistance(
        string line,
        NidekRtSerialDataSource source,
        bool isNight,
        List<NidekRtSerialEyeRefraction> refractions,
        out string? warning)
    {
        warning = null;
        var match = WorkingDistanceLineRegex.Match(line);
        if (!match.Success)
        {
            return false;
        }

        var kind = CreateKind(source, match.Groups["code"].Value);
        if (kind is null)
        {
            return false;
        }

        var workingDistance = ParseCompactDecimal(match.Groups["wd"].Value);
        if (workingDistance is null)
        {
            warning = $"WD-Zeile ohne lesbaren Working-Distance-Wert ignoriert: {line}";
            return true;
        }

        foreach (var current in refractions.Where(refraction => refraction.Kind == kind).ToArray())
        {
            var index = refractions.IndexOf(current);
            refractions[index] = current with
            {
                WorkingDistance = workingDistance,
                IsNight = current.IsNight || isNight
            };
        }

        return true;
    }

    private static NidekRtSerialRefractionKind? CreateKind(NidekRtSerialDataSource source, string code)
    {
        return source switch
        {
            NidekRtSerialDataSource.Lensmeter => NidekRtSerialRefractionKind.Lensmeter,
            NidekRtSerialDataSource.Autorefraction => NidekRtSerialRefractionKind.Objective,
            NidekRtSerialDataSource.Wavefront => NidekRtSerialRefractionKind.Wavefront,
            NidekRtSerialDataSource.Refractor when code.Length > 0 && char.IsUpper(code[0]) => NidekRtSerialRefractionKind.Final,
            NidekRtSerialDataSource.Refractor when code.Length > 0 && char.IsLower(code[0]) => NidekRtSerialRefractionKind.Subjective,
            _ => null
        };
    }

    private static void UpsertRefraction(List<NidekRtSerialEyeRefraction> refractions, NidekRtSerialEyeRefraction measurement)
    {
        var existing = refractions.FindIndex(refraction =>
            refraction.Kind == measurement.Kind
            && string.Equals(refraction.Eye, measurement.Eye, StringComparison.OrdinalIgnoreCase));
        if (existing < 0)
        {
            refractions.Add(measurement);
            return;
        }

        refractions[existing] = measurement with
        {
            Add = refractions[existing].Add,
            Pd = refractions[existing].Pd,
            VisualAcuity = refractions[existing].VisualAcuity,
            WorkingDistance = refractions[existing].WorkingDistance
        };
    }

    private static decimal? ParseSignedDecimal(string text)
    {
        var normalized = text.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);
        if (normalized.Length == 0)
        {
            return null;
        }

        if (normalized[0] != '+' && normalized[0] != '-')
        {
            normalized = "+" + normalized;
        }

        return decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static decimal? ParseCompactDecimal(string text)
    {
        var normalized = text.Trim();
        if (normalized.Length == 0)
        {
            return null;
        }

        return decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static decimal? ParsePdValue(string text)
    {
        var normalized = text.Trim();
        if (normalized.Length == 0)
        {
            return null;
        }

        if (normalized.Contains('.', StringComparison.Ordinal))
        {
            var token = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return string.IsNullOrWhiteSpace(token) ? null : ParseCompactDecimal(token);
        }

        var digits = new string(normalized.Where(char.IsDigit).ToArray());
        return digits.Length switch
        {
            >= 4 => ParseCompactDecimal(digits[..4]),
            > 0 => ParseCompactDecimal(digits),
            _ => null
        };
    }

    private static int? ParseAxis(string text)
    {
        var normalized = text.Trim();
        if (normalized.Length == 0)
        {
            return null;
        }

        return int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static string GetPathSegment(NidekRtSerialRefractionKind kind)
    {
        return kind switch
        {
            NidekRtSerialRefractionKind.Lensmeter => "Lensmeter",
            NidekRtSerialRefractionKind.Objective => "Objective",
            NidekRtSerialRefractionKind.Wavefront => "Wavefront",
            NidekRtSerialRefractionKind.Subjective => "Subjective",
            NidekRtSerialRefractionKind.Final => "Final",
            _ => kind.ToString()
        };
    }

    private static string FormatModel(NidekRtSerialPhoropterModel model)
    {
        return model switch
        {
            NidekRtSerialPhoropterModel.Rt2100 => "RT-2100",
            NidekRtSerialPhoropterModel.Rt3100 => "RT-3100",
            NidekRtSerialPhoropterModel.Rt5100 => "RT-5100",
            _ => "NIDEK RT Serial"
        };
    }

    private static string FormatSource(NidekRtSerialDataSource source)
    {
        return source switch
        {
            NidekRtSerialDataSource.Lensmeter => "LM",
            NidekRtSerialDataSource.Autorefraction => "RM/AR",
            NidekRtSerialDataSource.Wavefront => "WF",
            NidekRtSerialDataSource.Refractor => "RT",
            NidekRtSerialDataSource.Keratometry => "KM",
            NidekRtSerialDataSource.Tonometry => "NT",
            _ => source.ToString()
        };
    }

    private static string FormatDecimal(decimal? value)
    {
        return value?.ToString("+0.00;-0.00;0.00", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static string FormatPlainDecimal(decimal? value)
    {
        return value?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static string FormatPdMeasurement(decimal? value)
    {
        return value?.ToString("0.0#", CultureInfo.InvariantCulture) ?? string.Empty;
    }
}

public sealed class NidekRtSerialPhoropterOutputWriter
{
    public const string DeviceOutputFormat = "NIDEK RT Serial ASCII";
    public const string DefaultFileNameTemplate = "NIDEK_RT_Serial_Send.txt";

    public NidekRtSerialPhoropterOutputResult BuildFrame(
        PatientData patient,
        IReadOnlyList<AisHistoricalMeasurementRecord> selectedMeasurements,
        NidekRtSerialPhoropterModel model)
    {
        ArgumentNullException.ThrowIfNull(patient);
        ArgumentNullException.ThrowIfNull(selectedMeasurements);

        var warnings = new List<string>();
        var blocks = new List<byte[]>();
        if (model is NidekRtSerialPhoropterModel.Rt3100 or NidekRtSerialPhoropterModel.Rt5100)
        {
            blocks.Add(BuildIdBlock(patient.PatientNumber));
        }

        var ar = selectedMeasurements.FirstOrDefault(record => record.SourceKind == AisHistoricalMeasurementSourceKind.Autorefraction);
        if (ar is not null)
        {
            blocks.Add(BuildSphericalCylinderAxisBlock("DRM", "O", ar));
            blocks.Add(BuildPdBlock("DPM", ar));
        }

        var lm = selectedMeasurements.FirstOrDefault(record => record.SourceKind == AisHistoricalMeasurementSourceKind.Lensmeter);
        if (lm is not null)
        {
            blocks.Add(BuildSphericalCylinderAxisBlock("DLM", "*", lm));
            blocks.Add(BuildAddBlock(lm));
            blocks.Add(BuildPdBlock("DLM", lm));
        }

        blocks = blocks.Where(block => block.Length > 0).ToList();
        if (blocks.Count == 0)
        {
            return new NidekRtSerialPhoropterOutputResult(
                false,
                Array.Empty<byte>(),
                string.Empty,
                string.Empty,
                warnings,
                "Keine exportierbaren LM-/AR-Daten fuer NIDEK RT-Serial-Ausgabe vorhanden.");
        }

        var bytes = new List<byte> { NidekRtSerialControlChars.SH };
        for (var index = 0; index < blocks.Count; index++)
        {
            if (index > 0)
            {
                bytes.Add(NidekRtSerialControlChars.EB);
            }

            bytes.AddRange(blocks[index]);
        }

        bytes.Add(NidekRtSerialControlChars.ET);
        return new NidekRtSerialPhoropterOutputResult(
            true,
            bytes.ToArray(),
            ToVisibleText(bytes.ToArray()),
            ToHexDump(bytes.ToArray()),
            warnings,
            null);
    }

    private static byte[] BuildIdBlock(string? patientNumber)
    {
        var id = new string((patientNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        if (id.Length > 20)
        {
            id = id[^20..];
        }

        var padded = id.Length <= 12
            ? id.PadLeft(12)
            : id.PadLeft(20);

        return Concat(Encode("DRL"), new[] { NidekRtSerialControlChars.SX }, Encode(padded));
    }

    private static byte[] BuildSphericalCylinderAxisBlock(
        string blockCode,
        string eyePrefix,
        AisHistoricalMeasurementRecord record)
    {
        var parts = new List<byte>();
        AddEyeSphericalCylinderAxis(parts, blockCode, eyePrefix, "R", record.RightEye);
        AddEyeSphericalCylinderAxis(parts, null, eyePrefix, "L", record.LeftEye);
        return parts.ToArray();
    }

    private static void AddEyeSphericalCylinderAxis(
        List<byte> parts,
        string? blockCode,
        string eyePrefix,
        string eye,
        AisHistoricalEyeRefraction? refraction)
    {
        if (refraction?.HasExportableRefraction != true)
        {
            return;
        }

        if (parts.Count > 0)
        {
            parts.Add(NidekRtSerialControlChars.EB);
        }

        if (!string.IsNullOrWhiteSpace(blockCode))
        {
            parts.AddRange(Encode(blockCode));
            parts.Add(NidekRtSerialControlChars.SX);
        }

        parts.AddRange(Encode($"{eyePrefix}{eye}{FormatDiopterField(refraction.Sphere)}{FormatDiopterField(refraction.Cylinder)}{FormatAxisField(refraction.Axis)}"));
    }

    private static byte[] BuildAddBlock(AisHistoricalMeasurementRecord record)
    {
        var parts = new List<byte>();
        AddEyeAdd(parts, "ALM", "R", record.RightEye?.Add);
        AddEyeAdd(parts, null, "L", record.LeftEye?.Add);
        return parts.ToArray();
    }

    private static void AddEyeAdd(List<byte> parts, string? blockCode, string eye, string? add)
    {
        if (string.IsNullOrWhiteSpace(add))
        {
            return;
        }

        if (parts.Count > 0)
        {
            parts.Add(NidekRtSerialControlChars.EB);
        }

        if (!string.IsNullOrWhiteSpace(blockCode))
        {
            parts.AddRange(Encode(blockCode));
            parts.Add(NidekRtSerialControlChars.SX);
        }

        parts.AddRange(Encode($"{eye}A{FormatDiopterField(add)}"));
    }

    private static byte[] BuildPdBlock(string blockCode, AisHistoricalMeasurementRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.Pd))
        {
            return Array.Empty<byte>();
        }

        var normalized = NormalizeDecimal(record.Pd);
        if (normalized is null)
        {
            return Array.Empty<byte>();
        }

        var pdText = Math.Round(normalized.Value, 0, MidpointRounding.AwayFromZero)
            .ToString("0000", CultureInfo.InvariantCulture);
        return Concat(Encode(blockCode), new[] { NidekRtSerialControlChars.SX }, Encode($"PD{pdText}"));
    }

    private static string FormatDiopterField(string? value)
    {
        var number = NormalizeDecimal(value) ?? 0m;
        return number.ToString("+00.00;-00.00; 00.00", CultureInfo.InvariantCulture);
    }

    private static string FormatAxisField(string? value)
    {
        if (!int.TryParse(value?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var axis))
        {
            axis = 0;
        }

        return axis.ToString("000", CultureInfo.InvariantCulture);
    }

    private static decimal? NormalizeDecimal(string? value)
    {
        var normalized = value?.Trim().Replace(" ", string.Empty, StringComparison.Ordinal).Replace(',', '.');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static byte[] Encode(string text)
    {
        return Encoding.ASCII.GetBytes(text);
    }

    private static byte[] Concat(params byte[][] parts)
    {
        var bytes = new List<byte>();
        foreach (var part in parts)
        {
            bytes.AddRange(part);
        }

        return bytes.ToArray();
    }

    private static string ToVisibleText(byte[] bytes)
    {
        var builder = new StringBuilder();
        foreach (var value in bytes)
        {
            builder.Append(value switch
            {
                NidekRtSerialControlChars.SH => "<SH>",
                NidekRtSerialControlChars.SX => "<SX>",
                NidekRtSerialControlChars.EB => "<EB>",
                NidekRtSerialControlChars.ET => "<ET>",
                NidekRtSerialControlChars.CR => "<CR>",
                _ => ((char)value).ToString()
            });
        }

        return builder.ToString();
    }

    private static string ToHexDump(byte[] bytes)
    {
        return string.Join(" ", bytes.Select(value => value.ToString("X2", CultureInfo.InvariantCulture)));
    }
}
