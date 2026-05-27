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
