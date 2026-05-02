namespace XdtDeviceBridge.Core;

public sealed record ExportFieldRecord(
    string FieldCode,
    string? Value,
    int SortOrder);
