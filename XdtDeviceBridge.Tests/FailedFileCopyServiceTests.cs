using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class FailedFileCopyServiceTests
{
    private static readonly DateTime FailedAtUtc = new(2026, 5, 3, 14, 15, 0, DateTimeKind.Utc);

    private readonly FailedFileCopyService _service = new();

    [Fact]
    public void CopyFailedFiles_ShouldThrowArgumentExceptionForEmptyErrorFolder()
    {
        var files = CreateSourceFiles();

        Assert.Throws<ArgumentException>(() => _service.CopyFailedFiles(
            string.Empty,
            "MEDISTAR NIDEK ARK1S",
            files.AisFilePath,
            files.DeviceFilePath,
            FailedAtUtc,
            "Testfehler"));
    }

    [Fact]
    public void CopyFailedFiles_ShouldCopyFilesToExpectedErrorStructure()
    {
        var errorFolder = CreateTempFolder();
        var files = CreateSourceFiles();

        var result = _service.CopyFailedFiles(
            errorFolder,
            "MEDISTAR + NIDEK ARK1S",
            files.AisFilePath,
            files.DeviceFilePath,
            FailedAtUtc,
            "Testfehler");

        Assert.False(result.HasErrors);
        Assert.Contains(Path.Combine(errorFolder, "2026", "05", "03", "MEDISTAR_NIDEK_ARK1S", "AIS", "TestPatient.gdt"), result.CopiedFiles);
        Assert.Contains(Path.Combine(errorFolder, "2026", "05", "03", "MEDISTAR_NIDEK_ARK1S", "Device", "ARK1S.xml"), result.CopiedFiles);
        Assert.Contains(result.CopiedFiles, filePath => filePath.EndsWith("error.txt", StringComparison.OrdinalIgnoreCase));
        Assert.All(result.CopiedFiles, filePath => Assert.True(File.Exists(filePath)));
    }

    [Fact]
    public void CopyFailedFiles_ShouldCreateErrorTxtWithFailureReason()
    {
        var errorFolder = CreateTempFolder();
        var files = CreateSourceFiles();

        var result = _service.CopyFailedFiles(
            errorFolder,
            "Profil",
            files.AisFilePath,
            files.DeviceFilePath,
            FailedAtUtc,
            "Dieser Dateityp wird noch nicht unterstützt.");

        var errorFile = Assert.Single(result.CopiedFiles, filePath => Path.GetFileName(filePath) == "error.txt");
        var content = File.ReadAllText(errorFile);
        Assert.Contains("Dieser Dateityp wird noch nicht unterstützt.", content);
        Assert.Contains("Schnittstellenprofil: Profil", content);
        Assert.Contains(files.AisFilePath, content);
        Assert.Contains(files.DeviceFilePath, content);
    }

    [Fact]
    public void CopyFailedFiles_ShouldCreateTargetFolders()
    {
        var errorFolder = Path.Combine(CreateTempFolder(), "error", "nested");
        var files = CreateSourceFiles();

        var result = _service.CopyFailedFiles(
            errorFolder,
            "Profil",
            files.AisFilePath,
            files.DeviceFilePath,
            FailedAtUtc,
            "Testfehler");

        Assert.False(result.HasErrors);
        Assert.True(Directory.Exists(Path.Combine(errorFolder, "2026", "05", "03", "Profil", "AIS")));
        Assert.True(Directory.Exists(Path.Combine(errorFolder, "2026", "05", "03", "Profil", "Device")));
    }

    [Fact]
    public void CopyFailedFiles_ShouldUseSuffixWhenTargetFileExists()
    {
        var errorFolder = CreateTempFolder();
        var files = CreateSourceFiles();

        _service.CopyFailedFiles(errorFolder, "Profil", files.AisFilePath, files.DeviceFilePath, FailedAtUtc, "Erster Fehler");
        var second = _service.CopyFailedFiles(errorFolder, "Profil", files.AisFilePath, files.DeviceFilePath, FailedAtUtc, "Zweiter Fehler");

        Assert.False(second.HasErrors);
        Assert.Contains(second.CopiedFiles, filePath => filePath.EndsWith("TestPatient_1.gdt", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(second.CopiedFiles, filePath => filePath.EndsWith("ARK1S_1.xml", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(second.CopiedFiles, filePath => filePath.EndsWith("error_1.txt", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CopyFailedFiles_ShouldKeepOriginalFiles()
    {
        var errorFolder = CreateTempFolder();
        var files = CreateSourceFiles();

        var result = _service.CopyFailedFiles(
            errorFolder,
            "Profil",
            files.AisFilePath,
            files.DeviceFilePath,
            FailedAtUtc,
            "Testfehler");

        Assert.False(result.HasErrors);
        Assert.True(File.Exists(files.AisFilePath));
        Assert.True(File.Exists(files.DeviceFilePath));
    }

    [Fact]
    public void CopyFailedFiles_ShouldReturnErrorForMissingSourceFile()
    {
        var errorFolder = CreateTempFolder();
        var files = CreateSourceFiles();
        File.Delete(files.DeviceFilePath);

        var result = _service.CopyFailedFiles(
            errorFolder,
            "Profil",
            files.AisFilePath,
            files.DeviceFilePath,
            FailedAtUtc,
            "Testfehler");

        Assert.True(result.HasErrors);
        Assert.Contains(result.Issues, issue => issue.Contains("Quelldatei fehlt", StringComparison.Ordinal));
        Assert.Contains(result.CopiedFiles, filePath => Path.GetFileName(filePath) == "TestPatient.gdt");
        Assert.Contains(result.CopiedFiles, filePath => Path.GetFileName(filePath) == "error.txt");
    }

    private static SourceFiles CreateSourceFiles()
    {
        var folder = CreateTempFolder();
        var aisFilePath = Path.Combine(folder, "TestPatient.gdt");
        var deviceFilePath = Path.Combine(folder, "ARK1S.xml");
        File.WriteAllText(aisFilePath, "gdt");
        File.WriteAllText(deviceFilePath, "xml");
        return new SourceFiles(aisFilePath, deviceFilePath);
    }

    private static string CreateTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed record SourceFiles(string AisFilePath, string DeviceFilePath);
}
