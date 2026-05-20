using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileActivationEvaluationService
{
    private const string ProfileArea = "Profil";
    private const string DependencyArea = "Abhaengigkeiten";
    private const string FolderArea = "Ordner";
    private const string AttachmentArea = "XDT-Anhang";
    private const string LicenseArea = "Lizenz";

    private readonly FolderSafetyValidator _folderSafetyValidator;

    public InterfaceProfileActivationEvaluationService()
        : this(new FolderSafetyValidator())
    {
    }

    public InterfaceProfileActivationEvaluationService(FolderSafetyValidator folderSafetyValidator)
    {
        _folderSafetyValidator = folderSafetyValidator;
    }

    public InterfaceProfileActivationEvaluationResult Evaluate(
        InterfaceProfileDefinition? profile,
        ProfileCatalog catalog,
        IReadOnlyList<LicensedDeviceState>? licenseStates = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var checks = new List<InterfaceProfileActivationCheckResult>();
        if (profile is null)
        {
            AddBlocker(checks, ProfileArea, "profile.missing", "Schnittstellenprofil fehlt.");
            return CreateResult(checks);
        }

        EvaluateProfile(profile, checks);
        EvaluateDependencies(profile, catalog, checks);
        EvaluateFolders(profile.FolderOptions, checks);
        EvaluateAttachmentConfiguration(profile.FolderOptions, checks);
        EvaluateLicense(profile, licenseStates ?? Array.Empty<LicensedDeviceState>(), checks);

        return CreateResult(checks);
    }

    private static InterfaceProfileActivationEvaluationResult CreateResult(
        IReadOnlyList<InterfaceProfileActivationCheckResult> checks)
    {
        if (checks.Any(check => check.Severity == InterfaceProfileActivationSeverity.Blocker))
        {
            return new InterfaceProfileActivationEvaluationResult(
                InterfaceProfileActivationStatus.Blocked,
                CanActivate: false,
                Checks: checks);
        }

        if (checks.Any(check => check.Severity == InterfaceProfileActivationSeverity.Warning))
        {
            return new InterfaceProfileActivationEvaluationResult(
                InterfaceProfileActivationStatus.ReadyWithWarnings,
                CanActivate: true,
                Checks: checks);
        }

        return new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Ready,
            CanActivate: true,
            Checks: checks);
    }

    private static void EvaluateProfile(
        InterfaceProfileDefinition profile,
        List<InterfaceProfileActivationCheckResult> checks)
    {
        if (profile.Metadata.ProfileKind != ProfileKind.InterfaceProfile)
        {
            AddBlocker(
                checks,
                ProfileArea,
                "profile.kind.invalid",
                "Profil ist kein Schnittstellenprofil.",
                profile.Metadata.ProfileKind.ToString());
        }

        if (string.IsNullOrWhiteSpace(profile.Metadata.Id))
        {
            AddBlocker(checks, ProfileArea, "profile.id.missing", "Profil-ID fehlt.");
        }

        if (string.IsNullOrWhiteSpace(profile.Metadata.Name))
        {
            AddWarning(checks, ProfileArea, "profile.name.missing", "Anzeigename des Schnittstellenprofils fehlt.");
        }

        if (profile.Metadata.IsBuiltIn)
        {
            AddBlocker(
                checks,
                ProfileArea,
                "profile.builtin",
                "BuiltIn-Schnittstellenprofile duerfen nicht direkt aktiviert oder veraendert werden.",
                "Bitte eine UserDefined-Kopie verwenden.");
        }

        if (!profile.Metadata.IsUserDefined && !profile.Metadata.IsBuiltIn)
        {
            AddWarning(
                checks,
                ProfileArea,
                "profile.source.unknown",
                "Profil ist weder BuiltIn noch UserDefined markiert.");
        }

        if (profile.IsActive)
        {
            AddWarning(
                checks,
                ProfileArea,
                "profile.active",
                "Schnittstellenprofil ist bereits aktiv.",
                "Der Aktivierungsassistent sollte nur bewusst gepruefte Profile aktivieren.");
        }
    }

    private static void EvaluateDependencies(
        InterfaceProfileDefinition profile,
        ProfileCatalog catalog,
        List<InterfaceProfileActivationCheckResult> checks)
    {
        EvaluateDependency(
            checks,
            "dependency.ais",
            "AIS-Profil",
            profile.AisProfileId,
            catalog.AisProfiles.Any(item => string.Equals(item.Metadata.Id, profile.AisProfileId, StringComparison.OrdinalIgnoreCase)));

        EvaluateDependency(
            checks,
            "dependency.device",
            "Geraeteprofil",
            profile.DeviceProfileId,
            catalog.DeviceProfiles.Any(item => string.Equals(item.Metadata.Id, profile.DeviceProfileId, StringComparison.OrdinalIgnoreCase)));

        EvaluateDependency(
            checks,
            "dependency.export",
            "Exportprofil",
            profile.ExportProfileId,
            catalog.ExportProfiles.Any(item => string.Equals(item.Metadata.Id, profile.ExportProfileId, StringComparison.OrdinalIgnoreCase)));
    }

    private static void EvaluateDependency(
        List<InterfaceProfileActivationCheckResult> checks,
        string code,
        string label,
        string profileId,
        bool exists)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            AddBlocker(checks, DependencyArea, $"{code}.missing", $"{label}-Referenz fehlt.");
            return;
        }

        if (!exists)
        {
            AddBlocker(
                checks,
                DependencyArea,
                $"{code}.notfound",
                $"{label} ist nicht im lokalen Profilkatalog vorhanden.",
                profileId);
            return;
        }

        AddInfo(checks, DependencyArea, $"{code}.ok", $"{label} ist vorhanden.", profileId);
    }

    private void EvaluateFolders(
        InterfaceFolderOptions options,
        List<InterfaceProfileActivationCheckResult> checks)
    {
        EvaluateFolder(
            checks,
            "folder.aisImport",
            "AIS-Importordner",
            options.AisImportFolder,
            isRequired: true);
        EvaluateFolder(
            checks,
            "folder.deviceImport",
            options.IsAttachmentOnlyMode ? "Dokument-Importordner" : "Geraete-Importordner",
            options.DeviceImportFolder,
            isRequired: true);
        EvaluateFolder(
            checks,
            "folder.export",
            "Exportordner ans AIS",
            options.ExportFolder,
            isRequired: true);
        EvaluateFolder(
            checks,
            "folder.archive",
            "Archivordner",
            options.ArchiveFolder,
            isRequired: options.ArchiveProcessedFiles);
        EvaluateFolder(
            checks,
            "folder.error",
            "Fehlerordner",
            options.ErrorFolder,
            isRequired: options.MoveFailedFilesToErrorFolder);
    }

    private void EvaluateAttachmentConfiguration(
        InterfaceFolderOptions options,
        List<InterfaceProfileActivationCheckResult> checks)
    {
        var attachmentIsRelevant = options.IsAttachmentOnlyMode
            || options.IsAttachmentProcessingEnabled
            || options.AttachmentRequirementMode == AttachmentRequirementMode.Required;

        if (!options.IsAttachmentProcessingEnabled)
        {
            AddInfo(
                checks,
                AttachmentArea,
                "attachment.disabled",
                "XDT-Anhang-Automatik ist aktuell deaktiviert.",
                "Das ist bei importierten Schnittstellenprofilen erwartbar und kein Blocker.");
        }

        if (!attachmentIsRelevant)
        {
            return;
        }

        if (options.IsAttachmentOnlyMode)
        {
            AddInfo(
                checks,
                AttachmentArea,
                "attachment.folder.import.attachmentOnlyDeviceFolder",
                "Dokumentgeraete verwenden den Geraete-/Dokument-Importordner als Dateieingang.",
                options.DeviceImportFolder);
        }
        else
        {
            EvaluateFolder(
                checks,
                "attachment.folder.import",
                "XDT-Anhang Importordner",
                options.AttachmentImportFolder,
                isRequired: true);
        }

        EvaluateFolder(
            checks,
            "attachment.folder.export",
            "XDT-Anhang Exportordner",
            options.AttachmentExportFolder,
            isRequired: true);

        if (string.IsNullOrWhiteSpace(options.AttachmentFileNameTemplate))
        {
            AddBlocker(
                checks,
                AttachmentArea,
                "attachment.filenameTemplate.missing",
                "Dateiname-Template fuer XDT-Anhaenge fehlt.");
        }

        if (!Enum.IsDefined(options.AttachmentTransferMode))
        {
            AddBlocker(
                checks,
                AttachmentArea,
                "attachment.transferMode.invalid",
                "Transfermodus fuer XDT-Anhaenge ist ungueltig.",
                options.AttachmentTransferMode.ToString());
        }

        if (options.AttachmentWaitTimeoutSeconds < 0)
        {
            AddBlocker(
                checks,
                AttachmentArea,
                "attachment.waitTime.invalid",
                "Wartezeit auf XDT-Anhang darf nicht negativ sein.");
        }
        else if (options.AttachmentWaitTimeoutSeconds < 5)
        {
            AddWarning(
                checks,
                AttachmentArea,
                "attachment.waitTime.short",
                "Wartezeit auf XDT-Anhang ist sehr kurz.",
                $"{options.AttachmentWaitTimeoutSeconds} Sekunden");
        }
        else if (options.AttachmentWaitTimeoutSeconds > 3600)
        {
            AddWarning(
                checks,
                AttachmentArea,
                "attachment.waitTime.long",
                "Wartezeit auf XDT-Anhang ist sehr lang.",
                $"{options.AttachmentWaitTimeoutSeconds} Sekunden");
        }

        if (options.AttachmentFileStabilityWaitSeconds < 0)
        {
            AddBlocker(
                checks,
                AttachmentArea,
                "attachment.stabilityWait.invalid",
                "Dateistabilitaetswartezeit fuer XDT-Anhaenge darf nicht negativ sein.");
        }
        else if (options.AttachmentFileStabilityWaitSeconds > 60)
        {
            AddWarning(
                checks,
                AttachmentArea,
                "attachment.stabilityWait.long",
                "Dateistabilitaetswartezeit fuer XDT-Anhaenge ist sehr lang.",
                $"{options.AttachmentFileStabilityWaitSeconds} Sekunden");
        }

        if (string.IsNullOrWhiteSpace(options.AttachmentExternalLinkDocumentName))
        {
            AddWarning(
                checks,
                AttachmentArea,
                "attachment.6302.empty",
                "Feld 6302 Dokumentenname ist leer und muss vor Aktivierung geprueft werden.");
        }

        if (string.IsNullOrWhiteSpace(options.AttachmentExternalLinkFileFormat))
        {
            AddWarning(
                checks,
                AttachmentArea,
                "attachment.6303.empty",
                "Feld 6303 Dateiformat ist leer; Ableitung aus Dateiendung muss vor Aktivierung geprueft werden.");
        }

        if (string.IsNullOrWhiteSpace(options.AttachmentExternalLinkDescription))
        {
            AddInfo(checks, AttachmentArea, "attachment.6304.empty", "Feld 6304 Beschreibung ist leer; dieses Feld ist optional.");
        }

        if (string.IsNullOrWhiteSpace(options.AttachmentExternalLinkPathTemplate))
        {
            AddBlocker(checks, AttachmentArea, "attachment.6305.missing", "Feld 6305 Zielpfad/Pfadtemplate kann nicht gebildet werden.");
        }

        if (options.IsAttachmentOnlyMode)
        {
            if (options.AttachmentQuietPeriodSeconds < 1 || options.AttachmentQuietPeriodSeconds > 300)
            {
                AddBlocker(
                    checks,
                    AttachmentArea,
                    "attachmentOnly.quietPeriod.invalid",
                    "Wartezeit nach letzter Dokumentdatei muss zwischen 1 und 300 Sekunden liegen.",
                    $"{options.AttachmentQuietPeriodSeconds} Sekunden");
            }
            else if (options.AttachmentCompletionMode == AttachmentCompletionMode.WaitForQuietPeriod)
            {
                AddInfo(
                    checks,
                    AttachmentArea,
                    "attachmentOnly.quietPeriod.ok",
                    "Dokumentgeraet uebertraegt nach Wartezeit ab letzter Datei.",
                    $"{options.AttachmentQuietPeriodSeconds} Sekunden");
            }
            else
            {
                AddInfo(
                    checks,
                    AttachmentArea,
                    "attachmentOnly.manualConfirmation",
                    "Dokumentgeraet wartet auf manuelle Bestaetigung mit Uebertragen.");
            }
        }
    }

    private void EvaluateFolder(
        List<InterfaceProfileActivationCheckResult> checks,
        string code,
        string label,
        string path,
        bool isRequired)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            if (isRequired)
            {
                AddBlocker(checks, FolderArea, $"{code}.missing", $"{label} fehlt.");
                return;
            }

            AddWarning(checks, FolderArea, $"{code}.optionalMissing", $"{label} ist nicht konfiguriert.");
            return;
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            AddByRequirement(checks, isRequired, FolderArea, $"{code}.invalid", $"{label} ist syntaktisch ungueltig.", ex.Message);
            return;
        }

        if (!Path.IsPathFullyQualified(path))
        {
            AddByRequirement(checks, isRequired, FolderArea, $"{code}.relative", $"{label} ist kein absoluter Pfad.", path);
            return;
        }

        var safetyResult = _folderSafetyValidator.ValidateFolderForCleanup(path);
        foreach (var issue in safetyResult.Issues.Where(issue => issue.Severity == FolderSafetyValidationIssueSeverity.Error))
        {
            AddByRequirement(checks, isRequired, FolderArea, $"{code}.unsafe", $"{label}: {issue.Message}", issue.Path);
        }

        if (safetyResult.Issues.Any(issue => issue.Severity == FolderSafetyValidationIssueSeverity.Error))
        {
            return;
        }

        if (!Directory.Exists(fullPath))
        {
            AddWarning(checks, FolderArea, $"{code}.notFound", $"{label} ist aktuell nicht erreichbar.", fullPath);
            return;
        }

        try
        {
            _ = Directory.EnumerateFileSystemEntries(fullPath).Take(1).ToList();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            AddWarning(checks, FolderArea, $"{code}.accessDenied", $"{label} ist aktuell nicht lesbar.", ex.Message);
            return;
        }

        AddInfo(checks, FolderArea, $"{code}.ok", $"{label} ist vorhanden.", fullPath);
    }

    private static void EvaluateLicense(
        InterfaceProfileDefinition profile,
        IReadOnlyList<LicensedDeviceState> licenseStates,
        List<InterfaceProfileActivationCheckResult> checks)
    {
        if (!profile.IsLicenseRequired)
        {
            AddInfo(checks, LicenseArea, "license.notRequired", "Schnittstellenprofil ist nicht lizenzpflichtig.");
            return;
        }

        AddWarning(
            checks,
            LicenseArea,
            "license.required",
            "Schnittstellenprofil ist lizenzpflichtig; Lizenzstatus vor Aktivierung pruefen.",
            "Es wird keine harte Lizenzsperre durch die Aktivierungsbewertung eingefuehrt.");

        var state = licenseStates.FirstOrDefault(item =>
            string.Equals(item.InterfaceProfileId, profile.Metadata.Id, StringComparison.OrdinalIgnoreCase));
        if (state is null)
        {
            return;
        }

        var severity = state.IsCoveredByLicense || state.IsInGracePeriod
            ? InterfaceProfileActivationSeverity.Info
            : InterfaceProfileActivationSeverity.Warning;
        Add(
            checks,
            severity,
            LicenseArea,
            "license.state",
            state.StatusMessage,
            state.DisplayName);
    }

    private static void AddByRequirement(
        List<InterfaceProfileActivationCheckResult> checks,
        bool isRequired,
        string area,
        string code,
        string message,
        string? detail = null)
    {
        if (isRequired)
        {
            AddBlocker(checks, area, code, message, detail);
            return;
        }

        AddWarning(checks, area, code, message, detail);
    }

    private static void AddBlocker(
        List<InterfaceProfileActivationCheckResult> checks,
        string area,
        string code,
        string message,
        string? detail = null)
    {
        Add(checks, InterfaceProfileActivationSeverity.Blocker, area, code, message, detail);
    }

    private static void AddWarning(
        List<InterfaceProfileActivationCheckResult> checks,
        string area,
        string code,
        string message,
        string? detail = null)
    {
        Add(checks, InterfaceProfileActivationSeverity.Warning, area, code, message, detail);
    }

    private static void AddInfo(
        List<InterfaceProfileActivationCheckResult> checks,
        string area,
        string code,
        string message,
        string? detail = null)
    {
        Add(checks, InterfaceProfileActivationSeverity.Info, area, code, message, detail);
    }

    private static void Add(
        List<InterfaceProfileActivationCheckResult> checks,
        InterfaceProfileActivationSeverity severity,
        string area,
        string code,
        string message,
        string? detail = null)
    {
        checks.Add(new InterfaceProfileActivationCheckResult(area, code, message, severity, detail));
    }
}
