using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileActivationPreparationPreviewServiceTests
{
    private readonly InterfaceProfileActivationPreparationPreviewService _service = new();

    [Fact]
    public void CreateEmpty_ShouldReturnNoProfileHint()
    {
        var preview = _service.CreateEmpty();

        Assert.Equal("-", preview.ProfileName);
        Assert.Equal("Nicht bewertet", preview.StatusText);
        Assert.Equal("Nein", preview.V1CanActivateText);
        Assert.Equal("Nicht eindeutig", preview.GuardDecisionText);
        Assert.Equal("Nein", preview.GuardCanProceedText);
        Assert.Contains("Bitte wählen", preview.SummaryMessage);
        Assert.Contains("nichts gespeichert", preview.SafetyNotice);
        Assert.Contains("nichts aktiviert", preview.SafetyNotice);
    }

    [Fact]
    public void Create_WithReadyAndAllowedGuard_ShouldShowV1Activatable()
    {
        var profile = CreateProfile();
        var result = Result(
            InterfaceProfileActivationStatus.Ready,
            canActivate: true,
            Check(InterfaceProfileActivationSeverity.Info, "AIS-Profil ist vorhanden.", "dependency.ais.ok"));
        var guardResult = Guard(
            canProceed: true,
            InterfaceProfileActivationGuardDecision.Allowed,
            message: "Schutzprüfung: Eine spätere V1-Aktivierung wäre technisch zulässig.");

        var preview = _service.Create(profile, result, guardResult);

        Assert.Equal("Ready", preview.StatusText);
        Assert.Equal("Ja", preview.V1CanActivateText);
        Assert.Equal("Technisch zulässig", preview.GuardDecisionText);
        Assert.Equal("Ja", preview.GuardCanProceedText);
        Assert.Contains("nach V1 grundsätzlich aktivierbar", preview.SummaryMessage);
        Assert.Contains("Aktivierbar nach V1: Ja", preview.MessageText);
        Assert.DoesNotContain("Warnungsbestätigung", preview.MessageText);
        Assert.DoesNotContain("Aktivierungsplan", preview.MessageText);
    }

    [Fact]
    public void Create_WithReadyWithoutGuard_ShouldStayNotV1Activatable()
    {
        var profile = CreateProfile();
        var result = Result(
            InterfaceProfileActivationStatus.Ready,
            canActivate: true,
            Check(InterfaceProfileActivationSeverity.Info, "AIS-Profil ist vorhanden.", "dependency.ais.ok"));

        var preview = _service.Create(profile, result);

        Assert.Equal("Ready", preview.StatusText);
        Assert.Equal("Nein", preview.V1CanActivateText);
        Assert.Contains("technische Voraussetzung fehlt", preview.SummaryMessage);
    }

    [Fact]
    public void Create_WithReadyWithWarnings_ShouldShowWarningsButNotV1Activatable()
    {
        var profile = CreateProfile();
        var result = Result(
            InterfaceProfileActivationStatus.ReadyWithWarnings,
            canActivate: true,
            Check(InterfaceProfileActivationSeverity.Warning, "Lizenzstatus sollte vor Aktivierung geprüft werden.", "license.required"),
            Check(InterfaceProfileActivationSeverity.Info, "XDT-Anhang-Automatik ist deaktiviert.", "attachment.disabled"));
        var guardResult = Guard(
            canProceed: false,
            InterfaceProfileActivationGuardDecision.Blocked,
            blockers: new[]
            {
                GuardReason(
                    InterfaceProfileActivationSeverity.Blocker,
                    "ReadyWithWarnings wird in V1 nicht produktiv aktiviert.",
                    "guard.evaluation.warningNotAllowedInV1")
            },
            warnings: new[]
            {
                GuardReason(
                    InterfaceProfileActivationSeverity.Warning,
                    "Lizenzstatus sollte vor Aktivierung geprüft werden.",
                    "license.required")
            });

        var preview = _service.Create(profile, result, guardResult);

        Assert.Equal("ReadyWithWarnings", preview.StatusText);
        Assert.Equal("Nein", preview.V1CanActivateText);
        Assert.Equal(1, preview.WarningCount);
        Assert.Contains(preview.ImportantWarnings, item => item.Contains("Lizenzstatus"));
        Assert.Contains("wird in V1 nicht aktiviert", preview.SummaryMessage);
        Assert.Contains("Aktivierbar nach V1: Nein", preview.MessageText);
        Assert.DoesNotContain("bestätigt", preview.MessageText);
    }

    [Fact]
    public void Create_WithBlockedEvaluation_ShouldShowBlockers()
    {
        var profile = CreateProfile();
        var result = Result(
            InterfaceProfileActivationStatus.Blocked,
            canActivate: false,
            Check(InterfaceProfileActivationSeverity.Blocker, "AIS-Importordner fehlt.", "folder.aisImport.missing"),
            Check(InterfaceProfileActivationSeverity.Warning, "Lizenzstatus sollte vor Aktivierung geprüft werden.", "license.required"));
        var guardResult = Guard(
            canProceed: false,
            InterfaceProfileActivationGuardDecision.Blocked,
            blockers: new[]
            {
                GuardReason(InterfaceProfileActivationSeverity.Blocker, "AIS-Importordner fehlt.", "folder.aisImport.missing")
            });

        var preview = _service.Create(profile, result, guardResult);

        Assert.Equal("Blocked", preview.StatusText);
        Assert.Equal("Nein", preview.V1CanActivateText);
        Assert.Equal(1, preview.BlockerCount);
        Assert.Contains(preview.ImportantBlockers, item => item.Contains("AIS-Importordner"));
        Assert.Contains("Dieses Profil kann nicht aktiviert werden.", preview.SummaryMessage);
    }

    [Fact]
    public void Create_WithBuiltInProfile_ShouldNotBeV1Activatable()
    {
        var profile = CreateProfile() with
        {
            Metadata = CreateProfile().Metadata with
            {
                IsBuiltIn = true,
                IsUserDefined = false
            }
        };
        var result = Result(InterfaceProfileActivationStatus.Ready, canActivate: true);
        var guardResult = Guard(canProceed: true, InterfaceProfileActivationGuardDecision.Allowed);

        var preview = _service.Create(profile, result, guardResult);

        Assert.Equal("Nein", preview.V1CanActivateText);
    }

    [Fact]
    public void Create_ShouldLimitImportantLists()
    {
        var profile = CreateProfile();
        var checks = Enumerable.Range(1, 8)
            .Select(index => Check(
                InterfaceProfileActivationSeverity.Warning,
                $"Warnung {index}",
                $"warning.{index}"))
            .ToArray();
        var result = Result(InterfaceProfileActivationStatus.ReadyWithWarnings, canActivate: true, checks);

        var preview = _service.Create(profile, result);

        Assert.Equal(8, preview.WarningCount);
        Assert.Equal(5, preview.ImportantWarnings.Count);
    }

    [Fact]
    public void Create_ShouldNotMutateProfileOrActivateIt()
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
        var originalIsActive = profile.IsActive;
        var originalAttachmentEnabled = profile.FolderOptions.IsAttachmentProcessingEnabled;
        var result = Result(InterfaceProfileActivationStatus.Ready, canActivate: true);
        var guardResult = Guard(canProceed: true, InterfaceProfileActivationGuardDecision.Allowed);

        _ = _service.Create(profile, result, guardResult);

        Assert.Equal(originalId, profile.Metadata.Id);
        Assert.Equal(originalName, profile.Metadata.Name);
        Assert.Equal(originalIsActive, profile.IsActive);
        Assert.Equal(originalAttachmentEnabled, profile.FolderOptions.IsAttachmentProcessingEnabled);
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

    private static InterfaceProfileActivationEvaluationResult Result(
        InterfaceProfileActivationStatus status,
        bool canActivate,
        params InterfaceProfileActivationCheckResult[] checks)
    {
        return new InterfaceProfileActivationEvaluationResult(status, canActivate, checks);
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
}
