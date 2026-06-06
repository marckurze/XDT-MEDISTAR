using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class XdtBoxInstallationDataPolicyTests
{
    private readonly XdtBoxInstallationDataPolicy _policy = new();

    [Fact]
    public void GetCustomerDataItems_ShouldClassifyConfigurationAsCustomerData()
    {
        var paths = CreateAppDataPaths();
        var backupFolder = Path.Combine(Path.GetPathRoot(paths.BaseFolder) ?? "C:\\", "XDTBox", "Backup");

        var items = _policy.GetCustomerDataItems(paths, backupFolder);
        var byKey = items.ToDictionary(item => item.Key, StringComparer.Ordinal);

        Assert.Equal(XdtBoxInstallationDataCategory.CustomerData, byKey["profiles"].Category);
        Assert.Equal(paths.ProfilesFolder, byKey["profiles"].Location);
        Assert.Equal(XdtBoxInstallationUpdateRule.PreserveCustomerData, byKey["profiles"].UpdateRule);
        Assert.Equal(XdtBoxUninstallRule.KeepByDefaultDeleteOnlyWithExplicitConfirmation, byKey["profiles"].UninstallRule);
        Assert.Equal(paths.TemplatePackagesFolder, byKey["template-packages"].Location);
        Assert.Equal(Path.Combine(paths.LicensesFolder, "license.xdtboxlic"), byKey["license-file"].Location);
        Assert.Equal(Path.Combine(paths.BaseFolder, "device-image-overrides.json"), byKey["device-image-overrides"].Location);
        Assert.Equal(Path.Combine(paths.BaseFolder, DeviceTechnicalProfileService.DefaultOverrideFileName), byKey["device-info-overrides"].Location);
        Assert.Equal(Path.Combine(paths.BaseFolder, "ui", "app-settings.json"), byKey["app-settings"].Location);
        Assert.Equal(paths.DeviceGracePeriodsFile, byKey["grace-periods"].Location);
        Assert.Equal(backupFolder, byKey["backups"].Location);
        Assert.Equal(XdtBoxInstallationDataCategory.TemporaryDiagnosticData, byKey["logs"].Category);
    }

    [Fact]
    public void GetAppComponentItems_ShouldClassifyInstalledFilesAsReplaceable()
    {
        var installationFolder = Path.Combine(Path.GetTempPath(), "XDTBox", "App");

        var items = _policy.GetAppComponentItems(installationFolder);

        Assert.Contains(items, item =>
            item.Key == "program-files"
            && item.Category == XdtBoxInstallationDataCategory.AppComponent
            && item.UpdateRule == XdtBoxInstallationUpdateRule.MayReplaceWithAppUpdate
            && item.UninstallRule == XdtBoxUninstallRule.RemoveWithApplication);
        Assert.Contains(items, item =>
            item.Key == "builtin-definitions"
            && item.Category == XdtBoxInstallationDataCategory.BuiltInTemplate
            && item.UpdateRule == XdtBoxInstallationUpdateRule.MayRepairBuiltInOnly);
        Assert.Contains(items, item =>
            item.Key == "standard-device-images"
            && item.BackupRule.Contains("Nicht als Kundendaten", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(items, item =>
            item.Key == "standard-device-info"
            && item.Location.EndsWith(Path.Combine("Assets", "DeviceInfo"), StringComparison.OrdinalIgnoreCase)
            && item.BackupRule.Contains("Nicht als Kundendaten", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetManufacturerToolItems_ShouldExcludePrivateKeysFromCustomerSetup()
    {
        var paths = new LicenseManagerPathProvider().GetPaths(Path.Combine(Path.GetTempPath(), "XDTBox", "Lizenzaktivierung"));

        var items = _policy.GetManufacturerToolItems(paths);

        Assert.Contains(items, item =>
            item.Key == "license-manager-keys"
            && item.Category == XdtBoxInstallationDataCategory.ManufacturerSecret
            && item.UpdateRule == XdtBoxInstallationUpdateRule.ExcludeFromCustomerSetup
            && item.UninstallRule == XdtBoxUninstallRule.ExcludeFromCustomerSetup);
        Assert.Contains(items, item =>
            item.Key == "license-manager-history"
            && item.Category == XdtBoxInstallationDataCategory.ManufacturerToolData);
    }

    [Fact]
    public void ClassifyPathForUninstall_ShouldNeverDeleteExternalPracticeFoldersAutomatically()
    {
        var paths = CreateAppDataPaths();
        var installationFolder = Path.Combine(Path.GetTempPath(), "XDTBox", "App");
        var backupFolder = Path.Combine(Path.GetPathRoot(paths.BaseFolder) ?? "C:\\", "XDTBox", "Backup");

        Assert.Equal(
            XdtBoxPathRemovalDecision.MayRemoveWithApplication,
            _policy.ClassifyPathForUninstall(Path.Combine(installationFolder, "XdtDeviceBridge.App.exe"), paths, installationFolder, backupFolder));
        Assert.Equal(
            XdtBoxPathRemovalDecision.DeleteOnlyAfterExplicitCustomerDataConfirmation,
            _policy.ClassifyPathForUninstall(Path.Combine(paths.ProfilesFolder, "interfaces", "interface-user.json"), paths, installationFolder, backupFolder));
        Assert.Equal(
            XdtBoxPathRemovalDecision.DeleteOnlyAfterExplicitCustomerDataConfirmation,
            _policy.ClassifyPathForUninstall(Path.Combine(backupFolder, "XDTBox_Backup_20260604_120000.xdtboxbackup"), paths, installationFolder, backupFolder));
        Assert.Equal(
            XdtBoxPathRemovalDecision.NeverDeleteAutomatically,
            _policy.ClassifyPathForUninstall(@"C:\XDTBox\RT3100RS232\Patient2Box", paths, installationFolder, backupFolder));
        Assert.Equal(
            XdtBoxPathRemovalDecision.NeverDeleteAutomatically,
            _policy.ClassifyPathForUninstall(@"D:\Praxis\AIS\Export", paths, installationFolder, backupFolder));
    }

    [Fact]
    public void CurrentDefaultCustomerDataRoot_ShouldDocumentLegacyLocalAppDataName()
    {
        Assert.Equal("XdtDeviceBridge", XdtBoxInstallationDataPolicy.CurrentCustomerDataRootName);
        Assert.Contains("XdtDeviceBridge", XdtBoxInstallationDataPolicy.CurrentDefaultCustomerDataRoot);
        Assert.Contains("ProgramData", XdtBoxInstallationDataPolicy.RecommendedFutureMachineDataRoot);
    }

    private static AppDataPaths CreateAppDataPaths()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return new AppDataPathProvider().GetPaths(baseFolder);
    }
}
