namespace XdtDeviceBridge.Tests;

public sealed class ProductiveUiSourceTests
{
    [Fact]
    public void ProcessingTab_ShouldRemoveManualScanAndAutomaticProcessingCheckbox()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));

        Assert.DoesNotContain("Aktive Profile einmalig scannen", xaml);
        Assert.DoesNotContain("Gefundene Dateipaare automatisch verarbeiten", xaml);
        Assert.Contains("Überwachung starten", xaml);
        Assert.Contains("Überwachung stoppen", xaml);
    }

    [Fact]
    public void ProcessingRuntime_ShouldTreatAutomaticPairProcessingAsAlwaysEnabled()
    {
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Contains("private static bool IsAutomaticPairProcessingEnabled()", code);
        Assert.Contains("return true;", ExtractMethodBody(code, "private static bool IsAutomaticPairProcessingEnabled", "private static T? FindVisualChildByTag"));
        Assert.DoesNotContain("EnableAutomaticPairProcessingCheckBox", code);
    }

    [Fact]
    public void MainWindow_ShouldAutoStartMonitoringOnceAfterContentRendered()
    {
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Contains("_hasAutoStartedPeriodicScan", code);
        Assert.Contains("_userStoppedPeriodicScan", code);
        Assert.Contains("_appSettings.AutoStartMonitoringOnAppStart", code);
        Assert.Contains("TryAutoStartPeriodicScanOnce", code);
        Assert.Contains("Dispatcher.BeginInvoke((Action)TryAutoStartPeriodicScanOnce", code);
    }

    [Fact]
    public void BackupMigrationTab_ShouldExposeRequiredControlsAndWarnings()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));

        Assert.Contains("Sicherung/Umzug", xaml);
        Assert.Contains("Sicherung erstellen", xaml);
        Assert.Contains("Sicherung auswählen", xaml);
        Assert.Contains("Wiederherstellen", xaml);
        Assert.Contains("Bei Hardwaretausch bitte neue Lizenz anfordern. Karenzzeit 7 Tage ab Umzug der Hardware.", xaml);
        Assert.Contains("Es werden keine Patientendaten oder Messdateien gesichert.", xaml);
    }

    [Fact]
    public void TabUtilityButtons_ShouldExposeHelpInfoAndSettings()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Contains("x:Name=\"TabUtilityButtonsPanel\"", xaml);
        Assert.Contains("x:Name=\"TabHelpButton\"", xaml);
        Assert.Contains("x:Name=\"AppSettingsButton\"", xaml);
        Assert.Contains("Header=\"Hilfe\"", xaml);
        Assert.Contains("Header=\"Info\"", xaml);
        Assert.Contains("Style=\"{StaticResource XdtBoxTabUtilityButtonStyle}\"", xaml);
        Assert.DoesNotContain("x:Name=\"HeaderHelpButton\"", xaml);
        Assert.Contains("OpenHelpCenter_Click", code);
        Assert.Contains("OpenAboutDialog_Click", code);
        Assert.Contains("OpenAppSettings_Click", code);
        Assert.Contains("TabHelpButton_Click", code);
    }

    [Fact]
    public void AppSettingsDialog_ShouldExposeStartupAndTrayOptions()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "AppSettingsDialog.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Contains("App beim Start direkt minimieren und ins Systray legen", xaml);
        Assert.Contains("Überwachung der Ordner beim Start automatisch starten", xaml);
        Assert.Contains("Beim Schließen ins Systray minimieren statt beenden", xaml);
        Assert.Contains("Bestätigung anzeigen, wenn bei laufender Überwachung beendet werden soll", xaml);
        Assert.Contains("LoadAppSettings", code);
        Assert.Contains("SaveAppSettings", code);
        Assert.Contains("ApplyStartupTrayPreferenceOnce", code);
        Assert.Contains("_appSettings.CloseToTrayInsteadOfExit", code);
    }

    [Fact]
    public void ProfileTemplatesTab_ShouldUseExpandedSectionsByDefault()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));

        Assert.Contains("Header=\"Profilübersicht\" IsExpanded=\"True\"", xaml);
        Assert.Contains("Header=\"Templatepakete, neue Profile und RS232-Test\" IsExpanded=\"True\"", xaml);
        Assert.Contains("Header=\"Profilnamen\" IsExpanded=\"True\"", xaml);
        Assert.Contains("Header=\"Exportregeln\" IsExpanded=\"True\"", xaml);
        Assert.Contains("Header=\"Exportregel-Entwurf\" IsExpanded=\"True\"", xaml);
        Assert.Contains("Header=\"Regelvorschau\" IsExpanded=\"True\"", xaml);
        Assert.Contains("Header=\"Test &amp; Vorschau\" IsExpanded=\"True\"", xaml);
        Assert.Contains("Header=\"Verfügbare Platzhalter\"", xaml);
        Assert.Contains("Header=\"Profil- und Template-Meldungen\" IsExpanded=\"True\"", xaml);
    }

    [Fact]
    public void InterfaceProfilesTab_ShouldUseExpandedEditableSectionsByDefault()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));

        Assert.Contains("<Expander Header=\"Ordner\" IsExpanded=\"True\"", xaml);
        Assert.Contains("x:Name=\"InterfaceSerialCommunicationGroupBox\" Header=\"Serielle Gerätekommunikation / COM-Port\" IsExpanded=\"True\"", xaml);
        Assert.Contains("x:Name=\"InterfaceDeviceOutputGroupBox\" Header=\"Ausgabe an Gerät\" IsExpanded=\"True\"", xaml);
        Assert.Contains("x:Name=\"InterfaceAttachmentSettingsGroupBox\" Header=\"XDT-Anhänge für AIS\" IsExpanded=\"True\"", xaml);
        Assert.Contains("<Expander Header=\"Ordnerbereinigung\" IsExpanded=\"True\"", xaml);
        Assert.Contains("<Expander Header=\"Archivierung\" IsExpanded=\"True\"", xaml);
        Assert.Contains("<Expander Header=\"Prüfung vor Aktivierung\" IsExpanded=\"True\"", xaml);
        Assert.Contains("Header=\"Alle Prüfpunkte\" IsExpanded=\"True\"", xaml);
    }

    [Fact]
    public void DeviceImages_ShouldLoadThroughNoLockConverter()
    {
        var mainWindow = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var floatingWindow = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "FloatingInterfaceProfileWindow.xaml"));
        var converter = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "DeviceImageSourceConverter.cs"));

        Assert.Contains("Converter={StaticResource DeviceImageSourceConverter}", mainWindow);
        Assert.Contains("Converter={StaticResource DeviceImageSourceConverter}", floatingWindow);
        Assert.Contains("BitmapCacheOption.OnLoad", converter);
        Assert.Contains("FileShare.ReadWrite | FileShare.Delete", converter);
        Assert.Contains("bitmap.Freeze()", converter);
    }

    [Fact]
    public void ActivationPreviewLayout_ShouldSeparateStatusAndActionButtons()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));

        var start = xaml.IndexOf("Header=\"Prüfung vor Aktivierung\"", StringComparison.Ordinal);
        Assert.True(start >= 0);
        var end = xaml.IndexOf("InterfaceActivationPreviewFolderChecksGrid", start, StringComparison.Ordinal);
        Assert.True(end > start);
        var section = xaml[start..end];

        Assert.Contains("<Grid.RowDefinitions>", section);
        Assert.Contains("<StackPanel Grid.Row=\"1\"", section);
        Assert.Contains("TextWrapping=\"Wrap\"", section);
        Assert.DoesNotContain("<WrapPanel Grid.Row=\"1\"", section);
    }

    [Fact]
    public void FloatingDeviceWindow_ShouldRemainStructurallySeparatedFromMainUiSettings()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "FloatingInterfaceProfileWindow.xaml"));

        Assert.DoesNotContain("TabUtilityButtonsPanel", xaml);
        Assert.DoesNotContain("AppSettingsButton", xaml);
        Assert.DoesNotContain("Sicherung/Umzug", xaml);
    }

    [Fact]
    public void AboutDialog_ShouldContainManufacturerDataAndAssemblyVersionBinding()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "AboutXdtBoxWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Contains("Technik-Apparat M.Kurze", xaml);
        Assert.Contains("Felsenstraße 4", xaml);
        Assert.Contains("90574 Roßtal", xaml);
        Assert.Contains("info@XDTBox.com", xaml);
        Assert.Contains("www.XDTBox.com", xaml);
        Assert.Contains("Assembly.GetExecutingAssembly", code);
    }

    [Fact]
    public void LocalHelp_ShouldContainRequiredTopicsAndSafetyNotes()
    {
        var help = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", Path.Combine("Assets", "Help", "xdtbox-help.md")));
        var project = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "XdtDeviceBridge.App.csproj"));

        Assert.Contains(@"<Resource Include=""Assets\Help\xdtbox-help.md"" />", project);
        Assert.Contains("# Sicherung erstellen", help);
        Assert.Contains("# Sicherung wiederherstellen", help);
        Assert.Contains("# Hardwaretausch und 7 Tage Karenzzeit", help);
        Assert.Contains("# Lizenzstatus verstehen", help);
        Assert.Contains("# RS232 NIDEK allgemein", help);
        Assert.Contains("# TOPCON CV-5000/CV-5000S", help);
        Assert.Contains("# Fehlerbehebung", help);
        Assert.Contains("Die Untersuchungsart 8402 kommt aus AIS", help);
        Assert.Contains("Es werden keine Patientendaten oder Messdateien gesichert", help);
        Assert.Contains("Die endgültige produktive Lizenzblockade ist in dieser Version nicht hart aktiviert", help);
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
