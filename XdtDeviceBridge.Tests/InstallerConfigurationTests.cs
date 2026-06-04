namespace XdtDeviceBridge.Tests;

public sealed class InstallerConfigurationTests
{
    [Fact]
    public void VersionMetadata_ShouldBeXdtBoxVersion10()
    {
        var props = File.ReadAllText(FindWorkspaceFile("Directory.Build.props"));
        var version = File.ReadAllText(FindWorkspaceFile("VERSION")).Trim();
        var appCode = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Equal("1.0", version);
        Assert.Contains("<Version>1.0</Version>", props);
        Assert.Contains("<AssemblyVersion>1.0.0.0</AssemblyVersion>", props);
        Assert.Contains("<FileVersion>1.0.0.0</FileVersion>", props);
        Assert.Contains("<InformationalVersion>1.0</InformationalVersion>", props);
        Assert.Contains("<Product>XDTBox</Product>", props);
        Assert.Contains("<Company>Technik-Apparat M.Kurze</Company>", props);
        Assert.Contains("GetApplicationVersionText()", appCode);
        Assert.DoesNotContain("\"0.1.0\"", appCode);
    }

    [Fact]
    public void InstallerScript_ShouldDefineProfessionalCustomerSetup()
    {
        var installer = File.ReadAllText(FindWorkspaceFile("installer", "XDTBox.iss"));

        Assert.Contains("AppName={#MyAppName}", installer);
        Assert.Contains("#define MyAppVersion \"1.0\"", installer);
        Assert.Contains("AppVersion={#MyAppVersion}", installer);
        Assert.Contains("DefaultDirName=C:\\XDTBox", installer);
        Assert.Contains("OutputBaseFilename=XDTBox_Setup_1.0", installer);
        Assert.Contains("SetupIconFile=..\\XdtDeviceBridge.App\\Assets\\App\\XDTBox.ico", installer);
        Assert.Contains("UninstallDisplayIcon={app}\\{#MyAppExeName}", installer);
        Assert.Contains("PrivilegesRequired=admin", installer);
        Assert.Contains("MinVersion=10.0", installer);
        Assert.Contains("Name: \"{group}\\XDTBox\"", installer);
        Assert.Contains("Name: \"{autodesktop}\\XDTBox\"", installer);
    }

    [Fact]
    public void InstallerScript_ShouldSupportNewInstallUpdateDetectionAndMarker()
    {
        var installer = File.ReadAllText(FindWorkspaceFile("installer", "XDTBox.iss"));

        Assert.Contains("Neuinstallation", installer);
        Assert.Contains("Bestehende XDTBox aktualisieren", installer);
        Assert.Contains("TryDetectInstallDir", installer);
        Assert.Contains("Software\\XDTBox", installer);
        Assert.Contains("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall", installer);
        Assert.Contains("XdtDeviceBridge.App.exe", installer);
        Assert.Contains("XDTBox.installation.json", installer);
        Assert.Contains("InstallLocation", installer);
        Assert.Contains("MarkerFile", installer);
        Assert.Contains("Bitte erstellen Sie vor dem Update eine Sicherung unter Sicherung/Umzug", installer);
        Assert.Contains("Ein Update wird nicht blind in einen unplausiblen Ordner geschrieben", installer);
    }

    [Fact]
    public void InstallerScript_ShouldKeepCustomerDataByDefaultAndExcludeManufacturerTools()
    {
        var installer = File.ReadAllText(FindWorkspaceFile("installer", "XDTBox.iss"));

        Assert.Contains("XdtBox.LicenseManager.exe", installer);
        Assert.Contains("XdtBox.LicenseIssuer.exe", installer);
        Assert.Contains("*.pem", installer);
        Assert.Contains("*.key", installer);
        Assert.Contains("license-history.json", installer);
        Assert.Contains("license-manager-settings.json", installer);
        Assert.Contains("Kundendaten bleiben standardmaessig erhalten", installer);
        Assert.Contains("Letzte Bestaetigung", installer);
        Assert.Contains(@"{localappdata}\XdtDeviceBridge", installer);
        Assert.Contains("Externe AIS-/Geraete-/Archiv-/Fehlerordner und Praxisordner werden nicht geloescht", installer);
        Assert.Contains("DelTree(CustomerDataDir", installer);
    }

    [Fact]
    public void InstallerBuildScript_ShouldPublishOnlyCustomerAppAndGuardForbiddenFiles()
    {
        var script = File.ReadAllText(FindWorkspaceFile("scripts", "build-xdtbox-installer.ps1"));

        Assert.Contains("XdtDeviceBridge.App\\XdtDeviceBridge.App.csproj", script);
        Assert.Contains("dotnet publish", script);
        Assert.Contains("--self-contained true", script);
        Assert.Contains("artifacts\\publish\\XDTBox", script);
        Assert.Contains("artifacts\\installer", script);
        Assert.Contains("XDTBox_Setup_1.0.exe", script);
        Assert.Contains("XdtBox.LicenseManager*", script);
        Assert.Contains("XdtBox.LicenseIssuer*", script);
        Assert.Contains("*.pem", script);
        Assert.Contains("*.key", script);
        Assert.Contains("ISCC_EXE", script);
        Assert.Contains("Inno Setup 6 Compiler wurde nicht gefunden", script);
    }

    [Fact]
    public void InstallerDocumentation_ShouldDescribeVersion10AndVariantB()
    {
        var readme = File.ReadAllText(FindWorkspaceFile("README.md"));
        var policy = File.ReadAllText(FindWorkspaceFile("docs", "INSTALLATION_UPDATE_DATENPOLITIK.md"));
        var buildGuide = File.ReadAllText(FindWorkspaceFile("docs", "INSTALLER_BUILD_ANLEITUNG.md"));
        var help = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "Assets", "Help", "xdtbox-help.md"));

        Assert.Contains("`1.0`", readme);
        Assert.Contains("docs/INSTALLER_BUILD_ANLEITUNG.md", readme);
        Assert.Contains("XDTBox 1.0", policy);
        Assert.Contains("installer/XDTBox.iss", policy);
        Assert.Contains("XDTBox_Setup_1.0.exe", buildGuide);
        Assert.Contains("C:\\XDTBox", buildGuide);
        Assert.Contains("Inno Setup 6", buildGuide);
        Assert.Contains("self-contained", buildGuide);
        Assert.Contains("Deinstallation Variante B", buildGuide);
        Assert.Contains("XdtBox.LicenseManager", buildGuide);
        Assert.Contains("private Hersteller-Schluessel", buildGuide);
        Assert.Contains("# Installation, Update und Deinstallation", help);
        Assert.Contains("Der Deinstaller entfernt standardmäßig nur die App", help);
    }

    private static string FindWorkspaceFile(params string[] relativeSegments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(relativeSegments).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Workspace file not found: {Path.Combine(relativeSegments)}");
    }
}
