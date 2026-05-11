namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationPlan(
    InterfaceProfileActivationPlanStatus PlanStatus,
    bool CanExecuteLater,
    bool IsPreviewOnly,
    string ProfileId,
    string ProfileName,
    string SummaryMessage,
    IReadOnlyList<InterfaceProfileActivationPlanReason> Blockers,
    IReadOnlyList<InterfaceProfileActivationPlanReason> Warnings,
    IReadOnlyList<InterfaceProfileActivationPlanReason> Infos,
    IReadOnlyList<InterfaceProfileActivationPlanStep> PlannedSteps,
    IReadOnlyList<InterfaceProfileActivationPlanReason> MissingRequirements);
