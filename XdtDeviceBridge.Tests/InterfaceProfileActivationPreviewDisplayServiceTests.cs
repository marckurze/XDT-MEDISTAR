using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileActivationPreviewDisplayServiceTests
{
    private readonly InterfaceProfileActivationPreviewDisplayService _service = new();

    [Fact]
    public void CreateEmpty_ShouldShowNoSelectionHint()
    {
        var display = _service.CreateEmpty();

        Assert.Equal("Status: -", display.StatusText);
        Assert.Equal("Aktivierbar: -", display.CanActivateText);
        Assert.Contains("Bitte wählen", display.HintText);
        Assert.Empty(display.FolderChecks);
        Assert.Empty(display.AttachmentChecks);
        Assert.Empty(display.Rows);
    }

    [Fact]
    public void Create_ShouldCountBlockersWarningsAndInfos()
    {
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Blocked,
            CanActivate: false,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Info, "Profil", "Info"),
                Check(InterfaceProfileActivationSeverity.Blocker, "Abhaengigkeiten", "Blocker"),
                Check(InterfaceProfileActivationSeverity.Warning, "Lizenz", "Warnung")
            });

        var display = _service.Create(result);

        Assert.Equal("Status: Blockiert", display.StatusText);
        Assert.Equal("Aktivierbar: Nein", display.CanActivateText);
        Assert.Equal(1, display.BlockerCount);
        Assert.Equal(1, display.WarningCount);
        Assert.Equal(1, display.InfoCount);
        Assert.Equal("Blocker: 1 | Warnungen: 1 | Hinweise: 1", display.SummaryText);
    }

    [Fact]
    public void Create_ShouldOrderBlockersWarningsAndInfos()
    {
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.ReadyWithWarnings,
            CanActivate: true,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Info, "Profil", "Info"),
                Check(InterfaceProfileActivationSeverity.Warning, "Lizenz", "Warnung"),
                Check(InterfaceProfileActivationSeverity.Blocker, "Ordner", "Blocker")
            });

        var display = _service.Create(result);

        Assert.Collection(
            display.Rows,
            row => Assert.Equal("BLOCKER", row.Severity),
            row => Assert.Equal("WARNUNG", row.Severity),
            row => Assert.Equal("INFO", row.Severity));
    }

    [Fact]
    public void CreateError_ShouldExposeFriendlyErrorDisplay()
    {
        var display = _service.CreateError("Technischer Fehler.");

        Assert.Equal("Status: Fehler", display.StatusText);
        Assert.Equal("Aktivierbar: Nein", display.CanActivateText);
        Assert.Equal(1, display.BlockerCount);
        var row = Assert.Single(display.Rows);
        Assert.Equal("BLOCKER", row.Severity);
        Assert.Contains("Technischer Fehler", row.Detail);
    }

    [Fact]
    public void Create_WithProfile_ShouldBuildFolderDisplayInDomainOrder()
    {
        var profile = CreateProfile();
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Ready,
            CanActivate: true,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Info, "Ordner", "AIS OK", "folder.aisImport.ok"),
                Check(InterfaceProfileActivationSeverity.Info, "Ordner", "Device OK", "folder.deviceImport.ok"),
                Check(InterfaceProfileActivationSeverity.Info, "Ordner", "Export OK", "folder.export.ok")
            });

        var display = _service.Create(profile, result);

        Assert.Collection(
            display.FolderChecks,
            row => Assert.Equal("AIS-Importordner", row.Label),
            row => Assert.Equal("Geräte-Importordner", row.Label),
            row => Assert.Equal("AIS-Exportordner", row.Label),
            row => Assert.Equal("Archivordner", row.Label),
            row => Assert.Equal("Fehlerordner", row.Label),
            row => Assert.Equal("XDT-Anhang-Importordner", row.Label),
            row => Assert.Equal("XDT-Anhang-Exportordner", row.Label));
    }

    [Fact]
    public void Create_WithProfile_ShouldShowMissingAisImportFolderAsBlocker()
    {
        var profile = CreateProfile();
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Blocked,
            CanActivate: false,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Blocker, "Ordner", "AIS-Importordner fehlt.", "folder.aisImport.missing")
            });

        var display = _service.Create(profile, result);

        var row = Assert.Single(display.FolderChecks, item => item.Label == "AIS-Importordner");
        Assert.Equal("fehlt", row.Status);
        Assert.Equal("BLOCKER", row.Severity);
        Assert.Contains("fehlt", row.Message);
    }

    [Fact]
    public void Create_WithProfile_ShouldShowMissingRequiredAttachmentFolders()
    {
        var profile = CreateProfile(options => options with
        {
            AttachmentRequirementMode = AttachmentRequirementMode.Required,
            AttachmentImportFolder = "",
            AttachmentExportFolder = ""
        });
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Blocked,
            CanActivate: false,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Blocker, "Ordner", "XDT-Anhang Importordner fehlt.", "attachment.folder.import.missing"),
                Check(InterfaceProfileActivationSeverity.Blocker, "Ordner", "XDT-Anhang Exportordner fehlt.", "attachment.folder.export.missing")
            });

        var display = _service.Create(profile, result);

        Assert.Contains(display.FolderChecks, row =>
            row.Label == "XDT-Anhang-Importordner"
            && row.Status == "fehlt"
            && row.Severity == "BLOCKER");
        Assert.Contains(display.AttachmentChecks, row =>
            row.Label == "XDT-Anhang-Exportordner"
            && row.Status == "fehlt"
            && row.Severity == "BLOCKER");
    }

    [Fact]
    public void Create_WithAttachmentOnlyProfile_ShouldHideSeparateAttachmentImportFolder()
    {
        var profile = CreateProfile(options => options with
        {
            IsAttachmentOnlyMode = true,
            IsAttachmentProcessingEnabled = true,
            AttachmentRequirementMode = AttachmentRequirementMode.Required,
            AttachmentImportFolder = string.Empty
        });
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Ready,
            CanActivate: true,
            Checks: new[]
            {
                Check(
                    InterfaceProfileActivationSeverity.Info,
                    "XDT-Anhang",
                    "Dokumentgeräte verwenden den Geräte-/Dokument-Importordner.",
                    "attachment.folder.import.attachmentOnlyDeviceFolder")
            });

        var display = _service.Create(profile, result);

        Assert.Contains(display.FolderChecks, row => row.Label == "Dokument-Importordner");
        Assert.DoesNotContain(display.FolderChecks, row => row.Label == "XDT-Anhang-Importordner");
        Assert.Contains(display.AttachmentChecks, row =>
            row.Label == "Dokumenteingang"
            && row.Value == profile.FolderOptions.DeviceImportFolder);
    }

    [Fact]
    public void Create_WithFolderNotFoundWarning_ShouldShowReachabilityNoWithoutBlockerStatus()
    {
        var profile = CreateProfile();
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.ReadyWithWarnings,
            CanActivate: true,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Warning, "Ordner", "AIS-Importordner ist aktuell nicht erreichbar.", "folder.aisImport.notFound")
            });

        var display = _service.Create(profile, result);

        var row = Assert.Single(display.FolderChecks, item => item.Label == "AIS-Importordner");
        Assert.Equal("Warnung", row.Status);
        Assert.Equal("Nein", row.Reachability);
        Assert.Equal("WARNUNG", row.Severity);
    }

    [Fact]
    public void Create_WithProfile_ShouldShowOptionalDisabledAttachmentAsHint()
    {
        var profile = CreateProfile(options => options with
        {
            AttachmentRequirementMode = AttachmentRequirementMode.Optional,
            IsAttachmentProcessingEnabled = false
        });
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Ready,
            CanActivate: true,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Info, "XDT-Anhang", "XDT-Anhang-Automatik ist aktuell deaktiviert.", "attachment.disabled")
            });

        var display = _service.Create(profile, result);

        var row = Assert.Single(display.AttachmentChecks, item => item.Label == "Anhangverarbeitung");
        Assert.Equal("inaktiv", row.Value);
        Assert.Equal("Hinweis", row.Status);
        Assert.Equal("INFO", row.Severity);
        Assert.Equal(0, display.BlockerCount);
    }

    [Fact]
    public void Create_WithProfile_ShouldIncludeExternalLinkFields()
    {
        var profile = CreateProfile();
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Ready,
            CanActivate: true,
            Checks: Array.Empty<InterfaceProfileActivationCheckResult>());

        var display = _service.Create(profile, result);

        Assert.Contains(display.AttachmentChecks, row => row.Label == "6302 Dokumentenname");
        Assert.Contains(display.AttachmentChecks, row => row.Label == "6303 Dateiformat");
        Assert.Contains(display.AttachmentChecks, row => row.Label == "6304 Beschreibung");
        Assert.Contains(display.AttachmentChecks, row => row.Label == "6305 vollständiger Dateipfad");
    }

    private static InterfaceProfileActivationCheckResult Check(
        InterfaceProfileActivationSeverity severity,
        string area,
        string message,
        string? code = null)
    {
        return new InterfaceProfileActivationCheckResult(
            Area: area,
            Code: code ?? message,
            Message: message,
            Severity: severity,
            Detail: $"{message} Detail");
    }

    private static InterfaceProfileDefinition CreateProfile(
        Func<InterfaceFolderOptions, InterfaceFolderOptions>? configureOptions = null)
    {
        var folderOptions = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().FolderOptions with
        {
            AisImportFolder = @"C:\XdtBridge\AIS",
            DeviceImportFolder = @"C:\XdtBridge\Device",
            ExportFolder = @"C:\XdtBridge\Export",
            ArchiveFolder = @"C:\XdtBridge\Archive",
            ErrorFolder = @"C:\XdtBridge\Error",
            AttachmentImportFolder = @"C:\XdtBridge\AttachmentIn",
            AttachmentExportFolder = @"C:\XdtBridge\AttachmentOut",
            AttachmentExternalLinkDocumentName = "PDF-Befund",
            AttachmentExternalLinkFileFormat = "{ExtensionUpperWithoutDot}",
            AttachmentExternalLinkDescription = "Messprotokoll",
            AttachmentExternalLinkPathTemplate = "{Attachment.TargetFullPath}"
        };

        if (configureOptions is not null)
        {
            folderOptions = configureOptions(folderOptions);
        }

        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            FolderOptions = folderOptions
        };
    }
}
