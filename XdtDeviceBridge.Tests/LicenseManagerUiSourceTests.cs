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
    public void LicenseManager_ShouldShowIssuedLicensesAboveStructuredDetails()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml.cs"));
        var historyTab = ExtractBetween(xaml, "<TabItem Header=\"Ausgestellte Lizenzen\">", "<TabItem Header=\"Sicherung\">");
        var historyColumns = ExtractBetween(historyTab, "x:Name=\"HistoryGrid\"", "</DataGrid.Columns>");

        Assert.True(historyTab.IndexOf("x:Name=\"HistoryGrid\"", StringComparison.Ordinal) <
            historyTab.IndexOf("Details zur ausgew", StringComparison.Ordinal));
        Assert.Contains("Header=\"Kundennummer\"", historyColumns);
        Assert.Contains("Header=\"Kunde\"", historyColumns);
        Assert.Contains("Header=\"Ort\"", historyColumns);
        Assert.Contains("Header=\"Ger", historyColumns);
        Assert.Contains("Header=\"Ausstellungsdatum\"", historyColumns);
        Assert.DoesNotContain("Header=\"Installation", historyColumns);
        Assert.DoesNotContain("Header=\"Telefon", historyColumns);
        Assert.DoesNotContain("Header=\"Lizenztyp", historyColumns);
        Assert.DoesNotContain("Header=\"I\"", historyColumns);
        Assert.Contains("HistoryDetailCustomerText", historyTab);
        Assert.Contains("HistoryDevicesGrid", historyTab);
        Assert.DoesNotContain("HistoryDetailsTextBlock", xaml);
        Assert.Contains("ShowHistoryDetails(GetSelectedHistoryRecord())", code);
    }

    [Fact]
    public void LicenseManager_ShouldAllowRemovingLocalHistoryEntriesOnly()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml.cs"));

        Assert.Contains("DeleteHistoryEntryButton", xaml);
        Assert.Contains("Eintrag entfernen", xaml);
        Assert.Contains("DeleteHistoryEntry_Click", code);
        Assert.Contains("_historyRepository.Remove", code);
        Assert.Contains("Lizenzdateien, Private Keys und Kundenstammdaten wurden nicht", code);
        Assert.Contains("ReconcileCustomersAfterHistoryDeletion", code);
    }

    [Fact]
    public void LicenseManager_ShouldShowCustomerMonthlyTotalFooter()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml.cs"));

        Assert.Contains("CustomerMonthlyTotalTextBlock", xaml);
        Assert.Contains("UpdateCustomerMonthlyTotal", code);
        Assert.Contains("Gesamtsumme monatlicher Lizenzen", code);
        Assert.Contains("BillableDeviceCount", code);
        Assert.Contains("CalculateNetTotal", code);
    }

    [Fact]
    public void LicenseManager_ShouldShowOnlyBillableDevicesInLicenseRequest()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml.cs"));
        var requestGridColumns = ExtractBetween(xaml, "x:Name=\"RequestDevicesGrid\"", "</DataGrid.Columns>");

        Assert.Contains("RequestDevicesHintTextBlock", xaml);
        Assert.DoesNotContain("DataGridCheckBoxColumn", requestGridColumns);
        Assert.DoesNotContain("Header=\"Aktiv\"", requestGridColumns);
        Assert.Contains("Header=\"Standort\"", requestGridColumns);
        Assert.Contains("device.IsActive && device.IsLicenseRequired", code);
        Assert.Contains("request.Devices.Count == 0", code);
        Assert.Contains("alter Anfrage", code);
    }

    [Fact]
    public void LicenseManager_ShouldPromptForDuplicateCustomerNumberAndKeepDeviceLocationEditable()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml"));
        var detailXaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "CustomerDetailWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml.cs"));

        Assert.Contains("Kundennummer bereits vorhanden", code);
        Assert.Contains("diesem bestehenden Kunden zugeordnet", code);
        Assert.Contains("UpsertByInstallation", code);
        Assert.Contains("UpsertLicenseByInstallation", code);
        Assert.Contains("Binding=\"{Binding Location, Mode=TwoWay", xaml);
        Assert.Contains("Binding=\"{Binding Location, Mode=TwoWay", detailXaml);
    }

    [Fact]
    public void LicenseManagerCustomerDetails_ShouldStackTablesAndRemoveCancellationWarning()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "CustomerDetailWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "CustomerDetailWindow.xaml.cs"));

        var installations = xaml.IndexOf("Header=\"Installationen", StringComparison.Ordinal);
        var devices = xaml.IndexOf("Header=\"Aktive lizenzierte Anbindungen\"", StringComparison.Ordinal);
        var history = xaml.IndexOf("Header=\"Lizenzhistorie\"", StringComparison.Ordinal);

        Assert.True(installations >= 0 && devices > installations && history > devices);
        Assert.Contains("<ScrollViewer", xaml);
        Assert.DoesNotContain("Eine Stornierung entfernt", xaml);
        Assert.DoesNotContain("Offline-Lizenz wird dadurch nicht automatisch", xaml);
        Assert.Contains("Lizenz als storniert markieren", xaml);
        Assert.DoesNotContain("Hersteller- und Kosten", code);
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

        Assert.Contains("$setupFileName = \"XDTBox_Lizenzmanager_Setup_$Version.exe\"", script);
        Assert.Contains("XDTBox_Lizenzmanager_Setup_{#MyAppVersion}", inno);
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

    private static string ExtractBetween(string source, string startToken, string endToken)
    {
        var start = source.IndexOf(startToken, StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException($"Start token not found: {startToken}");
        }

        var end = source.IndexOf(endToken, start, StringComparison.Ordinal);
        if (end < 0)
        {
            throw new InvalidOperationException($"End token not found: {endToken}");
        }

        return source[start..end];
    }
}
