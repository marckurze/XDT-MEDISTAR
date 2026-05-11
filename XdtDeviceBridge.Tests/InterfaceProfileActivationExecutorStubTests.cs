using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileActivationExecutorStubTests
{
    private readonly IInterfaceProfileActivationExecutor _executor = new InterfaceProfileActivationExecutorStub();

    [Fact]
    public async Task ExecuteAsync_ShouldReturnReadyButNotExecutedForReadyUserDefinedProfile()
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

        var result = await _executor.ExecuteAsync(CreateReadyRequest(profile));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted, result.Status);
        Assert.False(result.Success);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
        Assert.False(result.ProcessingStarted);
        Assert.Empty(result.ExecutedSteps);
        Assert.Contains(result.NotExecutedSteps, step => step.Code == "activate.profile");
        Assert.Contains("nicht-produktiv", result.Message);
        Assert.Contains(result.Preconditions, item => item.Code == "executor.safePersistence.missing" && !item.IsSatisfied);
        Assert.Equal(originalId, profile.Metadata.Id);
        Assert.Equal(originalName, profile.Metadata.Name);
        Assert.Equal(originalIsActive, profile.IsActive);
        Assert.Equal(originalAttachmentEnabled, profile.FolderOptions.IsAttachmentProcessingEnabled);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRequireWarningConfirmationForReadyWithWarningsWithoutConfirmation()
    {
        var result = await _executor.ExecuteAsync(CreateReadyWithWarningsRequest(
            CreateUserDefinedProfile(),
            warningsAccepted: false));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.RequiresWarningConfirmation, result.Status);
        Assert.False(result.Success);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
        Assert.False(result.ProcessingStarted);
        Assert.Contains(result.Preconditions, item => item.Code == "warnings.accepted" && !item.IsSatisfied);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnReadyButNotExecutedForAcceptedWarningsWithoutPersistingConfirmation()
    {
        var profile = CreateUserDefinedProfile() with { IsActive = false };

        var result = await _executor.ExecuteAsync(CreateReadyWithWarningsRequest(
            profile,
            warningsAccepted: true));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted, result.Status);
        Assert.False(result.Success);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
        Assert.False(result.ProcessingStarted);
        Assert.False(profile.IsActive);
        Assert.Contains(result.Preconditions, item => item.Code == "warnings.accepted" && item.IsSatisfied);
        Assert.Contains(result.Preconditions, item => item.Code == "executor.freshReload.missing" && !item.IsSatisfied);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldBlockBlockedEvaluation()
    {
        var profile = CreateUserDefinedProfile();
        var evaluation = Evaluation(
            InterfaceProfileActivationStatus.Blocked,
            canActivate: false,
            Check(InterfaceProfileActivationSeverity.Blocker, "folder.aisImport.missing", "AIS-Importordner fehlt."));

        var result = await _executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: profile,
            ActivationPlan: Plan(profile, InterfaceProfileActivationPlanStatus.Blocked),
            EvaluationResult: evaluation,
            GuardResult: Guard(false, InterfaceProfileActivationGuardDecision.Blocked),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.Blocked),
            WarningsAccepted: false));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.Blocked, result.Status);
        Assert.False(result.Success);
        Assert.False(profile.IsActive);
        Assert.False(result.Saved);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldBlockBuiltInProfile()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();

        var result = await _executor.ExecuteAsync(CreateReadyRequest(profile));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.Blocked, result.Status);
        Assert.False(result.Success);
        Assert.False(result.Saved);
        Assert.Contains(result.Preconditions, item => item.Code == "profile.notBuiltIn" && !item.IsSatisfied);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotAvailableWithoutEvaluation()
    {
        var profile = CreateUserDefinedProfile();

        var result = await _executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: profile,
            ActivationPlan: Plan(profile, InterfaceProfileActivationPlanStatus.Ready),
            EvaluationResult: null,
            GuardResult: Guard(true, InterfaceProfileActivationGuardDecision.Allowed),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.NoWarnings),
            WarningsAccepted: false));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.NotAvailable, result.Status);
        Assert.False(result.Success);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
        Assert.False(result.ProcessingStarted);
        Assert.Contains(result.Preconditions, item => item.Code == "evaluation.present" && !item.IsSatisfied);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNeverStartProcessingOrReportExecutedSteps()
    {
        var result = await _executor.ExecuteAsync(CreateReadyRequest(CreateUserDefinedProfile()));

        Assert.Empty(result.ExecutedSteps);
        Assert.False(result.ProcessingStarted);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
    }

    private static InterfaceProfileActivationExecutorRequest CreateReadyRequest(
        InterfaceProfileDefinition profile)
    {
        return new InterfaceProfileActivationExecutorRequest(
            Profile: profile,
            ActivationPlan: Plan(profile, InterfaceProfileActivationPlanStatus.Ready),
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, canActivate: true),
            GuardResult: Guard(true, InterfaceProfileActivationGuardDecision.Allowed),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.NoWarnings),
            WarningsAccepted: false);
    }

    private static InterfaceProfileActivationExecutorRequest CreateReadyWithWarningsRequest(
        InterfaceProfileDefinition profile,
        bool warningsAccepted)
    {
        return new InterfaceProfileActivationExecutorRequest(
            Profile: profile,
            ActivationPlan: Plan(
                profile,
                warningsAccepted
                    ? InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings
                    : InterfaceProfileActivationPlanStatus.RequiresWarningConfirmation),
            EvaluationResult: Evaluation(
                InterfaceProfileActivationStatus.ReadyWithWarnings,
                canActivate: true,
                Check(InterfaceProfileActivationSeverity.Warning, "license.required", "Lizenzstatus prüfen.")),
            GuardResult: Guard(
                warningsAccepted,
                warningsAccepted
                    ? InterfaceProfileActivationGuardDecision.AllowedWithWarnings
                    : InterfaceProfileActivationGuardDecision.RequiresWarningConfirmation),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired),
            WarningsAccepted: warningsAccepted);
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
        InterfaceProfileActivationGuardDecision decision)
    {
        return new InterfaceProfileActivationGuardResult(
            canProceed,
            decision,
            BlockerReasons: Array.Empty<InterfaceProfileActivationGuardReason>(),
            WarningReasons: Array.Empty<InterfaceProfileActivationGuardReason>(),
            InfoReasons: Array.Empty<InterfaceProfileActivationGuardReason>(),
            Message: decision.ToString());
    }

    private static InterfaceProfileActivationWarningConfirmationResult WarningConfirmation(
        InterfaceProfileActivationWarningConfirmationStatus status)
    {
        return new InterfaceProfileActivationWarningConfirmationResult(
            CanRequestConfirmation: status == InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired,
            status,
            ProfileId: "interface-userdefined",
            ProfileName: "UserDefined Schnittstelle",
            Warnings: status == InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired
                ? new[]
                {
                    new InterfaceProfileActivationWarningConfirmationItem(
                        Area: "Test",
                        Code: "license.required",
                        Title: "Lizenzstatus prüfen.",
                        Detail: null,
                        Severity: InterfaceProfileActivationSeverity.Warning,
                        IsRequiredForActivation: true)
                }
                : Array.Empty<InterfaceProfileActivationWarningConfirmationItem>(),
            BlockingReasons: Array.Empty<InterfaceProfileActivationWarningConfirmationReason>(),
            Message: status.ToString());
    }

    private static InterfaceProfileActivationPlan Plan(
        InterfaceProfileDefinition profile,
        InterfaceProfileActivationPlanStatus status)
    {
        var canExecute = status is InterfaceProfileActivationPlanStatus.Ready or
            InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings;

        return new InterfaceProfileActivationPlan(
            status,
            CanExecuteLater: canExecute,
            IsPreviewOnly: true,
            ProfileId: profile.Metadata.Id,
            ProfileName: profile.Metadata.Name,
            SummaryMessage: status.ToString(),
            Blockers: status == InterfaceProfileActivationPlanStatus.Blocked
                ? new[]
                {
                    new InterfaceProfileActivationPlanReason(
                        InterfaceProfileActivationSeverity.Blocker,
                        "plan.blocked",
                        "Plan blockiert.")
                }
                : Array.Empty<InterfaceProfileActivationPlanReason>(),
            Warnings: status is InterfaceProfileActivationPlanStatus.RequiresWarningConfirmation or
                InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings
                ? new[]
                {
                    new InterfaceProfileActivationPlanReason(
                        InterfaceProfileActivationSeverity.Warning,
                        "license.required",
                        "Lizenzstatus prüfen.")
                }
                : Array.Empty<InterfaceProfileActivationPlanReason>(),
            Infos: Array.Empty<InterfaceProfileActivationPlanReason>(),
            PlannedSteps: new[]
            {
                new InterfaceProfileActivationPlanStep(
                    Code: "activate.profile",
                    Title: "Profil aktivieren",
                    Description: "Beschreibender Schritt.",
                    WouldExecuteLater: true,
                    IsBlocked: !canExecute,
                    Severity: canExecute
                        ? InterfaceProfileActivationSeverity.Info
                        : InterfaceProfileActivationSeverity.Warning)
            },
            MissingRequirements: Array.Empty<InterfaceProfileActivationPlanReason>());
    }
}
