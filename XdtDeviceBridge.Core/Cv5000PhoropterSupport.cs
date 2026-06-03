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
    IReadOnlyList<string> ParseWarnings,
    string? WorkingDistance = null);

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

public sealed record NidekRt6100InputSourceParseResult(
    IReadOnlyList<AisHistoricalMeasurementRecord> Records,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Errors)
{
    public bool Success => Errors.Count == 0;
}

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
            var historicalCandidate = line.Trim();
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

                historicalCandidate = value.Trim();
            }

            var match = HistoricalLineRegex.Match(historicalCandidate);
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

    public IReadOnlyList<AisHistoricalMeasurementRecord> CreateDefaultRt6100Selection(
        IEnumerable<AisHistoricalMeasurementRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        var candidates = records
            .Where(record => record.IsExportableToCv5000)
            .ToList();

        return new[]
            {
                AisHistoricalMeasurementSourceKind.Lensmeter,
                AisHistoricalMeasurementSourceKind.Autorefraction
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

    public Cv5000ImportWriteResult WriteFile(
        Cv5000ImportSelection selection,
        InterfaceProfileDefinition interfaceProfile,
        DateTimeOffset? timestamp = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(interfaceProfile);

        var output = interfaceProfile.DeviceOutput;
        if (output is null)
        {
            return new Cv5000ImportWriteResult(
                Success: false,
                TargetPath: null,
                XmlContent: null,
                Warnings: Array.Empty<string>(),
                ErrorMessage: "Keine Ausgabe-an-Gerät-Konfiguration im Schnittstellenprofil vorhanden.");
        }

        if (!output.IsEnabled)
        {
            return new Cv5000ImportWriteResult(
                Success: false,
                TargetPath: null,
                XmlContent: null,
                Warnings: Array.Empty<string>(),
                ErrorMessage: "Ausgabe an Gerät ist im Schnittstellenprofil nicht aktiv.");
        }

        if (string.IsNullOrWhiteSpace(output.OutputFolder))
        {
            return new Cv5000ImportWriteResult(
                Success: false,
                TargetPath: null,
                XmlContent: null,
                Warnings: Array.Empty<string>(),
                ErrorMessage: "Ausgabeordner an Gerät fehlt.");
        }

        var configuredSelection = selection with
        {
            TargetFolder = output.OutputFolder,
            TargetFileName = string.IsNullOrWhiteSpace(output.FileNameTemplate)
                ? "CVImport.xml"
                : output.FileNameTemplate
        };

        return WriteFile(configuredSelection, timestamp);
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

public sealed class NidekRt6100InputSourceXmlReader
{
    public NidekRt6100InputSourceParseResult ParseFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must not be empty.", nameof(path));
        }

        try
        {
            var document = XDocument.Load(path, LoadOptions.PreserveWhitespace);
            return Parse(document);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Xml.XmlException)
        {
            return new NidekRt6100InputSourceParseResult(
                Array.Empty<AisHistoricalMeasurementRecord>(),
                Array.Empty<string>(),
                new[] { $"NIDEK RT-6100 Eingangsquelle konnte nicht gelesen werden: {ex.Message}" });
        }
    }

    public NidekRt6100InputSourceParseResult Parse(XDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var root = document.Root;
        if (root is null)
        {
            return CreateUnsupported("XML-Dokument hat kein Wurzelelement.");
        }

        var common = Child(root, "Common") ?? root;
        var company = Value(common, "Company");
        var modelName = Value(common, "ModelName");
        if (!string.Equals(company?.Trim(), "NIDEK", StringComparison.OrdinalIgnoreCase))
        {
            return CreateUnsupported("Keine NIDEK-XML-Eingangsquelle.");
        }

        if (ContainsModel(modelName, "LM-7") || FindMeasure(root, "LM") is not null)
        {
            return ParseLensmeter(root, common, modelName);
        }

        if (ContainsModel(modelName, "ARK") || Child(Child(root, "R"), "AR") is not null || Child(Child(root, "L"), "AR") is not null)
        {
            return ParseAutorefraction(root, common, modelName);
        }

        return CreateUnsupported($"NIDEK-XML-Modell '{modelName ?? "-"}' ist keine RT-6100 LM-/AR-Eingangsquelle.");
    }

    private static NidekRt6100InputSourceParseResult ParseLensmeter(XElement root, XElement common, string? modelName)
    {
        var warnings = new List<string>();
        var measure = FindMeasure(root, "LM");
        var lm = Child(measure, "LM") ?? Child(root, "LM");
        if (lm is null)
        {
            return new NidekRt6100InputSourceParseResult(
                Array.Empty<AisHistoricalMeasurementRecord>(),
                Array.Empty<string>(),
                new[] { "NIDEK LM-XML enthaelt keinen LM-Messblock." });
        }

        var right = ParseEye(Child(lm, "R"));
        var left = ParseEye(Child(lm, "L"));
        AddIgnoredPrismWarnings(warnings, lm, modelName ?? "NIDEK LM");
        var record = CreateRecord(
            common,
            sourcePrefix: "LM",
            sourceKind: AisHistoricalMeasurementSourceKind.Lensmeter,
            originalLine: $"{modelName ?? "NIDEK LM"} {Value(common, "Date")} {Value(common, "Time")}".Trim(),
            right,
            left,
            pd: Value(Child(lm, "B"), "PD") ?? Value(lm, "PD"),
            vd: null,
            workingDistance: null,
            warnings);

        return record.IsExportableToCv5000
            ? new NidekRt6100InputSourceParseResult(new[] { record }, warnings, Array.Empty<string>())
            : new NidekRt6100InputSourceParseResult(
                Array.Empty<AisHistoricalMeasurementRecord>(),
                warnings,
                new[] { "NIDEK LM-XML enthaelt keine vollstaendig exportierbaren R-/L-Refraktionswerte." });
    }

    private static NidekRt6100InputSourceParseResult ParseAutorefraction(XElement root, XElement common, string? modelName)
    {
        var warnings = new List<string>();
        var right = ParseArkEye(Child(root, "R"), warnings);
        var left = ParseArkEye(Child(root, "L"), warnings);
        AddArkIgnoredDataWarnings(root, warnings);
        var record = CreateRecord(
            common,
            sourcePrefix: "AR",
            sourceKind: AisHistoricalMeasurementSourceKind.Autorefraction,
            originalLine: $"{modelName ?? "NIDEK ARK"} {Value(common, "Date")} {Value(common, "Time")}".Trim(),
            right,
            left,
            pd: Value(Child(root, "B"), "PD") ?? Value(root, "PD"),
            vd: ReadUnitNumber(Value(root, "VD")),
            workingDistance: ReadUnitNumber(Value(root, "WorkingDistance")),
            warnings);

        return record.IsExportableToCv5000
            ? new NidekRt6100InputSourceParseResult(new[] { record }, warnings, Array.Empty<string>())
            : new NidekRt6100InputSourceParseResult(
                Array.Empty<AisHistoricalMeasurementRecord>(),
                warnings,
                new[] { "NIDEK ARK-XML enthaelt keine vollstaendig exportierbaren ARMedian-R-/L-Refraktionswerte." });
    }

    private static AisHistoricalMeasurementRecord CreateRecord(
        XElement common,
        string sourcePrefix,
        AisHistoricalMeasurementSourceKind sourceKind,
        string originalLine,
        AisHistoricalEyeRefraction? right,
        AisHistoricalEyeRefraction? left,
        string? pd,
        string? vd,
        string? workingDistance,
        IReadOnlyList<string> warnings)
    {
        var date = ParseDate(Value(common, "Date")) ?? DateOnly.FromDateTime(DateTime.Today);
        var isExportable = right?.HasExportableRefraction == true || left?.HasExportableRefraction == true;
        return new AisHistoricalMeasurementRecord(
            date,
            sourcePrefix,
            sourceKind,
            Variant: null,
            OriginalLines: new[] { originalLine },
            RightEye: right,
            LeftEye: left,
            Pd: NormalizeNumber(pd),
            Vd: NormalizeNumber(vd),
            IsExportableToCv5000: isExportable,
            ParseWarnings: warnings,
            WorkingDistance: NormalizeNumber(workingDistance));
    }

    private static AisHistoricalEyeRefraction? ParseArkEye(XElement? eye, List<string> warnings)
    {
        var ar = Child(eye, "AR");
        var median = Child(ar, "ARMedian");
        if (median is null)
        {
            var trialLens = Child(ar, "TrialLens");
            if (trialLens is not null)
            {
                warnings.Add("NIDEK ARK-XML: ARMedian fehlt; TrialLens wurde als Fallback gelesen.");
                median = trialLens;
            }
        }

        return ParseEye(median, includeAdd: false);
    }

    private static AisHistoricalEyeRefraction? ParseEye(XElement? eye, bool includeAdd = true)
    {
        if (eye is null)
        {
            return null;
        }

        var sphere = Value(eye, "Sphere") ?? Value(eye, "Sphare");
        var cylinder = Value(eye, "Cylinder");
        var axis = Value(eye, "Axis");
        if (string.IsNullOrWhiteSpace(sphere)
            || string.IsNullOrWhiteSpace(cylinder)
            || string.IsNullOrWhiteSpace(axis))
        {
            return null;
        }

        return new AisHistoricalEyeRefraction(
            NormalizeNumber(sphere),
            NormalizeNumber(cylinder),
            NormalizeAxis(axis),
            includeAdd ? NormalizeNumber(Value(eye, "ADD") ?? Value(eye, "Add1")) : null);
    }

    private static void AddIgnoredPrismWarnings(List<string> warnings, XElement lm, string modelName)
    {
        var hasPrism = new[] { "Prism", "PrismBase", "PrismX", "PrismY" }
            .Any(name => Descendants(lm, name).Any(element => !string.IsNullOrWhiteSpace(element.Value)));
        if (hasPrism)
        {
            warnings.Add($"{modelName}: Prismenwerte wurden erkannt, aber fuer die RT-6100-Importvorschau nicht exportiert.");
        }
    }

    private static void AddArkIgnoredDataWarnings(XElement root, List<string> warnings)
    {
        var ignored = new[] { "SR", "KM", "CS", "PS", "AC", "RI", "Image", "RingImage", "AccImage", "RetroImage" }
            .Where(name => Descendants(root, name).Any())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (ignored.Length > 0)
        {
            warnings.Add("NIDEK ARK-XML: Zusatzdaten wurden fuer RT-6100 REF_Base ignoriert: " + string.Join(", ", ignored) + ".");
        }
    }

    private static NidekRt6100InputSourceParseResult CreateUnsupported(string message)
    {
        return new NidekRt6100InputSourceParseResult(
            Array.Empty<AisHistoricalMeasurementRecord>(),
            Array.Empty<string>(),
            new[] { message });
    }

    private static XElement? FindMeasure(XElement root, string type)
    {
        return DescendantsAndSelf(root, "Measure")
            .FirstOrDefault(element => string.Equals(AttributeValue(element, "Type") ?? AttributeValue(element, "type"), type, StringComparison.OrdinalIgnoreCase));
    }

    private static XElement? Child(XContainer? parent, string localName)
    {
        return parent?.Elements().FirstOrDefault(element => string.Equals(element.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<XElement> Descendants(XContainer parent, string localName)
    {
        return parent.Descendants().Where(element => string.Equals(element.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<XElement> DescendantsAndSelf(XElement element, string localName)
    {
        return element.DescendantsAndSelf().Where(candidate => string.Equals(candidate.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase));
    }

    private static string? Value(XContainer? parent, string localName)
    {
        var value = Child(parent, localName)?.Value;
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? AttributeValue(XElement element, string localName)
    {
        return element.Attributes().FirstOrDefault(attribute => string.Equals(attribute.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase))?.Value;
    }

    private static bool ContainsModel(string? modelName, string expected)
    {
        return NormalizeModel(modelName).Contains(NormalizeModel(expected), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeModel(string? value)
    {
        return string.Concat((value ?? string.Empty).Where(char.IsLetterOrDigit)).ToUpperInvariant();
    }

    private static DateOnly? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        foreach (var format in new[] { "yyyy-MM-dd", "yyyy/MM/dd", "yyyy.MM.dd", "yyyyMMdd" })
        {
            if (DateOnly.TryParseExact(trimmed, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private static string? ReadUnitNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var match = Regex.Match(value, @"[+-]?\d+(?:[.,]\d+)?", RegexOptions.CultureInvariant);
        return match.Success ? NormalizeNumber(match.Value) : null;
    }

    private static string? NormalizeNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Replace(" ", string.Empty, StringComparison.Ordinal).Replace(',', '.').Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string? NormalizeAxis(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = NormalizeNumber(value);
        return decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var axis)
            ? axis.ToString("0", CultureInfo.InvariantCulture)
            : normalized;
    }
}

public sealed class NidekRt6100InputXmlWriter
{
    public const string DefaultFileNameTemplate = "RTImport_{PatientNumber}_{yyyyMMdd}_{HHmmss}.xml";
    public const string DeviceOutputFormat = "NIDEK RT-6100 XML";

    public Cv5000ImportWriteResult BuildXml(Cv5000ImportSelection selection, DateTimeOffset? timestamp = null)
    {
        ArgumentNullException.ThrowIfNull(selection);

        var exportableRecords = selection.SelectedMeasurements
            .Where(IsRt6100InputSource)
            .Where(record => record.IsExportableToCv5000)
            .ToArray();
        if (exportableRecords.Length == 0)
        {
            return new Cv5000ImportWriteResult(
                Success: false,
                TargetPath: null,
                XmlContent: null,
                Warnings: Array.Empty<string>(),
                ErrorMessage: "Keine exportierbaren LM-/AR-Messdatensätze für den RT-6100-Import ausgewählt.");
        }

        var now = timestamp ?? DateTimeOffset.Now;
        var document = new XDocument(
            new XDeclaration("1.0", "UTF-16", null),
            new XElement(
                "Ophthalmology",
                CreateCommonElement(selection.Patient, now),
                new XElement(
                    "Measure",
                    new XAttribute("Type", "RT"),
                    new XElement(
                        "Phoropter",
                        new XElement("DiopterStep", new XAttribute("unit", "D"), "0.01"),
                        new XElement("AxisStep", new XAttribute("unit", "deg"), "1"),
                        new XElement("CylinderMode", "-"),
                        exportableRecords.Select(CreateCorrectedElement)))));

        return new Cv5000ImportWriteResult(
            Success: true,
            TargetPath: CreateTargetPath(selection.TargetFolder, selection.TargetFileName, selection.Patient, now),
            XmlContent: document.ToString(SaveOptions.None),
            Warnings: exportableRecords
                .SelectMany(record => record.ParseWarnings)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
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
                ErrorMessage = "Kein Zielpfad für die RT-6100-Importdatei angegeben."
            };
        }

        var folder = Path.GetDirectoryName(result.TargetPath);
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            return result with
            {
                Success = false,
                ErrorMessage = "Ausgabeordner an RT-6100 existiert nicht."
            };
        }

        File.WriteAllText(result.TargetPath, result.XmlContent, Encoding.Unicode);
        return result;
    }

    public Cv5000ImportWriteResult WriteFile(
        Cv5000ImportSelection selection,
        InterfaceProfileDefinition interfaceProfile,
        DateTimeOffset? timestamp = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(interfaceProfile);

        var output = interfaceProfile.DeviceOutput;
        if (output is null)
        {
            return new Cv5000ImportWriteResult(
                Success: false,
                TargetPath: null,
                XmlContent: null,
                Warnings: Array.Empty<string>(),
                ErrorMessage: "Keine Ausgabe-an-Gerät-Konfiguration im Schnittstellenprofil vorhanden.");
        }

        if (!output.IsEnabled)
        {
            return new Cv5000ImportWriteResult(
                Success: false,
                TargetPath: null,
                XmlContent: null,
                Warnings: Array.Empty<string>(),
                ErrorMessage: "Ausgabe an Gerät ist im Schnittstellenprofil nicht aktiv.");
        }

        if (string.IsNullOrWhiteSpace(output.OutputFolder))
        {
            return new Cv5000ImportWriteResult(
                Success: false,
                TargetPath: null,
                XmlContent: null,
                Warnings: Array.Empty<string>(),
                ErrorMessage: "Ausgabeordner an RT-6100 fehlt.");
        }

        var configuredSelection = selection with
        {
            TargetFolder = output.OutputFolder,
            TargetFileName = string.IsNullOrWhiteSpace(output.FileNameTemplate)
                ? DefaultFileNameTemplate
                : output.FileNameTemplate
        };

        return WriteFile(configuredSelection, timestamp);
    }

    private static bool IsRt6100InputSource(AisHistoricalMeasurementRecord record)
    {
        return record.SourceKind is AisHistoricalMeasurementSourceKind.Lensmeter
            or AisHistoricalMeasurementSourceKind.Autorefraction;
    }

    private static XElement CreateCommonElement(PatientData patient, DateTimeOffset now)
    {
        var patientNumber = string.IsNullOrWhiteSpace(patient.PatientNumber)
            ? "NO_ID"
            : patient.PatientNumber.Trim();

        return new XElement(
            "Common",
            new XElement("Company", "NIDEK"),
            new XElement("ModelName", "RT-6100"),
            new XElement("MachineNo", string.Empty),
            new XElement("ROMVersion", string.Empty),
            new XElement("Version", "NIDEK_RT_V1.00"),
            new XElement("Date", now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            new XElement("Time", now.ToString("HH:mm:ss", CultureInfo.InvariantCulture)),
            new XElement(
                "Patient",
                new XElement("No", patientNumber),
                new XElement("ID", patientNumber),
                new XElement("FirstName", patient.FirstName ?? string.Empty),
                new XElement("MiddleName", string.Empty),
                new XElement("LastName", patient.LastName ?? string.Empty),
                new XElement("Sex", string.Empty),
                new XElement("Age", string.Empty),
                new XElement("DOB", FormatDateOfBirth(patient.BirthDate)),
                new XElement("NameJ1", string.Empty),
                new XElement("NameJ2", string.Empty)));
    }

    private static XElement CreateCorrectedElement(AisHistoricalMeasurementRecord record)
    {
        var corrected = new XElement(
            "Corrected",
            new XAttribute("CorrectionType", record.SourceKind == AisHistoricalMeasurementSourceKind.Lensmeter ? "LM_Base" : "REF_Base"),
            new XAttribute("Vision", "Distant"),
            new XAttribute("Situation", "Standard"),
            new XElement("DisplayName", record.SourceKind == AisHistoricalMeasurementSourceKind.Lensmeter ? "Lensmeter" : "Autorefraction"));

        corrected.Add(new XElement("VD", new XAttribute("unit", "mm"), FormatXmlDecimal(record.Vd)));
        corrected.Add(new XElement("WorkingDistance", new XAttribute("unit", "cm"), FormatXmlDecimal(record.WorkingDistance)));

        var allowAdd = record.SourceKind == AisHistoricalMeasurementSourceKind.Lensmeter;
        AddEyeElement(corrected, "R", record.RightEye, allowAdd, record.Pd);
        AddEyeElement(corrected, "L", record.LeftEye, allowAdd, null);

        if (!string.IsNullOrWhiteSpace(record.Pd))
        {
            corrected.Add(new XElement("B", new XElement("PD", new XAttribute("unit", "mm"), FormatXmlDecimal(record.Pd))));
        }

        return corrected;
    }

    private static void AddEyeElement(XElement corrected, string eye, AisHistoricalEyeRefraction? values, bool allowAdd, string? pd)
    {
        if (values?.HasExportableRefraction != true)
        {
            return;
        }

        var eyeElement = new XElement(
            eye,
            new XElement("Sphere", new XAttribute("unit", "D"), FormatXmlDecimal(values.Sphere)),
            new XElement("Cylinder", new XAttribute("unit", "D"), FormatXmlDecimal(values.Cylinder)),
            new XElement("Axis", new XAttribute("unit", "deg"), NormalizeAxis(values.Axis)));

        if (allowAdd && !string.IsNullOrWhiteSpace(values.Add))
        {
            eyeElement.Add(new XElement("ADD", new XAttribute("unit", "D"), FormatXmlDecimal(values.Add)));
        }

        if (!string.IsNullOrWhiteSpace(pd))
        {
            eyeElement.Add(new XElement("PD", new XAttribute("unit", "mm"), FormatXmlDecimal(pd)));
        }

        corrected.Add(eyeElement);
    }

    private static string FormatDateOfBirth(string? birthDate)
    {
        return DateTime.TryParseExact(
            birthDate,
            "ddMMyyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : birthDate?.Trim() ?? string.Empty;
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

    private static string NormalizeAxis(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static string? CreateTargetPath(string? targetFolder, string? targetFileName, PatientData patient, DateTimeOffset timestamp)
    {
        if (string.IsNullOrWhiteSpace(targetFolder) || string.IsNullOrWhiteSpace(targetFileName))
        {
            return null;
        }

        var fileName = ExpandFileNameTemplate(targetFileName, patient, timestamp);
        return Path.Combine(targetFolder, Path.GetFileName(fileName));
    }

    private static string ExpandFileNameTemplate(string template, PatientData patient, DateTimeOffset timestamp)
    {
        var patientNumber = SanitizeFileNamePart(string.IsNullOrWhiteSpace(patient.PatientNumber) ? "NO_ID" : patient.PatientNumber.Trim());
        var expanded = template
            .Replace("{PatientNumber}", patientNumber, StringComparison.OrdinalIgnoreCase)
            .Replace("{Date:yyyyMMdd}", timestamp.ToString("yyyyMMdd", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{Time:HHmmss}", timestamp.ToString("HHmmss", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{yyyyMMdd}", timestamp.ToString("yyyyMMdd", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{HHmmss}", timestamp.ToString("HHmmss", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);

        return string.IsNullOrWhiteSpace(Path.GetExtension(expanded))
            ? $"{expanded}.xml"
            : expanded;
    }

    private static string SanitizeFileNamePart(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray();
        var sanitized = new string(chars).Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? "NO_ID" : sanitized;
    }
}
