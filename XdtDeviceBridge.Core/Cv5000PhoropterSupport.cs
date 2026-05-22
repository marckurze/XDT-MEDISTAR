using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace XdtDeviceBridge.Core;

public enum AisHistoricalMeasurementSourceKind
{
    Lensmeter,
    Autorefraction,
    Phoropter,
    Prescription,
    AutorefractionSubjective,
    Keratometry,
    Pachymetry,
    Tonometry,
    Unknown
}

public sealed record AisHistoricalEyeRefraction(
    string? Sphere,
    string? Cylinder,
    string? Axis,
    string? Add)
{
    public bool HasExportableRefraction =>
        !string.IsNullOrWhiteSpace(Sphere)
        && !string.IsNullOrWhiteSpace(Cylinder)
        && !string.IsNullOrWhiteSpace(Axis);
}

public sealed record AisHistoricalMeasurementRecord(
    DateOnly Date,
    string SourcePrefix,
    AisHistoricalMeasurementSourceKind SourceKind,
    string? Variant,
    IReadOnlyList<string> OriginalLines,
    AisHistoricalEyeRefraction? RightEye,
    AisHistoricalEyeRefraction? LeftEye,
    string? Pd,
    string? Vd,
    bool IsExportableToCv5000,
    IReadOnlyList<string> ParseWarnings);

public sealed record MedistarHistoricalMeasurementParseResult(
    PatientData Patient,
    IReadOnlyList<AisHistoricalMeasurementRecord> Records,
    IReadOnlyList<string> Warnings);

public sealed record Cv5000ImportSelection(
    PatientData Patient,
    IReadOnlyList<AisHistoricalMeasurementRecord> SelectedMeasurements,
    string? TargetFolder,
    string? TargetFileName);

public sealed record Cv5000ImportWriteResult(
    bool Success,
    string? TargetPath,
    string? XmlContent,
    IReadOnlyList<string> Warnings,
    string? ErrorMessage);

public sealed class MedistarHistoricalMeasurementParser
{
    private static readonly Regex HistoricalLineRegex = new(
        @"^(?<date>\d{2}\.\d{2}\.\d{4})\s+(?<prefix>V\d|P|Y)(?:\s+(?<variant>[FN]))?\s*(?<text>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex EyeRefractionRegex = new(
        @"(?<eye>[RL])\.\s*:S=\s*(?<sphere>[+-]\s*\d+(?:[.,]\d+)?)(?:\s+Z=\s*(?<cylinder>[+-]\s*\d+(?:[.,]\d+)?)\s*\*\s*(?<axis>\d+))?",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex AdditionRegex = new(
        @"\bA=\s*(?<value>[+-]\s*\d+(?:[.,]\d+)?)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex PdRegex = new(
        @"\bPD=\s*(?<value>\d+(?:[.,]\d+)?)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex VdRegex = new(
        @"\bVD=\s*(?<value>\d+(?:[.,]\d+)?)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public MedistarHistoricalMeasurementParseResult ParseFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must not be empty.", nameof(path));
        }

