using System.Text;
using System.Text.RegularExpressions;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class XdtBaukastenPreviewService
{
    private readonly BuilderManualProcessingPreviewService _manualPreviewService;
    private readonly MedistarHistoricalMeasurementParser _historyParser;
    private readonly TopconCv5000ImportXmlWriter _cv5000Writer;
    private readonly NidekRt6100InputXmlWriter _rt6100Writer;
    private readonly NidekRtSerialPhoropterOutputWriter _rtSerialWriter = new();
    private readonly XdtBaukastenDeviceCompatibilityService _compatibilityService;

    public XdtBaukastenPreviewService()
        : this(
            new BuilderManualProcessingPreviewService(),
            new MedistarHistoricalMeasurementParser(),
            new TopconCv5000ImportXmlWriter(),
            new NidekRt6100InputXmlWriter(),
            new XdtBaukastenDeviceCompatibilityService())
    {
    }

    public XdtBaukastenPreviewService(
        BuilderManualProcessingPreviewService manualPreviewService,
        MedistarHistoricalMeasurementParser historyParser,
        TopconCv5000ImportXmlWriter cv5000Writer,
        NidekRt6100InputXmlWriter rt6100Writer)
        : this(
            manualPreviewService,
            historyParser,
            cv5000Writer,
            rt6100Writer,
            new XdtBaukastenDeviceCompatibilityService())
    {
    }

    public XdtBaukastenPreviewService(
        BuilderManualProcessingPreviewService manualPreviewService,
        MedistarHistoricalMeasurementParser historyParser,
        TopconCv5000ImportXmlWriter cv5000Writer,
        NidekRt6100InputXmlWriter rt6100Writer,
        XdtBaukastenDeviceCompatibilityService compatibilityService)
    {
        _manualPreviewService = manualPreviewService ?? throw new ArgumentNullException(nameof(manualPreviewService));
        _historyParser = historyParser ?? throw new ArgumentNullException(nameof(historyParser));
        _cv5000Writer = cv5000Writer ?? throw new ArgumentNullException(nameof(cv5000Writer));
        _rt6100Writer = rt6100Writer ?? throw new ArgumentNullException(nameof(rt6100Writer));
        _compatibilityService = compatibilityService ?? throw new ArgumentNullException(nameof(compatibilityService));
    }

    public XdtBaukastenPreviewResult BuildPreview(
        XdtBaukastenState state,
        InterfaceProfileDefinition? interfaceProfile,
        DateTimeOffset? timestamp = null)
    {
        ArgumentNullException.ThrowIfNull(state);

        var messages = new List<string>();
        var exportProfile = state.CreateWorkingExportProfile();
        if (exportProfile is null)
        {
            return CreateFailure("Bitte zuerst ein Mapping-/Exportprofil auswählen.");
        }

        ProcessingPipelineResult? pipelineResult = null;
        var rawXdt = string.Empty;
        var aisView = string.Empty;
        var diagnostics = string.Empty;
        if (state.SerialInput is not null && state.DeviceInput is null)
        {
            messages.Add("RS232-Rohdaten wurden übernommen. Eine medizinische XDT-Vorschau wird erst erzeugt, wenn passende Parserdaten oder eine Gerätetestdatei vorliegen.");
            aisView = "Für reine RS232-Rohdaten ist noch keine AIS-Karteikartenansicht erzeugt.";
            diagnostics = CreateSerialDiagnosticsView(state.SerialInput);
        }
        else
        {
            var fileValidation = ValidateRequiredPreviewFiles(state);
            if (fileValidation is not null)
            {
                return CreateFailure(fileValidation);
            }

            var compatibility = _compatibilityService.EvaluateForWorkbench(state.DeviceProfile, state.DeviceInput!.SourcePath);
            if (!compatibility.AllowsPreview)
            {
                return CreateFailure(compatibility.Message);
            }

            if (compatibility.IsWarning)
            {
                messages.Add(compatibility.Message);
            }

            pipelineResult = _manualPreviewService.BuildPreview(new BuilderManualProcessingPreviewRequest(
                InterfaceProfile: interfaceProfile,
                DeviceProfile: state.DeviceProfile,
                ExportProfile: exportProfile,
                AisFilePath: state.AisInput!.SourcePath,
                DeviceFilePath: state.DeviceInput!.SourcePath));

            rawXdt = pipelineResult.ExportContent;
            aisView = CreateAisCardView(pipelineResult);
            diagnostics = CreateDiagnosticsView(pipelineResult, state, exportProfile, compatibility);
            messages.AddRange(pipelineResult.Issues.Select(issue => $"{issue.Severity}: {issue.Stage}: {issue.Message}"));
        }

        var deviceOutput = BuildDeviceOutputPreview(state, interfaceProfile, timestamp, messages);
        diagnostics = AppendDeviceOutputDiagnostics(diagnostics, messages);
        var documents = CreatePreviewDocuments(
            rawXdt,
            aisView,
            deviceOutput,
            diagnostics,
            pipelineResult,
            exportProfile,
            state.WorkingDeviceOutputRules);
        var output = new XdtBaukastenOutputPreview(
            RawXdt: rawXdt,
            AisView: aisView,
            DeviceOutput: deviceOutput,
            Diagnostics: diagnostics,
            Messages: messages,
            Documents: documents);

        return new XdtBaukastenPreviewResult(
            Success: pipelineResult is null || !pipelineResult.HasErrors,
            PipelineResult: pipelineResult,
            Output: output,
            Messages: messages);
    }

    private static string? ValidateRequiredPreviewFiles(XdtBaukastenState state)
    {
        if (state.AisInput is null || string.IsNullOrWhiteSpace(state.AisInput.SourcePath) || !File.Exists(state.AisInput.SourcePath))
        {
            return "Bitte zuerst eine AIS-Testdatei laden.";
        }

        if (state.DeviceInput is null || string.IsNullOrWhiteSpace(state.DeviceInput.SourcePath) || !File.Exists(state.DeviceInput.SourcePath))
        {
            return "Bitte zuerst eine Gerätetestdatei laden.";
        }

        return null;
    }

    private string BuildDeviceOutputPreview(
        XdtBaukastenState state,
        InterfaceProfileDefinition? interfaceProfile,
        DateTimeOffset? timestamp,
        List<string> messages)
    {
        if (!state.IsBidirectionalDevice)
        {
            return "Für dieses Gerät ist keine Ausgabe an ein bidirektionales Gerät vorhanden.";
        }

        if (state.AisInput is null || !File.Exists(state.AisInput.SourcePath))
        {
            return "Geräteausgabe-Vorschau nicht möglich: Bitte zuerst eine AIS-Datei mit Patientendaten/Historie laden.";
        }

        try
        {
            var history = _historyParser.ParseFile(state.AisInput.SourcePath);
            var isRt6100 = InterfaceProfileUiPolicy.IsNidekRt6100(interfaceProfile, state.DeviceProfile);
            var isCv5000 = InterfaceProfileUiPolicy.IsCv5000(interfaceProfile, state.DeviceProfile);
            if (isRt6100)
            {
                var selected = _historyParser.CreateDefaultRt6100Selection(history.Records);
                var result = _rt6100Writer.BuildXml(
                    new Cv5000ImportSelection(history.Patient, selected, null, NidekRt6100InputXmlWriter.DefaultFileNameTemplate),
                    timestamp);
                messages.AddRange(result.Warnings.Select(warning => $"Geräteausgabe RT-6100: {warning}"));
                if (!result.Success || string.IsNullOrWhiteSpace(result.XmlContent))
                {
                    return result.ErrorMessage ?? "RT-6100-Geräteausgabe konnte nicht erzeugt werden.";
                }

                var projected = XdtBaukastenDeviceOutputRuleService.ApplyRulesToXml(
                    result.XmlContent,
                    state.WorkingDeviceOutputRules,
                    history.Patient,
                    selected,
                    state.DeviceProfile);
                messages.AddRange(projected.Warnings.Select(warning => $"Geräteausgabe RT-6100: {warning}"));
                return projected.Content;
            }

            if (isCv5000)
            {
                var selected = _historyParser.CreateDefaultCv5000Selection(history.Records);
                var result = _cv5000Writer.BuildXml(
                    new Cv5000ImportSelection(history.Patient, selected, null, "CVImport.xml"),
                    timestamp);
                messages.AddRange(result.Warnings.Select(warning => $"Geräteausgabe CV-5000: {warning}"));
                if (!result.Success || string.IsNullOrWhiteSpace(result.XmlContent))
                {
                    return result.ErrorMessage ?? "CV-5000-Geräteausgabe konnte nicht erzeugt werden.";
                }

                var projected = XdtBaukastenDeviceOutputRuleService.ApplyRulesToXml(
                    result.XmlContent,
                    state.WorkingDeviceOutputRules,
                    history.Patient,
                    selected,
                    state.DeviceProfile);
                messages.AddRange(projected.Warnings.Select(warning => $"Geräteausgabe CV-5000: {warning}"));
                return projected.Content;
            }

            if (XdtBaukastenDeviceOutputRuleService.IsNidekRtSerial(state.DeviceProfile))
            {
                var selected = _historyParser.CreateDefaultRt6100Selection(history.Records);
                var model = DetectRtSerialModel(state.DeviceProfile);
                var result = _rtSerialWriter.BuildFrame(history.Patient, selected, model);
                if (!result.Success)
                {
                    return result.ErrorMessage ?? "NIDEK-RT-RS232-Geräteausgabe konnte nicht erzeugt werden.";
                }

                var rulePreview = XdtBaukastenDeviceOutputRuleService.BuildRuleTextPreview(
                    state.WorkingDeviceOutputRules,
                    history.Patient,
                    selected,
                    state.DeviceProfile);
                messages.AddRange(rulePreview.Warnings.Select(warning => $"Geräteausgabe NIDEK RT Serial: {warning}"));
                return "NIDEK RT-2100/3100/5100 RS232 ist vorbereitet. Bitte echte Praxis-Mitschnitte prüfen, bevor produktiv gesendet wird."
                    + Environment.NewLine
                    + Environment.NewLine
                    + result.VisibleContent
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Hexdump:"
                    + Environment.NewLine
                    + result.HexDump
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Baukasten-Regelwerte:"
                    + Environment.NewLine
                    + rulePreview.Content;
            }

            var textPreview = XdtBaukastenDeviceOutputRuleService.BuildRuleTextPreview(
                state.WorkingDeviceOutputRules,
                history.Patient,
                history.Records,
                state.DeviceProfile);
            messages.AddRange(textPreview.Warnings.Select(warning => $"Geräteausgabe: {warning}"));
            return textPreview.Content;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return $"Geräteausgabe-Vorschau konnte nicht erzeugt werden: {ex.Message}";
        }
    }

    private static XdtBaukastenPreviewResult CreateFailure(string message)
    {
        var output = new XdtBaukastenOutputPreview(
            RawXdt: string.Empty,
            AisView: message,
            DeviceOutput: message,
            Diagnostics: message,
            Messages: new[] { message },
            Documents: CreateFailureDocuments(message));

        return new XdtBaukastenPreviewResult(
            Success: false,
            PipelineResult: null,
            Output: output,
            Messages: new[] { message });
    }

    private static string AppendDeviceOutputDiagnostics(string diagnostics, IReadOnlyList<string> messages)
    {
        var deviceOutputMessages = messages
            .Where(message => message.StartsWith("Geräteausgabe", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (deviceOutputMessages.Length == 0)
        {
            return diagnostics;
        }

        var builder = new StringBuilder(diagnostics.TrimEnd());
        if (builder.Length > 0)
        {
            builder.AppendLine();
            builder.AppendLine();
        }

        builder.AppendLine("Geräteausgabe-Regeln");
        foreach (var message in deviceOutputMessages)
        {
            builder.AppendLine($"- {message}");
        }

        return builder.ToString().TrimEnd();
    }

    private static string CreateAisCardView(ProcessingPipelineResult result)
    {
        if (result.ExportRecords.Count == 0)
        {
            return "Noch keine fachlich sichtbare AIS-Ausgabe erzeugt.";
        }

        var visibleLines = result.ExportRecords
            .Where(record => !IsHiddenInAisCardView(record.FieldCode))
            .OrderBy(record => record.SortOrder)
            .Select(record => record.Value?.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        if (visibleLines.Length == 0)
        {
            return "Noch keine fachlich sichtbare AIS-Ausgabe erzeugt.";
        }

        return string.Join(Environment.NewLine, visibleLines);
    }

    private static IReadOnlyDictionary<XdtBaukastenResultView, XdtBaukastenPreviewDocument> CreatePreviewDocuments(
        string rawXdt,
        string aisView,
        string deviceOutput,
        string diagnostics,
        ProcessingPipelineResult? pipelineResult,
        ExportProfileDefinition exportProfile,
        IReadOnlyList<ExportRuleDefinition> deviceOutputRules)
    {
        return new Dictionary<XdtBaukastenResultView, XdtBaukastenPreviewDocument>
        {
            [XdtBaukastenResultView.RawXdt] = CreateRawXdtDocument(rawXdt, pipelineResult, exportProfile),
            [XdtBaukastenResultView.AisView] = CreateAisViewDocument(aisView, pipelineResult, exportProfile),
            [XdtBaukastenResultView.DeviceOutput] = CreateDeviceOutputDocument(deviceOutput, deviceOutputRules),
            [XdtBaukastenResultView.Diagnostics] = CreatePlainDocument(
                XdtBaukastenResultView.Diagnostics,
                diagnostics,
                XdtBaukastenRuleDirection.AisExport,
                warningPredicate: line => line.Contains("Fehler", StringComparison.OrdinalIgnoreCase)
                    || line.Contains("Warning", StringComparison.OrdinalIgnoreCase)
                    || line.Contains("Warnung", StringComparison.OrdinalIgnoreCase))
        };
    }

    private static IReadOnlyDictionary<XdtBaukastenResultView, XdtBaukastenPreviewDocument> CreateFailureDocuments(string message)
    {
        return new Dictionary<XdtBaukastenResultView, XdtBaukastenPreviewDocument>
        {
            [XdtBaukastenResultView.RawXdt] = CreatePlainDocument(XdtBaukastenResultView.RawXdt, string.Empty, XdtBaukastenRuleDirection.AisExport),
            [XdtBaukastenResultView.AisView] = CreatePlainDocument(XdtBaukastenResultView.AisView, message, XdtBaukastenRuleDirection.AisExport, _ => true),
            [XdtBaukastenResultView.DeviceOutput] = CreatePlainDocument(XdtBaukastenResultView.DeviceOutput, message, XdtBaukastenRuleDirection.DeviceOutput, _ => true),
            [XdtBaukastenResultView.Diagnostics] = CreatePlainDocument(XdtBaukastenResultView.Diagnostics, message, XdtBaukastenRuleDirection.AisExport, _ => true)
        };
    }

    private static XdtBaukastenPreviewDocument CreateRawXdtDocument(
        string plainText,
        ProcessingPipelineResult? pipelineResult,
        ExportProfileDefinition exportProfile)
    {
        var records = pipelineResult?.ExportRecords.OrderBy(record => record.SortOrder).ToArray()
            ?? Array.Empty<ExportFieldRecord>();
        var lines = SplitPreviewLines(plainText);
        var previewLines = new List<XdtBaukastenPreviewLine>(lines.Count);
        for (var index = 0; index < lines.Count; index++)
        {
            var record = index < records.Length ? records[index] : null;
            var rule = record is null ? null : FindRuleForRecord(exportProfile, record);
            previewLines.Add(new XdtBaukastenPreviewLine(
                index + 1,
                lines[index],
                XdtBaukastenResultView.RawXdt,
                XdtBaukastenRuleDirection.AisExport,
                rule?.Id,
                rule is null ? null : IndexOfRule(exportProfile.Rules, rule),
                rule?.TargetName,
                record?.FieldCode));
        }

        return new XdtBaukastenPreviewDocument(XdtBaukastenResultView.RawXdt, plainText, previewLines);
    }

    private static XdtBaukastenPreviewDocument CreateAisViewDocument(
        string plainText,
        ProcessingPipelineResult? pipelineResult,
        ExportProfileDefinition exportProfile)
    {
        var records = pipelineResult?.ExportRecords
            .Where(record => !IsHiddenInAisCardView(record.FieldCode))
            .OrderBy(record => record.SortOrder)
            .Where(record => !string.IsNullOrWhiteSpace(record.Value?.Trim()))
            .ToArray()
            ?? Array.Empty<ExportFieldRecord>();
        var lines = SplitPreviewLines(plainText);
        var previewLines = new List<XdtBaukastenPreviewLine>(lines.Count);
        for (var index = 0; index < lines.Count; index++)
        {
            var record = index < records.Length ? records[index] : null;
            var rule = record is null ? null : FindRuleForRecord(exportProfile, record);
            previewLines.Add(new XdtBaukastenPreviewLine(
                index + 1,
                lines[index],
                XdtBaukastenResultView.AisView,
                XdtBaukastenRuleDirection.AisExport,
                rule?.Id,
                rule is null ? null : IndexOfRule(exportProfile.Rules, rule),
                rule?.TargetName,
                record?.FieldCode));
        }

        return new XdtBaukastenPreviewDocument(XdtBaukastenResultView.AisView, plainText, previewLines);
    }

    private static XdtBaukastenPreviewDocument CreateDeviceOutputDocument(
        string plainText,
        IReadOnlyList<ExportRuleDefinition> deviceOutputRules)
    {
        var activeRules = deviceOutputRules
            .Where(rule => rule.IsEnabled)
            .OrderBy(rule => rule.SortOrder)
            .ThenBy(rule => rule.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var lines = SplitPreviewLines(plainText);
        var previewLines = new List<XdtBaukastenPreviewLine>(lines.Count);
        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];
            var rule = activeRules.FirstOrDefault(current => MatchesDeviceOutputLine(line, current));
            previewLines.Add(new XdtBaukastenPreviewLine(
                index + 1,
                line,
                XdtBaukastenResultView.DeviceOutput,
                XdtBaukastenRuleDirection.DeviceOutput,
                rule?.Id,
                rule is null ? null : IndexOfRule(deviceOutputRules, rule),
                rule?.TargetName,
                rule?.TargetFieldCode));
        }

        return new XdtBaukastenPreviewDocument(XdtBaukastenResultView.DeviceOutput, plainText, previewLines);
    }

    private static XdtBaukastenPreviewDocument CreatePlainDocument(
        XdtBaukastenResultView viewKind,
        string plainText,
        XdtBaukastenRuleDirection direction,
        Func<string, bool>? warningPredicate = null)
    {
        var lines = SplitPreviewLines(plainText);
        var previewLines = lines
            .Select((line, index) => new XdtBaukastenPreviewLine(
                index + 1,
                line,
                viewKind,
                direction,
                IsWarning: warningPredicate?.Invoke(line) == true))
            .ToArray();

        return new XdtBaukastenPreviewDocument(viewKind, plainText, previewLines);
    }

    private static IReadOnlyList<string> SplitPreviewLines(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Array.Empty<string>();
        }

        return text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
    }

    private static ExportRuleDefinition? FindRuleForRecord(ExportProfileDefinition exportProfile, ExportFieldRecord record)
    {
        return exportProfile.Rules
            .Where(rule => rule.IsEnabled)
            .OrderBy(rule => rule.SortOrder)
            .ThenBy(rule => rule.Id, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(rule =>
                rule.SortOrder == record.SortOrder
                && string.Equals(rule.TargetFieldCode, record.FieldCode, StringComparison.OrdinalIgnoreCase));
    }

    private static int IndexOfRule(IReadOnlyList<ExportRuleDefinition> rules, ExportRuleDefinition rule)
    {
        var ordered = rules
            .OrderBy(current => current.SortOrder)
            .ThenBy(current => current.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        for (var index = 0; index < ordered.Length; index++)
        {
            if (string.Equals(ordered[index].Id, rule.Id, StringComparison.OrdinalIgnoreCase))
            {
                return index + 1;
            }
        }

        return 0;
    }

    private static bool MatchesDeviceOutputLine(string line, ExportRuleDefinition rule)
    {
        if (string.IsNullOrWhiteSpace(rule.TargetFieldCode))
        {
            return false;
        }

        if (line.StartsWith(rule.TargetFieldCode + ":", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var leaf = rule.TargetFieldCode.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).LastOrDefault();
        if (string.IsNullOrWhiteSpace(leaf))
        {
            return false;
        }

        var pattern = $@"<[^<>\s/]*:?{Regex.Escape(leaf)}(?:\s|>|/)";
        return Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static string CreateDiagnosticsView(
        ProcessingPipelineResult result,
        XdtBaukastenState state,
        ExportProfileDefinition exportProfile,
        XdtBaukastenDeviceCompatibilityResult compatibility)
    {
        var builder = new StringBuilder();
        AppendCompatibilityDiagnostics(builder, state, exportProfile, compatibility);

        if (result.Patient is not null)
        {
            builder.AppendLine("Patient");
            builder.AppendLine($"- Nummer: {Display(result.Patient.PatientNumber)}");
            builder.AppendLine($"- Name: {Display(result.Patient.FirstName)} {Display(result.Patient.LastName)}".TrimEnd());
            builder.AppendLine($"- Geburtsdatum: {Display(result.Patient.BirthDate)}");
            builder.AppendLine();
        }

        builder.AppendLine($"Messwerte erkannt: {result.Measurements.Count}");
        foreach (var measurement in result.Measurements.Take(80))
        {
            builder.AppendLine($"- {measurement.SourcePath}: {measurement.Value} {measurement.Unit}".TrimEnd());
        }

        builder.AppendLine();
        builder.AppendLine($"XDT-Felder: {result.ExportRecords.Count}");
        foreach (var record in result.ExportRecords)
        {
            builder.AppendLine($"- {record.FieldCode}: {record.Value}");
        }

        if (result.Issues.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Hinweise/Fehler");
            foreach (var issue in result.Issues)
            {
                builder.AppendLine($"- {issue.Severity}: {issue.Message}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendCompatibilityDiagnostics(
        StringBuilder builder,
        XdtBaukastenState state,
        ExportProfileDefinition exportProfile,
        XdtBaukastenDeviceCompatibilityResult compatibility)
    {
        builder.AppendLine("Kompatibilität");
        builder.AppendLine($"- Profil: {Display(state.DeviceProfile?.Metadata.Name)}");
        builder.AppendLine($"- Profil-Hersteller: {Display(state.DeviceProfile?.Manufacturer)}");
        builder.AppendLine($"- Datei-Hersteller: {Display(compatibility.DetectedCompany)}");
        builder.AppendLine($"- Profil-Modell: {Display(state.DeviceProfile?.Model)}");
        builder.AppendLine($"- Datei-ModelName: {Display(compatibility.DetectedModelName)}");
        builder.AppendLine($"- Parser: {Display(state.DeviceProfile?.ParserMode)}");
        builder.AppendLine($"- Exportprofil: {Display(exportProfile.Metadata.Name)}");
        builder.AppendLine($"- Status: {compatibility.Status}");
        builder.AppendLine($"- Preview trotz Abweichung: {(compatibility.IsWarning && compatibility.AllowsPreview ? "Ja" : "Nein")}");
        if (compatibility.IsWarning)
        {
            builder.AppendLine($"- Hinweis: {compatibility.Message}");
            builder.AppendLine("- Baukastenmodus: Modellabweichungen blockieren nicht. Produktive Verarbeitung prüft strenger.");
        }

        builder.AppendLine();
    }

    private static string CreateSerialDiagnosticsView(SerialRawDeviceInput input)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"COM-Port: {input.PortName}");
        builder.AppendLine($"Empfangen am: {input.ReceivedAt:dd.MM.yyyy HH:mm:ss}");
        builder.AppendLine($"Zeichen: {input.RawText.Length}");
        builder.AppendLine();
        builder.AppendLine(input.RawText);
        if (!string.IsNullOrWhiteSpace(input.HexDump))
        {
            builder.AppendLine();
            builder.AppendLine("Hexdump");
            builder.AppendLine(input.HexDump);
        }

        return builder.ToString().TrimEnd();
    }

    private static NidekRtSerialPhoropterModel DetectRtSerialModel(DeviceProfileDefinition? profile)
    {
        var text = string.Join(" ", profile?.Metadata.Id, profile?.Metadata.Name, profile?.Model, profile?.Metadata.Product);
        var normalized = Regex.Replace(text, "[^A-Za-z0-9]", string.Empty).ToUpperInvariant();
        if (normalized.Contains("RT2100", StringComparison.Ordinal))
        {
            return NidekRtSerialPhoropterModel.Rt2100;
        }

        if (normalized.Contains("RT3100", StringComparison.Ordinal))
        {
            return NidekRtSerialPhoropterModel.Rt3100;
        }

        if (normalized.Contains("RT5100", StringComparison.Ordinal))
        {
            return NidekRtSerialPhoropterModel.Rt5100;
        }

        return NidekRtSerialPhoropterModel.Unknown;
    }

    private static string Display(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
    }

    private static bool IsHiddenInAisCardView(string fieldCode)
    {
        return fieldCode is "3000" or "3100" or "3101" or "3102" or "3103" or "3110" or "3622" or "8000" or "8402";
    }
}
