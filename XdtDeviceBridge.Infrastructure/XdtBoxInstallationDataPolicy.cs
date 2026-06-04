namespace XdtDeviceBridge.Infrastructure;

public enum XdtBoxInstallationDataCategory
{
    AppComponent,
    BuiltInTemplate,
    CustomerData,
    TemporaryDiagnosticData,
    ExternalPracticeData,
    ManufacturerToolData,
    ManufacturerSecret
}

public enum XdtBoxInstallationUpdateRule
{
    MayReplaceWithAppUpdate,
    MayRepairBuiltInOnly,
    PreserveCustomerData,
    OptionalCleanupOnly,
    NeverTouchFromCustomerInstaller,
    ExcludeFromCustomerSetup
}

public enum XdtBoxUninstallRule
{
    RemoveWithApplication,
    KeepByDefaultDeleteOnlyWithExplicitConfirmation,
    NeverDeleteAutomatically,
    ExcludeFromCustomerSetup
}

public enum XdtBoxPathRemovalDecision
{
    MayRemoveWithApplication,
    DeleteOnlyAfterExplicitCustomerDataConfirmation,
    NeverDeleteAutomatically,
    ExcludeFromCustomerSetup,
    UnknownKeepByDefault
}

public sealed record XdtBoxInstallationDataPolicyItem(
    string Key,
    string DisplayName,
    XdtBoxInstallationDataCategory Category,
    string Location,
    XdtBoxInstallationUpdateRule UpdateRule,
    XdtBoxUninstallRule UninstallRule,
    string BackupRule,
    string Note);

public sealed class XdtBoxInstallationDataPolicy
{
    public const string CurrentCustomerDataRootName = "XdtDeviceBridge";
    public const string RecommendedFutureMachineDataRoot = @"%ProgramData%\XDTBox";
    public const string CurrentDefaultCustomerDataRoot = @"%LocalAppData%\XdtDeviceBridge";

    public IReadOnlyList<XdtBoxInstallationDataPolicyItem> GetAppComponentItems(string installationFolder)
    {
        var root = string.IsNullOrWhiteSpace(installationFolder)
            ? "<Installationsordner>"
            : Path.GetFullPath(installationFolder);

        return new[]
        {
            AppItem("program-files", "EXE/DLL/Runtime-Dateien", root, "Programmdateien dürfen bei Updates ersetzt und beim Standard-Deinstall entfernt werden."),
            AppItem("themes", "Themes und WPF-Styles", Path.Combine(root, "Styles"), "UI-Ressourcen sind App-Bestandteil."),
            AppItem("help", "Hilfe und lokale Dokumentation", Path.Combine(root, "Assets", "Help"), "Hilfe darf mit der App aktualisiert werden."),
            AppItem("icons", "Icons und Branding-Assets", Path.Combine(root, "Assets"), "Standard-Assets sind App-Bestandteil."),
            AppItem("standard-device-images", "Standard-Gerätebilder als App-Assets", Path.Combine(root, "Assets", "Devices"), "Nur App-Assets; lokale Gerätebild-Overrides liegen in Kundendaten."),
            new(
                "builtin-definitions",
                "BuiltIn-Definitionen im Code",
                XdtBoxInstallationDataCategory.BuiltInTemplate,
                "XdtDeviceBridge.Core Default*-Definitions",
                XdtBoxInstallationUpdateRule.MayRepairBuiltInOnly,
                XdtBoxUninstallRule.RemoveWithApplication,
                "Nicht als Kundendaten sichern; werden aus App-Version neu bereitgestellt.",
                "BuiltIns dürfen aktualisiert/repariert werden, UserDefined-Profile und Praxiswerte nicht.")
        };
    }

    public IReadOnlyList<XdtBoxInstallationDataPolicyItem> GetCustomerDataItems(AppDataPaths paths, string? backupFolder = null)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var resolvedBackupFolder = string.IsNullOrWhiteSpace(backupFolder)
            ? @"C:\XDTBox\Backup beziehungsweise Dokumente\XDTBox\Backup"
            : Path.GetFullPath(backupFolder);

