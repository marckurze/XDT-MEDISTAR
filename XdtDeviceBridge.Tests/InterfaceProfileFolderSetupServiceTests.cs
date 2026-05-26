using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileFolderSetupServiceTests : IDisposable
{
    private readonly InterfaceProfileFolderSetupService _service = new();
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeFolderSetupTests", Guid.NewGuid().ToString("N"));

    [Theory]
    [InlineData("CV-5000 / CV-5000S", "CV5000")]
    [InlineData("CT-800A", "CT800A")]
    [InlineData("KR-1", "KR1")]
    [InlineData("ARK1S", "ARK1S")]
    [InlineData("SOLOS", "SOLOS")]
    [InlineData("Bad<Name>:42", "BadName42")]
    public void CreateDefaultFolderDeviceName_ShouldReturnShortPathSafeName(string product, string expected)
    {
        var profile = CreateDeviceProfile(product);

        var deviceName = _service.CreateDefaultFolderDeviceName(profile);

        Assert.Equal(expected, deviceName);
    }

    [Fact]
    public void CreateMainDefaultFolders_ShouldUseXdtBoxDeviceFolderStructure()
    {
        var defaults = _service.CreateMainDefaultFolders(
            DefaultDeviceProfileDefinitions.CreateTopconCv5000Default());

        Assert.Equal(@"C:\XDTBox\CV5000\Patient2Box", defaults.AisImportFolder);
        Assert.Equal(@"C:\XDTBox\CV5000\Device2Box", defaults.DeviceImportFolder);
        Assert.Equal(@"C:\XDTBox\CV5000\Box2AIS", defaults.ExportFolder);
        Assert.Equal(@"C:\XDTBox\CV5000\Archiv", defaults.ArchiveFolder);
        Assert.Equal(@"C:\XDTBox\CV5000\Fehler", defaults.ErrorFolder);
    }

    [Fact]
    public void CreateAttachmentDefaultFolders_ShouldUseXdtAttachmentFolderStructure()
    {
        var defaults = _service.CreateAttachmentDefaultFolders(
            DefaultDeviceProfileDefinitions.CreateTopconCt800ADefault());

        Assert.Equal(@"C:\XDTBox\CT800A\XDTAnhang\Device2Box", defaults.AttachmentImportFolder);
        Assert.Equal(@"C:\XDTBox\CT800A\XDTAnhang\Attachment", defaults.AttachmentExportFolder);
    }

    [Fact]
    public void CreateMainDefaultFolders_ShouldNotCreateDirectories()
    {
        var defaults = _service.CreateMainDefaultFolders(
            DefaultDeviceProfileDefinitions.CreateTopconKr1Default(),
            _tempRoot);

        Assert.False(Directory.Exists(_tempRoot));
        Assert.False(Directory.Exists(defaults.AisImportFolder));
        Assert.False(Directory.Exists(defaults.DeviceImportFolder));
        Assert.False(Directory.Exists(defaults.ExportFolder));
        Assert.False(Directory.Exists(defaults.ArchiveFolder));
        Assert.False(Directory.Exists(defaults.ErrorFolder));
    }

    [Fact]
    public void CreateDirectories_ShouldCreateMissingFoldersAndAcceptExistingFolders()
    {
        var existingFolder = Path.Combine(_tempRoot, "Existing");
        var newFolder = Path.Combine(_tempRoot, "New");
        Directory.CreateDirectory(existingFolder);

        var result = _service.CreateDirectories(new[]
        {
            new InterfaceProfileFolderCreationRequest("Archiv", existingFolder),
            new InterfaceProfileFolderCreationRequest("Fehler", newFolder)
        });

        Assert.True(result.Success);
        Assert.True(Directory.Exists(existingFolder));
        Assert.True(Directory.Exists(newFolder));
        Assert.Contains(result.Entries, entry => entry.Label == "Archiv" && entry.Success && entry.AlreadyExisted);
        Assert.Contains(result.Entries, entry => entry.Label == "Fehler" && entry.Success && !entry.AlreadyExisted);
    }

    [Fact]
    public void CreateDirectories_ShouldReportMissingAndInvalidPaths()
    {
        var result = _service.CreateDirectories(new[]
        {
            new InterfaceProfileFolderCreationRequest("AIS-Patienten Datei an XDTBox", " "),
            new InterfaceProfileFolderCreationRequest("Gerätedatei an XDTBox", "bad\0path")
        });

        Assert.False(result.Success);
        Assert.All(result.Entries, entry => Assert.False(entry.Success));
        Assert.Contains(result.Entries, entry =>
            entry.Label == "AIS-Patienten Datei an XDTBox"
            && entry.ErrorMessage == "Pfad fehlt.");
        Assert.Contains(result.Entries, entry =>
            entry.Label == "Gerätedatei an XDTBox"
            && entry.ErrorMessage == "Pfad enthält ungültige Zeichen.");
    }

    [Fact]
    public void MainWindowXaml_ShouldContainFolderSetupButtons()
    {
        var xaml = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.App", "MainWindow.xaml"));

        Assert.Contains("x:Name=\"InterfaceFolderDefaultButton\"", xaml);
        Assert.Contains("x:Name=\"InterfaceCreateFoldersButton\"", xaml);
        Assert.Contains("x:Name=\"InterfaceAttachmentFolderDefaultButton\"", xaml);
        Assert.Contains("x:Name=\"InterfaceCreateAttachmentFoldersButton\"", xaml);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    private static DeviceProfileDefinition CreateDeviceProfile(string product)
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault();

        return profile with
        {
            Metadata = profile.Metadata with
            {
                Product = product,
                Name = product
            },
            Model = product
        };
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
