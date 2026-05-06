using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileManualProcessor
{
    private readonly GdtParser _gdtParser = new();
    private readonly PatientDataMapper _patientDataMapper = new();
    private readonly XmlDeviceParser _xmlDeviceParser = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly XdtExportBuilder _xdtExportBuilder = new();
    private readonly ExportFileNameBuilder _fileNameBuilder = new();
    private readonly FileExportService _fileExportService = new();

    public InterfaceProfileManualProcessingResult Process(
        InterfaceProfileDefinition interfaceProfile,
        ExportProfileDefinition exportProfile,
        string aisFilePath,
        string deviceFilePath,
        DateTime timestamp)
    {
        ArgumentNullException.ThrowIfNull(interfaceProfile);
        ArgumentNullException.ThrowIfNull(exportProfile);

        var messages = new List<string>();
        var issues = new List<ProcessingIssue>();

        if (string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.ExportFolder))
        {
            return CreateErrorResult("Exportordner fehlt.", issues);
        }

        if (!string.Equals(Path.GetExtension(deviceFilePath), ".xml", StringComparison.OrdinalIgnoreCase))
        {
            return CreateErrorResult("Dieser Dateityp wird für die manuelle Paarverarbeitung noch nicht unterstützt.", issues);
        }

        var gdtResult = _gdtParser.ParseFile(aisFilePath);
        issues.AddRange(gdtResult.Issues.Select(issue => new ProcessingIssue(
            issue.Severity == GdtParseIssueSeverity.Error ? ProcessingIssueSeverity.Error : ProcessingIssueSeverity.Warning,
            ProcessingStage.GdtParsing,
            issue.Message)));
        if (gdtResult.HasErrors)
        {
            return CreateErrorResult("AIS-Datei konnte nicht fehlerfrei gelesen werden.", issues);
        }

        var patient = _patientDataMapper.Map(gdtResult.Records);

        var deviceResult = _xmlDeviceParser.ParseFile(deviceFilePath);
        issues.AddRange(deviceResult.Issues.Select(issue => new ProcessingIssue(
            issue.Severity == DeviceParseIssueSeverity.Error ? ProcessingIssueSeverity.Error : ProcessingIssueSeverity.Warning,
            ProcessingStage.DeviceParsing,
            issue.Message)));
        if (deviceResult.HasErrors)
        {
            return new InterfaceProfileManualProcessingResult(
                Success: false,
                ExportFilePath: null,
                ExportContent: null,
                PipelineResult: new ProcessingPipelineResult(patient, deviceResult.Measurements, [], string.Empty, issues),
                Messages: new[] { "Gerätedatei konnte nicht fehlerfrei gelesen werden." });
        }

        var mappingRules = _mappingAdapter.Adapt(exportProfile);
        var mappingResult = _mappingEngine.Map(patient, deviceResult.Measurements, mappingRules);
        issues.AddRange(mappingResult.Issues.Select(issue => new ProcessingIssue(
            issue.Severity == MappingIssueSeverity.Error ? ProcessingIssueSeverity.Error : ProcessingIssueSeverity.Warning,
            ProcessingStage.Mapping,
            issue.Message)));
        if (mappingResult.HasErrors)
        {
            return new InterfaceProfileManualProcessingResult(
                Success: false,
                ExportFilePath: null,
                ExportContent: null,
                PipelineResult: new ProcessingPipelineResult(patient, deviceResult.Measurements, mappingResult.Records, string.Empty, issues),
                Messages: new[] { "Mapping konnte nicht fehlerfrei ausgeführt werden." });
        }

        var exportResult = _xdtExportBuilder.Build(mappingResult.Records);
        issues.AddRange(exportResult.Issues.Select(issue => new ProcessingIssue(
            issue.Severity == XdtExportIssueSeverity.Error ? ProcessingIssueSeverity.Error : ProcessingIssueSeverity.Warning,
            ProcessingStage.Export,
            issue.Message)));
        if (exportResult.HasErrors)
        {
            return new InterfaceProfileManualProcessingResult(
                Success: false,
                ExportFilePath: null,
                ExportContent: exportResult.Content,
                PipelineResult: new ProcessingPipelineResult(patient, deviceResult.Measurements, mappingResult.Records, exportResult.Content, issues),
                Messages: new[] { "XDT-Export konnte nicht fehlerfrei erzeugt werden." });
        }

        var fileName = _fileNameBuilder.Build(
            $"{interfaceProfile.Metadata.Name}_{{PatientNumber}}_{{yyyyMMdd_HHmmss}}.XDT",
            patient,
            timestamp,
            interfaceProfile.Metadata.Name);
        var fileExportResult = _fileExportService.Export(
            interfaceProfile.FolderOptions.ExportFolder,
            fileName,
            exportResult.Content,
            exportProfile.OutputEncoding);
        if (fileExportResult.HasErrors)
        {
            messages.AddRange(fileExportResult.Issues.Select(issue => issue.Message));
            return new InterfaceProfileManualProcessingResult(
                Success: false,
                ExportFilePath: null,
                ExportContent: exportResult.Content,
                PipelineResult: new ProcessingPipelineResult(patient, deviceResult.Measurements, mappingResult.Records, exportResult.Content, issues),
                Messages: messages);
        }

        messages.Add("Dateipaar erfolgreich verarbeitet.");
        return new InterfaceProfileManualProcessingResult(
            Success: true,
            ExportFilePath: fileExportResult.FilePath,
            ExportContent: exportResult.Content,
            PipelineResult: new ProcessingPipelineResult(patient, deviceResult.Measurements, mappingResult.Records, exportResult.Content, issues),
            Messages: messages);
    }

    private static InterfaceProfileManualProcessingResult CreateErrorResult(
        string message,
        IReadOnlyList<ProcessingIssue> issues)
    {
        return new InterfaceProfileManualProcessingResult(
            Success: false,
            ExportFilePath: null,
            ExportContent: null,
            PipelineResult: null,
            Messages: new[] { message });
    }
}
