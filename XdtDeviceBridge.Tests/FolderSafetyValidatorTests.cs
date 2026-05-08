using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class FolderSafetyValidatorTests
{
    private readonly FolderSafetyValidator _validator = new();

    [Fact]
    public void ValidateFolderForCleanup_ShouldReportEmptyPath()
    {
        var result = _validator.ValidateFolderForCleanup("");

        AssertError(result, "Folder path must not be empty.");
    }

    [Fact]
    public void ValidateFolderForCleanup_ShouldReportRelativePath()
    {
        var result = _validator.ValidateFolderForCleanup(Path.Combine("relative", "folder"));

        AssertError(result, "Folder path must be fully qualified.");
    }

    [Fact]
    public void ValidateFolderForCleanup_ShouldReportDriveRoot()
    {
        var driveRoot = Path.GetPathRoot(Path.GetTempPath())!;

        var result = _validator.ValidateFolderForCleanup(driveRoot);

        AssertError(result, "Folder path must not be a drive or share root.");
    }

    [Fact]
    public void ValidateFolderForCleanup_ShouldReportWindowsFolder()
    {
        var windowsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        var result = _validator.ValidateFolderForCleanup(windowsFolder);

        AssertError(result, "Folder path must not be the Windows system folder.");
    }

    [Fact]
    public void ValidateFolderForCleanup_ShouldReportProgramFilesFolder()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        var result = _validator.ValidateFolderForCleanup(programFiles);

        AssertError(result, "Folder path must not be Program Files.");
    }

    [Fact]
    public void ValidateFolderForCleanup_ShouldReportUserProfileRoot()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var result = _validator.ValidateFolderForCleanup(userProfile);

        AssertError(result, "Folder path must not be the user profile root.");
    }

    [Fact]
    public void ValidateFolderForCleanup_ShouldAcceptNormalExistingSubFolder()
    {
        var folder = CreateExistingFolder();

        var result = _validator.ValidateFolderForCleanup(folder);

        Assert.False(result.HasErrors);
        Assert.False(result.HasWarnings);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void ValidateFolderForCleanup_ShouldWarnForMissingSubFolderWithoutError()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"), "missing");

        var result = _validator.ValidateFolderForCleanup(folder);

        Assert.False(result.HasErrors);
        Assert.True(result.HasWarnings);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == FolderSafetyValidationIssueSeverity.Warning
            && issue.Message == "Folder path does not exist.");
    }

    [Fact]
    public void ValidateInterfaceFolderOptions_ShouldValidateOnlyActiveCleanupOptions()
    {
        var safeFolder = CreateExistingFolder();
        var options = CreateFolderOptions(
            aisImportFolder: safeFolder,
            deviceImportFolder: Path.Combine("relative", "device"),
            exportFolder: Path.GetPathRoot(Path.GetTempPath())!,
            clearAisImportFolderBeforeProcessing: true,
            clearDeviceImportFolderBeforeProcessing: false,
            clearExportFolderAfterSuccessfulTransfer: false);

        var result = _validator.ValidateInterfaceFolderOptions(options);

        Assert.False(result.HasErrors);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void ValidateInterfaceFolderOptions_ShouldReturnNoIssuesWhenNoCleanupOptionIsActive()
    {
        var options = CreateFolderOptions(
            aisImportFolder: "",
            deviceImportFolder: Path.Combine("relative", "device"),
            exportFolder: Path.GetPathRoot(Path.GetTempPath())!,
            clearAisImportFolderBeforeProcessing: false,
            clearDeviceImportFolderBeforeProcessing: false,
            clearExportFolderAfterSuccessfulTransfer: false);

        var result = _validator.ValidateInterfaceFolderOptions(options);

        Assert.False(result.HasErrors);
        Assert.False(result.HasWarnings);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void ValidateInterfaceFolderOptions_ShouldAcceptEmptyAttachmentFolders()
    {
        var options = CreateFolderOptions(
            attachmentImportFolder: string.Empty,
            attachmentExportFolder: string.Empty);

        var result = _validator.ValidateInterfaceFolderOptions(options);

        Assert.False(result.HasErrors);
        Assert.False(result.HasWarnings);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void ValidateInterfaceFolderOptions_ShouldValidateConfiguredAttachmentFolders()
    {
        var options = CreateFolderOptions(
            attachmentImportFolder: Path.GetPathRoot(Path.GetTempPath())!,
            attachmentExportFolder: CreateExistingFolder());

        var result = _validator.ValidateInterfaceFolderOptions(options);

        AssertError(result, "Folder path must not be a drive or share root.");
    }

    private static string CreateExistingFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"), "cleanup");
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static InterfaceFolderOptions CreateFolderOptions(
        string aisImportFolder = "",
        string deviceImportFolder = "",
        string exportFolder = "",
        string attachmentImportFolder = "",
        string attachmentExportFolder = "",
        bool clearAisImportFolderBeforeProcessing = false,
        bool clearDeviceImportFolderBeforeProcessing = false,
        bool clearExportFolderAfterSuccessfulTransfer = false)
    {
        return new InterfaceFolderOptions(
            AisImportFolder: aisImportFolder,
            DeviceImportFolder: deviceImportFolder,
            ExportFolder: exportFolder,
            ArchiveFolder: string.Empty,
            ErrorFolder: string.Empty,
            ClearAisImportFolderBeforeProcessing: clearAisImportFolderBeforeProcessing,
            ClearDeviceImportFolderBeforeProcessing: clearDeviceImportFolderBeforeProcessing,
            ClearExportFolderAfterSuccessfulTransfer: clearExportFolderAfterSuccessfulTransfer,
            ArchiveProcessedFiles: false,
            MoveFailedFilesToErrorFolder: false,
            AttachmentImportFolder: attachmentImportFolder,
            AttachmentExportFolder: attachmentExportFolder);
    }

    private static void AssertError(FolderSafetyValidationResult result, string message)
    {
        Assert.True(result.HasErrors);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == FolderSafetyValidationIssueSeverity.Error
            && issue.Message == message);
    }
}