        var content = ReadText(path);
        return Parse(content);
    }

    public MedistarHistoricalMeasurementParseResult Parse(string content)
    {
        var patientNumber = string.Empty;
        var lastName = string.Empty;
        var firstName = string.Empty;
        var birthDate = string.Empty;
        var examinationType = string.Empty;
        var records = new List<RawHistoricalLine>();
        var warnings = new List<string>();
        var lineNumber = 0;

        using var reader = new StringReader(content ?? string.Empty);
        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            if (TryReadGdtField(line, out var fieldCode, out var value))
            {
                switch (fieldCode)
                {
                    case "3000":
                        patientNumber = value;
                        break;
                    case "3101":
                        lastName = value;
                        break;
                    case "3102":
                        firstName = value;
                        break;
                    case "3103":
                        birthDate = value;
                        break;
                    case "8402":
                        examinationType = value;
                        break;
                }

                continue;
            }

            var match = HistoricalLineRegex.Match(line.Trim());
            if (!match.Success)
            {
                continue;
            }

            if (!DateOnly.TryParseExact(
                    match.Groups["date"].Value,
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
            {
                warnings.Add($"Historienzeile {lineNumber}: Datum konnte nicht gelesen werden.");
                continue;
            }

            var prefix = match.Groups["prefix"].Value.Trim().ToUpperInvariant();
            records.Add(new RawHistoricalLine(
                date,
                prefix,
                match.Groups["variant"].Success ? match.Groups["variant"].Value.Trim().ToUpperInvariant() : null,
                line.Trim(),
                match.Groups["text"].Value.Trim(),
                lineNumber));
        }

        var grouped = records
            .GroupBy(line => new HistoricalGroupKey(line.Date, line.Prefix, line.Variant), HistoricalGroupKeyComparer.Instance)
            .Select(group => CreateRecord(group.Key, group.OrderBy(line => line.LineNumber).ToArray(), warnings))
            .OrderByDescending(record => record.Date)
            .ThenBy(record => record.SourcePrefix, StringComparer.OrdinalIgnoreCase)
            .ThenBy(record => record.Variant, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new MedistarHistoricalMeasurementParseResult(
            new PatientData(
                PatientNumber: EmptyToNull(patientNumber),
                LastName: EmptyToNull(lastName),
                FirstName: EmptyToNull(firstName),
                BirthDate: EmptyToNull(birthDate),
                PostalCodeCity: null,
                Street: null,
                GenderCode: null,
                SourceSystem: "MEDISTAR",
                TargetSystem: "XdtDeviceBridge",
                GdtVersion: null,
                ExaminationType: EmptyToNull(examinationType)),
            grouped,
            warnings);
    }

    public IReadOnlyList<AisHistoricalMeasurementRecord> CreateDefaultCv5000Selection(
        IEnumerable<AisHistoricalMeasurementRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        var candidates = records
            .Where(record => record.IsExportableToCv5000)
            .ToList();

        return new[]
            {
                AisHistoricalMeasurementSourceKind.Lensmeter,
                AisHistoricalMeasurementSourceKind.Autorefraction,
                AisHistoricalMeasurementSourceKind.Phoropter
            }
            .Select(kind => candidates
                .Where(record => record.SourceKind == kind)
                .OrderByDescending(record => record.Date)
                .FirstOrDefault())
            .Where(record => record is not null)
            .Select(record => record!)
            .ToArray();
    }

    private static AisHistoricalMeasurementRecord CreateRecord(
        HistoricalGroupKey key,
        IReadOnlyList<RawHistoricalLine> lines,
        List<string> globalWarnings)
    {
        AisHistoricalEyeRefraction? rightEye = null;
        AisHistoricalEyeRefraction? leftEye = null;
        string? pd = null;
        string? vd = null;
        var recordWarnings = new List<string>();

        foreach (var line in lines)
        {
            foreach (Match match in EyeRefractionRegex.Matches(line.Text))
            {
                var eye = CreateEyeRefraction(line.Text, match);
                if (string.Equals(match.Groups["eye"].Value, "R", StringComparison.OrdinalIgnoreCase))
                {
                    rightEye = eye;
                }
                else
                {
                    leftEye = eye;
                }
            }

            pd ??= ReadOptionalNumber(line.Text, PdRegex);
            vd ??= ReadOptionalNumber(line.Text, VdRegex);
        }

        var sourceKind = MapSourceKind(key.Prefix);
        var hasExportableEyes = rightEye?.HasExportableRefraction == true || leftEye?.HasExportableRefraction == true;
        var isExportable = IsCv5000RefractiveSource(sourceKind) && hasExportableEyes;
        if (IsCv5000RefractiveSource(sourceKind) && !isExportable)
        {
            var warning = $"{key.Date:dd.MM.yyyy} {key.Prefix}: keine vollständig exportierbare R-/L-Refraktionszeile gefunden.";
            recordWarnings.Add(warning);
            globalWarnings.Add(warning);
        }

        return new AisHistoricalMeasurementRecord(
            key.Date,
            key.Prefix,
            sourceKind,
            key.Variant,
            lines.Select(line => line.OriginalLine).ToArray(),
            rightEye,
            leftEye,
            pd,
            vd,
            isExportable,
            recordWarnings);
    }

    private static AisHistoricalEyeRefraction CreateEyeRefraction(string text, Match match)
    {
        var add = ReadOptionalNumber(text, AdditionRegex);
        return new AisHistoricalEyeRefraction(
            NormalizeNumber(match.Groups["sphere"].Value),
            NormalizeNumber(match.Groups["cylinder"].Value),
            NormalizeAxis(match.Groups["axis"].Value),
            add);
    }

    private static bool TryReadGdtField(string line, out string fieldCode, out string value)
    {
        fieldCode = string.Empty;
        value = string.Empty;

        if (string.IsNullOrWhiteSpace(line)
            || line.Length < 7
            || !line.Take(3).All(char.IsDigit)
            || !line.Skip(3).Take(4).All(char.IsDigit))
        {
            return false;
        }

        fieldCode = line.Substring(3, 4);
        value = line[7..].Trim();
        return true;
    }

    private static string ReadText(string path)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var bytes = File.ReadAllBytes(path);
        var utf8 = new UTF8Encoding(false, true);
        try
        {
            return utf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.GetEncoding(1252).GetString(bytes);
        }
    }

    private static AisHistoricalMeasurementSourceKind MapSourceKind(string sourcePrefix)
    {
        return sourcePrefix.ToUpperInvariant() switch
        {
            "V0" => AisHistoricalMeasurementSourceKind.Lensmeter,
            "V1" => AisHistoricalMeasurementSourceKind.Autorefraction,
            "V2" => AisHistoricalMeasurementSourceKind.Phoropter,
            "V3" => AisHistoricalMeasurementSourceKind.Prescription,
            "V4" => AisHistoricalMeasurementSourceKind.AutorefractionSubjective,
            "V7" => AisHistoricalMeasurementSourceKind.Keratometry,
            "P" => AisHistoricalMeasurementSourceKind.Pachymetry,
            "Y" => AisHistoricalMeasurementSourceKind.Tonometry,
            _ => AisHistoricalMeasurementSourceKind.Unknown
        };
    }

    private static bool IsCv5000RefractiveSource(AisHistoricalMeasurementSourceKind kind)
    {
        return kind is AisHistoricalMeasurementSourceKind.Lensmeter
            or AisHistoricalMeasurementSourceKind.Autorefraction
            or AisHistoricalMeasurementSourceKind.Phoropter
            or AisHistoricalMeasurementSourceKind.Prescription
            or AisHistoricalMeasurementSourceKind.AutorefractionSubjective;
    }

    private static string? ReadOptionalNumber(string text, Regex regex)
    {
        var match = regex.Match(text);
        return match.Success ? NormalizeNumber(match.Groups["value"].Value) : null;
    }

    private static string? EmptyToNull(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? NormalizeNumber(string value)
    {
        var normalized = value.Replace(" ", string.Empty, StringComparison.Ordinal).Replace(',', '.').Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string? NormalizeAxis(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record RawHistoricalLine(
        DateOnly Date,
        string Prefix,
        string? Variant,
        string OriginalLine,
        string Text,
        int LineNumber);

    private sealed record HistoricalGroupKey(DateOnly Date, string Prefix, string? Variant);

    private sealed class HistoricalGroupKeyComparer : IEqualityComparer<HistoricalGroupKey>
    {
        public static HistoricalGroupKeyComparer Instance { get; } = new();

        public bool Equals(HistoricalGroupKey? x, HistoricalGroupKey? y)
        {
            return x?.Date == y?.Date
                && string.Equals(x?.Prefix, y?.Prefix, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x?.Variant ?? string.Empty, y?.Variant ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(HistoricalGroupKey obj)
        {
            return HashCode.Combine(
                obj.Date,
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Prefix),
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Variant ?? string.Empty));
        }
    }
}

public sealed class TopconCv5000ImportXmlWriter
{
    private static readonly XNamespace CommonNamespace = "http://www.joia.or.jp/standardized/namespaces/Common";
    private static readonly XNamespace SbjNamespace = "http://www.joia.or.jp/standardized/namespaces/SBJ";
    private static readonly XNamespace XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

    public Cv5000ImportWriteResult BuildXml(Cv5000ImportSelection selection, DateTimeOffset? timestamp = null)
    {
        ArgumentNullException.ThrowIfNull(selection);

        var exportableRecords = selection.SelectedMeasurements
            .Where(record => record.IsExportableToCv5000)
            .ToArray();
        if (exportableRecords.Length == 0)
        {
            return new Cv5000ImportWriteResult(
                Success: false,
                TargetPath: null,
                XmlContent: null,
                Warnings: Array.Empty<string>(),
                ErrorMessage: "Keine exportierbaren refraktiven Messdatensätze für den CV-5000-Import ausgewählt.");
        }

        var now = timestamp ?? DateTimeOffset.Now;
        var document = new XDocument(
            new XDeclaration("1.0", "UTF-8", "no"),
            new XElement(
                "Ophthalmology",
                new XAttribute(XNamespace.Xmlns + "nsCommon", CommonNamespace),
                new XAttribute(XNamespace.Xmlns + "nsSBJ", SbjNamespace),
                new XAttribute(XNamespace.Xmlns + "xsi", XsiNamespace),
                new XAttribute(
                    XsiNamespace + "schemaLocation",
                    "http://www.joia.or.jp/standardized/namespaces/Common Common_schema.xsd http://www.joia.or.jp/standardized/namespaces/SBJ SBJ_schema.xsd"),
                CreateCommonElement(selection.Patient, now),
                new XElement(
                    SbjNamespace + "Measure",
                    new XAttribute("type", "SBJ"),
                    new XElement(
                        SbjNamespace + "RefractionTest",
                        exportableRecords.Select((record, index) => CreateTypeElement(record, index + 1))))));

        return new Cv5000ImportWriteResult(
            Success: true,
            TargetPath: CreateTargetPath(selection.TargetFolder, selection.TargetFileName),
            XmlContent: document.ToString(SaveOptions.None),
            Warnings: Array.Empty<string>(),
            ErrorMessage: null);
    }

    public Cv5000ImportWriteResult WriteFile(Cv5000ImportSelection selection, DateTimeOffset? timestamp = null)
    {
        var result = BuildXml(selection, timestamp);
        if (!result.Success || string.IsNullOrWhiteSpace(result.XmlContent))
        {
            return result;
        }

        if (string.IsNullOrWhiteSpace(result.TargetPath))
        {
            return result with
            {
                Success = false,
                ErrorMessage = "Kein Zielpfad für die CV-5000-Importdatei angegeben."
            };
        }

        var folder = Path.GetDirectoryName(result.TargetPath);
        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllText(result.TargetPath, result.XmlContent, new UTF8Encoding(false));
        return result;
    }

    private static XElement CreateCommonElement(PatientData patient, DateTimeOffset now)
    {
        return new XElement(
            CommonNamespace + "Common",
            new XElement(CommonNamespace + "Company", "Topcon Europe Medical B.V."),
            new XElement(CommonNamespace + "ModelName", "IMAGEnet i-base"),
            new XElement(CommonNamespace + "MachineNo", string.Empty),
            new XElement(CommonNamespace + "ROMVersion", string.Empty),
            new XElement(CommonNamespace + "Version", "3.24.0"),
            new XElement(CommonNamespace + "Date", now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            new XElement(CommonNamespace + "Time", now.ToString("HH:mm:ss", CultureInfo.InvariantCulture)),
            new XElement(
                CommonNamespace + "Patient",
                new XElement(CommonNamespace + "No.", patient.PatientNumber ?? string.Empty),
                new XElement(CommonNamespace + "ID", patient.PatientNumber ?? string.Empty),
                new XElement(CommonNamespace + "FirstName", patient.FirstName ?? string.Empty),
                new XElement(CommonNamespace + "MiddleName", string.Empty),
                new XElement(CommonNamespace + "LastName", patient.LastName ?? string.Empty),
                new XElement(CommonNamespace + "Sex", string.Empty),
                new XElement(CommonNamespace + "Age", string.Empty),
                new XElement(CommonNamespace + "DOB", patient.BirthDate ?? string.Empty),
                new XElement(CommonNamespace + "NameJ1", string.Empty),
                new XElement(CommonNamespace + "NameJ2", string.Empty)));
    }

    private static XElement CreateTypeElement(AisHistoricalMeasurementRecord record, int index)
    {
        return new XElement(
            SbjNamespace + "Type",
            new XAttribute("No", index.ToString(CultureInfo.InvariantCulture)),
            new XElement(SbjNamespace + "TypeName", CreateTypeName(record.SourceKind)),
            new XElement(
                SbjNamespace + "ExamDistance",
                new XAttribute("No", "1"),
                new XElement(SbjNamespace + "Distance", new XAttribute("unit", "cm"), "500.000"),
                CreateRefractionDataElement(record),
                CreatePdElement(record)));
    }

    private static XElement CreateRefractionDataElement(AisHistoricalMeasurementRecord record)
    {
        var refractionData = new XElement(SbjNamespace + "RefractionData");
        AddEyeElement(refractionData, "R", record.RightEye);
        AddEyeElement(refractionData, "L", record.LeftEye);
        if (!string.IsNullOrWhiteSpace(record.Vd))
        {
            refractionData.Add(new XElement(SbjNamespace + "VD", new XAttribute("unit", "mm"), FormatXmlDecimal(record.Vd)));
        }

        return refractionData;
    }

    private static void AddEyeElement(XElement refractionData, string eye, AisHistoricalEyeRefraction? values)
    {
        if (values?.HasExportableRefraction != true)
        {
            return;
        }

        refractionData.Add(
            new XElement(
                SbjNamespace + eye,
                new XElement(SbjNamespace + "Sph", new XAttribute("unit", "D"), FormatXmlDecimal(values.Sphere)),
                new XElement(SbjNamespace + "Cyl", new XAttribute("unit", "D"), FormatXmlDecimal(values.Cylinder)),
                new XElement(SbjNamespace + "Axis", new XAttribute("unit", "deg"), values.Axis?.Trim() ?? string.Empty)));
    }

    private static XElement? CreatePdElement(AisHistoricalMeasurementRecord record)
    {
        return string.IsNullOrWhiteSpace(record.Pd)
            ? null
            : new XElement(
                SbjNamespace + "PD",
                new XElement(SbjNamespace + "B", new XAttribute("unit", "mm"), FormatXmlDecimal(record.Pd)));
    }

    private static string CreateTypeName(AisHistoricalMeasurementSourceKind sourceKind)
    {
        return sourceKind switch
        {
            AisHistoricalMeasurementSourceKind.Lensmeter => "Lensmeter",
            AisHistoricalMeasurementSourceKind.Autorefraction => "Autorefraction",
            AisHistoricalMeasurementSourceKind.Phoropter => "Previous Phoropter",
            AisHistoricalMeasurementSourceKind.Prescription => "Last Prescription",
            AisHistoricalMeasurementSourceKind.AutorefractionSubjective => "Subjective Autorefraction",
            _ => "Refraction"
        };
    }

    private static string FormatXmlDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Replace(" ", string.Empty, StringComparison.Ordinal).Replace(',', '.').Trim();
        return decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            ? number.ToString("0.00", CultureInfo.InvariantCulture)
            : normalized;
    }

    private static string? CreateTargetPath(string? targetFolder, string? targetFileName)
    {
        if (string.IsNullOrWhiteSpace(targetFolder) || string.IsNullOrWhiteSpace(targetFileName))
        {
            return null;
        }

        return Path.Combine(targetFolder, Path.GetFileName(targetFileName));
    }
}
