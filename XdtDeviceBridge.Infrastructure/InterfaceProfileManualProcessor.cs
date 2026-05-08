using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileManualProcessor : IInterfaceProfileManualProcessor
{
    private readonly GdtParser _gdtParser = new();
    private readonly PatientDataMapper _patientDataMapper = new();
    private readonly XmlDeviceParser _xmlDeviceParser = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly XdtExportBuilder _xdtExportBuilder = new();
    private readonly ExportFileNameBuilder _fileNameBuilder = new();
    private readonly FileExportService _fileExportService = new();
    private readonly ProcessedFileArchiveService _processedFileArchiveService = new();
    private readonly FailedFileCopyService _failedFileCopyService = new();

    public InterfaceProfileManualProcessingResult Process(
        InterfaceProfileDefinition interfaceProfile,
        ExportProfileDefinition exportProfile,
        string aisFilePath,
        string deviceFilePath,
        DateTime timestamp,
        Func<PatientData, AttachmentProcessingStatus?>? attachmentPreparation = null)
    {
        ArgumentNullException.ThrowIfNull(interfaceProfile);
        ArgumentNullException.ThrowIfNull(exportProfile);

        var messages = new List<string>();
        var issues = new List<ProcessingIssue>();

        if (string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.ExportFolder))
        {
            return CreateFailureResult(
                interfaceProfile,
                aisFilePath,
                deviceFilePath,
                timestamp,
                new[] { "Exportordner fehlt." },
                pipelineResult: null,
                exportContent: null);
        }

        if (!string.Equals(Path.GetExtension(deviceFilePath), ".xml", StringComparison.OrdinalIgnoreCase))
        {
            return CreateFailureResult(
                interfaceProfile,
                aisFilePath,
                deviceFilePath,
                timestamp,
                new[] { "Dieser Dateityp wird für die manuelle Paarverarbeitung noch nicht unterstützt." },
                pipelineResult: null,
                exportContent: null);
        }

        var gdtResult = _gdtParser.ParseFile(aisFilePath);
        issues.AddRange(gdtResult.Issues.Select(issue => new ProcessingIssue(
            issue.Severity == GdtParseIssueSeverity.Error ? ProcessingIssueSeverity.Error : ProcessingIssueSeverity.Warning,
            ProcessingStage.GdtParsing,
            issue.Message)));
        if (gdtResult.HasErrors)
        {
            return CreateFailureResult(
                interfaceProfile,
                aisFilePath,
                deviceFilePath,
                timestamp,
                new[] { "AIS-Datei konnte nicht fehlerfrei gelesen werden." },
                new ProcessingPipelineResult(null, [], [], string.Empty, issues),
                exportContent: null);
        }

        var patient = _patientDataMapper.Map(gdtResult.Records);

        var deviceResult = _xmlDeviceParser.ParseFile(deviceFilePath);
        issues.AddRange(deviceResult.Issues.Select(issue => new ProcessingIssue(
            issue.Severity == DeviceParseIssueSeverity.Error ? ProcessingIssueSeverity.Error : ProcessingIssueSeverity.Warning,
            ProcessingStage.DeviceParsing,
            issue.Message)));
        if (deviceResult.HasErrors)
        {
            return CreateFailureResult(
                interfaceProfile,
                aisFilePath,
                deviceFilePath,
                timestamp,
                new[] { "Gerätedatei konnte nicht fehlerfrei gelesen werden." },
                new ProcessingPipelineResult(patient, deviceResult.Measurements, [], string.Empty, issues),
                exportContent: null);
        }

        var mappingRules = _mappingAdapter.Adapt(exportProfile);
        var mappingResult = _mappingEngine.Map(patient, deviceResult.Measurements, mappingRules);
        issues.AddRange(mappingResult.Issues.Select(issue => new ProcessingIssue(
            issue.Severity == MappingIssueSeverity.Error ? ProcessingIssueSeverity.Error : ProcessingIssueSeverity.Warning,
            ProcessingStage.Mapping,
            issue.Message)));
        if (mappingResult.HasErrors)
        {
            return CreateFailureResult(
                interfaceProfile,
                aisFilePath,
                deviceFilePath,
                timestamp,
                new[] { "Mapping konnte nicht fehlerfrei ausgeführt werden." },
                new ProcessingPipelineResult(patient, deviceResult.Measurements, mappingResult.Records, string.Empty, issues),
                exportContent: null);
        }

        var exportRecords = mappingResult.Records;
        var attachmentStatus = attachmentPreparation?.Invoke(patient);
        if (attachmentStatus is not null)
        {
            messages.Add(attachmentStatus.Message);
            if (ShouldAppendAttachmentFields(attachmentStatus))
            {
                exportRecords = AppendAttachmentFields(mappingResult.Records, attachmentStatus.PreparedFields);
                messages.Add("XDT-Anhang-Linkfelder wurden in die Exportdatei übernommen.");
            }
        }

        var exportResult = _xdtExportBuilder.Build(exportRecords);
        issues.AddRange(exportResult.Issues.Select(issue => new ProcessingIssue(
            issue.Severity == XdtExportIssueSeverity.Error ? ProcessingIssueSeverity.Error : ProcessingIssueSeverity.Warning,
            ProcessingStage.Export,
            issue.Message)));
        if (exportResult.HasErrors)
        {
            return CreateFailureResult(
                interfaceProfile,
                aisFilePath,
                deviceFilePath,
                timestamp,
                new[] { "XDT-Export konnte nicht fehlerfrei erzeugt werden." },
                new ProcessingPipelineResult(patient, deviceResult.Measurements, exportRecords, exportResult.Content, issues),
                exportResult.Content);
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
            return CreateFailureResult(
                interfaceProfile,
                aisFilePath,
                deviceFilePath,
                timestamp,
                fileExportResult.Issues.Select(issue => issue.Message).ToList(),
                new ProcessingPipelineResult(patient, deviceResult.Measurements, exportRecords, exportResult.Content, issues),
                exportResult.Content);
        }

        messages.Add("Dateipaar erfolgreich verarbeitet.");
        var archiveResult = ArchiveProcessedFilesIfEnabled(
            interfaceProfile,
            aisFilePath,
            deviceFilePath,
            timestamp.ToUniversalTime(),
            messages);

        return new InterfaceProfileManualProcessingResult(
            Success: true,
            ExportFilePath: fileExportResult.FilePath,
            ExportContent: exportResult.Content,
            PipelineResult: new ProcessingPipelineResult(patient, deviceResult.Measurements, exportRecords, exportResult.Content, issues),
            ArchiveResult: archiveResult,
            FailedFileCopyResult: null,
            Messages: messages,
            AttachmentStatus: attachmentStatus);
    }

    private static bool ShouldAppendAttachmentFields(AttachmentProcessingStatus attachmentStatus)
    {
        if (!attachmentStatus.Success
            || attachmentStatus.WasSkipped
            || attachmentStatus.PreparedFields.Count == 0)
        {
            return false;
        }

        return attachmentStatus.PreparedFields.Any(field => field.FieldCode == "6302")
            && attachmentStatus.PreparedFields.Any(field => field.FieldCode == "6303")
            && attachmentStatus.PreparedFields.Any(field => field.FieldCode == "6305");
    }

    private static IReadOnlyList<ExportFieldRecord> AppendAttachmentFields(
        IReadOnlyList<ExportFieldRecord> existingRecords,
        IReadOnlyList<ExportFieldRecord> attachmentFields)
    {
        var combined = existingRecords.ToList();
        var nextSortOrder = combined.Count == 0 ? 0 : combined.Max(record => record.SortOrder);
        foreach (var field in attachmentFields)
        {
            combined.Add(new ExportFieldRecord(
                field.FieldCode,
                field.Value,
                ++nextSortOrder));
        }

        return combined;
    }

    private InterfaceProfileManualProcessingResult CreateFailureResult(
        InterfaceProfileDefinition interfaceProfile,
        string aisFilePath,
        string deviceFilePath,
        DateTime failedAt,
        IReadOnlyList<string> failureMessages,
        ProcessingPipelineResult? pipelineResult,
        string? exportContent)
    {
        var messages = failureMessages.ToList();
        var failedFileCopyResult = CopyFailedFilesIfEnabled(
            interfaceProfile,
            aisFilePath,
            deviceFilePath,
            failedAt.ToUniversalTime(),
            string.Join(Environment.NewLine, failureMessages),
            messages);

        return new InterfaceProfileManualProcessingResult(
            Success: false,
            ExportFilePath: null,
            ExportContent: exportContent,
            PipelineResult: pipelineResult,
            ArchiveResult: null,
            FailedFileCopyResult: failedFileCopyResult,
            Messages: messages);
    }

    private ProcessedFileArchiveResult? ArchiveProcessedFilesIfEnabled(
        InterfaceProfileDefinition interfaceProfile,
        string aisFilePath,
        string deviceFilePath,
        DateTime processedAtUtc,
        List<string> messages)
    {
        if (!interfaceProfile.FolderOptions.ArchiveProcessedFiles)
        {
            messages.Add("Archivierung ist für dieses Schnittstellenprofil deaktiviert.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.ArchiveFolder))
        {
            var archiveResult = new ProcessedFileArchiveResult(
                ArchivedFiles: Array.Empty<string>(),
                Issues: new[] { "Archivordner fehlt." },
                HasErrors: true);
            messages.Add("Archivierung fehlgeschlagen: Archivordner fehlt.");
            return archiveResult;
        }

        try
        {
            var moveFiles = interfaceProfile.FolderOptions.ArchiveProcessedFileMode == ArchiveProcessedFileMode.Move;
            var archiveResult = _processedFileArchiveService.ArchiveProcessedFiles(
                interfaceProfile.FolderOptions.ArchiveFolder,
                interfaceProfile.Metadata.Name,
                aisFilePath,
                deviceFilePath,
                processedAtUtc,
                moveFiles);

            if (archiveResult.HasErrors)
            {
                messages.Add("Archivierung mit Fehlern abgeschlossen.");
                messages.AddRange(archiveResult.Issues);
                return archiveResult;
            }

            messages.Add(moveFiles
                ? "Importdateien wurden ins Archiv verschoben:"
                : "Importdateien wurden ins Archiv kopiert. Originale bleiben erhalten:");
            messages.AddRange(archiveResult.ArchivedFiles);
            return archiveResult;
        }
        catch (Exception ex)
        {
            var archiveResult = new ProcessedFileArchiveResult(
                ArchivedFiles: Array.Empty<string>(),
                Issues: new[] { ex.Message },
                HasErrors: true);
            messages.Add($"Archivierung fehlgeschlagen: {ex.Message}");
            return archiveResult;
        }
    }

    private FailedFileCopyResult? CopyFailedFilesIfEnabled(
        InterfaceProfileDefinition interfaceProfile,
        string aisFilePath,
        string deviceFilePath,
        DateTime failedAtUtc,
        string failureReason,
        List<string> messages)
    {
        if (!interfaceProfile.FolderOptions.MoveFailedFilesToErrorFolder)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.ErrorFolder))
        {
            var failedFileCopyResult = new FailedFileCopyResult(
                CopiedFiles: Array.Empty<string>(),
                Issues: new[] { "Fehlerordner ist nicht konfiguriert." },
                HasErrors: true);
            messages.Add("Fehlerordner ist nicht konfiguriert.");
            return failedFileCopyResult;
        }

        try
        {
            var failedFileCopyResult = _failedFileCopyService.CopyFailedFiles(
                interfaceProfile.FolderOptions.ErrorFolder,
                interfaceProfile.Metadata.Name,
                aisFilePath,
                deviceFilePath,
                failedAtUtc,
                failureReason);

            if (failedFileCopyResult.HasErrors)
            {
                messages.Add("Fehlerablage fehlgeschlagen.");
                messages.AddRange(failedFileCopyResult.Issues);
                return failedFileCopyResult;
            }

            messages.Add("Fehlerhafte Importdateien wurden in den Fehlerordner kopiert; Originale bleiben erhalten:");
            messages.AddRange(failedFileCopyResult.CopiedFiles);
            return failedFileCopyResult;
        }
        catch (Exception ex)
        {
            var failedFileCopyResult = new FailedFileCopyResult(
                CopiedFiles: Array.Empty<string>(),
                Issues: new[] { ex.Message },
                HasErrors: true);
            messages.Add($"Fehlerablage fehlgeschlagen: {ex.Message}");
            return failedFileCopyResult;
        }
    }
}
