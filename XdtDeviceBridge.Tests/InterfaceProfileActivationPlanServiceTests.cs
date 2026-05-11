using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileActivationPlanServiceTests
{
    private readonly InterfaceProfileActivationPlanService _service = new();

    [Fact]
    public void CreatePlan_ShouldReturnNotAvailableWithoutProfile()
    {
        var plan = _service.CreatePlan(new InterfaceProfileActivationPlanRequest(
            Profile: null,
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, true),
            GuardResult: Guard(true, InterfaceProfileActivationGuardDecision.Allowed),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.NoWarnings)));

        Assert.Equal(InterfaceProfileActivationPlanStatus.NotAvailable, plan.PlanStatus);
        Assert.False(plan.CanExecuteLater);
        Assert.True(plan.IsPreviewOnly);
        Assert.Contains("Kein Schnittstellenprofil", plan.SummaryMessage);
        Assert.Contains(plan.Blockers, reason => reason.Code == "activationPlan.profile.missing");
    }

    [Fact]
    public void CreatePlan_ShouldReturnNotAvailableWithoutEvaluation()
    {
        var plan = _service.CreatePlan(new InterfaceProfileActivationPlanRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: null,
            GuardResult: Guard(false, InterfaceProfileActivationGuardDecision.Unknown),
            WarningConfirmationResult: null));

        Assert.Equal(InterfaceProfileActivationPlanStatus.NotAvailable, plan.PlanStatus);
        Assert.False(plan.CanExecuteLater);
        Assert.Contains("Keine aktuelle Aktivierungsbewertung", plan.SummaryMessage);
        Assert.Contains(plan.Blockers, reason => reason.Code == "activationPlan.evaluation.missing");
    }

    [Fact]
    public void CreatePlan_ShouldReturnBlockedForBlockedEvaluation()
    {
        var evaluation = Evaluation(
            InterfaceProfileActivationStatus.Blocked,
            false,
            Check(InterfaceProfileActivationSeverity.Blocker, "folder.aisImport.missing", "AIS-Importordner fehlt."));

        var plan = _service.CreatePlan(new InterfaceProfileActivationPlanRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: evaluation,
            GuardResult: Guard(false, InterfaceProfileActivationGuardDecision.Blocked,
                blockers: new[] { GuardReason(InterfaceProfileActivationSeverity.Blocker, "folder.aisImport.missing", "AIS-Importordner fehlt.") }),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.Blocked)));

        Assert.Equal(InterfaceProfileActivationPlanStatus.Blocked, plan.PlanStatus);
        Assert.False(plan.CanExecuteLater);
        Assert.Contains(plan.Blockers, reason => reason.Code == "folder.aisImport.missing");
        Assert.Contains(plan.PlannedSteps, step => step.Code == "resolve.blockers" && step.IsBlocked);
    }

    [Fact]
    public void CreatePlan_ShouldReturnBlockedForGuardBlocked()
    {
        var plan = _service.CreatePlan(new InterfaceProfileActivationPlanRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, true),
            GuardResult: Guard(false, InterfaceProfileActivationGuardDecision.Blocked,
                blockers: new[] { GuardReason(InterfaceProfileActivationSeverity.Blocker, "guard.profile.builtin", "BuiltIn-Schutz.") }),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.NoWarnings)));

        Assert.Equal(InterfaceProfileActivationPlanStatus.Blocked, plan.PlanStatus);
        Assert.False(plan.CanExecuteLater);
        Assert.Contains("Schutzprüfung", plan.SummaryMessage);
        Assert.Contains(plan.Blockers, reason => reason.Code == "guard.profile.builtin");
    }

    [Fact]
    public void CreatePlan_ShouldReturnReadyForReadyEvaluationWithoutWarnings()
    {
        var plan = _service.CreatePlan(new InterfaceProfileActivationPlanRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, true),
            GuardResult: Guard(true, InterfaceProfileActivationGuardDecision.Allowed),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.NoWarnings)));

        Assert.Equal(InterfaceProfileActivationPlanStatus.Ready, plan.PlanStatus);
        Assert.True(plan.CanExecuteLater);
        Assert.True(plan.IsPreviewOnly);
        Assert.Contains("technisch vorbereitet", plan.SummaryMessage);
        Assert.Contains(plan.PlannedSteps, step => step.Code == "activate.profile" && step.WouldExecuteLater && !step.IsBlocked);
        Assert.Contains(plan.PlannedSteps, step => step.Code == "save.profile" && step.WouldExecuteLater && !step.IsBlocked);
        Assert.Contains(plan.PlannedSteps, step => step.Code == "processing.not.started" && !step.WouldExecuteLater);
    }

    [Fact]
    public void CreatePlan_ShouldRequireWarningConfirmationForReadyWithWarnings()
    {
        var plan = _service.CreatePlan(new InterfaceProfileActivationPlanRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.ReadyWithWarnings, true,
                Check(InterfaceProfileActivationSeverity.Warning, "license.required", "Lizenzstatus prüfen.")),
            GuardResult: Guard(false, InterfaceProfileActivationGuardDecision.RequiresWarningConfirmation,
                warnings: new[] { GuardReason(InterfaceProfileActivationSeverity.Warning, "license.required", "Lizenzstatus prüfen.") }),
            WarningConfirmationResult: WarningConfirmation(
                InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired,
                warnings: new[] { WarningItem("license.required", "Lizenzstatus prüfen.") }),
            WarningsAccepted: false));

        Assert.Equal(InterfaceProfileActivationPlanStatus.RequiresWarningConfirmation, plan.PlanStatus);
        Assert.False(plan.CanExecuteLater);
        Assert.Contains(plan.Warnings, reason => reason.Code == "license.required");
        Assert.Contains(plan.PlannedSteps, step => step.Code == "warnings.confirm" && step.IsBlocked);
    }

    [Fact]
    public void CreatePlan_ShouldModelReadyWithAcceptedWarningsWithoutPersistingConfirmation()
    {
        var profile = CreateUserDefinedProfile();

        var plan = _service.CreatePlan(new InterfaceProfileActivationPlanRequest(
            Profile: profile,
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.ReadyWithWarnings, true,
                Check(InterfaceProfileActivationSeverity.Warning, "license.required", "Lizenzstatus prüfen.")),
            GuardResult: Guard(true, InterfaceProfileActivationGuardDecision.AllowedWithWarnings,
                warnings: new[] { GuardReason(InterfaceProfileActivationSeverity.Warning, "license.required", "Lizenzstatus prüfen.") }),
            WarningConfirmationResult: WarningConfirmation(
                InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired,
                warnings: new[] { WarningItem("license.required", "Lizenzstatus prüfen.") }),
            WarningsAccepted: true));

        Assert.Equal(InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings, plan.PlanStatus);
        Assert.True(plan.CanExecuteLater);
        Assert.True(plan.IsPreviewOnly);
        Assert.Contains(plan.Warnings, reason => reason.Code == "license.required");
        Assert.Contains(plan.PlannedSteps, step => step.Code == "warnings.accepted.hypothetical" && !step.WouldExecuteLater);
        Assert.False(profile.IsActive);
    }

    [Fact]
    public void CreatePlan_ShouldBlockBuiltInProfile()
    {
        var plan = _service.CreatePlan(new InterfaceProfileActivationPlanRequest(
            Profile: DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, true),
            GuardResult: Guard(false, InterfaceProfileActivationGuardDecision.Blocked,
                blockers: new[] { GuardReason(InterfaceProfileActivationSeverity.Blocker, "guard.profile.builtin", "BuiltIn-Schutz.") }),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.Blocked)));

        Assert.Equal(InterfaceProfileActivationPlanStatus.Blocked, plan.PlanStatus);
        Assert.False(plan.CanExecuteLater);
        Assert.Contains(plan.Blockers, reason => reason.Code == "activationPlan.profile.builtin");
        Assert.Contains("BuiltIn", plan.SummaryMessage);
    }

    [Fact]
    public void CreatePlan_ShouldBlockNonUserDefinedProfile()
    {
        var profile = CreateUserDefinedProfile() with
        {
            Metadata = CreateUserDefinedProfile().Metadata with
            {
                IsBuiltIn = false,
                IsUserDefined = false
            }
        };

        var plan = _service.CreatePlan(new InterfaceProfileActivationPlanRequest(
            Profile: profile,
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, true),
            GuardResult: Guard(false, InterfaceProfileActivationGuardDecision.Blocked,
                blockers: new[] { GuardReason(InterfaceProfileActivationSeverity.Blocker, "guard.profile.notUserDefined", "UserDefined-Schutz.") }),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.Blocked)));

        Assert.Equal(InterfaceProfileActivationPlanStatus.Blocked, plan.PlanStatus);
        Assert.False(plan.CanExecuteLater);
        Assert.Contains(plan.Blockers, reason => reason.Code == "activationPlan.profile.notUserDefined");
        Assert.Contains("UserDefined", plan.SummaryMessage);
    }

    [Fact]
    public void CreatePlan_ShouldReturnUnknownWithoutGuardDecision()
    {
        var plan = _service.CreatePlan(new InterfaceProfileActivationPlanRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, true),
            GuardResult: null,
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.NoWarnings)));

        Assert.Equal(InterfaceProfileActivationPlanStatus.Unknown, plan.PlanStatus);
        Assert.False(plan.CanExecuteLater);
        Assert.Contains(plan.Blockers, reason => reason.Code == "activationPlan.guard.missing");
    }

    [Fact]
    public void CreatePlan_ShouldNotMutateProfileOrActivateIt()
    {
        var profile = CreateUserDefinedProfile() with
        {
            IsActive = false,
            FolderOptions = CreateUserDefinedProfile().FolderOptions with
            {
                IsAttachmentProcessingEnabled = false
            }
        };
        var originalId = profile.Metadata.Id;
        var originalName = profile.Metadata.Name;
        var originalIsActive = profile.IsActive;
        var originalAttachmentEnabled = profile.FolderOptions.IsAttachmentProcessingEnabled;

        var plan = _service.CreatePlan(new InterfaceProfileActivationPlanRequest(
            Profile: profile,
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, true),
            GuardResult: Guard(true, InterfaceProfileActivationGuardDecision.Allowed),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.NoWarnings)));

        Assert.True(plan.CanExecuteLater);
        Assert.Equal(originalId, profile.Metadata.Id);
        Assert.Equal(originalName, profile.Metadata.Name);
        Assert.Equal(originalIsActive, profile.IsActive);
        Assert.Equal(originalAttachmentEnabled, profile.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.False(profile.IsActive);
        Assert.False(profile.FolderOptions.IsAttachmentProcessingEnabled);
    }

    [Fact]
    public void CreatePlan_ShouldKeepPlannedStepsDescriptiveOnly()
    {
        var plan = _service.CreatePlan(new InterfaceProfileActivationPlanRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, true),
            GuardResult: Guard(true, InterfaceProfileActivationGuardDecision.Allowed),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.NoWarnings)));

        Assert.NotEmpty(plan.PlannedSteps);
        Assert.All(plan.PlannedSteps, step => Assert.False(string.IsNullOrWhiteSpace(step.Title)));
        Assert.All(plan.PlannedSteps, step => Assert.False(string.IsNullOrWhiteSpace(step.Description)));
        Assert.True(plan.IsPreviewOnly);
        Assert.Contains(plan.PlannedSteps, step => step.Code == "processing.not.started");
    }

    private static InterfaceProfileDefinition CreateUserDefinedProfile()
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().Metadata with
            {
                Id = "interface-userdefined",
                Name = "UserDefined Schnittstelle",
                IsBuiltIn = false,
                IsUserDefined = true
            },
            IsActive = false
        };
    }

    private static InterfaceProfileActivationEvaluationResult Evaluation(
        InterfaceProfileActivationStatus status,
        bool canActivate,
        params InterfaceProfileActivationCheckResult[] checks)
    {
        return new InterfaceProfileActivationEvaluationResult(status, canActivate, checks);
    }

    private static InterfaceProfileActivationCheckResult Check(
        InterfaceProfileActivationSeverity severity,
        string code,
        string message)
    {
        return new InterfaceProfileActivationCheckResult(
            Area: "Test",
            Code: code,
            Message: message,
            Severity: severity,
            Detail: null);
    }

    private static InterfaceProfileActivationGuardResult Guard(
        bool canProceed,
        InterfaceProfileActivationGuardDecision decision,
        IReadOnlyList<InterfaceProfileActivationGuardReason>? blockers = null,
        IReadOnlyList<InterfaceProfileActivationGuardReason>? warnings = null,
        IReadOnlyList<InterfaceProfileActivationGuardReason>? infos = null)
    {
        return new InterfaceProfileActivationGuardResult(
            canProceed,
            decision,
            blockers ?? Array.Empty<InterfaceProfileActivationGuardReason>(),
            warnings ?? Array.Empty<InterfaceProfileActivationGuardReason>(),
            infos ?? Array.Empty<InterfaceProfileActivationGuardReason>(),
            "Guard-Testmeldung");
    }

    private static InterfaceProfileActivationGuardReason GuardReason(
        InterfaceProfileActivationSeverity severity,
        string code,
        string message)
    {
        return new InterfaceProfileActivationGuardReason(
            severity,
            code,
            message);
    }

    private static InterfaceProfileActivationWarningConfirmationResult WarningConfirmation(
        InterfaceProfileActivationWarningConfirmationStatus status,
        IReadOnlyList<InterfaceProfileActivationWarningConfirmationItem>? warnings = null)
    {
        return new InterfaceProfileActivationWarningConfirmationResult(
            CanRequestConfirmation: status == InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired,
            status,
            "interface-userdefined",
            "UserDefined Schnittstelle",
            warnings ?? Array.Empty<InterfaceProfileActivationWarningConfirmationItem>(),
            Array.Empty<InterfaceProfileActivationWarningConfirmationReason>(),
            "Warning-Confirmation-Testmeldung");
    }

    private static InterfaceProfileActivationWarningConfirmationItem WarningItem(
        string code,
        string title)
    {
        return new InterfaceProfileActivationWarningConfirmationItem(
            Area: "Test",
            Code: code,
            Title: title,
            Detail: null,
            Severity: InterfaceProfileActivationSeverity.Warning,
            IsRequiredForActivation: true);
    }
}
