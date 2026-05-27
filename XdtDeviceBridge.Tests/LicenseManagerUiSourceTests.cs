namespace XdtDeviceBridge.Tests;

public sealed class LicenseManagerUiSourceTests
{
    [Fact]
    public void LicenseManager_ShouldContainRequiredTabs()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml"));

        Assert.Contains("Lizenz erstellen", xaml);
        Assert.Contains("Ausgestellte Lizenzen", xaml);
        Assert.Contains("Einstellungen", xaml);
    }

    [Fact]
    public void LicenseManager_ShouldExposeCoreLicenseActions()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml"));

        Assert.Contains("Lizenzanfrage öffnen", xaml);
        Assert.Contains("Lizenz erzeugen", xaml);
        Assert.Contains("Neue Lizenz für diesen Kunden erstellen", xaml);
        Assert.Contains("Kundendaten übernehmen", xaml);
    }

    [Fact]
    public void CustomerAppLicenseTab_ShouldCollectCustomerDataForRequests()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Contains("Kundendaten für Lizenzanforderung", xaml);
        Assert.Contains("Praxis-/Firmenname", xaml);
        Assert.Contains("Lizenzpflichtig ist ausschließlich die Anzahl aktivierter Geräteanbindungen", xaml);
        Assert.Contains("ReadLicenseCustomerDataFromEditor", code);
        Assert.Contains("_profileCatalog.DeviceProfiles", code);
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
