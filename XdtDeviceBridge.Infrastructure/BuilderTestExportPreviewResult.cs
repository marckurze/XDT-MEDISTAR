using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record BuilderTestExportPreviewResult(
    bool Success,
    string ExportContent,
    IReadOnlyList<ExportFieldRecord> ExportRecords,
    IReadOnlyList<string> Issues)
{
    public bool HasAttachmentFields => ExportRecords.Any(record =>
        record.FieldCode is "6302" or "6303" or "6304" or "6305");
}
