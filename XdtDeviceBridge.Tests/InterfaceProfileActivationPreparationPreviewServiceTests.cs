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
        Assert.Contains("Bitte wählen", preview.SummaryMessage);
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
}
