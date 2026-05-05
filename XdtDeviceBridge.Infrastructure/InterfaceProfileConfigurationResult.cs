using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileConfigurationResult(
    InterfaceProfileDefinition? Profile,
    IReadOnlyList<InterfaceProfileConfigurationIssue> Issues)
{
    public bool Success => Profile is not null && Issues.All(issue => issue.Severity != InterfaceProfileConfigurationIssueSeverity.Error);
}
