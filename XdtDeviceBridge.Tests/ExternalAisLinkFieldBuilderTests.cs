using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class ExternalAisLinkFieldBuilderTests
{
    private readonly ExternalAisLinkFieldBuilder _builder = new();

    [Fact]
    public void Build_ShouldUseConfiguredDocumentNameFor6302()
    {
        var result = Build(CreateOptions(documentName: "PDF-Befund"), @"C:\Xdt\Out\Patient.pdf");

        Assert.True(result.Success);
        Assert.Equal("PDF-Befund", result.FieldSet!.DocumentName);
    }

    [Fact]
    public void Build_ShouldUseDefaultDocumentNameFor6302WhenEmpty()
    {
        var result = Build(CreateOptions(documentName: ""), @"C:\Xdt\Out\Patient.pdf");

        Assert.True(result.Success);
        Assert.Equal("Datei", result.FieldSet!.DocumentName);
    }

    [Fact]
    public void Build_ShouldUseConfiguredFileFormatFor6303()
    {
        var result = Build(CreateOptions(fileFormat: "DCM"), @"C:\Xdt\Out\Image.pdf");

        Assert.True(result.Success);
        Assert.Equal("DCM", result.FieldSet!.FileFormat);
    }

    [Fact]
    public void Build_ShouldDeriveFileFormatPdfFor6303()
    {
        var result = Build(CreateOptions(fileFormat: ""), @"C:\Xdt\Out\Patient.pdf");

        Assert.True(result.Success);
        Assert.Equal("PDF", result.FieldSet!.FileFormat);
    }

    [Theory]
    [InlineData(@"C:\Xdt\Out\Image.jpg")]
    [InlineData(@"C:\Xdt\Out\Image.jpeg")]
    public void Build_ShouldDeriveFileFormatJpgFor6303(string targetPath)
    {
        var result = Build(CreateOptions(fileFormat: ""), targetPath);

        Assert.True(result.Success);
        Assert.Equal("JPG", result.FieldSet!.FileFormat);
    }

    [Fact]
    public void Build_ShouldDeriveFileFormatDcmFor6303()
    {
        var result = Build(CreateOptions(fileFormat: ""), @"C:\Xdt\Out\Image.dcm");

        Assert.True(result.Success);
        Assert.Equal("DCM", result.FieldSet!.FileFormat);
    }

    [Fact]
    public void Build_ShouldLeaveDescriptionEmptyFor6304WhenNotConfigured()
    {
        var result = Build(CreateOptions(description: ""), @"C:\Xdt\Out\Patient.pdf");

        Assert.True(result.Success);
        Assert.Equal(string.Empty, result.FieldSet!.Description);
        Assert.False(result.FieldSet.HasDescription);
    }

    [Fact]
    public void Build_ShouldUseConfiguredDescriptionFor6304()
    {
        var result = Build(CreateOptions(description: "Messwerte Autorefraktor"), @"C:\Xdt\Out\Patient.pdf");

        Assert.True(result.Success);
        Assert.Equal("Messwerte Autorefraktor", result.FieldSet!.Description);
    }

    [Fact]
    public void Build_ShouldUseFinalTargetPathFor6305()
    {
        var targetPath = @"C:\Xdt\Out\Patient.pdf";

        var result = Build(CreateOptions(pathTemplate: ""), targetPath);

        Assert.True(result.Success);
        Assert.Equal(targetPath, result.FieldSet!.FullPath);
    }

    [Fact]
    public void Build_ShouldReplaceTargetFullPathPlaceholderFor6305()
    {
        var targetPath = @"C:\Xdt\Out\Patient.pdf";

        var result = Build(CreateOptions(pathTemplate: "{Attachment.TargetFullPath}"), targetPath);

        Assert.True(result.Success);
        Assert.Equal(targetPath, result.FieldSet!.FullPath);
    }

    [Fact]
    public void Build_ShouldIgnoreFixedConfiguredPathFor6305()
    {
        var targetPath = @"C:\Xdt\Out\Patient.pdf";

        var result = Build(CreateOptions(pathTemplate: @"C:\Old\Patient.pdf"), targetPath);

        Assert.True(result.Success);
        Assert.Equal(targetPath, result.FieldSet!.FullPath);
    }

    [Fact]
    public void Build_ShouldReturnControlledErrorWhenTargetPathIsEmpty()
    {
        var result = Build(CreateOptions(), "");

        Assert.False(result.Success);
        Assert.Null(result.FieldSet);
        Assert.Contains("must not be empty", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_ShouldReturnControlledErrorWhenTargetPathIsRelative()
    {
        var result = Build(CreateOptions(), @"Out\Patient.pdf");

        Assert.False(result.Success);
        Assert.Null(result.FieldSet);
        Assert.Contains("fully qualified", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_ShouldNotCreateXdtLengthPrefixes()
    {
        var result = Build(CreateOptions(
            documentName: "PDF-Befund",
            fileFormat: "PDF",
            description: "Messwerte Autorefraktor"),
            @"C:\Xdt\Out\Patient.pdf");

        Assert.True(result.Success);
        Assert.Equal("PDF-Befund", result.FieldSet!.DocumentName);
        Assert.Equal("PDF", result.FieldSet.FileFormat);
        Assert.False(result.FieldSet.DocumentName.StartsWith("0156302", StringComparison.Ordinal));
        Assert.False(result.FieldSet.FileFormat.StartsWith("0126303", StringComparison.Ordinal));
        Assert.False(result.FieldSet.FullPath.StartsWith("0456305", StringComparison.Ordinal));
    }

    [Fact]
    public void Build_ShouldNotPerformFileOperations()
    {
        var targetPath = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"), "Patient.pdf");

        var result = Build(CreateOptions(), targetPath);

        Assert.True(result.Success);
        Assert.Equal(targetPath, result.FieldSet!.FullPath);
        Assert.False(File.Exists(targetPath));
        Assert.False(Directory.Exists(Path.GetDirectoryName(targetPath)!));
    }

    private ExternalAisLinkFieldBuildResult Build(InterfaceFolderOptions options, string targetPath)
    {
        return _builder.Build(options, targetPath);
    }

    private static InterfaceFolderOptions CreateOptions(
        string documentName = "Datei",
        string fileFormat = "{ExtensionUpperWithoutDot}",
        string description = "",
        string pathTemplate = "{Attachment.TargetFullPath}")
    {
        return new InterfaceFolderOptions(
            AisImportFolder: string.Empty,
            DeviceImportFolder: string.Empty,
            ExportFolder: string.Empty,
            ArchiveFolder: string.Empty,
            ErrorFolder: string.Empty,
            ClearAisImportFolderBeforeProcessing: false,
            ClearDeviceImportFolderBeforeProcessing: false,
            ClearExportFolderAfterSuccessfulTransfer: false,
            ArchiveProcessedFiles: false,
            MoveFailedFilesToErrorFolder: false,
            AttachmentExternalLinkDocumentName: documentName,
            AttachmentExternalLinkFileFormat: fileFormat,
            AttachmentExternalLinkDescription: description,
            AttachmentExternalLinkPathTemplate: pathTemplate);
    }
}
