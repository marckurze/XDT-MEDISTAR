namespace XdtDeviceBridge.Core;

public sealed record FieldRecord(
    string FieldCode,
    string Value,
    int LineNumber,
    int DeclaredLength,
    int ActualLength,
    bool IsLengthValid);