        return new[]
        {
            CustomerItem("customer-root", "Aktueller Kundendatenstamm", paths.BaseFolder, "Root der aktuellen lokalen XDTBox-Konfiguration; langfristige Migration nach ProgramData prüfen."),
            CustomerItem("profiles", "Profile inklusive UserDefined und lokale Schnittstellenprofile", paths.ProfilesFolder, "Enthält AIS-, Geräte-, Export- und Schnittstellenprofile mit Ordnern/COM/Praxiswerten."),
            CustomerItem("templates", "Legacy-/lokale Templateablage", paths.TemplatesFolder, "Aktuell vorbereiteter lokaler Templateordner; nicht durch Installer löschen."),
            CustomerItem("template-packages", "Baukasten-Templates und lokale Templatepakete", paths.TemplatePackagesFolder, "Wird vom XDT-Baukasten und der Profilverwaltung genutzt."),
            CustomerItem("licenses", "Lizenzordner", paths.LicensesFolder, "Enthält Kundendaten, signierte Lizenzdatei und Karenzzeitdaten."),
            CustomerItem("license-json", "Aktive Lizenzbewertung license.json", paths.LicenseFile, "Lokale Lizenz-/Auswertungsdatei; bei Update erhalten."),
            CustomerItem("license-file", "Signierte Lizenzdatei .xdtboxlic", Path.Combine(paths.LicensesFolder, "license.xdtboxlic"), "Optional im Backup enthalten; nie durch Update löschen."),
            CustomerItem("license-customer", "Lizenzkundendaten", Path.Combine(paths.LicensesFolder, "license-customer-data.json"), "Praxis-/Kundendaten für Lizenzanforderungen."),
            CustomerItem("grace-periods", "Lizenz-Karenzzeitdaten", paths.DeviceGracePeriodsFile, "Maschinenbezogene Karenzzeitdaten; bei Update erhalten."),
            CustomerItem("license-requests", "Lizenzanforderungen", paths.LicenseRequestsFolder, "Lokale Lizenzanforderungsdateien; keine App-Dateien."),
            CustomerItem("device-images", "Lokale Gerätebilder", Path.Combine(paths.BaseFolder, "DeviceImages"), "Vom Kunden gepflegte Gerätebilder."),
            CustomerItem("device-image-overrides", "Gerätebild-Overrides", Path.Combine(paths.BaseFolder, "device-image-overrides.json"), "Verweist BuiltIn-Geräte auf lokale Bild-Overrides."),
            CustomerItem("app-settings", "AppSettings", Path.Combine(paths.BaseFolder, "ui", "app-settings.json"), "Autostart-/Tray-/Komforteinstellungen."),
            CustomerItem("floating-window-state", "UI-Komfortdaten Floating-Fenster", Path.Combine(paths.BaseFolder, "ui", "floating-interface-windows.json"), "Fensterpositionen und Abdockstatus."),
            CustomerItem("installation-info", "Installationsinformation", paths.InstallationInfoFile, "Lokale Installationskennung und technische Metadaten."),
            DiagnosticItem("logs", "Logs und Diagnoseausgaben", paths.LogsFolder, "Optional bereinigbar, aber nicht ungefragt beim Update löschen."),
            CustomerItem("backups", "Sicherung/Umzug-Backups", resolvedBackupFolder, "Backups nur nach ausdrücklicher Bestätigung löschen.")
        };
    }

    public IReadOnlyList<XdtBoxInstallationDataPolicyItem> GetManufacturerToolItems(LicenseManagerPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        return new[]
        {
            ManufacturerItem("license-manager-base", "Hersteller-Lizenztool Arbeitsordner", paths.BaseFolder, "Nicht Bestandteil des Kunden-Setups."),
            ManufacturerItem("license-manager-history", "Ausgestellte-Lizenzen-Historie", paths.HistoryFile, "Herstellerdaten; getrennt vom Kunden-PC halten."),
            ManufacturerItem("license-manager-settings", "Hersteller-Lizenztool Einstellungen", paths.SettingsFile, "Herstellerdaten; nicht mit Kundeninstaller verteilen."),
            ManufacturerSecretItem("license-manager-keys", "Private Hersteller-Schlüssel", paths.KeysFolder, "Private Keys niemals verteilen und niemals durch Kundeninstaller anfassen."),
            ManufacturerItem("license-issuer-output", "LicenseIssuer Ausgabedaten", paths.LicensesFolder, "Hersteller-/Supportdaten, wenn auf Hersteller-PC genutzt.")
        };
    }

    public XdtBoxPathRemovalDecision ClassifyPathForUninstall(
        string path,
        AppDataPaths customerPaths,
        string? installationFolder = null,
        string? backupFolder = null)
    {
        ArgumentNullException.ThrowIfNull(customerPaths);

        if (string.IsNullOrWhiteSpace(path))
        {
            return XdtBoxPathRemovalDecision.UnknownKeepByDefault;
        }

        var fullPath = Path.GetFullPath(path);
        if (!string.IsNullOrWhiteSpace(installationFolder)
            && IsSameOrBelow(fullPath, Path.GetFullPath(installationFolder)))
        {
            return XdtBoxPathRemovalDecision.MayRemoveWithApplication;
        }

        if (IsSameOrBelow(fullPath, customerPaths.BaseFolder)
            || IsSameOrBelow(fullPath, customerPaths.ProfilesFolder)
            || IsSameOrBelow(fullPath, customerPaths.TemplatePackagesFolder)
            || IsSameOrBelow(fullPath, customerPaths.LicensesFolder)
            || (!string.IsNullOrWhiteSpace(backupFolder) && IsSameOrBelow(fullPath, Path.GetFullPath(backupFolder))))
        {
            return XdtBoxPathRemovalDecision.DeleteOnlyAfterExplicitCustomerDataConfirmation;
        }

        return XdtBoxPathRemovalDecision.NeverDeleteAutomatically;
    }

    private static XdtBoxInstallationDataPolicyItem AppItem(string key, string displayName, string location, string note)
    {
        return new XdtBoxInstallationDataPolicyItem(
            key,
            displayName,
            XdtBoxInstallationDataCategory.AppComponent,
            location,
            XdtBoxInstallationUpdateRule.MayReplaceWithAppUpdate,
            XdtBoxUninstallRule.RemoveWithApplication,
            "Nicht als Kundendaten sichern; wird aus Installer/App-Version wiederhergestellt.",
            note);
    }

    private static XdtBoxInstallationDataPolicyItem CustomerItem(string key, string displayName, string location, string note)
    {
        return new XdtBoxInstallationDataPolicyItem(
            key,
            displayName,
            XdtBoxInstallationDataCategory.CustomerData,
            location,
            XdtBoxInstallationUpdateRule.PreserveCustomerData,
            XdtBoxUninstallRule.KeepByDefaultDeleteOnlyWithExplicitConfirmation,
            "In Sicherung/Umzug aufnehmen, soweit Datei/Ordner vorhanden und keine Patientendaten enthalten.",
            note);
    }

    private static XdtBoxInstallationDataPolicyItem DiagnosticItem(string key, string displayName, string location, string note)
    {
        return new XdtBoxInstallationDataPolicyItem(
            key,
            displayName,
            XdtBoxInstallationDataCategory.TemporaryDiagnosticData,
            location,
            XdtBoxInstallationUpdateRule.OptionalCleanupOnly,
            XdtBoxUninstallRule.KeepByDefaultDeleteOnlyWithExplicitConfirmation,
            "Optional; Diagnose- und Logdaten nicht ungefragt löschen.",
            note);
    }

    private static XdtBoxInstallationDataPolicyItem ManufacturerItem(string key, string displayName, string location, string note)
    {
        return new XdtBoxInstallationDataPolicyItem(
            key,
            displayName,
            XdtBoxInstallationDataCategory.ManufacturerToolData,
            location,
            XdtBoxInstallationUpdateRule.ExcludeFromCustomerSetup,
            XdtBoxUninstallRule.ExcludeFromCustomerSetup,
            "Nicht Teil der Kundensicherung.",
            note);
    }

    private static XdtBoxInstallationDataPolicyItem ManufacturerSecretItem(string key, string displayName, string location, string note)
    {
        return new XdtBoxInstallationDataPolicyItem(
            key,
            displayName,
            XdtBoxInstallationDataCategory.ManufacturerSecret,
            location,
            XdtBoxInstallationUpdateRule.ExcludeFromCustomerSetup,
            XdtBoxUninstallRule.ExcludeFromCustomerSetup,
            "Nicht Teil der Kundensicherung; separat und geschützt verwalten.",
            note);
    }

    private static bool IsSameOrBelow(string path, string root)
    {
        if (string.IsNullOrWhiteSpace(root))
        {
            return false;
        }

        var normalizedPath = Path.GetFullPath(path);
        var normalizedRoot = Path.GetFullPath(root);
        var normalizedRootWithSeparator = normalizedRoot.EndsWith(Path.DirectorySeparatorChar)
            ? normalizedRoot
            : normalizedRoot + Path.DirectorySeparatorChar;

        return string.Equals(normalizedPath, normalizedRoot, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(normalizedRootWithSeparator, StringComparison.OrdinalIgnoreCase);
    }
}
