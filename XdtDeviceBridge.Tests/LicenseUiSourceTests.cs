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
}
