using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileActivationPreparationPreviewServiceTests
{
    private readonly InterfaceProfileActivationPreparationPreviewService _service = new();

    [Fact]
    public void Create_ShouldSummarizeBlockedProfile()
    {
        var profile = CreateProfile();
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Blocked,
            CanActivate: false,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Blocker, "AIS-Importordner fehlt.", "folder.aisImport.missing"),
                Check(InterfaceProfileActivationSeverity.Blocker, "XDT-Anhang Exportordner fehlt.", "attachment.folder.export.missing"),
                Check(InterfaceProfileActivationSeverity.Warning, "Lizenzstatus sollte vor Aktivierung geprüft werden.", "license.required")
            });

        var preview = _service.Create(profile, result);

        Assert.Equal("Blockiert", preview.StatusText);
        Assert.Equal("Nein", preview.CanActivateText);
        Assert.Equal(2, preview.BlockerCount);
        Assert.Contains("kann aktuell nicht aktiviert werden", preview.SummaryMessage);
        Assert.Contains(preview.ImportantBlockers, item => item.Contains("AIS-Importordner"));
        Assert.Contains("keine Änderungen gespeichert", preview.MessageText);
    }

    [Fact]
    public void Create_ShouldSummarizeReadyWithWarningsProfile()
    {
        var profile = CreateProfile();
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.ReadyWithWarnings,
            CanActivate: true,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Warning, "Lizenzstatus sollte vor Aktivierung geprüft werden.", "license.required"),
                Check(InterfaceProfileActivationSeverity.Info, "XDT-Anhang-Automatik ist deaktiviert.", "attachment.disabled")
            });

        var preview = _service.Create(profile, result);

        Assert.Equal("Aktivierbar mit Warnungen", preview.StatusText);
        Assert.Equal("Ja", preview.CanActivateText);
        Assert.Equal(0, preview.BlockerCount);
        Assert.Equal(1, preview.WarningCount);
        Assert.Contains("Warnungen", preview.SummaryMessage);
        Assert.Contains(preview.ImportantWarnings, item => item.Contains("Lizenzstatus"));
    }

    [Fact]
    public void Create_ShouldSummarizeReadyProfile()
    {
        var profile = CreateProfile();
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Ready,
            CanActivate: true,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Info, "AIS-Profil ist vorhanden.", "dependency.ais.ok")
            });

        var preview = _service.Create(profile, result);

        Assert.Equal("Aktivierbar", preview.StatusText);
        Assert.Equal("Ja", preview.CanActivateText);
        Assert.Empty(preview.ImportantBlockers);
        Assert.Empty(preview.ImportantWarnings);
        Assert.Contains("grundsätzlich aktivierbar", preview.SummaryMessage);
        Assert.Contains("noch nichts aktiviert", preview.MessageText);
    }

    [Fact]
    public void CreateEmpty_ShouldReturnNoProfileHint()
    {
        var preview = _service.CreateEmpty();

        Assert.Equal("-", preview.ProfileName);
        Assert.Equal("Nicht bewertet", preview.StatusText);
        Assert.Equal("Nein", preview.CanActivateText);
        Assert.Equal("Nicht eindeutig", preview.GuardDecisionText);
        Assert.Equal("Nein", preview.GuardCanProceedText);
        Assert.Equal("nicht verfügbar", preview.WarningConfirmationStatusText);
        Assert.Equal(0, preview.WarningConfirmationItemCount);
        Assert.Contains("Bitte wählen", preview.SummaryMessage);
    }

    [Fact]
    public void Create_WithBlockedGuard_ShouldShowTechnicalProtectionDecision()
    {
        var profile = CreateProfile();
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Blocked,
            CanActivate: false,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Blocker, "AIS-Importordner fehlt.", "folder.aisImport.missing")
            });
        var guardResult = Guard(
            canProceed: false,
            InterfaceProfileActivationGuardDecision.Blocked,
            blockers: new[]
            {
                GuardReason(InterfaceProfileActivationSeverity.Blocker, "AIS-Importordner fehlt.", "folder.aisImport.missing")
            },
            message: "Schutzprüfung: Aktivierung blockiert. Bitte beheben Sie zuerst die blockierenden Punkte.");

        var preview = _service.Create(profile, result, guardResult);

        Assert.Equal("Blockiert", preview.GuardDecisionText);
        Assert.Equal("Nein", preview.GuardCanProceedText);
        Assert.Contains(preview.GuardReasons, item => item.Contains("AIS-Importordner"));
        Assert.Contains("Technische Schutzprüfung", preview.MessageText);
        Assert.Contains("Entscheidung: Blockiert", preview.MessageText);
    }

    [Fact]
    public void Create_WithReadyGuard_ShouldShowTechnicalAllowedDecision()
    {
        var profile = CreateProfile();
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Ready,
            CanActivate: true,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Info, "AIS-Profil ist vorhanden.", "dependency.ais.ok")
            });
        var guardResult = Guard(
            canProceed: true,
            InterfaceProfileActivationGuardDecision.Allowed,
            message: "Schutzprüfung: Eine spätere Aktivierung wäre technisch zulässig.");

        var preview = _service.Create(profile, result, guardResult);

        Assert.Equal("Technisch zulässig", preview.GuardDecisionText);
        Assert.Equal("Ja", preview.GuardCanProceedText);
        Assert.Contains("Technisch freigegeben: Ja", preview.MessageText);
        Assert.Contains("keine Änderungen gespeichert", preview.MessageText);
        Assert.Contains("nichts aktiviert", preview.MessageText);
    }

    [Fact]
    public void Create_WithReadyWithWarningsGuard_ShouldRequireWarningConfirmationReadOnly()
    {
        var profile = CreateProfile();
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.ReadyWithWarnings,
            CanActivate: true,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Warning, "Lizenzstatus sollte vor Aktivierung geprüft werden.", "license.required")
            });
        var guardResult = Guard(
            canProceed: false,
            InterfaceProfileActivationGuardDecision.RequiresWarningConfirmation,
            warnings: new[]
            {
                GuardReason(InterfaceProfileActivationSeverity.Warning, "Lizenzstatus sollte vor Aktivierung geprüft werden.", "license.required")
            },
            message: "Schutzprüfung: Warnungen müssten vor einer späteren Aktivierung bewusst bestätigt werden.");

        var preview = _service.Create(profile, result, guardResult);

        Assert.Equal("Warnungsbestätigung erforderlich", preview.GuardDecisionText);
        Assert.Equal("Nein", preview.GuardCanProceedText);
        Assert.Contains(preview.GuardReasons, item => item.Contains("Lizenzstatus"));
        Assert.Contains("Warnungsbestätigung erforderlich", preview.MessageText);
        Assert.Contains("Technisch freigegeben: Nein", preview.MessageText);
        Assert.Contains("keine Änderungen gespeichert", preview.MessageText);
    }

    [Fact]
    public void Create_WithReadyWithWarnings_ShouldShowWarningConfirmationPreview()
    {
        var profile = CreateProfile();
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.ReadyWithWarnings,
            CanActivate: true,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Warning, "Lizenzstatus sollte vor Aktivierung geprüft werden.", "license.required"),
                Check(InterfaceProfileActivationSeverity.Info, "XDT-Anhang-Automatik ist deaktiviert.", "attachment.disabled")
            });
        var guardResult = Guard(
            canProceed: false,
            InterfaceProfileActivationGuardDecision.RequiresWarningConfirmation,
            warnings: new[]
            {
                GuardReason(InterfaceProfileActivationSeverity.Warning, "Lizenzstatus sollte vor Aktivierung geprüft werden.", "license.required")
            });
        var warningConfirmation = WarningConfirmation(
            canRequestConfirmation: true,
            InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired,
            warnings: new[]
            {
                WarningItem("Lizenzstatus sollte vor Aktivierung geprüft werden.", "license.required")
            },
            message: "Vor einer späteren Aktivierung müssen diese Warnungen bewusst bestätigt werden.");

        var preview = _service.Create(profile, result, guardResult, warningConfirmation);

        Assert.Equal("erforderlich", preview.WarningConfirmationStatusText);
        Assert.Equal(1, preview.WarningConfirmationItemCount);
        Assert.Contains(preview.WarningConfirmationItems, item => item.Contains("Lizenzstatus"));
        Assert.Contains("Warnungsbestätigung:", preview.MessageText);
        Assert.Contains("Folgende Warnungen", preview.MessageText);
        Assert.Contains("Diese Warnungen wurden in diesem Schritt nicht bestätigt.", preview.MessageText);
        Assert.DoesNotContain(preview.WarningConfirmationItems, item => item.Contains("XDT-Anhang-Automatik"));
    }

    [Fact]
    public void Create_WithReadyWithoutWarnings_ShouldShowWarningConfirmationNotRequired()
    {
        var profile = CreateProfile();
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Ready,
            CanActivate: true,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Info, "AIS-Profil ist vorhanden.", "dependency.ais.ok")
            });
        var guardResult = Guard(
            canProceed: true,
            InterfaceProfileActivationGuardDecision.Allowed);
        var warningConfirmation = WarningConfirmation(
            canRequestConfirmation: false,
            InterfaceProfileActivationWarningConfirmationStatus.NoWarnings,
            message: "Es sind keine Warnungen vorhanden, die bestätigt werden müssen.");

        var preview = _service.Create(profile, result, guardResult, warningConfirmation);

        Assert.Equal("nicht erforderlich", preview.WarningConfirmationStatusText);
        Assert.Equal(0, preview.WarningConfirmationItemCount);
        Assert.Empty(preview.WarningConfirmationItems);
        Assert.Contains("Es sind keine Warnungen vorhanden", preview.MessageText);
        Assert.DoesNotContain("Folgende Warnungen", preview.MessageText);
    }

    [Fact]
    public void Create_WithBlockedEvaluation_ShouldShowWarningConfirmationNotPossible()
    {
        var profile = CreateProfile();
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Blocked,
            CanActivate: false,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Blocker, "AIS-Importordner fehlt.", "folder.aisImport.missing"),
                Check(InterfaceProfileActivationSeverity.Warning, "Lizenzstatus sollte vor Aktivierung geprüft werden.", "license.required")
            });
        var guardResult = Guard(
            canProceed: false,
            InterfaceProfileActivationGuardDecision.Blocked,
            blockers: new[]
            {
                GuardReason(InterfaceProfileActivationSeverity.Blocker, "AIS-Importordner fehlt.", "folder.aisImport.missing")
            });
        var warningConfirmation = WarningConfirmation(
            canRequestConfirmation: false,
            InterfaceProfileActivationWarningConfirmationStatus.Blocked,
            warnings: new[]
            {
                WarningItem("Lizenzstatus sollte vor Aktivierung geprüft werden.", "license.required")
            },
            message: "Warnungen können erst bestätigt werden, wenn keine Blocker mehr vorhanden sind.");

        var preview = _service.Create(profile, result, guardResult, warningConfirmation);

        Assert.Equal("nicht möglich", preview.WarningConfirmationStatusText);
        Assert.Equal(1, preview.WarningConfirmationItemCount);
        Assert.Contains("keine Blocker mehr vorhanden", preview.WarningConfirmationMessage);
        Assert.Contains("Vorhandene Warnungen", preview.MessageText);
        Assert.Contains("nicht bestätigt", preview.MessageText);
    }

    [Fact]
    public void Create_WithUnknownEvaluation_ShouldShowWarningConfirmationUnavailable()
    {
        var profile = CreateProfile();
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Unknown,
            CanActivate: false,
            Checks: Array.Empty<InterfaceProfileActivationCheckResult>());
        var guardResult = Guard(
            canProceed: false,
            InterfaceProfileActivationGuardDecision.Unknown);
        var warningConfirmation = WarningConfirmation(
            canRequestConfirmation: false,
            InterfaceProfileActivationWarningConfirmationStatus.Unknown,
            message: "Die Aktivierungsbewertung ist nicht eindeutig.");

        var preview = _service.Create(profile, result, guardResult, warningConfirmation);

        Assert.Equal("nicht verfügbar", preview.WarningConfirmationStatusText);
        Assert.Equal(0, preview.WarningConfirmationItemCount);
        Assert.Contains("nicht eindeutig", preview.WarningConfirmationMessage);
        Assert.Contains("Warnungsbestätigung:", preview.MessageText);
    }

    [Fact]
    public void Create_ShouldNotMutateProfile()
    {
        var profile = CreateProfile() with
        {
            IsActive = true,
            FolderOptions = CreateProfile().FolderOptions with
            {
                IsAttachmentProcessingEnabled = true
            }
        };
        var originalId = profile.Metadata.Id;
        var originalName = profile.Metadata.Name;
        var originalIsActive = profile.IsActive;
        var originalAttachmentEnabled = profile.FolderOptions.IsAttachmentProcessingEnabled;
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.ReadyWithWarnings,
            CanActivate: true,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Warning, "Profil ist bereits aktiv.", "profile.active")
            });

        _ = _service.Create(profile, result);

        Assert.Equal(originalId, profile.Metadata.Id);
        Assert.Equal(originalName, profile.Metadata.Name);
        Assert.Equal(originalIsActive, profile.IsActive);
        Assert.Equal(originalAttachmentEnabled, profile.FolderOptions.IsAttachmentProcessingEnabled);
    }

    [Fact]
    public void Create_WithGuard_ShouldNotMutateProfileOrActivateIt()
    {
        var profile = CreateProfile() with
        {
            IsActive = false,
            FolderOptions = CreateProfile().FolderOptions with
            {
                IsAttachmentProcessingEnabled = false
            }
        };
        var originalId = profile.Metadata.Id;
        var originalName = profile.Metadata.Name;
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Ready,
            CanActivate: true,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Info, "Profil wirkt aktivierbar.", "profile.ready")
            });
        var guardResult = Guard(
            canProceed: true,
            InterfaceProfileActivationGuardDecision.Allowed,
            message: "Schutzprüfung: Eine spätere Aktivierung wäre technisch zulässig.");

        _ = _service.Create(profile, result, guardResult);

        Assert.Equal(originalId, profile.Metadata.Id);
        Assert.Equal(originalName, profile.Metadata.Name);
        Assert.False(profile.IsActive);
        Assert.False(profile.FolderOptions.IsAttachmentProcessingEnabled);
    }

    [Fact]
    public void Create_WithWarningConfirmationPreview_ShouldNotMutateProfileOrConfirmWarnings()
    {
        var profile = CreateProfile() with
        {
            IsActive = false,
            FolderOptions = CreateProfile().FolderOptions with
            {
                IsAttachmentProcessingEnabled = false
            }
        };
        var originalId = profile.Metadata.Id;
        var originalName = profile.Metadata.Name;
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.ReadyWithWarnings,
            CanActivate: true,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Warning, "Lizenzstatus sollte vor Aktivierung geprüft werden.", "license.required")
            });
        var guardResult = Guard(
            canProceed: false,
            InterfaceProfileActivationGuardDecision.RequiresWarningConfirmation);
        var warningConfirmation = WarningConfirmation(
            canRequestConfirmation: true,
            InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired,
            warnings: new[]
            {
                WarningItem("Lizenzstatus sollte vor Aktivierung geprüft werden.", "license.required")
            });

        var preview = _service.Create(profile, result, guardResult, warningConfirmation);

        Assert.Equal(originalId, profile.Metadata.Id);
        Assert.Equal(originalName, profile.Metadata.Name);
        Assert.False(profile.IsActive);
        Assert.False(profile.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Contains("keine Warnungen bestätigt", preview.SafetyNotice);
        Assert.Contains("keine Warnungen bestätigt", preview.MessageText);
    }

    private static InterfaceProfileDefinition CreateProfile()
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().Metadata with
            {
                Id = "interface-userdefined",
                Name = "Importierte Schnittstelle",
                IsBuiltIn = false,
                IsUserDefined = true
            },
            IsActive = false
        };
    }

    private static InterfaceProfileActivationCheckResult Check(
        InterfaceProfileActivationSeverity severity,
        string message,
        string code)
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
        IReadOnlyList<InterfaceProfileActivationGuardReason>? infos = null,
        string message = "Schutzprüfung.")
    {
        return new InterfaceProfileActivationGuardResult(
            canProceed,
            decision,
            blockers ?? Array.Empty<InterfaceProfileActivationGuardReason>(),
            warnings ?? Array.Empty<InterfaceProfileActivationGuardReason>(),
            infos ?? Array.Empty<InterfaceProfileActivationGuardReason>(),
            message);
    }

    private static InterfaceProfileActivationGuardReason GuardReason(
        InterfaceProfileActivationSeverity severity,
        string message,
        string code)
    {
        return new InterfaceProfileActivationGuardReason(
            severity,
            code,
            message);
    }

    private static InterfaceProfileActivationWarningConfirmationResult WarningConfirmation(
        bool canRequestConfirmation,
        InterfaceProfileActivationWarningConfirmationStatus status,
        IReadOnlyList<InterfaceProfileActivationWarningConfirmationItem>? warnings = null,
        IReadOnlyList<InterfaceProfileActivationWarningConfirmationReason>? blockers = null,
        string message = "Vor einer späteren Aktivierung müssen diese Warnungen bewusst bestätigt werden.")
    {
        return new InterfaceProfileActivationWarningConfirmationResult(
            canRequestConfirmation,
            status,
            "interface-userdefined",
            "Importierte Schnittstelle",
            warnings ?? Array.Empty<InterfaceProfileActivationWarningConfirmationItem>(),
            blockers ?? Array.Empty<InterfaceProfileActivationWarningConfirmationReason>(),
            message);
    }

    private static InterfaceProfileActivationWarningConfirmationItem WarningItem(
        string title,
        string code)
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
