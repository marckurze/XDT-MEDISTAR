using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record XdtBaukastenDeviceOutputRulePreviewResult(
    string Content,
    IReadOnlyList<string> Warnings);

public static class XdtBaukastenDeviceOutputRuleService
{
    private const string Category = "Ausgabe an Gerät";
    private static readonly Regex PlaceholderRegex = new(@"\{(?<token>[^{}]+)\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static IReadOnlyList<ExportRuleDefinition> CreateDefaultRules(DeviceProfileDefinition? profile)
    {
        if (profile?.IsBidirectional != true)
        {
            return Array.Empty<ExportRuleDefinition>();
        }

        if (profile.Metadata.IsUserDefined && !profile.Metadata.IsBuiltIn)
        {
            return Array.Empty<ExportRuleDefinition>();
        }

        if (IsRt6100(profile))
        {
            return CreateRt6100Rules();
        }

        if (IsNidekRtSerial(profile))
        {
            return CreateNidekRtSerialRules();
        }

        if (IsCv5000(profile))
        {
            return CreateCv5000Rules();
        }

        return Array.Empty<ExportRuleDefinition>();
    }

    public static IReadOnlyList<XdtBaukastenPlaceholder> CreatePlaceholders(
        DeviceProfileDefinition? profile,
        PatientData? patient,
        IReadOnlyList<AisHistoricalMeasurementRecord> historicalRecords)
    {
        if (profile?.IsBidirectional != true)
        {
            return new[]
            {
                new XdtBaukastenPlaceholder(
                    Category,
                    "Keine Geräteausgabe",
                    "{DeviceOutput.NotAvailable}",
                    "Für nicht bidirektionale Geräte gibt es keine Ausgabe-an-Gerät-Platzhalter.",
                    IsPreparedOnly: true)
            };
        }

        if (profile.Metadata.IsUserDefined && !profile.Metadata.IsBuiltIn)
        {
            return new[]
            {
                new XdtBaukastenPlaceholder(
                    Category,
                    "Geräteausgabe vorbereitet",
                    "{DeviceOutput.Prepared}",
                    "Für dieses bidirektionale Entwurfsgerät können eigene Geräteausgabe-Regeln angelegt werden.",
                    IsPreparedOnly: true)
            };
        }

        if (IsRt6100(profile))
        {
            var selected = SelectRt6100Records(historicalRecords);
            return CreatePlaceholders(CreateRt6100PlaceholderSpecs(), patient, selected, isRt6100: true);
        }

        if (IsNidekRtSerial(profile))
        {
            var selected = SelectNidekRtSerialRecords(historicalRecords);
            return CreatePlaceholders(CreateNidekRtSerialPlaceholderSpecs(), patient, selected, isRt6100: false);
        }

        if (IsCv5000(profile))
        {
            var selected = SelectCv5000Records(historicalRecords);
            return CreatePlaceholders(CreateCv5000PlaceholderSpecs(), patient, selected, isRt6100: false);
        }

        return new[]
        {
            new XdtBaukastenPlaceholder(
                Category,
                "Geräteausgabe vorbereitet",
                "{DeviceOutput.Prepared}",
                "Für dieses bidirektionale Entwurfsgerät können eigene Geräteausgabe-Regeln angelegt werden.",
                IsPreparedOnly: true)
        };
    }

    public static XdtBaukastenDeviceOutputRulePreviewResult ApplyRulesToXml(
        string xml,
        IReadOnlyList<ExportRuleDefinition> rules,
        PatientData patient,
        IReadOnlyList<AisHistoricalMeasurementRecord> selectedRecords,
        DeviceProfileDefinition? profile)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return new XdtBaukastenDeviceOutputRulePreviewResult(string.Empty, new[] { "Geräteausgabe-XML ist leer." });
        }

