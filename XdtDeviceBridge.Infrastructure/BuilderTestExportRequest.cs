using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record BuilderTestExportRequest(
    string TargetFolder,
    string ExportFileName,
    string OutputEncoding,
    string ExportProfileName,
    IReadOnlyList<ExportRuleDefinition> ExportRules,
    PatientData? Patient,
    IReadOnlyList<MeasurementValue> Measurements,
    InterfaceFolderOptions? FolderOptions,
    string? SourceAttachmentPath,
    bool? IsSourceAttachmentStable,
    DateTime ProcessingTimestamp);
