namespace XdtDeviceBridge.Tests;

public sealed class AppIconSourceTests
{
    [Fact]
    public void AppIconFile_ShouldExistInRepositoryAssetFolder()
    {
        var iconPath = FindWorkspaceFile("XdtDeviceBridge.App", Path.Combine("Assets", "App", "XDTBox.ico"));

        Assert.True(new FileInfo(iconPath).Length > 0);
    }

    [Fact]
    public void CustomerAppProject_ShouldUseXdtBoxApplicationIcon()
    {
        var project = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "XdtDeviceBridge.App.csproj"));
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));

        Assert.Contains(@"<ApplicationIcon>Assets\App\XDTBox.ico</ApplicationIcon>", project);
        Assert.Contains(@"<Resource Include=""Assets\App\XDTBox.ico"" />", project);
        Assert.Contains(@"Icon=""Assets/App/XDTBox.ico""", xaml);
    }

    [Fact]
    public void CustomerAppNotifyIcon_ShouldUseXdtBoxIconResource()
    {
        var code = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml.cs"));

        Assert.Contains(@"private const string AppIconResourcePath = ""Assets/App/XDTBox.ico"";", code);
        Assert.Contains("Icon = LoadNotifyIcon()", code);
        Assert.Contains("System.Windows.Application.GetResourceStream(new Uri(AppIconResourcePath, UriKind.Relative))", code);
    }

    [Fact]
    public void LicenseManagerProjectAndWindow_ShouldUseXdtBoxIcon()
    {
        var project = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "XdtBox.LicenseManager.csproj"));
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml"));

        Assert.Contains(@"<ApplicationIcon>..\XdtDeviceBridge.App\Assets\App\XDTBox.ico</ApplicationIcon>", project);
        Assert.Contains(@"<Resource Include=""..\XdtDeviceBridge.App\Assets\App\XDTBox.ico"" Link=""Assets\App\XDTBox.ico"" />", project);
        Assert.Contains(@"Icon=""Assets/App/XDTBox.ico""", xaml);
    }

    [Fact]
    public void LicenseIssuerProject_ShouldUseXdtBoxApplicationIcon()
    {
        var project = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseIssuer", "XdtBox.LicenseIssuer.csproj"));

        Assert.Contains(@"<ApplicationIcon>..\XdtDeviceBridge.App\Assets\App\XDTBox.ico</ApplicationIcon>", project);
    }

    [Fact]
    public void BrandingLogoAndTheme_ShouldExistInRepository()
    {
        var logoPath = FindWorkspaceFile("XdtDeviceBridge.App", Path.Combine("Assets", "Branding", "XDTBox_Logo_Schriftzug.png"));
        var themePath = FindWorkspaceFile("XdtDeviceBridge.App", Path.Combine("Styles", "XdtBoxTheme.xaml"));

        Assert.True(new FileInfo(logoPath).Length > 0);

        var theme = File.ReadAllText(themePath);
        Assert.Contains("XdtBoxPrimaryBrush", theme);
        Assert.Contains("XdtBoxAccentBrush", theme);
        Assert.Contains("XdtBoxDarkTextBrush", theme);
        Assert.Contains("XdtBoxPanelBackgroundBrush", theme);
        Assert.Contains("XdtBoxBorderBrush", theme);
    }

    [Fact]
    public void CustomerAppWindow_ShouldReferenceBrandingHeaderAndTheme()
    {
        var project = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "XdtDeviceBridge.App.csproj"));
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));

        Assert.Contains(@"<Resource Include=""Assets\Branding\XDTBox_Logo_Schriftzug.png"" />", project);
        Assert.Contains(@"Source=""Styles/XdtBoxTheme.xaml""", xaml);
        Assert.Contains(@"Source=""Assets/Branding/XDTBox_Logo_Schriftzug.png""", xaml);
        Assert.Contains("XdtBoxBrandHeaderStyle", xaml);
        Assert.Contains(@"Title=""XDTBox""", xaml);
    }

    [Fact]
    public void LicenseManagerWindow_ShouldReferenceBrandingHeaderAndTheme()
    {
        var project = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "XdtBox.LicenseManager.csproj"));
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseManager", "MainWindow.xaml"));

        Assert.Contains(@"<Resource Include=""..\XdtDeviceBridge.App\Assets\Branding\XDTBox_Logo_Schriftzug.png"" Link=""Assets\Branding\XDTBox_Logo_Schriftzug.png"" />", project);
        Assert.Contains(@"<Page Include=""..\XdtDeviceBridge.App\Styles\XdtBoxTheme.xaml"" Link=""Styles\XdtBoxTheme.xaml"" />", project);
        Assert.Contains(@"Source=""Styles/XdtBoxTheme.xaml""", xaml);
        Assert.Contains(@"Source=""Assets/Branding/XDTBox_Logo_Schriftzug.png""", xaml);
        Assert.Contains("XdtBoxBrandHeaderStyle", xaml);
    }

    [Fact]
    public void FloatingDeviceWindow_ShouldNotReceiveBrandingHeaderLayout()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "FloatingInterfaceProfileWindow.xaml"));

        Assert.DoesNotContain("XDTBox_Logo_Schriftzug.png", xaml);
        Assert.DoesNotContain("XdtBoxBrandHeaderStyle", xaml);
        Assert.DoesNotContain(@"Source=""Styles/XdtBoxTheme.xaml""", xaml);
    }

    private static string FindWorkspaceFile(string projectFolder, string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, projectFolder, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Workspace file not found: {projectFolder}/{relativePath}");
    }
}
