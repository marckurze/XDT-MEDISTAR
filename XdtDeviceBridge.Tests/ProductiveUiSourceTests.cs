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
    public void TabUtilityButtons_ShouldExposeHelpInfoSettingsAndExit()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Contains("x:Name=\"TabUtilityButtonsPanel\"", xaml);
        Assert.Contains("x:Name=\"TabHelpButton\"", xaml);
        Assert.Contains("x:Name=\"AppSettingsButton\"", xaml);
        Assert.Contains("x:Name=\"AppExitButton\"", xaml);
        Assert.Contains("Content=\"Beenden\"", xaml);
        Assert.Contains("ToolTip=\"XDTBox vollständig beenden\"", xaml);
        Assert.Contains("Header=\"Hilfe\"", xaml);
        Assert.Contains("Header=\"Info\"", xaml);
        Assert.Contains("Style=\"{StaticResource XdtBoxTabUtilityButtonStyle}\"", xaml);
        Assert.DoesNotContain("x:Name=\"HeaderHelpButton\"", xaml);
        Assert.Contains("OpenHelpCenter_Click", code);
        Assert.Contains("OpenAboutDialog_Click", code);
        Assert.Contains("OpenAppSettings_Click", code);
        Assert.Contains("TabHelpButton_Click", code);
        Assert.Contains("AppExitButton_Click", code);
        Assert.Contains("RequestApplicationExit();", ExtractMethodBody(code, "private void AppExitButton_Click", "private void InitializeProfileOverview"));
    }

    [Fact]
    public void MainWindowXaml_ShouldOnlyReferenceKnownStaticResources()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var theme = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", Path.Combine("Styles", "XdtBoxTheme.xaml")));
        var definedKeys = ExtractResourceKeys(xaml).Concat(ExtractResourceKeys(theme)).ToHashSet(StringComparer.Ordinal);
        var referencedKeys = ExtractReferencedResourceKeys(xaml).ToArray();

        var missingKeys = referencedKeys
            .Where(key => !definedKeys.Contains(key))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            missingKeys.Length == 0,
            $"MainWindow.xaml references unknown WPF resources: {string.Join(", ", missingKeys)}");
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
    public void RemovedProfileTemplatesManualPreview_ShouldNotLeaveOldWorkbenchCodeBehind()
    {
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.DoesNotContain("RefreshManualProcessingPreview", code);
        Assert.DoesNotContain("BuilderManualProcessingPreviewRequest", code);
        Assert.DoesNotContain("ResolveBuilderDeviceProfile(exportProfile)", code);
        Assert.DoesNotContain("ResolveBuilderInterfaceProfile(exportProfile)", code);
        Assert.Contains("XdtBaukastenPreviewService", code);
    }

    [Fact]
    public void XdtBaukastenTab_ShouldExposeIndependentWorkflowSurface()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Contains("<TabItem Header=\"XDT-Baukasten\">", xaml);
        Assert.DoesNotContain("<TabItem Header=\"Profile &amp; Templates\">", xaml);
        Assert.Contains("x:Name=\"XdtBaukastenRoot\"", xaml);
        Assert.Contains("Baukasten-Template laden", xaml);
        Assert.Contains("Template Paket importieren", xaml);
        Assert.Contains("Gerät laden", xaml);
        Assert.Contains("Konfiguration als Template speichern", xaml);
        Assert.Contains("Template Paket exportieren", xaml);
        var workbenchSection = ExtractSection(
            xaml,
            "<TabItem Header=\"XDT-Baukasten\">",
            "<TabItem Header=\"Profilverwaltung\">");
        Assert.DoesNotContain("Neues AIS anlegen", workbenchSection);
        Assert.DoesNotContain("AIS wählen", workbenchSection);
        Assert.Contains("Neues Gerät anlegen", workbenchSection);
        Assert.Contains("XdtBaukastenAisProfileComboBox", workbenchSection);
        Assert.True(File.Exists(FindWorkspaceFile("XdtDeviceBridge.App", "NewDeviceProfileDialog.xaml")));
        Assert.Contains("XdtBaukastenState", code);
        Assert.Contains("XdtBaukastenPreviewService", code);
        Assert.Contains("XdtBaukastenUndoBuffer", code);
        Assert.Contains("TabBaukastenUndoButton", xaml);
        Assert.Contains("ToolTip=\"Letzte Baukasten-Änderung rückgängig machen\"", xaml);
        Assert.Contains("ResolveXdtBaukastenInterfaceProfile", code);
    }

    [Fact]
    public void XdtBaukastenTab_ShouldContainSketchSectionsAndFourResultViews()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var section = ExtractSection(
            xaml,
            "<TabItem Header=\"XDT-Baukasten\">",
            "<TabItem Header=\"Profilverwaltung\">");

        Assert.Contains("Geräteidentität", section);
        Assert.Contains("Testdaten und Rohdaten", section);
        Assert.Contains("Anzeige Rohdaten von AIS Datei", section);
        Assert.Contains("Anzeige Rohdaten vom Gerät", section);
        Assert.Contains("Mapping / Exportprofil", section);
        Assert.Contains("Verarbeitung starten", section);
        Assert.Contains("Roh-XDT-Ausgabe an das AIS", section);
        Assert.Contains("XdtBaukastenResultLinesGrid", section);
        Assert.Contains("Binding=\"{Binding LineNumber}\"", section);
        Assert.Contains("Roh-XDT", section);
        Assert.Contains("Ansicht im AIS", section);
        Assert.Contains("Geräteausgabe", section);
        Assert.Contains("Diagnose", section);
        Assert.Contains("Konfiguration Exportregeln", section);
        Assert.Contains("XdtBaukastenRuleDirectionPanel", section);
        Assert.Contains("Export an AIS", section);
        Assert.Contains("Export an Gerät", section);
        Assert.Contains("x:Name=\"XdtBaukastenRuleNumberColumn\"", section);
        Assert.Contains("Binding=\"{Binding RowNumber}\"", section);
        Assert.Contains("XdtBaukastenDraftTargetLabel", section);
        Assert.Contains("XdtBaukastenAddExportRuleButton", section);
        Assert.Contains("XdtBaukastenDeleteExportRule_Click", section);
        Assert.Contains("Exportregel Entwurf", section);
        Assert.Contains("Verfügbare Platzhalter", section);
        Assert.Contains("AIS-/Patienten-Platzhalter", section);
        Assert.Contains("Geräte-/Messwert-Platzhalter", section);
        Assert.Contains("Ausgabe-an-Gerät-Platzhalter", section);
        Assert.Contains("ExampleValue", section);
        Assert.DoesNotContain("Leseansicht", section);
    }

    [Fact]
    public void XdtBaukastenTab_ShouldExposeTemplateLoadMessageAndEncodingSafeReader()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Contains("Baukasten-Template laden", xaml);
        Assert.Contains("XdtBaukastenTemplateLibraryService", code);
        Assert.Contains(".xdtbaukasten.template.json", code);
        Assert.Contains("LoadXdtBaukastenTemplate", code);
        Assert.Contains("XdtBaukastenTemplatePackageImportDialog", code);
        Assert.Contains("Template Paket wurde im Baukasten importiert", code);
        Assert.Contains("Baukasten-Konfiguration gespeichert", code);
        Assert.DoesNotContain("bisherigen Importbereich", code);
        Assert.Contains("XdtBaukastenTopStatusText", xaml);
        Assert.Contains("XdtBaukastenTextEncodingReader", code);
        Assert.Contains("XdtBaukastenPlaceholderValueService", code);
        Assert.Contains("RefreshXdtBaukastenPreviewIfPossible", code);
        Assert.Contains("Baukastenmodus: Modellabweichungen", xaml);
        Assert.Contains("compatibility.AllowsPreview", code);
    }

    [Fact]
    public void InterfaceProfilesTab_ShouldExposeFiltersAndAisOutputInfoWithoutLicenseCheckbox()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));
        var section = ExtractSection(
            xaml,
            "<TabItem Header=\"Schnittstellenprofile\">",
            "<TabItem Header=\"Sicherung/Umzug\">");

        Assert.Contains("InterfaceManufacturerFilterComboBox", section);
        Assert.Contains("InterfaceAisFilterComboBox", section);
        Assert.Contains("InterfaceProfileSelectionGrid", section);
        Assert.Contains("Gerätehersteller:", section);
        Assert.Contains("AIS-System:", section);
        Assert.Contains("Schnittstellenprofil:", section);
        Assert.Contains("x:Name=\"InterfaceProfileComboBox\"", section);
        Assert.Contains("MaxWidth=\"560\"", section);
        Assert.Contains("AIS Ausgabe Info", section);
        Assert.Contains("Aktive Schnittstellenprofile zählen immer als Geräteanbindung.", section);
        Assert.DoesNotContain("InterfaceIsLicenseRequiredCheckBox", section);
        Assert.DoesNotContain("Lizenzpflichtig", section);
        Assert.Contains("PopulateInterfaceProfileFilters", code);
        Assert.Contains("GetFilteredInterfaceProfiles", code);
        Assert.Contains("ShowAisOutputInfo_Click", code);
    }

    [Fact]
    public void InterfaceProfilesTab_ShouldKeepAttachment6305PathSeparateFromFolderButtons()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var section = ExtractSection(
            xaml,
            "x:Name=\"InterfaceAttachmentLinkPathTemplateLabel\"",
            "Header=\"Ordnerbereinigung\"");
        var pathTextBoxSection = ExtractSection(
            section,
            "<TextBox Grid.Row=\"0\"",
            "x:Name=\"InterfaceAttachmentFolderDefaultButton\"");
        var folderDefaultButtonSection = ExtractSection(
            section,
            "x:Name=\"InterfaceAttachmentFolderDefaultButton\"",
            "x:Name=\"InterfaceCreateAttachmentFoldersButton\"");
        var createFolderButtonSection = ExtractSection(
            section,
            "x:Name=\"InterfaceCreateAttachmentFoldersButton\"",
            "x:Name=\"InterfaceAttachmentFolderSetupStatusTextBlock\"");

        Assert.Contains("<Grid x:Name=\"InterfaceAttachmentFolderSetupPanel\"", section);
        Assert.Contains("<ColumnDefinition Width=\"*\"/>", section);
        Assert.Contains("<ColumnDefinition Width=\"Auto\"/>", section);
        Assert.Contains("Grid.Row=\"14\"", section);
        Assert.Contains("Grid.Column=\"1\"", section);
        Assert.Contains("Grid.ColumnSpan=\"2\"", section);
        Assert.Contains("Grid.Column=\"0\"", pathTextBoxSection);
        Assert.DoesNotContain("Grid.ColumnSpan", pathTextBoxSection);
        Assert.Contains("Grid.Column=\"1\"", folderDefaultButtonSection);
        Assert.Contains("MinWidth=\"150\"", folderDefaultButtonSection);
        Assert.Contains("Grid.Column=\"2\"", createFolderButtonSection);
        Assert.Contains("MinWidth=\"150\"", createFolderButtonSection);
        Assert.DoesNotContain("<WrapPanel x:Name=\"InterfaceAttachmentFolderSetupPanel\"", section);
        Assert.DoesNotContain("Grid.Row=\"15\"", section);
        Assert.DoesNotContain("Margin=\"-", section);
    }

    [Fact]
    public void AisOutputInfoDialog_ShouldExposeFieldTableAndCardHighlighting()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "AisOutputInfoWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "AisOutputInfoWindow.xaml.cs"));
        var service = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.Infrastructure", "AisOutputInfoService.cs"));

        Assert.Contains("AIS Ausgabe Info", xaml);
        Assert.Contains("Untersuchungsart:", xaml);
        Assert.DoesNotContain("8402 Default:", xaml);
        Assert.Contains("Feldkennung", xaml);
        Assert.Contains("Ausgabeart/Bedeutung", xaml);
        Assert.Contains("AIS-Relevanz", xaml);
        Assert.Contains("Karteikarte", xaml);
        Assert.Contains("Hinweis", xaml);
        Assert.Contains("IsCardField", xaml);
        Assert.Contains("IsOptional", xaml);
        Assert.Contains("💡", xaml);
        Assert.Contains("FontSize=\"14\"", xaml);
        Assert.Contains("#FFF7D8", xaml);
        Assert.Contains("AisOutputInfoViewModel", code);
        Assert.Contains("Standard Zeilenbenennung in MEDISTAR", xaml);
        Assert.Contains("StandardCardLineInfos", code);
        Assert.Contains("MedistarCardLineInfoService", service);
        Assert.Contains("V8", service);
        Assert.DoesNotContain("R.:S=", xaml + code + service);
        Assert.DoesNotContain("L.:S=", xaml + code + service);
        Assert.DoesNotContain("VKT=3.33", xaml + code + service);
        Assert.DoesNotContain("AL=24.16", xaml + code + service);
        Assert.DoesNotContain("SourcePath}", xaml + code);
        Assert.DoesNotContain("{value}", xaml + code);
    }

    [Fact]
    public void XdtBaukastenTab_ShouldNotDependOnOldProfileTemplatesControlNames()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var section = ExtractSection(
            xaml,
            "<TabItem Header=\"XDT-Baukasten\">",
            "<TabItem Header=\"Profilverwaltung\">");

        Assert.DoesNotContain("BuilderAisFilePathTextBox", section);
        Assert.DoesNotContain("BuilderDeviceFilePathTextBox", section);
        Assert.DoesNotContain("SerialTestRawTextBox", section);
        Assert.DoesNotContain("x:Name=\"ExportRulesGrid\"", section);
        Assert.Contains("XdtBaukastenExportRulesGrid", section);
    }

    [Fact]
    public void XdtBaukastenInitialization_ShouldNotDependOnRemovedProfileTemplatesTab()
    {
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));
        var dependentBody = ExtractMethodBody(
            code,
            "private void InitializeProfileDependentTabs",
            "private void ClearProfileManagementTabOnProfileLoadFailure");

        Assert.Contains("LoadProfileCatalogForUi", code);
        Assert.DoesNotContain("InitializeLegacyProfileTemplatesTab", code);
        Assert.DoesNotContain("ClearLegacyProfileTemplatesTabOnProfileLoadFailure", code);
        Assert.Contains("InitializeProfileDependentTabs", code);
        Assert.Contains("InitializeXdtBaukasten", dependentBody);
        Assert.Contains("InitializeProfileManagementTab", dependentBody);
        Assert.Contains("InitializeInterfaceProfileConfiguration", dependentBody);
        Assert.DoesNotContain("AisProfileCountText", dependentBody);
        Assert.DoesNotContain("TemplatePackageExportInterfaceProfileComboBox", dependentBody);
        Assert.DoesNotContain("BuilderAttachmentDiagnosticInterfaceProfileComboBox", dependentBody);
        Assert.DoesNotContain("ProfileMessagesTextBox", dependentBody);
    }

    [Fact]
    public void ProfileManagementTab_ShouldExposeCentralProfileMaintenanceSurface()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));
        var section = ExtractSection(
            xaml,
            "<TabItem Header=\"Profilverwaltung\">",
            "<TabItem Header=\"Schnittstellenprofile\">");

        Assert.Contains("ProfileManagementGrid", section);
        Assert.Contains("Profilverwaltung", section);
        Assert.Contains("Übersicht / Filter", section);
        Assert.Contains("Profile und lokale Templates", section);
        Assert.Contains("Details und Aktionen", section);
        Assert.Contains("Profile neu laden / BuiltIns reparieren", section);
        Assert.Contains("Umbenennen", section);
        Assert.Contains("Duplizieren", section);
        Assert.Contains("Löschen", section);
        Assert.Contains("Im XDT-Baukasten öffnen", section);
        Assert.Contains("Im Schnittstellenprofil öffnen", section);
        Assert.Contains("Neues AIS anlegen", section);
        Assert.DoesNotContain("Neues Gerät anlegen", section);
        Assert.Contains("Gerät laden / Bild pflegen", section);
        Assert.Contains("Neues Exportprofil aus Vorlage", section);
        Assert.Contains("Neues Schnittstellenprofil", section);
        Assert.Contains("Baukasten-Template erstellen", section);
        Assert.Contains("Templatepaket importieren", section);
        Assert.Contains("Ausgewähltes Template/Paket exportieren", section);
        Assert.Contains("XDT-Baukasten = Entwurf/Test/Vorschau", section);
        Assert.Contains("ProfileManagementService", code);
        Assert.Contains("InitializeProfileManagementTab", code);
        Assert.Contains("_profileManagementRows", code);
        Assert.Contains("IsProfileManagementUiReady", code);
        Assert.Contains("if (!IsProfileManagementUiReady)", code);
        Assert.True(File.Exists(FindWorkspaceFile("XdtDeviceBridge.App", "NewAisProfileDialog.xaml")));
        Assert.True(File.Exists(FindWorkspaceFile("XdtDeviceBridge.App", "NewDeviceProfileDialog.xaml")));
    }

    [Fact]
    public void ProfileManagementActions_ShouldSitAboveCompactDetailsArea()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var section = ExtractSection(
            xaml,
            "<TabItem Header=\"Profilverwaltung\">",
            "<TabItem Header=\"Schnittstellenprofile\">");

        var actionsIndex = section.IndexOf("Header=\"Aktionen\"", StringComparison.Ordinal);
        var detailsIndex = section.IndexOf("Header=\"Details und Aktionen\"", StringComparison.Ordinal);
        var detailsSection = ExtractSection(
            section,
            "Header=\"Details und Aktionen\"",
            "</GroupBox>");

        Assert.True(actionsIndex >= 0, "Profilverwaltung soll einen eigenen Aktionsbereich haben.");
        Assert.True(detailsIndex >= 0, "Profilverwaltung soll weiterhin den Details-Bereich anzeigen.");
        Assert.True(actionsIndex < detailsIndex, "Der Aktionsbereich soll rechts oberhalb der Details stehen.");
        Assert.Contains("Height=\"160\"", detailsSection);
        Assert.Contains("ProfileManagementDetailsTextBox", detailsSection);
        Assert.DoesNotContain("ProfileManagementNewAisButton", detailsSection);
        Assert.DoesNotContain("ProfileManagementDeleteButton", detailsSection);
    }

    [Fact]
    public void ProfileManagementTab_ShouldNotDependOnOldProfileTemplatesControls()
    {
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));
        var body = ExtractMethodBody(
            code,
            "private void InitializeProfileManagementTab",
            "private void ClearProfileManagementTabOnProfileLoadFailure");

        Assert.Contains("_profileManagementService", body);
        Assert.Contains("ProfileManagementGrid", code);
        Assert.DoesNotContain("AisProfileCountText", body);
        Assert.DoesNotContain("ExportProfileComboBox", body);
        Assert.DoesNotContain("TemplatePackageExportInterfaceProfileComboBox", body);
        Assert.DoesNotContain("BuilderAttachmentDiagnosticInterfaceProfileComboBox", body);
        Assert.DoesNotContain("ProfileMessagesTextBox", body);
    }

    [Fact]
    public void InterfaceProfilesInitialization_ShouldNotDependOnOldProfileTemplatesControls()
    {
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));
        var body = ExtractMethodBody(
            code,
            "private void InitializeInterfaceProfileConfiguration",
            "private void InterfaceProfileComboBox_SelectionChanged");

        Assert.Contains("InterfaceProfileComboBox", body);
        Assert.DoesNotContain("AisProfileCountText", body);
        Assert.DoesNotContain("ExportProfileComboBox", body);
        Assert.DoesNotContain("TemplatePackageExportInterfaceProfileComboBox", body);
        Assert.DoesNotContain("BuilderAttachmentDiagnosticInterfaceProfileComboBox", body);
        Assert.DoesNotContain("ProfileMessagesTextBox", body);
        Assert.DoesNotContain("UpdateProfileRenameActionButtons", body);
    }

    [Fact]
    public void XdtBaukastenSerialCaptureWindow_ShouldReturnRawDataToBaukasten()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "XdtBaukastenSerialCaptureWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "XdtBaukastenSerialCaptureWindow.xaml.cs"));
        var mainCode = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Contains("COM Port abhören", xaml);
        Assert.Contains("Übernehmen in Baukasten", xaml);
        Assert.Contains("ParityComboBox", xaml);
        Assert.Contains("StopBitsComboBox", xaml);
        Assert.Contains("HandshakeComboBox", xaml);
        Assert.Contains("DtrCheckBox", xaml);
        Assert.Contains("RtsCheckBox", xaml);
        Assert.Contains("Testkommando senden", xaml);
        Assert.Contains("allowWorkbenchAccept", code);
        Assert.Contains("Send_Click", code);
        Assert.Contains("_communicationService.WriteAsync", code);
        Assert.Contains("SerialDiagnosticsFormatter.ToHexDump", code);
        Assert.Contains("CapturedInput", code);
        Assert.Contains("SerialDeviceCommunicationService.ValidateSettings", code);
        Assert.Contains("_xdtBaukastenState.SetSerialInput(dialog.CapturedInput)", mainCode);
        Assert.Contains("XdtBaukastenAisRawTextBox.Text", mainCode);
    }

    [Fact]
    public void ProductiveNidekRtSerialWindow_ShouldExposeLiveDiagnosticsAndListenOnly()
    {
        var mainXaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var floatingXaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "FloatingInterfaceProfileWindow.xaml"));
        var floatingCode = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "FloatingInterfaceProfileWindow.xaml.cs"));
        var mainCode = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Contains("InterfaceSerialDtrCheckBox", mainXaml);
        Assert.Contains("InterfaceSerialRtsCheckBox", mainXaml);
        Assert.Contains("RS232-Diagnose öffnen", mainXaml);
        Assert.Contains("OpenInterfaceSerialDiagnostics_Click", mainCode);
        Assert.Contains("allowWorkbenchAccept: false", mainCode);
        Assert.Contains("NIDEK-RT Sendemodus", mainXaml);
        Assert.Contains("InterfaceNidekRtSerialSendModeComboBox", mainXaml);
        Assert.Contains("NIDEK-RT Sendeinhalt", mainXaml);
        Assert.Contains("InterfaceNidekRtSerialFrameVariantComboBox", mainXaml);
        Assert.Contains("LmOnlyWithoutAdd", mainXaml);
        Assert.Contains("ReferenceWithoutIdArAl", mainXaml);
        Assert.Contains("RT-Referenz ohne ID (AR/AL)", mainXaml);
        Assert.Contains("LegacyRt3100DirectFrame", mainXaml);
        Assert.Contains("RT-3100 Praxisvariante (getestet)", mainXaml);
        Assert.Contains("DirectWriterFrame", mainXaml);
        Assert.DoesNotContain("SerialTestDtrCheckBox", mainXaml);
        Assert.DoesNotContain("SerialTestRtsCheckBox", mainXaml);
        Assert.Contains("COM-Port nur abhören", floatingXaml);
        Assert.Contains("Sendetest", floatingXaml);
        Assert.Contains("RS anfordern", floatingXaml);
        Assert.Contains("Direkt Writer-Frame senden", floatingXaml);
        Assert.Contains("RS + Writer ohne SD-Warten", floatingXaml);
        Assert.Contains("Frame-Variante", floatingXaml);
        Assert.Contains("NidekRtFrameVariantComboBox", floatingXaml);
        Assert.Contains("ReferenceWithoutIdArAl", floatingXaml);
        Assert.Contains("LegacyRt3100DirectFrame", floatingXaml);
        Assert.Contains("CR nach EOT", floatingXaml);
        Assert.Contains("Serielle Diagnose", floatingXaml);
        Assert.Contains("SerialDiagnosticsText", floatingXaml);
        Assert.Contains("Rückgabe abhören und verarbeiten", floatingXaml);
        Assert.Contains("HasNidekRtSerialPendingReturn", floatingXaml);
        Assert.Contains("SerialListenOnlyRequested", floatingCode);
        Assert.Contains("SerialProcessReturnRequested", floatingCode);
        Assert.Contains("SerialDirectWriterRequested", floatingCode);
        Assert.Contains("SelectedNidekRtSerialOutputFrameVariant", floatingCode);
        Assert.Contains("AppendCarriageReturnAfterEot", floatingCode);
        Assert.Contains("RunNidekRtSerialListenOnlyAsync", mainCode);
        Assert.Contains("RunNidekRtSerialProcessReturnAsync", mainCode);
        Assert.Contains("RunNidekRtSerialSendTestAsync", mainCode);
        Assert.Contains("SendSelectionDirectWithoutReturnAsync", mainCode);
        Assert.Contains("Senden angefordert", mainCode);
        Assert.Contains("Writer-Frame wird erzeugt", mainCode);
        Assert.Contains("Writer-Hexdump", mainCode);
        Assert.Contains("COM-Port wird geöffnet", mainCode);
        Assert.Contains("Writer-Frame gesendet", mainCode);
        Assert.Contains("AppendNidekRtSerialDiagnostic", mainCode);
        Assert.Contains("NidekRtSerialSendModeInfo.Resolve", mainCode);
        Assert.Contains("NidekRtSerialOutputFrameVariantInfo.Resolve", mainCode);
        Assert.Contains("SendCompleted", mainCode);
        Assert.Contains("Nur-Abhören abgeschlossen", mainCode);
        Assert.Contains("Es wurde keine XDT-Ausgabe erzeugt", mainCode);
        Assert.Contains("Produktives Rückgabe-Abhören", mainCode);
        Assert.Contains("RS/SD", mainCode);
    }

    [Fact]
    public void OldProfileTemplatesTab_ShouldBeRemoved()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.DoesNotContain("Header=\"Profile &amp; Templates\"", xaml);
        Assert.DoesNotContain("Header=\"Templatepakete, neue Profile und RS232-Test\"", xaml);
        Assert.DoesNotContain("Header=\"Profilnamen\"", xaml);
        Assert.DoesNotContain("Header=\"Regelvorschau\"", xaml);
        Assert.DoesNotContain("ProfileMessagesTextBox", xaml);
        Assert.DoesNotContain("InitializeLegacyProfileTemplatesTab", code);
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
        var appProject = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "XdtDeviceBridge.App.csproj"));

        Assert.Contains("Converter={StaticResource DeviceImageSourceConverter}", mainWindow);
        Assert.Contains("Converter={StaticResource DeviceImageSourceConverter}", floatingWindow);
        Assert.Contains("BitmapCacheOption.OnLoad", converter);
        Assert.Contains("FileShare.ReadWrite | FileShare.Delete", converter);
        Assert.Contains("bitmap.Freeze()", converter);
        Assert.Contains(@"<Resource Include=""Assets\Devices\*.png"" />", appProject);
        Assert.True(File.Exists(FindWorkspaceFile("XdtDeviceBridge.App", Path.Combine("Assets", "Devices", "device-nidek-rt3100-serial-default.png"))));
        Assert.True(File.Exists(FindWorkspaceFile("XdtDeviceBridge.App", Path.Combine("Assets", "Devices", "device-document-attachment-default.png"))));
        Assert.True(File.Exists(FindWorkspaceFile("XdtDeviceBridge.App", Path.Combine("Assets", "Devices", "Topcon_CV5000_freigestellt.png"))));
    }

    [Fact]
    public void XdtBaukastenDeviceImagePlaceholder_ShouldOpenImageManagementFromWholeContainer()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));
        var imageContainer = ExtractSection(
            xaml,
            "x:Name=\"XdtBaukastenDeviceImageContainer\"",
            "<Grid Grid.Column=\"2\">");
        var imageElement = ExtractSection(
            imageContainer,
            "<Image x:Name=\"XdtBaukastenDeviceImage\"",
            "<TextBlock x:Name=\"XdtBaukastenDeviceImagePlaceholder\"");
        var clickHandler = ExtractMethodBody(
            code,
            "private void XdtBaukastenDeviceImage_MouseLeftButtonUp",
            "private void XdtBaukastenLoadAisOrSerial_Click");

        Assert.Contains("MouseLeftButtonUp=\"XdtBaukastenDeviceImage_MouseLeftButtonUp\"", imageContainer);
        Assert.Contains("Cursor=\"Hand\"", imageContainer);
        Assert.Contains("Gerätebild auswählen oder austauschen", imageContainer);
        Assert.Contains("Kein Gerätebild hinterlegt. Klicken, um ein Bild auszuwählen.", imageContainer);
        Assert.DoesNotContain("MouseLeftButtonUp=", imageElement);
        Assert.Contains("LoadDeviceProfileDialog(catalog.DeviceProfiles, paths, _deviceProfileImageOverrideService)", clickHandler);
        Assert.Contains("BuiltIn-Fachprofile wurden nicht überschrieben", clickHandler);
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
        Assert.Contains("# Bereiche der XDTBox", help);
        Assert.Contains("Verarbeitung: Produktiver Betrieb.", help);
        Assert.Contains("XDT-Baukasten: Arbeits- und Testbereich", help);
        Assert.Contains("Profilverwaltung: Zentrale Verwaltung", help);
        Assert.Contains("Schnittstellenprofile: Konkrete Praxisanbindung.", help);
        Assert.Contains("Sicherung/Umzug: Sichern und Wiederherstellen", help);
        Assert.Contains("Lizenz: Lizenzstatus", help);
        Assert.Contains("# RS232 NIDEK allgemein", help);
        Assert.Contains("# TOPCON CV-5000/CV-5000S", help);
        Assert.Contains("# Fehlerbehebung", help);
        Assert.Contains("Die Untersuchungsart 8402 kommt aus AIS", help);
        Assert.Contains("Es werden keine Patientendaten oder Messdateien gesichert", help);
        Assert.Contains("Die endgültige produktive Lizenzblockade ist in dieser Version nicht hart aktiviert", help);
        Assert.DoesNotContain("Profile & Templates", help);
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

    private static string ExtractSection(string text, string startMarker, string endMarker)
    {
        var start = text.IndexOf(startMarker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Start marker {startMarker} not found.");
        var end = text.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        Assert.True(end > start, $"End marker {endMarker} not found.");
        return text[start..end];
    }

    private static IEnumerable<string> ExtractResourceKeys(string xaml)
    {
        return System.Text.RegularExpressions.Regex
            .Matches(xaml, @"x:Key=""([^""]+)""")
            .Select(match => match.Groups[1].Value);
    }

    private static IEnumerable<string> ExtractReferencedResourceKeys(string xaml)
    {
        return System.Text.RegularExpressions.Regex
            .Matches(xaml, @"\{(?:StaticResource|DynamicResource)\s+([^},]+)")
            .Select(match => match.Groups[1].Value.Trim());
    }
}
