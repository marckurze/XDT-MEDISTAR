using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class BuilderTestExportService
{
    private readonly MappingEngine _mappingEngine;
    private readonly XdtExportBuilder _xdtExportBuilder;
    private readonly IAttachmentExternalLinkPreparationService _attachmentPreparationService;
    private readonly FileExportService _fileExportService;

    public BuilderTestExportService()
        : this(
            new MappingEngine(),
            new XdtExportBuilder(),
            new AttachmentExternalLinkPreparationService(),
            new FileExportService())
    {
    }

    public BuilderTestExportService(
        MappingEngine mappingEngine,
        XdtExportBuilder xdtExportBuilder,
        IAttachmentExternalLinkPreparationService attachmentPreparationService,
        FileExportService fileExportService)
    {
        _mappingEngine = mappingEngine ?? throw new ArgumentNullException(nameof(mappingEngine));
        _xdtExportBuilder = xdtExportBuilder ?? throw new ArgumentNullException(nameof(xdtExportBuilder));
        _attachmentPreparationService = attachmentPreparationService ?? throw new ArgumentNullException(nameof(attachmentPreparationService));
        _fileExportService = fileExportService ?? throw new ArgumentNullException(nameof(fileExportService));
    }

    public BuilderTestExportPreviewResult BuildPreview(BuilderTestExportPreviewRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var issues = new List<string>();
        var patient = request.Patient ?? CreateEmptyPatientData();
        var measurements = request.Measurements ?? Array.Empty<MeasurementValue>();
        var mappingRules = request.ExportRules
            .Select(rule => CreatePreviewMappingRule(rule, patient, measurements))
            .ToList();

        var mappingResult = _mappingEngine.Map(patient, measurements, mappingRules);
        issues.AddRange(mappingResult.Issues.Select(issue =>
            $"{issue.Severity}: {issue.Message} SourcePath={issue.SourcePath}, TargetFieldCode={issue.TargetFieldCode}"));

        var exportRecords = AppendTransientAttachmentFields(mappingResult.Records, request.TransientAttachmentFields);
        var exportResult = _xdtExportBuilder.Build(exportRecords);
        issues.AddRange(exportResult.Issues.Select(issue =>
            $"{issue.Severity}: {issue.Message} FieldCode={issue.FieldCode}, Value={issue.Value}"));

        return new BuilderTestExportPreviewResult(
            Success: !mappingResult.HasErrors && !exportResult.HasErrors,
            ExportContent: exportResult.Content,
            ExportRecords: exportRecords,
            Issues: issues);
    }

    public BuilderTestExportResult Export(BuilderTestExportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var issues = new List<string>();
        if (string.IsNullOrWhiteSpace(request.TargetFolder))
        {
            issues.Add("Testexport-Zielordner darf nicht leer sein.");
        }

        if (string.IsNullOrWhiteSpace(request.ExportFileName))
        {
            issues.Add("Testexport-Dateiname darf nicht leer sein.");
        }

        if (issues.Count > 0)
        {
            return Fail(issues);
        }

        IReadOnlyList<ExportFieldRecord> attachmentFields = Array.Empty<ExportFieldRecord>();
        AttachmentExternalLinkPreparationResult? attachmentResult = null;
        if (!string.IsNullOrWhiteSpace(request.SourceAttachmentPath))
        {
            if (request.FolderOptions is null)
            {
                issues.Add("Schnittstellenprofil-Ordneroptionen für XDT-Anhang fehlen.");
                return Fail(issues);
            }

            Directory.CreateDirectory(request.TargetFolder);
            var testFolderOptions = request.FolderOptions with
            {
                AttachmentExportFolder = request.TargetFolder
            };

            attachmentResult = _attachmentPreparationService.Prepare(new AttachmentExternalLinkPreparationRequest(
                FolderOptions: testFolderOptions,
                SourceAttachmentPath: request.SourceAttachmentPath,
                Patient: request.Patient,
                ProcessingTimestamp: request.ProcessingTimestamp,
                OriginalExtension: Path.GetExtension(request.SourceAttachmentPath),
                IsSourceStable: request.IsSourceAttachmentStable));

            if (!attachmentResult.Success)
            {
                issues.Add(attachmentResult.ErrorMessage ?? "XDT-Anhang konnte für den Testexport nicht vorbereitet werden.");
                return Fail(issues);
            }

            attachmentFields = attachmentResult.ExportFields;
        }

        var preview = BuildPreview(new BuilderTestExportPreviewRequest(
            ExportProfileName: request.ExportProfileName,
            ExportRules: request.ExportRules,
            Patient: request.Patient,
            Measurements: request.Measurements,
            TransientAttachmentFields: attachmentFields));
        issues.AddRange(preview.Issues);

        if (string.IsNullOrWhiteSpace(preview.ExportContent))
        {
            issues.Add("Testexport-Inhalt ist leer.");
            return Fail(issues, preview.ExportContent, preview.ExportRecords, attachmentResult);
        }

        var exportResult = _fileExportService.Export(
            request.TargetFolder,
            request.ExportFileName,
            preview.ExportContent,
            request.OutputEncoding);
        issues.AddRange(exportResult.Issues.Select(issue => $"{issue.Severity}: {issue.Message}"));

        return new BuilderTestExportResult(
            Success: !preview.Issues.Any(issue => issue.StartsWith("Error", StringComparison.OrdinalIgnoreCase))
                && !exportResult.HasErrors,
            ExportFilePath: exportResult.FilePath,
            AttachmentTargetPath: attachmentResult?.TargetPath,
            AttachmentTargetFileName: attachmentResult?.TargetFileName,
            ExportContent: preview.ExportContent,
            ExportRecords: preview.ExportRecords,
            Issues: issues);
    }

    public static IReadOnlyList<ExportFieldRecord> AppendTransientAttachmentFields(
        IReadOnlyList<ExportFieldRecord> existingRecords,
        IReadOnlyList<ExportFieldRecord>? transientAttachmentFields)
    {
        var combined = existingRecords.ToList();
        if (transientAttachmentFields is null || transientAttachmentFields.Count == 0)
        {
            return combined;
        }

        var nextSortOrder = combined.Count == 0 ? 0 : combined.Max(record => record.SortOrder);
        foreach (var field in transientAttachmentFields.OrderBy(field => field.SortOrder))
        {
            if (string.IsNullOrWhiteSpace(field.Value))
            {
                continue;
            }

            combined.Add(new ExportFieldRecord(
                FieldCode: field.FieldCode,
                Value: field.Value,
                SortOrder: ++nextSortOrder));
        }

        return combined;
    }

    private static BuilderTestExportResult Fail(
        IReadOnlyList<string> issues,
        string exportContent = "",
        IReadOnlyList<ExportFieldRecord>? exportRecords = null,
        AttachmentExternalLinkPreparationResult? attachmentResult = null)
    {
        return new BuilderTestExportResult(
            Success: false,
            ExportFilePath: null,
            AttachmentTargetPath: attachmentResult?.TargetPath,
            AttachmentTargetFileName: attachmentResult?.TargetFileName,
            ExportContent: exportContent,
            ExportRecords: exportRecords ?? Array.Empty<ExportFieldRecord>(),
            Issues: issues);
    }

    private static MappingRule CreatePreviewMappingRule(
        ExportRuleDefinition rule,
        PatientData patient,
        IReadOnlyList<MeasurementValue> measurements)
    {
        return new MappingRule(
            Id: $"builder-preview-{rule.Id}",
            TargetFieldCode: string.IsNullOrWhiteSpace(rule.TargetFieldCode) ? "PREVIEW" : rule.TargetFieldCode,
            TargetName: rule.TargetName,
            SourcePath: GetPreviewSourcePath(rule, patient, measurements),
            OutputTemplate: rule.OutputTemplate,
            SortOrder: rule.SortOrder,
            IsEnabled: rule.IsEnabled);
    }

    private static string GetPreviewSourcePath(
        ExportRuleDefinition rule,
        PatientData patient,
        IReadOnlyList<MeasurementValue> measurements)
    {
        if (!string.IsNullOrWhiteSpace(rule.SourcePath))
        {
            return rule.SourcePath;
        }

        if (!string.IsNullOrWhiteSpace(patient.PatientNumber))
        {
            return "AIS.PatientNumber";
        }

        var firstMeasurementPath = measurements.FirstOrDefault(measurement =>
            !string.IsNullOrWhiteSpace(measurement.SourcePath))?.SourcePath;
        return firstMeasurementPath is null ? "AIS.PatientNumber" : $"Device.{firstMeasurementPath}";
    }

    private static PatientData CreateEmptyPatientData()
    {
        return new PatientData(
            PatientNumber: null,
            LastName: null,
            FirstName: null,
            BirthDate: null,
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: null,
            TargetSystem: null,
            GdtVersion: null,
            ExaminationType: null);
    }
}
