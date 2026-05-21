using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class ManualDocumentSelectionServiceTests
{
    private readonly ManualDocumentSelectionService _service = new();

    [Fact]
    public void AddFiles_ShouldAcceptSupportedFilesInGivenOrder()
    {
        var folder = CreateTempFolder();
        var pdf = CreateFile(folder, "befund.pdf", "pdf");
        var jpg = CreateFile(folder, "bild.jpg", "jpg");

        var result = _service.AddFiles(new[] { pdf, jpg });

        Assert.Empty(result.RejectedMessages);
        Assert.Collection(
            result.AcceptedFiles,
            file =>
            {
                Assert.Equal("befund.pdf", file.FileName);
                Assert.Equal(".pdf", file.Extension);
                Assert.True(file.IsStable);
            },
            file =>
            {
                Assert.Equal("bild.jpg", file.FileName);
                Assert.Equal(".jpg", file.Extension);
                Assert.True(file.IsSupported);
            });
    }

    [Fact]
    public void AddFiles_ShouldRejectUnsupportedFilesAndFolders()
    {
        var folder = CreateTempFolder();
        var unsupported = CreateFile(folder, "start.exe", "bin");
        var subFolder = Path.Combine(folder, "unterordner");
        Directory.CreateDirectory(subFolder);

        var result = _service.AddFiles(new[] { unsupported, subFolder });

        Assert.Empty(result.AcceptedFiles);
        Assert.Contains(result.RejectedMessages, message => message.Contains("Dateityp wird", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.RejectedMessages, message => message.Contains("Ordner werden nicht übernommen", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AddFiles_ShouldNotAddDuplicateFileTwice()
    {
        var folder = CreateTempFolder();
        var pdf = CreateFile(folder, "befund.pdf", "pdf");

        var result = _service.AddFiles(new[] { pdf, pdf });

        Assert.Single(result.AcceptedFiles);
        Assert.Contains(result.RejectedMessages, message => message.Contains("bereits in der Liste", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AddFiles_ShouldRespectExistingCandidatesForDuplicateCheck()
    {
        var folder = CreateTempFolder();
        var pdf = CreateFile(folder, "befund.pdf", "pdf");
        var existing = Assert.Single(_service.AddFiles(new[] { pdf }).AcceptedFiles);

        var result = _service.AddFiles(new[] { pdf }, new[] { existing });

        Assert.Empty(result.AcceptedFiles);
        Assert.Contains(result.RejectedMessages, message => message.Contains("bereits in der Liste", StringComparison.OrdinalIgnoreCase));
    }

    private static string CreateFile(string folder, string fileName, string content)
    {
        var path = Path.Combine(folder, fileName);
        File.WriteAllText(path, content);
        return path;
    }

    private static string CreateTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
