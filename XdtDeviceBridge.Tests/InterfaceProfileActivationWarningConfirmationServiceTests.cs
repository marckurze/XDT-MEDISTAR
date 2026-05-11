using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileActivationWarningConfirmationServiceTests
{
    private readonly InterfaceProfileActivationWarningConfirmationService _service = new();

    [Fact]
    public void PrepareWarningConfirmation_ShouldRejectMissingEvaluation()
    {
        var result = _service.PrepareWarningConfirmation(new InterfaceProfileActivationWarningConfirmationRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: null));

        Assert.False(result.CanRequestConfirmation);
        Assert.Equal(InterfaceProfileActivationWarningConfirmationStatus.MissingEvaluation, result.Status);
        Assert.Contains("Keine aktuelle Aktivierungsbewertung", result.Message);
        Assert.Contains(result.BlockingReasons, reason => reason.Code == "warningConfirmation.evaluation.missing");
    }

    [Fact]
    public void PrepareWarningConfirmation_ShouldRejectMissingProfile()
    {
        var result = _service.PrepareWarningConfirmation(new InterfaceProfileActivationWarningConfirmationRequest(
            Profile: null,
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.ReadyWithWarnings, true,
                Check(InterfaceProfileActivationSeverity.Warning, "license.required", "Lizenzstatus prüfen."))));

        Assert.False(result.CanRequestConfirmation);
        Assert.Equal(InterfaceProfileActivationWarningConfirmationStatus.NotAvailable, result.Status);
        Assert.Contains("Kein Schnittstellenprofil", result.Message);
        Assert.Contains(result.BlockingReasons, reason => reason.Code == "warningConfirmation.profile.missing");
    }

    [Fact]
    public void PrepareWarningConfirmation_ShouldRejectBlockedEvaluationButKeepWarningsVisible()
    {
        var evaluation = Evaluation(
            InterfaceProfileActivationStatus.Blocked,
            false,
            Check(InterfaceProfileActivationSeverity.Blocker, "folder.aisImport.missing", "AIS-Importordner fehlt."),
            Check(InterfaceProfileActivationSeverity.Warning, "license.required", "Lizenzstatus prüfen."));

        var result = _service.PrepareWarningConfirmation(new InterfaceProfileActivationWarningConfirmationRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: evaluation));

        Assert.False(result.CanRequestConfirmation);
        Assert.Equal(InterfaceProfileActivationWarningConfirmationStatus.Blocked, result.Status);
        Assert.Single(result.Warnings);
        Assert.Contains(result.BlockingReasons, reason => reason.Code == "folder.aisImport.missing");
        Assert.Contains("keine Blocker mehr vorhanden", result.Message);
    }

    [Fact]
    public void PrepareWarningConfirmation_ShouldReturnNoWarningsForReadyEvaluation()
    {
        var result = _service.PrepareWarningConfirmation(new InterfaceProfileActivationWarningConfirmationRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, true,
                Check(InterfaceProfileActivationSeverity.Info, "dependency.ais.ok", "AIS-Profil ist vorhanden."))));

        Assert.False(result.CanRequestConfirmation);
        Assert.Equal(InterfaceProfileActivationWarningConfirmationStatus.NoWarnings, result.Status);
        Assert.Empty(result.Warnings);
        Assert.Contains("keine Warnungen", result.Message);
    }

    [Fact]
    public void PrepareWarningConfirmation_ShouldCreateConfirmationItemsForReadyWithWarnings()
    {
        var result = _service.PrepareWarningConfirmation(new InterfaceProfileActivationWarningConfirmationRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.ReadyWithWarnings, true,
                Check(InterfaceProfileActivationSeverity.Warning, "license.required", "Lizenzstatus prüfen.", "Lizenz vor Aktivierung prüfen."))));

        Assert.True(result.CanRequestConfirmation);
        Assert.Equal(InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired, result.Status);
        var item = Assert.Single(result.Warnings);
        Assert.Equal("license.required", item.Code);
        Assert.Equal("Lizenzstatus prüfen.", item.Title);
        Assert.Equal("Lizenz vor Aktivierung prüfen.", item.Detail);
        Assert.Equal(InterfaceProfileActivationSeverity.Warning, item.Severity);
        Assert.True(item.IsRequiredForActivation);
        Assert.Contains("bewusst bestätigt", result.Message);
    }

    [Theory]
    [InlineData(InterfaceProfileActivationStatus.Unknown)]
    [InlineData(InterfaceProfileActivationStatus.NotEvaluated)]
    public void PrepareWarningConfirmation_ShouldRejectUnknownOrNotEvaluatedStatus(
        InterfaceProfileActivationStatus status)
    {
        var result = _service.PrepareWarningConfirmation(new InterfaceProfileActivationWarningConfirmationRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: Evaluation(status, false)));

        Assert.False(result.CanRequestConfirmation);
        Assert.Equal(InterfaceProfileActivationWarningConfirmationStatus.Unknown, result.Status);
        Assert.Contains("nicht eindeutig", result.Message);
    }

    [Fact]
    public void PrepareWarningConfirmation_ShouldRejectBuiltInProfile()
    {
        var result = _service.PrepareWarningConfirmation(new InterfaceProfileActivationWarningConfirmationRequest(
            Profile: DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.ReadyWithWarnings, true,
                Check(InterfaceProfileActivationSeverity.Warning, "license.required", "Lizenzstatus prüfen."))));

        Assert.False(result.CanRequestConfirmation);
        Assert.Equal(InterfaceProfileActivationWarningConfirmationStatus.Blocked, result.Status);
        Assert.Contains(result.BlockingReasons, reason => reason.Code == "warningConfirmation.profile.builtin");
        Assert.Contains("BuiltIn", result.Message);
    }

    [Fact]
    public void PrepareWarningConfirmation_ShouldRejectNonUserDefinedProfile()
    {
        var profile = CreateUserDefinedProfile() with
        {
            Metadata = CreateUserDefinedProfile().Metadata with
            {
                IsBuiltIn = false,
                IsUserDefined = false
            }
        };

        var result = _service.PrepareWarningConfirmation(new InterfaceProfileActivationWarningConfirmationRequest(
            Profile: profile,
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.ReadyWithWarnings, true,
                Check(InterfaceProfileActivationSeverity.Warning, "license.required", "Lizenzstatus prüfen."))));

        Assert.False(result.CanRequestConfirmation);
        Assert.Equal(InterfaceProfileActivationWarningConfirmationStatus.Blocked, result.Status);
        Assert.Contains(result.BlockingReasons, reason => reason.Code == "warningConfirmation.profile.notUserDefined");
        Assert.Contains("UserDefined", result.Message);
    }

    [Fact]
    public void PrepareWarningConfirmation_ShouldNotCreateItemsForInfos()
    {
        var result = _service.PrepareWarningConfirmation(new InterfaceProfileActivationWarningConfirmationRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, true,
                Check(InterfaceProfileActivationSeverity.Info, "attachment.disabled", "Anhangverarbeitung deaktiviert."))));

        Assert.Empty(result.Warnings);
        Assert.DoesNotContain(result.Warnings, item => item.Severity == InterfaceProfileActivationSeverity.Info);
    }

    [Fact]
    public void PrepareWarningConfirmation_ShouldNotCreateItemsForBlockers()
    {
        var result = _service.PrepareWarningConfirmation(new InterfaceProfileActivationWarningConfirmationRequest(
            Profile: CreateUserDefinedProfile(),
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Blocked, false,
                Check(InterfaceProfileActivationSeverity.Blocker, "folder.aisImport.missing", "AIS-Importordner fehlt."))));

        Assert.Empty(result.Warnings);
        Assert.Contains(result.BlockingReasons, reason => reason.Code == "folder.aisImport.missing");
    }

    [Fact]
    public void PrepareWarningConfirmation_ShouldNotMutateProfileOrActivateIt()
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

        var result = _service.PrepareWarningConfirmation(new InterfaceProfileActivationWarningConfirmationRequest(
            Profile: profile,
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.ReadyWithWarnings, true,
                Check(InterfaceProfileActivationSeverity.Warning, "license.required", "Lizenzstatus prüfen."))));

        Assert.True(result.CanRequestConfirmation);
        Assert.Equal(originalId, profile.Metadata.Id);
        Assert.Equal(originalName, profile.Metadata.Name);
        Assert.Equal(originalIsActive, profile.IsActive);
        Assert.Equal(originalAttachmentEnabled, profile.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.False(profile.IsActive);
        Assert.False(profile.FolderOptions.IsAttachmentProcessingEnabled);
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
        string message,
        string? detail = null)
    {
        return new InterfaceProfileActivationCheckResult(
            Area: "Test",
            Code: code,
            Message: message,
            Severity: severity,
            Detail: detail);
    }
}
