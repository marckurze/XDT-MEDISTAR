using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TechnicianWhiteboardServiceTests
{
    private readonly TechnicianWhiteboardService _service = new();
    private readonly AppDataPathProvider _pathProvider = new();

    [Fact]
    public void SaveAndLoad_ShouldPreserveTextAndImageItems()
    {
        using var temp = new TempFolder();
        var paths = _pathProvider.GetPaths(Path.Combine(temp.Path, "data"));
        var state = new TechnicianWhiteboardState(
            new[]
            {
                new TechnicianWhiteboardTextItem("text-1", "Techniker-Notiz", 12, 34, 220, 80, true, false, true, "#0B4D93", 22)
            },
            new[]
            {
                new TechnicianWhiteboardImageItem("image-1", Path.Combine(temp.Path, "bild.png"), 40, 50, 120, 90)
            });

        _service.Save(paths, state);
        var loaded = _service.LoadOrEmpty(paths);

        var text = Assert.Single(loaded.TextItems);
        Assert.Equal("Techniker-Notiz", text.Text);
        Assert.True(text.IsBold);
        Assert.True(text.IsUnderline);
        Assert.Equal(22, text.FontSize);
        var image = Assert.Single(loaded.ImageItems);
        Assert.Equal(120, image.Width);
        Assert.Equal(90, image.Height);
    }

    [Fact]
    public void CopyImageIntoWhiteboard_ShouldCopyImageIntoCustomerDataFolder()
    {
        using var temp = new TempFolder();
        var paths = _pathProvider.GetPaths(Path.Combine(temp.Path, "data"));
        var sourceImage = Path.Combine(temp.Path, "source.png");
        File.WriteAllBytes(sourceImage, new byte[] { 1, 2, 3, 4 });

        var copied = _service.CopyImageIntoWhiteboard(paths, sourceImage);

        Assert.True(File.Exists(copied));
        Assert.StartsWith(_service.GetImagesFolder(paths), copied, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, File.ReadAllBytes(copied));
    }

    private sealed class TempFolder : IDisposable
    {
        public TempFolder()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"xdtbox-whiteboard-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
