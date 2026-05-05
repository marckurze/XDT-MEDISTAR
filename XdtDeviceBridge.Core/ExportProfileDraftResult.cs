namespace XdtDeviceBridge.Core;

public sealed record ExportProfileDraftResult(
    ExportProfileDefinition? Profile,
    IReadOnlyList<string> Issues)
{
    public bool Success => Profile is not null && Issues.Count == 0;
}
