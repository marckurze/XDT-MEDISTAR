using System.Text;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class FileExportServiceTests
{
    [Fact]
    public void Export_ShouldWriteFileSuccessfully()
    {
        var service = new FileExportService();
        var folder = CreateTempFolder();

        var result = service.Export(folder, "ARK1S.XDT", "ABC", "UTF-8");

        Assert.False(result.HasErrors);
        Assert.NotNull(result.FilePath);
        Assert.True(File.Exists(result.FilePath));
        Assert.Equal("ABC", File.ReadAllText(result.FilePath));
    }

    [Fact]
    public void Export_ShouldCreateFolderIfNotExists()
    {
        var service = new FileExportService();
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        var result = service.Export(folder, "ARK1S.XDT", "ABC", "UTF-8");

        Assert.False(result.HasErrors);
        Assert.True(Directory.Exists(folder));
    }

    [Fact]
    public void Export_Windows1252_ShouldHandleUmlauts()
    {
        var service = new FileExportService();
        var folder = CreateTempFolder();

        var result = service.Export(folder, "ARK1S.XDT", "äöüß", "Windows-1252");

        Assert.False(result.HasErrors);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var text = File.ReadAllText(result.FilePath!, Encoding.GetEncoding(1252));
        Assert.Equal("äöüß", text);
    }

    [Fact]
    public void Export_Utf8_ShouldWork()
    {
        var service = new FileExportService();
        var folder = CreateTempFolder();

        var result = service.Export(folder, "ARK1S.XDT", "äöüß", "UTF-8");

        Assert.False(result.HasErrors);
        var text = File.ReadAllText(result.FilePath!, new UTF8Encoding(false));
        Assert.Equal("äöüß", text);
    }

    [Fact]
    public void Export_EmptyFolder_ShouldCreateError()
    {
        var service = new FileExportService();
        var result = service.Export("", "ARK1S.XDT", "ABC", "UTF-8");

        Assert.True(result.HasErrors);
    }

    [Fact]
    public void Export_EmptyFileName_ShouldCreateError()
    {
        var service = new FileExportService();
        var result = service.Export(CreateTempFolder(), "", "ABC", "UTF-8");

        Assert.True(result.HasErrors);
    }

    [Fact]
    public void Export_EmptyContent_ShouldCreateError()
    {
        var service = new FileExportService();
        var result = service.Export(CreateTempFolder(), "ARK1S.XDT", "", "UTF-8");

        Assert.True(result.HasErrors);
    }

    [Fact]
    public void Export_ExistingFile_ShouldUseSuffix()
    {
        var service = new FileExportService();
        var folder = CreateTempFolder();
        var first = service.Export(folder, "ARK1S.XDT", "ONE", "UTF-8");
        var second = service.Export(folder, "ARK1S.XDT", "TWO", "UTF-8");

        Assert.NotNull(first.FilePath);
        Assert.NotNull(second.FilePath);
        Assert.NotEqual(first.FilePath, second.FilePath);
        Assert.EndsWith("_1.XDT", second.FilePath, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
