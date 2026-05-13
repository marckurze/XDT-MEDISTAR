using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record ExportRuleRemovalResult(
    bool Success,
    string Message,
    ExportProfileDefinition? UpdatedProfile,
    ExportRuleDefinition? RemovedRule,
    IReadOnlyList<string> Issues)
{
    public static ExportRuleRemovalResult Blocked(
        string message,
        ExportProfileDefinition? updatedProfile = null,
        ExportRuleDefinition? removedRule = null,
        IReadOnlyList<string>? issues = null)
    {
        return new ExportRuleRemovalResult(
            Success: false,
            Message: message,
            UpdatedProfile: updatedProfile,
            RemovedRule: removedRule,
            Issues: issues ?? Array.Empty<string>());
    }

    public static ExportRuleRemovalResult Removed(ExportProfileDefinition updatedProfile, ExportRuleDefinition removedRule)
    {
        return new ExportRuleRemovalResult(
            Success: true,
            Message: $"Exportregel entfernt: {removedRule.TargetName}",
            UpdatedProfile: updatedProfile,
            RemovedRule: removedRule,
            Issues: Array.Empty<string>());
    }
}
