using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class AttachmentTransferServiceTests
{
    private readonly AttachmentTransferService _service = new();

    [Fact]
    public void Transfer_CopyShouldCopyFileToTargetFolder()
    {
        var sourceFile = CreateSourceFile("source.pdf", "attachment");
        var targetFolder = CreateTempFolder();

        var result = _service.Transfer(sourceFile, targetFolder, "target.PDF", AttachmentTransferMode.Copy);

        Assert.True(result.Success);
        Assert.True(result.WasCopied);
        Assert.False(result.WasMoved);
        Assert.Equal("target.PDF", result.FileName);
        Assert.Equal(Path.Combine(targetFolder, "target.PDF"), result.TargetPath);
        Assert.True(File.Exists(result.TargetPath));
        Assert.Equal("attachment", File.ReadAllText(result.TargetPath));
    }

    [Fact]
    public void Transfer_CopyShouldKeepSourceFile()
    {
        var sourceFile = CreateSourceFile("source.pdf", "attachment");
        var targetFolder = CreateTempFolder();

        var result = _service.Transfer(sourceFile, targetFolder, "target.PDF", AttachmentTransferMode.Copy);

        Assert.True(result.Success);
        Assert.True(File.Exists(sourceFile));
    }

    [Fact]
    public void Transfer_MoveShouldMoveFileToTargetFolder()
    {
        var sourceFile = CreateSourceFile("source.pdf", "attachment");
        var targetFolder = CreateTempFolder();

        var result = _service.Transfer(sourceFile, targetFolder, "target.PDF", AttachmentTransferMode.Move);

        Assert.True(result.Success);
        Assert.True(result.WasMoved);
        Assert.False(result.WasCopied);
        Assert.False(File.Exists(sourceFile));
        Assert.True(File.Exists(result.TargetPath));
        Assert.Equal("attachment", File.ReadAllText(result.TargetPath!));
    }

    [Fact]
    public void Transfer_MoveShouldKeepSourceWhenTargetValidationFails()
    {
        var sourceFile = CreateSourceFile("source.pdf", "attachment");

        var result = _service.Transfer(sourceFile, string.Empty, "target.PDF", AttachmentTransferMode.Move);

        Assert.False(result.Success);
        Assert.True(File.Exists(sourceFile));
    }

    [Fact]
    public void Transfer_ShouldNotOverwriteExistingTargetFile()
    {
        var sourceFile = CreateSourceFile("source.pdf", "new");
        var targetFolder = CreateTempFolder();
        var existingFile = Path.Combine(targetFolder, "target.PDF");
        File.WriteAllText(existingFile, "old");

        var result = _service.Transfer(sourceFile, targetFolder, "target.PDF", AttachmentTransferMode.Copy);

        Assert.True(result.Success);
        Assert.Equal("target_001.PDF", result.FileName);
        Assert.Equal("old", File.ReadAllText(existingFile));
        Assert.Equal("new", File.ReadAllText(result.TargetPath!));
    }

    [Fact]
    public void Transfer_ShouldIncrementCollisionSuffix()
    {
        var sourceFile = CreateSourceFile("source.pdf", "new");
        var targetFolder = CreateTempFolder();
        File.WriteAllText(Path.Combine(targetFolder, "target.PDF"), "old");
        File.WriteAllText(Path.Combine(targetFolder, "target_001.PDF"), "old-1");

        var result = _service.Transfer(sourceFile, targetFolder, "target.PDF", AttachmentTransferMode.Copy);

        Assert.True(result.Success);
        Assert.Equal("target_002.PDF", result.FileName);
    }

    [Fact]
    public void Transfer_ShouldReturnControlledErrorForMissingSourceFile()
    {
        var targetFolder = CreateTempFolder();
        var sourceFile = Path.Combine(CreateTempFolder(), "missing.pdf");

        var result = _service.Transfer(sourceFile, targetFolder, "target.PDF", AttachmentTransferMode.Copy);

        Assert.False(result.Success);
        Assert.Contains("does not exist", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Transfer_ShouldRejectFolderAsSource()
    {
        var sourceFolder = CreateTempFolder();
        var targetFolder = CreateTempFolder();

        var result = _service.Transfer(sourceFolder, targetFolder, "target.PDF", AttachmentTransferMode.Copy);

        Assert.False(result.Success);
        Assert.Contains("not a folder", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Transfer_ShouldReturnControlledErrorForEmptyTargetFolder()
    {
        var sourceFile = CreateSourceFile("source.pdf", "attachment");

        var result = _service.Transfer(sourceFile, string.Empty, "target.PDF", AttachmentTransferMode.Copy);

        Assert.False(result.Success);
        Assert.Contains("target folder must not be empty", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Transfer_ShouldRejectUnsafeTargetFolder()
    {
        var sourceFile = CreateSourceFile("source.pdf", "attachment");
        var root = Path.GetPathRoot(Path.GetTempPath())!;

        var result = _service.Transfer(sourceFile, root, "target.PDF", AttachmentTransferMode.Copy);

        Assert.False(result.Success);
        Assert.Contains("unsafe", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Transfer_ShouldSanitizeInvalidFileName()
    {
        var sourceFile = CreateSourceFile("source.pdf", "attachment");
        var targetFolder = CreateTempFolder();

        var result = _service.Transfer(sourceFile, targetFolder, "bad:name?.PDF", AttachmentTransferMode.Copy);

        Assert.True(result.Success);
        Assert.Equal("bad_name_.PDF", result.FileName);
        Assert.True(File.Exists(Path.Combine(targetFolder, "bad_name_.PDF")));
    }

    [Fact]
    public void Transfer_ShouldNotUsePathPartsFromDesiredFileName()
    {
        var sourceFile = CreateSourceFile("source.pdf", "attachment");
        var targetFolder = CreateTempFolder();

        var result = _service.Transfer(sourceFile, targetFolder, @"..\nested\target.PDF", AttachmentTransferMode.Copy);

        Assert.True(result.Success);
        Assert.Equal("target.PDF", result.FileName);
        Assert.Equal(Path.Combine(targetFolder, "target.PDF"), result.TargetPath);
        Assert.False(Directory.Exists(Path.Combine(targetFolder, "nested")));
    }

    private static string CreateSourceFile(string fileName, string content)
    {
        var folder = CreateTempFolder();
        var filePath = Path.Combine(folder, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    private static string CreateTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
