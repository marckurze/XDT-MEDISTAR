namespace XdtDeviceBridge.Tests;

public sealed class LicenseUiSourceTests
{
    [Fact]
    public void MainWindow_ShouldShowHardwareMigrationWarningInLicenseTab()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));

        Assert.Contains("Achtung: Bei Hardwaretausch bitte neue Lizenz anfordern, Karenzzeit 7 Tage ab Umzug der Hardware.", xaml);
    }

    [Fact]
    public void MainWindow_ShouldAcceptXdtboxlicLicenseImport()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Contains("Lizenz importieren", xaml);
        Assert.Contains("*.xdtboxlic", code);
        Assert.Contains("ImportSignedLicenseFile", code);
    }

    [Fact]
    public void MainWindow_ShouldMarkLegacyLicenseImportAsUnsigned()
    {
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Contains("Legacy-Lizenzdatei importiert (Signatur nicht kryptografisch geprüft)", code);
        Assert.DoesNotContain("signiert gültig", code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MainWindow_ShouldPersistSignedLicenseImportAndShowThanksMessage()
    {
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));
        var body = ExtractMethodBody(code, "ImportSignedLicenseFile", "UpdateGracePeriods_Click");

        Assert.Contains("File.Copy(filePath, GetSignedLicenseFilePath(paths), overwrite: true);", body);
        Assert.Contains("persistedSignedLicense", body);
        Assert.Contains("XdtBoxLicenseConstants.CreateSuccessfulLicenseImportMessage", body);
    }

    [Fact]
    public void MainWindow_UpdateGracePeriods_ShouldKeepSignedLicenseAsLeadingSource()
    {
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));
        var body = ExtractMethodBody(code, "UpdateGracePeriods_Click", "ExportTemplatePackage_Click");

        Assert.Contains("LoadCurrentDisplayLicenseFromLocalSource(paths, installation)", body);
        Assert.Contains("RefreshLicensedDeviceStatesFromLocalLicense();", body);
        Assert.DoesNotContain("_licenseFileRepository.Load(paths.LicenseFile)", body);
    }

    [Fact]
    public void MainWindow_LocalLicenseSource_ShouldPreferSignedLicenseBeforeLegacyJson()
    {
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));
        var body = ExtractMethodBody(
            code,
            "private LicenseInfo? LoadCurrentDisplayLicenseFromLocalSource",
            "private void ShowLicensedDeviceStates");

        Assert.True(
            body.IndexOf("GetSignedLicenseFilePath(paths)", StringComparison.Ordinal)
            < body.IndexOf("paths.LicenseFile", StringComparison.Ordinal));
        Assert.Contains("CreateDisplayLicenseInfo(result.Payload", body);
        Assert.Contains("RSA-PSS-SHA256 geprüft", body);
    }

    private static string FindWorkspaceFile(string projectFolder, string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, projectFolder, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Workspace file not found: {projectFolder}/{fileName}");
    }

    private static string ExtractMethodBody(string code, string startMethodName, string nextMethodName)
    {
        var start = code.IndexOf(startMethodName, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Method {startMethodName} not found.");

        var end = code.IndexOf(nextMethodName, start + startMethodName.Length, StringComparison.Ordinal);
        Assert.True(end > start, $"Next method {nextMethodName} not found after {startMethodName}.");

        return code[start..end];
    }
}
