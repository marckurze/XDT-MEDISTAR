using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileScanIntervalUpdateResult(
    InterfaceProfileDefinition? Profile,
    int PreviousIntervalSeconds,
    int RequestedIntervalSeconds,
    int EffectiveIntervalSeconds,
    bool Changed,
    bool ReachedMinimum,
    bool ReachedMaximum,
    bool CreatedUserDefinedCopy,
    string Message,
    IReadOnlyList<InterfaceProfileConfigurationIssue> Issues)
{
    public bool Success => Profile is not null && Issues.All(issue => issue.Severity != InterfaceProfileConfigurationIssueSeverity.Error);
}
