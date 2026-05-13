using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record ExportProfileDeletionResult(
    bool Success,
    string Message,
    ExportProfileDefinition? Profile,
    IReadOnlyList<InterfaceProfileDefinition> ReferencingInterfaceProfiles)
{
    public static ExportProfileDeletionResult Blocked(
        string message,
        ExportProfileDefinition? profile = null,
        IReadOnlyList<InterfaceProfileDefinition>? referencingInterfaceProfiles = null)
    {
        return new ExportProfileDeletionResult(
            Success: false,
            Message: message,
            Profile: profile,
            ReferencingInterfaceProfiles: referencingInterfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>());
    }

    public static ExportProfileDeletionResult Deleted(ExportProfileDefinition profile)
    {
        return new ExportProfileDeletionResult(
            Success: true,
            Message: $"Exportprofil gelöscht: {profile.Metadata.Name}",
            Profile: profile,
            ReferencingInterfaceProfiles: Array.Empty<InterfaceProfileDefinition>());
    }
}
