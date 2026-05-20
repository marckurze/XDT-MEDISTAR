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
        Func<PatientData, AttachmentProcessingStatus?>? attachmentPreparation = null,
        Func<PatientData, string?>? documentationTextProvider = null)
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

        var isAttachmentOnlyMode = interfaceProfile.FolderOptions.IsAttachmentOnlyMode;
        if (!isAttachmentOnlyMode
            && !string.Equals(Path.GetExtension(deviceFilePath), ".xml", StringComparison.OrdinalIgnoreCase))
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

        var documentationText = string.Empty;
        var deviceResult = isAttachmentOnlyMode
            ? CreateAttachmentOnlyDeviceResult(documentationTextProvider?.Invoke(patient), out documentationText)
            : _xmlDeviceParser.ParseFile(deviceFilePath);
        if (!isAttachmentOnlyMode)
        {
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
        }

        var effectiveExportProfile = isAttachmentOnlyMode
            ? RemoveAttachmentOnlyDocumentationRuleWhenEmpty(exportProfile, documentationText)
            : exportProfile;
        var mappingRules = _mappingAdapter.Adapt(effectiveExportProfile);
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

        var exportRecords = isAttachmentOnlyMode
            ? ExpandAttachmentOnlyDocumentationRecords(mappingResult.Records, documentationText)
            : mappingResult.Records;
        var attachmentStatus = attachmentPreparation?.Invoke(patient);
        if (attachmentStatus is not null)
        {
            messages.Add(attachmentStatus.Message);
            if (ShouldAppendAttachmentFields(attachmentStatus))
            {
                exportRecords = AppendAttachmentFields(exportRecords, attachmentStatus.PreparedFields);
                messages.Add("XDT-Anhang-Linkfelder wurden in die Exportdatei übernommen.");
            }
        }

        if (isAttachmentOnlyMode && !ShouldAppendAttachmentFields(attachmentStatus))
        {
            var failureMessages = new List<string> { "Dokumentanhang konnte nicht für MEDISTAR vorbereitet werden." };
            if (attachmentStatus is not null && !string.IsNullOrWhiteSpace(attachmentStatus.Message))
            {
                failureMessages.Add(attachmentStatus.Message);
            }

            return CreateFailureResult(
                interfaceProfile,
                aisFilePath,
                deviceFilePath,
                timestamp,
                failureMessages,
                new ProcessingPipelineResult(patient, deviceResult.Measurements, exportRecords, string.Empty, issues),
                exportContent: null);
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
            messages,
            allowMissingDeviceFile: isAttachmentOnlyMode);

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

    private static DeviceParseResult CreateAttachmentOnlyDeviceResult(
        string? documentationText,
        out string normalizedDocumentationText)
    {
        normalizedDocumentationText = documentationText?.Trim() ?? string.Empty;
        var measurements = string.IsNullOrWhiteSpace(normalizedDocumentationText)
            ? Array.Empty<MeasurementValue>()
            : new[]
            {
                new MeasurementValue(
                    SourcePath: "AttachmentOnly/DocumentationText",
                    DisplayName: "Dokumentationstext",
                    Value: normalizedDocumentationText,
                    Unit: null,
                    Eye: null,
                    Group: "AttachmentOnly")
            };

        return new DeviceParseResult(measurements, Array.Empty<DeviceParseIssue>());
    }

    private static ExportProfileDefinition RemoveAttachmentOnlyDocumentationRuleWhenEmpty(
        ExportProfileDefinition exportProfile,
        string documentationText)
    {
        if (!string.IsNullOrWhiteSpace(documentationText))
        {
            return exportProfile;
        }

        var rules = exportProfile.Rules
            .Where(rule => !IsAttachmentOnlyDocumentationRule(rule))
            .ToList();
        return exportProfile with
        {
            Rules = rules
        };
    }

    private static bool IsAttachmentOnlyDocumentationRule(ExportRuleDefinition rule)
    {
        return string.Equals(rule.TargetFieldCode, "6227", StringComparison.Ordinal)
            && string.Equals(rule.SourcePath, "Device.AttachmentOnly/DocumentationText", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<ExportFieldRecord> ExpandAttachmentOnlyDocumentationRecords(
        IReadOnlyList<ExportFieldRecord> records,
        string documentationText)
    {
        var expandedRecords = new List<ExportFieldRecord>();
        var documentationLines = SplitDocumentationLines(documentationText);
        foreach (var record in records)
        {
            if (!string.Equals(record.FieldCode, "6227", StringComparison.Ordinal))
            {
                expandedRecords.Add(record);
                continue;
            }

            foreach (var line in documentationLines)
            {
                expandedRecords.Add(new ExportFieldRecord(record.FieldCode, line, record.SortOrder));
            }
        }

        return expandedRecords;
    }

    private static IReadOnlyList<string> SplitDocumentationLines(string value)
    {
        return value
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
    }

    private static bool ShouldAppendAttachmentFields(AttachmentProcessingStatus? attachmentStatus)
    {
        if (attachmentStatus is null
            || !attachmentStatus.Success
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
        List<string> messages,
        bool allowMissingDeviceFile = false)
    {
        var options = interfaceProfile.FolderOptions;
        var removeAisFromImportFolder = options.ClearAisImportFolderBeforeProcessing;
        var removeDeviceFromImportFolder = options.ClearDeviceImportFolderBeforeProcessing;
        if (!options.ArchiveProcessedFiles
            && !removeAisFromImportFolder
            && !removeDeviceFromImportFolder)
        {
            messages.Add("Archivierung ist für dieses Schnittstellenprofil deaktiviert.");
            return null;
        }

        if (!options.ArchiveProcessedFiles)
        {
            var removedFiles = new List<string>();
            var issues = new List<string>();
            RemoveKnownProcessedFileIfRequested(aisFilePath, removeAisFromImportFolder, removedFiles, issues, allowMissingSourceFile: false);
            RemoveKnownProcessedFileIfRequested(deviceFilePath, removeDeviceFromImportFolder, removedFiles, issues, allowMissingDeviceFile);

            if (removedFiles.Count > 0)
            {
                messages.Add("Bekannte verarbeitete Importdateien wurden aus dem Importordner entfernt:");
                messages.AddRange(removedFiles);
            }

            if (issues.Count > 0)
            {
                messages.Add("Entfernen bekannter verarbeiteter Importdateien mit Fehlern abgeschlossen.");
                messages.AddRange(issues);
            }

            return null;
        }

        if (string.IsNullOrWhiteSpace(options.ArchiveFolder))
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
            var moveAllFiles = options.ArchiveProcessedFileMode == ArchiveProcessedFileMode.Move;
            var archivedFiles = new List<string>();
            var issues = new List<string>();
            ArchiveKnownProcessedFile(
                options.ArchiveFolder,
                interfaceProfile.Metadata.Name,
                aisFilePath,
                "AIS",
                processedAtUtc,
                moveAllFiles || removeAisFromImportFolder,
                archivedFiles,
                issues,
                allowMissingSourceFile: false);
            ArchiveKnownProcessedFile(
                options.ArchiveFolder,
                interfaceProfile.Metadata.Name,
                deviceFilePath,
                "Device",
                processedAtUtc,
                moveAllFiles || removeDeviceFromImportFolder,
                archivedFiles,
                issues,
                allowMissingDeviceFile);

            var archiveResult = new ProcessedFileArchiveResult(
                ArchivedFiles: archivedFiles,
                Issues: issues,
                HasErrors: issues.Count > 0);

            if (archiveResult.HasErrors)
            {
                messages.Add("Archivierung mit Fehlern abgeschlossen.");
                messages.AddRange(archiveResult.Issues);
                return archiveResult;
            }

            messages.Add(CreateArchiveSuccessMessage(
                moveAllFiles,
                removeAisFromImportFolder || removeDeviceFromImportFolder));
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

    private void ArchiveKnownProcessedFile(
        string archiveFolder,
        string interfaceProfileName,
        string sourceFilePath,
        string category,
        DateTime processedAtUtc,
        bool moveFile,
        List<string> archivedFiles,
        List<string> issues,
        bool allowMissingSourceFile = false)
    {
        if (allowMissingSourceFile && !File.Exists(sourceFilePath))
        {
            return;
        }

        var result = _processedFileArchiveService.ArchiveProcessedFile(
            archiveFolder,
            interfaceProfileName,
            sourceFilePath,
            category,
            processedAtUtc,
            moveFile);
        archivedFiles.AddRange(result.ArchivedFiles);
        issues.AddRange(result.Issues);
    }

    private static void RemoveKnownProcessedFileIfRequested(
        string sourceFilePath,
        bool shouldRemove,
        List<string> removedFiles,
        List<string> issues,
        bool allowMissingSourceFile = false)
    {
        if (!shouldRemove)
        {
            return;
        }

        if (!File.Exists(sourceFilePath))
        {
            if (allowMissingSourceFile)
            {
                return;
            }

            issues.Add($"Quelldatei fehlt: {sourceFilePath}");
            return;
        }

        try
        {
            File.Delete(sourceFilePath);
            removedFiles.Add(sourceFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            issues.Add($"Entfernen fehlgeschlagen für {sourceFilePath}: {ex.Message}");
        }
    }

    private static string CreateArchiveSuccessMessage(bool moveAllFiles, bool hasRemoveImportFolderOption)
    {
        if (moveAllFiles)
        {
            return "Importdateien wurden ins Archiv verschoben:";
        }

        return hasRemoveImportFolderOption
            ? "Importdateien wurden gemäß Profilregel archiviert; zu entfernende Importdateien wurden verschoben:"
            : "Importdateien wurden ins Archiv kopiert. Originale bleiben erhalten:";
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
