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
    public void LicenseManager_ShouldUseGermanProductNamePaymentLabelsAndUnlimitedValidity()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml"));
        var detailXaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "CustomerDetailWindow.xaml"));
        var project = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "XdtBox.LicenseManager.csproj"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml.cs"));

        Assert.Contains("XDTBox Lizenzmanager", xaml);
        Assert.Contains("<Product>XDTBox Lizenzmanager</Product>", project);
        Assert.Contains("Kunde öffnen", xaml);
        Assert.Contains("Geräte", xaml);
        Assert.Contains("Gültig bis", xaml);
        Assert.Contains("SEPA-Lastschrift", xaml);
        Assert.Contains("Banküberweisung", xaml);
        Assert.Contains("Banküberweisung", detailXaml);
        Assert.Contains("unbefristet", xaml);
        Assert.Contains("UnlimitedValidUntilUtc", code);
    }

    [Fact]
    public void LicenseManagerSources_ShouldNotContainVisibleMojibake()
    {
        var files = new[]
        {
            FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml"),
            FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml.cs"),
            FindWorkspaceFile("XdtBox.LicenseManager", "CustomerDetailWindow.xaml"),
            FindWorkspaceFile("XdtBox.LicenseManager", "CustomerDetailWindow.xaml.cs"),
            FindWorkspaceFile("XdtDeviceBridge.Infrastructure", "LicenseManagerCustomerPdfExporter.cs")
        };

        var forbidden = new[] { "Ã", "Geraete", "Geraeteanbindung", "Gueltig", "gueltig", "Kunde oeffnen", "Lizenzuebersicht" };
        foreach (var file in files)
        {
            var source = File.ReadAllText(file);
            foreach (var token in forbidden)
            {
                Assert.DoesNotContain(token, source);
            }
        }
    }

    [Fact]
    public void LicenseManagerInstallerFiles_ShouldExistAndUseDedicatedSetupName()
    {
        var script = File.ReadAllText(FindWorkspaceFile("scripts", "build-xdtbox-licensemanager-installer.ps1"));
        var inno = File.ReadAllText(FindWorkspaceFile("installer", "XDTBox.LicenseManager.iss"));

        Assert.Contains("XDTBox_Lizenzmanager_Setup_1.0.exe", script);
        Assert.Contains("XDTBox_Lizenzmanager_Setup_1.0", inno);
        Assert.Contains("XDTBox Lizenzmanager", inno);
        Assert.Contains("license-manager-customers.json", script);
        Assert.Contains("BEGIN PRIVATE KEY", script);
        Assert.Contains("XdtDeviceBridge.App.exe", script);
        Assert.Contains("XdtBox.LicenseIssuer.exe", script);
        Assert.Contains("Remove-LicenseIssuerExecutableFromPublish", script);
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
