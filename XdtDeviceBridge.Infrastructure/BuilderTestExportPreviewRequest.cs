using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record BuilderTestExportPreviewRequest(
    string ExportProfileName,
    IReadOnlyList<ExportRuleDefinition> ExportRules,
    PatientData? Patient,
    IReadOnlyList<MeasurementValue> Measurements,
    IReadOnlyList<ExportFieldRecord> TransientAttachmentFields);
