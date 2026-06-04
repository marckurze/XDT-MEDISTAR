namespace XdtDeviceBridge.Tests;

public sealed class SerialCommunicationUiSourceTests
{
    [Fact]
    public void NewDeviceProfileDialog_ShouldExposeConnectionKindSelection()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "NewDeviceProfileDialog.xaml"));

        Assert.Contains("Anbindung / Datenquelle:", xaml);
        Assert.Contains("LAN / Datei über Eingangsordner", xaml);
        Assert.Contains("Seriell RS232 / COM-Port", xaml);
    }

    [Fact]
    public void MainWindow_ShouldExposeSerialCommunicationEditorAndSeparateDiagnosticsEntryPoint()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));

        Assert.DoesNotContain("RS232 / COM-Port testen", xaml);
        Assert.DoesNotContain("x:Name=\"SerialTestPortComboBox\"", xaml);
        Assert.DoesNotContain("x:Name=\"SerialTestProtocolComboBox\"", xaml);
        Assert.DoesNotContain("x:Name=\"SerialTestNidekModeComboBox\"", xaml);
        Assert.DoesNotContain("x:Name=\"SerialTestNidekAnalysisTextBox\"", xaml);
        Assert.Contains("x:Name=\"InterfaceSerialCommunicationGroupBox\"", xaml);
        Assert.Contains("Serielle Gerätekommunikation / COM-Port", xaml);
        Assert.Contains("x:Name=\"InterfaceSerialPortComboBox\"", xaml);
        Assert.Contains("x:Name=\"InterfaceSerialDtrCheckBox\"", xaml);
        Assert.Contains("x:Name=\"InterfaceSerialRtsCheckBox\"", xaml);
        Assert.Contains("x:Name=\"OpenInterfaceSerialDiagnosticsButton\"", xaml);
        Assert.Contains("RS232-Diagnose öffnen", xaml);
        Assert.Contains("x:Name=\"InterfaceDeviceImportFolderTextBox\"", xaml);
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