        var warnings = new List<string>();
        var document = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
        var activeRules = rules.Where(rule => rule.IsEnabled).ToArray();
        var activeTargets = activeRules
            .Select(rule => rule.TargetFieldCode)
            .Where(target => !string.IsNullOrWhiteSpace(target))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var defaultRule in CreateDefaultRules(profile))
        {
            if (activeTargets.Contains(defaultRule.TargetFieldCode))
            {
                continue;
            }

            var target = FindTargetElement(document, defaultRule.TargetFieldCode, createIfMissing: false);
            target?.Remove();
        }

        var tokenValues = CreateTokenValueMap(patient, selectedRecords, IsRt6100(profile));
        foreach (var rule in activeRules)
        {
            if (string.IsNullOrWhiteSpace(rule.TargetFieldCode))
            {
                warnings.Add($"Geräteausgabe-Regel {rule.TargetName}: Ziel-Element fehlt.");
                continue;
            }

            var value = ResolveRuleValue(rule, tokenValues);
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var target = FindTargetElement(document, rule.TargetFieldCode, createIfMissing: true);
            if (target is null)
            {
                warnings.Add($"Geräteausgabe-Regel {rule.TargetName}: Ziel {rule.TargetFieldCode} konnte in der Vorschau nicht zugeordnet werden.");
                continue;
            }

            target.Value = FormatXmlTargetValue(rule.TargetFieldCode, value);
        }

        return new XdtBaukastenDeviceOutputRulePreviewResult(document.ToString(SaveOptions.None), warnings);
    }

    public static XdtBaukastenDeviceOutputRulePreviewResult BuildRuleTextPreview(
        IReadOnlyList<ExportRuleDefinition> rules,
        PatientData patient,
        IReadOnlyList<AisHistoricalMeasurementRecord> records,
        DeviceProfileDefinition? profile)
    {
        var activeRules = rules.Where(rule => rule.IsEnabled).ToArray();
        if (activeRules.Length == 0)
        {
            return new XdtBaukastenDeviceOutputRulePreviewResult(
                "Für dieses bidirektionale Entwurfsgerät sind noch keine Geräteausgabe-Regeln definiert. Mit + kann eine Regel angelegt werden.",
                Array.Empty<string>());
        }

        var selectedRecords = IsRt6100(profile) ? SelectRt6100Records(records) : SelectCv5000Records(records);
        var tokenValues = CreateTokenValueMap(patient, selectedRecords, IsRt6100(profile));
        var lines = activeRules
            .Select(rule => $"{rule.TargetFieldCode}: {ResolveRuleValue(rule, tokenValues)}".TrimEnd())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        return new XdtBaukastenDeviceOutputRulePreviewResult(
            lines.Length == 0
                ? "Geräteausgabe-Regeln vorhanden, aber ohne aktuell auflösbare Werte."
                : string.Join(Environment.NewLine, lines),
            Array.Empty<string>());
    }

    private static IReadOnlyList<ExportRuleDefinition> CreateCv5000Rules()
    {
        var rules = new List<ExportRuleDefinition>();
        AddRule(rules, "cv5000-patient-no", "Common/Patient/No.", "Patientennummer", "CV5000Input.PatientNumber", "Patientennummer in CVImport.xml.");
        AddRule(rules, "cv5000-patient-id", "Common/Patient/ID", "Patient-ID", "CV5000Input.Patient.ID", "Patienten-ID in CVImport.xml.");
        AddRule(rules, "cv5000-patient-firstname", "Common/Patient/FirstName", "Vorname", "CV5000Input.Patient.FirstName", "Patientenvorname in CVImport.xml.");
        AddRule(rules, "cv5000-patient-lastname", "Common/Patient/LastName", "Nachname", "CV5000Input.Patient.LastName", "Patientennachname in CVImport.xml.");
        AddRule(rules, "cv5000-patient-dob", "Common/Patient/DOB", "Geburtsdatum", "CV5000Input.Patient.DOB", "Geburtsdatum in CVImport.xml.");
        AddCvRefractionRules(rules, "Lensmeter", "PhoropterInput.Lensmeter");
        AddCvRefractionRules(rules, "Autorefraction", "PhoropterInput.Autoref");
        AddCvRefractionRules(rules, "Previous Phoropter", "PhoropterInput.Phoropter");
        return rules;
    }

    private static IReadOnlyList<ExportRuleDefinition> CreateRt6100Rules()
    {
        var rules = new List<ExportRuleDefinition>();
        AddRule(rules, "rt6100-patient-no", "Common/Patient/No", "Patient No", "RT6100Input.Patient.No", "Patientennummer in RT-6100-Input-XML.");
        AddRule(rules, "rt6100-patient-id", "Common/Patient/ID", "Patient ID", "RT6100Input.Patient.ID", "Patienten-ID in RT-6100-Input-XML.");
        AddRule(rules, "rt6100-patient-firstname", "Common/Patient/FirstName", "Vorname", "RT6100Input.Patient.FirstName", "Patientenvorname in RT-6100-Input-XML.");
        AddRule(rules, "rt6100-patient-lastname", "Common/Patient/LastName", "Nachname", "RT6100Input.Patient.LastName", "Patientennachname in RT-6100-Input-XML.");
        AddRule(rules, "rt6100-patient-dob", "Common/Patient/DOB", "Geburtsdatum", "RT6100Input.Patient.DOB", "Geburtsdatum in RT-6100-Input-XML.");
        AddRtRefractionRules(rules, "LM_Base", "PhoropterInput.LM_Base");
        AddRtRefractionRules(rules, "REF_Base", "PhoropterInput.REF_Base");
        return rules;
    }

    private static IReadOnlyList<ExportRuleDefinition> CreateNidekRtSerialRules()
    {
        var rules = new List<ExportRuleDefinition>();
        AddRule(rules, "nidek-rtserial-patient-id", "Serial/ID", "Patient ID", "NidekRtSerial.PatientNumber", "Patienten-ID im seriellen PC-zu-RT-Block.");
        AddRule(rules, "nidek-rtserial-patient-firstname", "Serial/Patient/FirstName", "Vorname", "NidekRtSerial.Patient.FirstName", "Patientenvorname fuer vorbereitete serielle Ausgabe.");
        AddRule(rules, "nidek-rtserial-patient-lastname", "Serial/Patient/LastName", "Nachname", "NidekRtSerial.Patient.LastName", "Patientennachname fuer vorbereitete serielle Ausgabe.");
        AddRtSerialEyeRules(rules, "LM", "Lensmeter", "PhoropterInput.Lensmeter");
        AddRtSerialEyeRules(rules, "AR", "Autoref", "PhoropterInput.Autoref");
        return rules;
    }

    private static void AddRtSerialEyeRules(List<ExportRuleDefinition> rules, string targetPrefix, string namePrefix, string sourcePrefix)
    {
        foreach (var eye in new[] { ("R", "Right", "rechts"), ("L", "Left", "links") })
        {
            AddRule(rules, CreateId("nidek-rtserial", targetPrefix, eye.Item2, "sphere"), $"Serial/{targetPrefix}/{eye.Item1}/Sphere", $"{namePrefix} {eye.Item3} Sphere", $"{sourcePrefix}.{eye.Item2}.Sphere", $"{namePrefix} {eye.Item3} Sphaere.");
            AddRule(rules, CreateId("nidek-rtserial", targetPrefix, eye.Item2, "cylinder"), $"Serial/{targetPrefix}/{eye.Item1}/Cylinder", $"{namePrefix} {eye.Item3} Cylinder", $"{sourcePrefix}.{eye.Item2}.Cylinder", $"{namePrefix} {eye.Item3} Zylinder.");
            AddRule(rules, CreateId("nidek-rtserial", targetPrefix, eye.Item2, "axis"), $"Serial/{targetPrefix}/{eye.Item1}/Axis", $"{namePrefix} {eye.Item3} Axis", $"{sourcePrefix}.{eye.Item2}.Axis", $"{namePrefix} {eye.Item3} Achse.");
            AddRule(rules, CreateId("nidek-rtserial", targetPrefix, eye.Item2, "add"), $"Serial/{targetPrefix}/{eye.Item1}/ADD", $"{namePrefix} {eye.Item3} ADD", $"{sourcePrefix}.{eye.Item2}.ADD", $"{namePrefix} {eye.Item3} Addition.");
        }

        AddRule(rules, CreateId("nidek-rtserial", targetPrefix, "pd"), $"Serial/{targetPrefix}/PD", $"{namePrefix} PD", $"{sourcePrefix}.PD", $"{namePrefix} Binokular-PD.");
    }

    private static void AddCvRefractionRules(List<ExportRuleDefinition> rules, string typeName, string sourcePrefix)
    {
        var targetPrefix = $"SBJ/{typeName}";
        var namePrefix = typeName switch
        {
            "Lensmeter" => "Lensmeter",
            "Autorefraction" => "Autoref",
            "Previous Phoropter" => "Phoropter",
            _ => typeName
        };

        AddEyeRules(rules, targetPrefix, namePrefix, sourcePrefix, sphereLeaf: "Sph", cylinderLeaf: "Cyl", addLeaf: "ADD");
        AddRule(rules, CreateId(sourcePrefix, "pd-b"), $"{targetPrefix}/PD/B", $"{namePrefix} PD", $"{sourcePrefix}.Both.PD", $"{namePrefix} Binokular-PD.");
    }

    private static void AddRtRefractionRules(List<ExportRuleDefinition> rules, string correctionType, string sourcePrefix)
    {
        var targetPrefix = $"RT/{correctionType}";
        AddEyeRules(rules, targetPrefix, correctionType, sourcePrefix, sphereLeaf: "Sphere", cylinderLeaf: "Cylinder", addLeaf: "ADD");
        AddRule(rules, CreateId(sourcePrefix, "pd-b"), $"{targetPrefix}/B/PD", $"{correctionType} PD", $"{sourcePrefix}.Both.PD", $"{correctionType} Binokular-PD.");
    }

    private static void AddEyeRules(
        List<ExportRuleDefinition> rules,
        string targetPrefix,
        string namePrefix,
        string sourcePrefix,
        string sphereLeaf,
        string cylinderLeaf,
        string addLeaf)
    {
        foreach (var eye in new[] { ("R", "Right", "rechts"), ("L", "Left", "links") })
        {
            AddRule(rules, CreateId(sourcePrefix, eye.Item2, "sphere"), $"{targetPrefix}/{eye.Item1}/{sphereLeaf}", $"{namePrefix} {eye.Item3} Sphere", $"{sourcePrefix}.{eye.Item2}.Sphere", $"{namePrefix} {eye.Item3} Sphäre.");
            AddRule(rules, CreateId(sourcePrefix, eye.Item2, "cylinder"), $"{targetPrefix}/{eye.Item1}/{cylinderLeaf}", $"{namePrefix} {eye.Item3} Cylinder", $"{sourcePrefix}.{eye.Item2}.Cylinder", $"{namePrefix} {eye.Item3} Zylinder.");
            AddRule(rules, CreateId(sourcePrefix, eye.Item2, "axis"), $"{targetPrefix}/{eye.Item1}/Axis", $"{namePrefix} {eye.Item3} Axis", $"{sourcePrefix}.{eye.Item2}.Axis", $"{namePrefix} {eye.Item3} Achse.");
            AddRule(rules, CreateId(sourcePrefix, eye.Item2, "add"), $"{targetPrefix}/{eye.Item1}/{addLeaf}", $"{namePrefix} {eye.Item3} ADD", $"{sourcePrefix}.{eye.Item2}.ADD", $"{namePrefix} {eye.Item3} Addition.");
            AddRule(rules, CreateId(sourcePrefix, eye.Item2, "pd"), $"{targetPrefix}/{eye.Item1}/PD", $"{namePrefix} {eye.Item3} PD", $"{sourcePrefix}.{eye.Item2}.PD", $"{namePrefix} {eye.Item3} PD.");
        }
    }

    private static void AddRule(
        List<ExportRuleDefinition> rules,
        string id,
        string target,
        string name,
        string sourcePath,
        string description)
    {
        rules.Add(new ExportRuleDefinition(
            id,
            target,
            name,
            ExportRuleType.Template,
            sourcePath,
            "{value}",
            (rules.Count + 1) * 10,
            true,
            description));
    }

    private static IReadOnlyList<PlaceholderSpec> CreateCv5000PlaceholderSpecs()
    {
        return CreateCv5000Rules()
            .GroupBy(rule => rule.SourcePath ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key))
            .Select(group => new PlaceholderSpec(group.First().TargetName, group.Key, group.First().Description ?? group.Key))
            .ToArray();
    }

    private static IReadOnlyList<PlaceholderSpec> CreateRt6100PlaceholderSpecs()
    {
        return CreateRt6100Rules()
            .GroupBy(rule => rule.SourcePath ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key))
            .Select(group => new PlaceholderSpec(group.First().TargetName, group.Key, group.First().Description ?? group.Key))
            .ToArray();
    }

    private static IReadOnlyList<PlaceholderSpec> CreateNidekRtSerialPlaceholderSpecs()
    {
        return CreateNidekRtSerialRules()
            .GroupBy(rule => rule.SourcePath ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key))
            .Select(group => new PlaceholderSpec(group.First().TargetName, group.Key, group.First().Description ?? group.Key))
            .ToArray();
    }

    private static IReadOnlyList<XdtBaukastenPlaceholder> CreatePlaceholders(
        IReadOnlyList<PlaceholderSpec> specs,
        PatientData? patient,
        IReadOnlyList<AisHistoricalMeasurementRecord> records,
        bool isRt6100)
    {
        var tokenValues = CreateTokenValueMap(patient ?? EmptyPatient(), records, isRt6100);
        return specs
            .Select(spec => new XdtBaukastenPlaceholder(
                Category,
                spec.DisplayName,
                "{" + spec.SourcePath + "}",
                spec.Description,
                tokenValues.TryGetValue(spec.SourcePath, out var value) ? DisplayPlaceholderValue(value) : "-"))
            .ToArray();
    }

    private static XElement? FindTargetElement(XDocument document, string targetPath, bool createIfMissing)
    {
        var parts = targetPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return null;
        }

        if (string.Equals(parts[0], "Common", StringComparison.OrdinalIgnoreCase))
        {
            var common = document.Descendants().FirstOrDefault(element => element.Name.LocalName == "Common");
            return common is null ? null : WalkPath(common, parts.Skip(1).ToArray(), createIfMissing);
        }

        if (string.Equals(parts[0], "SBJ", StringComparison.OrdinalIgnoreCase) && parts.Length >= 4)
        {
            return FindCv5000Element(document, parts, createIfMissing);
        }

        if (string.Equals(parts[0], "RT", StringComparison.OrdinalIgnoreCase) && parts.Length >= 4)
        {
            return FindRt6100Element(document, parts, createIfMissing);
        }

        return null;
    }

    private static XElement? FindCv5000Element(XDocument document, IReadOnlyList<string> parts, bool createIfMissing)
    {
        var typeName = parts[1];
        var type = document
            .Descendants()
            .FirstOrDefault(element =>
                element.Name.LocalName == "Type"
                && string.Equals(
                    element.Elements().FirstOrDefault(child => child.Name.LocalName == "TypeName")?.Value.Trim(),
                    typeName,
                    StringComparison.OrdinalIgnoreCase));

        if (type is null)
        {
            return null;
        }

        if (string.Equals(parts[2], "PD", StringComparison.OrdinalIgnoreCase))
        {
            var examDistance = type.Descendants().FirstOrDefault(element => element.Name.LocalName == "ExamDistance");
            return examDistance is null ? null : WalkPath(examDistance, parts.Skip(2).ToArray(), createIfMissing);
        }

        var refractionData = type.Descendants().FirstOrDefault(element => element.Name.LocalName == "RefractionData");
        return refractionData is null ? null : WalkPath(refractionData, parts.Skip(2).ToArray(), createIfMissing);
    }

    private static XElement? FindRt6100Element(XDocument document, IReadOnlyList<string> parts, bool createIfMissing)
    {
        var correctionType = parts[1];
        var corrected = document
            .Descendants()
            .FirstOrDefault(element =>
                element.Name.LocalName == "Corrected"
                && string.Equals(
                    element.Attribute("CorrectionType")?.Value.Trim(),
                    correctionType,
                    StringComparison.OrdinalIgnoreCase));

        return corrected is null ? null : WalkPath(corrected, parts.Skip(2).ToArray(), createIfMissing);
    }

    private static XElement? WalkPath(XElement root, IReadOnlyList<string> parts, bool createIfMissing)
    {
        var current = root;
        foreach (var part in parts)
        {
            var next = current.Elements().FirstOrDefault(element => string.Equals(element.Name.LocalName, part, StringComparison.OrdinalIgnoreCase));
            if (next is null)
            {
                if (!createIfMissing)
                {
                    return null;
                }

                next = new XElement(current.Name.Namespace + part);
                current.Add(next);
            }

            current = next;
        }

        return current;
    }

    private static string ResolveRuleValue(ExportRuleDefinition rule, IReadOnlyDictionary<string, string> tokenValues)
    {
        var template = rule.OutputTemplate ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(rule.SourcePath) && string.Equals(template.Trim(), "{value}", StringComparison.OrdinalIgnoreCase))
        {
            return tokenValues.TryGetValue(rule.SourcePath, out var sourceValue) ? sourceValue : string.Empty;
        }

        if (PlaceholderRegex.IsMatch(template))
        {
            return PlaceholderRegex.Replace(template, match =>
            {
                var token = match.Groups["token"].Value.Trim();
                return string.Equals(token, "value", StringComparison.OrdinalIgnoreCase)
                    ? !string.IsNullOrWhiteSpace(rule.SourcePath) && tokenValues.TryGetValue(rule.SourcePath, out var value) ? value : string.Empty
                    : tokenValues.TryGetValue(token, out var replacement) ? replacement : string.Empty;
            });
        }

        return template;
    }

    private static Dictionary<string, string> CreateTokenValueMap(
        PatientData patient,
        IReadOnlyList<AisHistoricalMeasurementRecord> records,
        bool isRt6100)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        AddPatientValues(values, patient, isRt6100);

        foreach (var record in records)
        {
            foreach (var prefix in CreateRecordPrefixes(record, isRt6100))
            {
                AddRecordValues(values, prefix, record);
            }
        }

        return values;
    }

    private static void AddPatientValues(Dictionary<string, string> values, PatientData patient, bool isRt6100)
    {
        AddValue(values, "AIS.PatientNumber", patient.PatientNumber);
        AddValue(values, "AIS.FirstName", patient.FirstName);
        AddValue(values, "AIS.LastName", patient.LastName);
        AddValue(values, "AIS.DateOfBirth", patient.BirthDate);
        AddValue(values, "NidekRtSerial.PatientNumber", patient.PatientNumber);
        AddValue(values, "NidekRtSerial.Patient.ID", patient.PatientNumber);
        AddValue(values, "NidekRtSerial.Patient.FirstName", patient.FirstName);
        AddValue(values, "NidekRtSerial.Patient.LastName", patient.LastName);
        AddValue(values, "NidekRtSerial.Patient.DOB", patient.BirthDate);

        if (isRt6100)
        {
            AddValue(values, "RT6100Input.Patient.No", string.IsNullOrWhiteSpace(patient.PatientNumber) ? "NO_ID" : patient.PatientNumber);
            AddValue(values, "RT6100Input.Patient.ID", string.IsNullOrWhiteSpace(patient.PatientNumber) ? "NO_ID" : patient.PatientNumber);
            AddValue(values, "RT6100Input.Patient.FirstName", patient.FirstName);
            AddValue(values, "RT6100Input.Patient.LastName", patient.LastName);
            AddValue(values, "RT6100Input.Patient.DOB", FormatRtDateOfBirth(patient.BirthDate));
            return;
        }

        AddValue(values, "CV5000Input.PatientNumber", patient.PatientNumber);
        AddValue(values, "CV5000Input.Patient.No", patient.PatientNumber);
        AddValue(values, "CV5000Input.Patient.ID", patient.PatientNumber);
        AddValue(values, "CV5000Input.Patient.FirstName", patient.FirstName);
        AddValue(values, "CV5000Input.Patient.LastName", patient.LastName);
        AddValue(values, "CV5000Input.Patient.BirthDate", patient.BirthDate);
        AddValue(values, "CV5000Input.Patient.DOB", patient.BirthDate);
    }

    private static IEnumerable<string> CreateRecordPrefixes(AisHistoricalMeasurementRecord record, bool isRt6100)
    {
        if (isRt6100)
        {
            return record.SourceKind switch
            {
                AisHistoricalMeasurementSourceKind.Lensmeter => new[] { "PhoropterInput.LM_Base", "RT6100Input.LM_Base" },
                AisHistoricalMeasurementSourceKind.Autorefraction => new[] { "PhoropterInput.REF_Base", "RT6100Input.REF_Base" },
                _ => Array.Empty<string>()
            };
        }

        return record.SourceKind switch
        {
            AisHistoricalMeasurementSourceKind.Lensmeter => new[] { "PhoropterInput.Lensmeter", "CV5000Input.Lensmeter" },
            AisHistoricalMeasurementSourceKind.Autorefraction => new[] { "PhoropterInput.Autoref", "CV5000Input.Autoref" },
            AisHistoricalMeasurementSourceKind.Phoropter => new[] { "PhoropterInput.Phoropter", "CV5000Input.Phoropter" },
            AisHistoricalMeasurementSourceKind.Prescription => new[] { "PhoropterInput.Prescription", "CV5000Input.Prescription" },
            AisHistoricalMeasurementSourceKind.AutorefractionSubjective => new[] { "PhoropterInput.SubjectiveAutoref", "CV5000Input.SubjectiveAutoref" },
            _ => Array.Empty<string>()
        };
    }

    private static void AddRecordValues(Dictionary<string, string> values, string prefix, AisHistoricalMeasurementRecord record)
    {
        AddEyeValues(values, prefix, "Right", record.RightEye, record.Pd);
        AddEyeValues(values, prefix, "Left", record.LeftEye, null);
        AddValue(values, $"{prefix}.Both.PD", record.Pd);
        AddValue(values, $"{prefix}.PD", record.Pd);
        AddValue(values, $"{prefix}.VD", record.Vd);
    }

    private static void AddEyeValues(
        Dictionary<string, string> values,
        string prefix,
        string eye,
        AisHistoricalEyeRefraction? refraction,
        string? pd)
    {
        if (refraction is null)
        {
            return;
        }

        AddValue(values, $"{prefix}.{eye}.Sphere", refraction.Sphere);
        AddValue(values, $"{prefix}.{eye}.Sph", refraction.Sphere);
        AddValue(values, $"{prefix}.{eye}.Cylinder", refraction.Cylinder);
        AddValue(values, $"{prefix}.{eye}.Cyl", refraction.Cylinder);
        AddValue(values, $"{prefix}.{eye}.Axis", refraction.Axis);
        AddValue(values, $"{prefix}.{eye}.ADD", refraction.Add);
        AddValue(values, $"{prefix}.{eye}.Add", refraction.Add);
        AddValue(values, $"{prefix}.{eye}.PD", pd);
    }

    private static void AddValue(Dictionary<string, string> values, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            values.TryAdd(key, value.Trim());
        }
    }

    private static IReadOnlyList<AisHistoricalMeasurementRecord> SelectCv5000Records(IEnumerable<AisHistoricalMeasurementRecord> records)
    {
        return SelectRecords(
            records,
            AisHistoricalMeasurementSourceKind.Lensmeter,
            AisHistoricalMeasurementSourceKind.Autorefraction,
            AisHistoricalMeasurementSourceKind.Phoropter);
    }

    private static IReadOnlyList<AisHistoricalMeasurementRecord> SelectRt6100Records(IEnumerable<AisHistoricalMeasurementRecord> records)
    {
        return SelectRecords(
            records,
            AisHistoricalMeasurementSourceKind.Lensmeter,
            AisHistoricalMeasurementSourceKind.Autorefraction);
    }

    private static IReadOnlyList<AisHistoricalMeasurementRecord> SelectNidekRtSerialRecords(IEnumerable<AisHistoricalMeasurementRecord> records)
    {
        return SelectRecords(
            records,
            AisHistoricalMeasurementSourceKind.Lensmeter,
            AisHistoricalMeasurementSourceKind.Autorefraction);
    }

    private static IReadOnlyList<AisHistoricalMeasurementRecord> SelectRecords(
        IEnumerable<AisHistoricalMeasurementRecord> records,
        params AisHistoricalMeasurementSourceKind[] kinds)
    {
        var candidates = records.Where(record => record.IsExportableToCv5000).ToList();
        return kinds
            .Select(kind => candidates
                .Where(record => record.SourceKind == kind)
                .OrderByDescending(record => record.Date)
                .FirstOrDefault())
            .Where(record => record is not null)
            .Select(record => record!)
            .ToArray();
    }

    private static bool IsCv5000(DeviceProfileDefinition? profile)
    {
        return NormalizeProfileText(profile).Contains("CV5000", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRt6100(DeviceProfileDefinition? profile)
    {
        return NormalizeProfileText(profile).Contains("RT6100", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsNidekRtSerial(DeviceProfileDefinition? profile)
    {
        return NidekRtSerialPhoropterParser.IsParserMode(profile?.ParserMode)
            || NormalizeProfileText(profile).Contains("RT2100", StringComparison.OrdinalIgnoreCase)
            || NormalizeProfileText(profile).Contains("RT3100", StringComparison.OrdinalIgnoreCase)
            || NormalizeProfileText(profile).Contains("RT5100", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeProfileText(DeviceProfileDefinition? profile)
    {
        if (profile is null)
        {
            return string.Empty;
        }

        var text = string.Join(" ", profile.Metadata.Id, profile.Metadata.Name, profile.Metadata.Product, profile.Model);
        return Regex.Replace(text, "[^A-Za-z0-9]", string.Empty).ToUpperInvariant();
    }

    private static string CreateId(params string[] parts)
    {
        return "device-output-" + Regex.Replace(string.Join("-", parts), "[^A-Za-z0-9]+", "-").Trim('-').ToLowerInvariant();
    }

    private static string FormatXmlTargetValue(string targetPath, string value)
    {
        var trimmed = value.Trim();
        if (targetPath.Contains("Axis", StringComparison.OrdinalIgnoreCase)
            || targetPath.Contains("Patient", StringComparison.OrdinalIgnoreCase)
            || targetPath.Contains("DOB", StringComparison.OrdinalIgnoreCase)
            || targetPath.Contains("Common/", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        var normalized = trimmed.Replace(" ", string.Empty, StringComparison.Ordinal).Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            ? number.ToString("0.00", CultureInfo.InvariantCulture)
            : trimmed;
    }

    private static string DisplayPlaceholderValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "-";
        }

        var trimmed = value.Trim();
        return trimmed.Length <= 32 ? trimmed : trimmed[..29] + "...";
    }

    private static string FormatRtDateOfBirth(string? birthDate)
    {
        return DateTime.TryParseExact(
            birthDate,
            "ddMMyyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? parsed.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture)
            : birthDate?.Trim() ?? string.Empty;
    }

    private static PatientData EmptyPatient()
    {
        return new PatientData(null, null, null, null, null, null, null, null, null, null, null);
    }

    private sealed record PlaceholderSpec(string DisplayName, string SourcePath, string Description);
}
