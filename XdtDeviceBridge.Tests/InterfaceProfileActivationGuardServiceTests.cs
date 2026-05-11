using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileActivationGuardServiceTests
{
    private readonly InterfaceProfileActivationGuardService _service = new();

    [Fact]
    public void ValidateActivationRequest_ShouldBlockMissingEvaluation()
    {
        var result = _service.ValidateActivationRequest(new InterfaceProfileActivationRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: null));

        Assert.False(result.CanProceed);
        Assert.Equal(InterfaceProfileActivationGuardDecision.Unknown, result.Decision);
        Assert.Contains(result.BlockerReasons, reason => reason.Code == "guard.evaluation.missing");
        Assert.Contains("keine eindeutige aktuelle Bewertung", result.Message);
    }

    [Fact]
    public void ValidateActivationRequest_ShouldBlockBlockedEvaluationAndCopyBlockers()
    {
        var evaluation = Evaluation(
            InterfaceProfileActivationStatus.Blocked,
            canActivate: false,
            Check(InterfaceProfileActivationSeverity.Blocker, "folder.aisImport.missing", "AIS-Importordner fehlt."));

        var result = _service.ValidateActivationRequest(new InterfaceProfileActivationRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: evaluation));

        Assert.False(result.CanProceed);
        Assert.Equal(InterfaceProfileActivationGuardDecision.Blocked, result.Decision);
        Assert.Contains(result.BlockerReasons, reason => reason.Code == "folder.aisImport.missing");
        Assert.Contains("blockierenden Punkte", result.Message);
    }

    [Fact]
    public void ValidateActivationRequest_ShouldAllowReadyEvaluation()
    {
        var evaluation = Evaluation(InterfaceProfileActivationStatus.Ready, canActivate: true);

        var result = _service.ValidateActivationRequest(new InterfaceProfileActivationRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: evaluation));

        Assert.True(result.CanProceed);
        Assert.Equal(InterfaceProfileActivationGuardDecision.Allowed, result.Decision);
        Assert.Empty(result.BlockerReasons);
    }

    [Fact]
    public void ValidateActivationRequest_ShouldRequireWarningConfirmationForReadyWithWarnings()
    {
        var evaluation = Evaluation(
            InterfaceProfileActivationStatus.ReadyWithWarnings,
            canActivate: true,
            Check(InterfaceProfileActivationSeverity.Warning, "license.required", "Lizenzstatus prüfen."));

        var result = _service.ValidateActivationRequest(new InterfaceProfileActivationRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: evaluation,
            WarningsAccepted: false));

        Assert.False(result.CanProceed);
        Assert.Equal(InterfaceProfileActivationGuardDecision.RequiresWarningConfirmation, result.Decision);
        Assert.Contains(result.WarningReasons, reason => reason.Code == "license.required");
        Assert.Contains("Warnungen", result.Message);
    }

    [Fact]
    public void ValidateActivationRequest_ShouldAllowReadyWithWarningsWhenWarningsAccepted()
    {
        var evaluation = Evaluation(
            InterfaceProfileActivationStatus.ReadyWithWarnings,
            canActivate: true,
            Check(InterfaceProfileActivationSeverity.Warning, "license.required", "Lizenzstatus prüfen."));

        var result = _service.ValidateActivationRequest(new InterfaceProfileActivationRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: evaluation,
            WarningsAccepted: true));

        Assert.True(result.CanProceed);
        Assert.Equal(InterfaceProfileActivationGuardDecision.AllowedWithWarnings, result.Decision);
        Assert.Contains(result.WarningReasons, reason => reason.Code == "license.required");
    }

    [Fact]
    public void ValidateActivationRequest_ShouldBlockBuiltInProfile()
    {
        var evaluation = Evaluation(InterfaceProfileActivationStatus.Ready, canActivate: true);

        var result = _service.ValidateActivationRequest(new InterfaceProfileActivationRequest(
            Profile: DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            EvaluationResult: evaluation));

        Assert.False(result.CanProceed);
        Assert.Equal(InterfaceProfileActivationGuardDecision.Blocked, result.Decision);
        Assert.Contains(result.BlockerReasons, reason => reason.Code == "guard.profile.builtin");
    }

    [Fact]
    public void ValidateActivationRequest_ShouldBlockNonUserDefinedProfile()
    {
        var profile = CreateUserDefinedProfile() with
        {
            Metadata = CreateUserDefinedProfile().Metadata with
            {
                IsBuiltIn = false,
                IsUserDefined = false
            }
        };
        var evaluation = Evaluation(InterfaceProfileActivationStatus.Ready, canActivate: true);

        var result = _service.ValidateActivationRequest(new InterfaceProfileActivationRequest(
            Profile: profile,
            EvaluationResult: evaluation));

        Assert.False(result.CanProceed);
        Assert.Equal(InterfaceProfileActivationGuardDecision.Blocked, result.Decision);
        Assert.Contains(result.BlockerReasons, reason => reason.Code == "guard.profile.notUserDefined");
    }

    [Fact]
    public void ValidateActivationRequest_ShouldNotMutateProfileOrActivateIt()
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
        var evaluation = Evaluation(InterfaceProfileActivationStatus.Ready, canActivate: true);

        var result = _service.ValidateActivationRequest(new InterfaceProfileActivationRequest(
            Profile: profile,
            EvaluationResult: evaluation));

        Assert.True(result.CanProceed);
        Assert.Equal(originalId, profile.Metadata.Id);
        Assert.Equal(originalName, profile.Metadata.Name);
        Assert.Equal(originalIsActive, profile.IsActive);
        Assert.Equal(originalAttachmentEnabled, profile.FolderOptions.IsAttachmentProcessingEnabled);
    }

    [Fact]
    public void ValidateActivationRequest_ShouldCopyEvaluationWarningsAndInfos()
    {
        var evaluation = Evaluation(
            InterfaceProfileActivationStatus.ReadyWithWarnings,
            canActivate: true,
            Check(InterfaceProfileActivationSeverity.Warning, "license.required", "Lizenzstatus prüfen."),
            Check(InterfaceProfileActivationSeverity.Info, "attachment.disabled", "Anhangverarbeitung deaktiviert."));

        var result = _service.ValidateActivationRequest(new InterfaceProfileActivationRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: evaluation,
            WarningsAccepted: true));

        Assert.Single(result.WarningReasons);
        Assert.Single(result.InfoReasons);
        Assert.Equal("license.required", result.WarningReasons[0].Code);
        Assert.Equal("attachment.disabled", result.InfoReasons[0].Code);
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
}
