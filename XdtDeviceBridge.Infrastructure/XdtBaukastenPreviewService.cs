using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class XdtBaukastenPreviewService
{
    private readonly BuilderManualProcessingPreviewService _manualPreviewService;
    private readonly MedistarHistoricalMeasurementParser _historyParser;
    private readonly TopconCv5000ImportXmlWriter _cv5000Writer;
    private readonly NidekRt6100InputXmlWriter _rt6100Writer;
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
        var output = new XdtBaukastenOutputPreview(
            RawXdt: rawXdt,
            AisView: aisView,
            DeviceOutput: deviceOutput,
            Diagnostics: diagnostics,
            Messages: messages);

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
            Messages: new[] { message });

        return new XdtBaukastenPreviewResult(
            Success: false,
            PipelineResult: null,
            Output: output,
            Messages: new[] { message });
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

    private static string Display(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
    }

    private static bool IsHiddenInAisCardView(string fieldCode)
    {
        return fieldCode is "3000" or "3100" or "3101" or "3102" or "3103" or "3110" or "3622" or "8000" or "8402";
    }
}
