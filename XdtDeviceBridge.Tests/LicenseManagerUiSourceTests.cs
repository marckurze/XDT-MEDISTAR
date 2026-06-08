namespace XdtDeviceBridge.Tests;

public sealed class LicenseManagerUiSourceTests
{
    [Fact]
    public void LicenseManager_ShouldContainRequiredTabs()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml"));

        Assert.Contains("Lizenz erstellen", xaml);
        Assert.Contains("Kunden", xaml);
        Assert.Contains("Ausgestellte Lizenzen", xaml);
        Assert.Contains("Sicherung", xaml);
        Assert.Contains("Einstellungen", xaml);
    }

    [Fact]
    public void LicenseManager_ShouldExposeCoreLicenseActions()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml"));

        Assert.Contains("Lizenzanfrage", xaml);
        Assert.Contains("Lizenz erzeugen", xaml);
        Assert.Contains("Neue Lizenz", xaml);
        Assert.Contains("Kundendaten", xaml);
    }

    [Fact]
    public void LicenseManager_ShouldExposeCustomerBillingAndBackupActions()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml.cs"));

        Assert.Contains("CustomerPricePerDeviceTextBox", xaml);
        Assert.Contains("Kundenliste als PDF exportieren", xaml);
        Assert.Contains("Kunde", xaml);
        Assert.Contains("Sicherung erstellen", xaml);
        Assert.Contains("Sicherung wiederherstellen", xaml);
        Assert.Contains("LicenseManagerCustomerRepository", code);
        Assert.Contains("LicenseManagerBackupService", code);
        Assert.Contains("LicenseManagerCustomerPdfExporter", code);
    }

    [Fact]
    public void LicenseManager_ShouldExposePaymentFieldsFromLicenseRequest()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml.cs"));

        Assert.Contains("InvoiceEmailTextBox", xaml);
        Assert.Contains("CustomerIbanTextBox", xaml);
        Assert.Contains("CustomerBicTextBox", xaml);
        Assert.Contains("CustomerAccountHolderTextBox", xaml);
        Assert.Contains("SepaConsentCheckBox", xaml);
        Assert.Contains("AlwaysInvoiceCheckBox", xaml);
        Assert.Contains("Iban: NormalizeOptional(CustomerIbanTextBox.Text)", code);
        Assert.Contains("InvoiceEmail: NormalizeOptional(InvoiceEmailTextBox.Text)", code);
    }

    [Fact]
    public void CustomerAppLicenseTab_ShouldCollectCustomerDataForRequests()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Contains("Kundendaten", xaml);
        Assert.Contains("Praxis-/Firmenname", xaml);
        Assert.Contains("Lizenzpflichtig", xaml);
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
