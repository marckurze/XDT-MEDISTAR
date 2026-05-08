using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class AttachmentExternalLinkPreparationServiceTests
{
    private static readonly DateTime Timestamp = new(2026, 5, 7, 22, 17, 23, DateTimeKind.Utc);

    [Fact]
    public void Prepare_CopyShouldTransferFileAndCreateExternalLinkFields()
    {
        var sourceFile = CreateSourceFile("report.pdf", "attachment");
        var targetFolder = CreateTempFolder();
        var service = new AttachmentExternalLinkPreparationService();
        var request = CreateRequest(sourceFile, targetFolder, AttachmentTransferMode.Copy);

        var result = service.Prepare(request);

        Assert.True(result.Success);
        Assert.True(File.Exists(sourceFile));
        Assert.True(File.Exists(result.TargetPath!));
        Assert.Equal(AttachmentTransferMode.Copy, result.TransferMode);
        Assert.Equal(Path.Combine(targetFolder, "11253_07052026_221723.PDF"), result.TargetPath);
        Assert.Contains(result.ExportFields, field => field.FieldCode == "6302" && field.Value == "PDF-Befund");
        Assert.Contains(result.ExportFields, field => field.FieldCode == "6303" && field.Value == "PDF");
        Assert.Contains(result.ExportFields, field => field.FieldCode == "6304" && field.Value == "Messprotokoll");
        Assert.Contains(result.ExportFields, field => field.FieldCode == "6305" && field.Value == result.TargetPath);
    }

    [Fact]
    public void Prepare_MoveShouldTransferFileAndCreateExternalLinkFields()
    {
        var sourceFile = CreateSourceFile("report.pdf", "attachment");
        var targetFolder = CreateTempFolder();
        var service = new AttachmentExternalLinkPreparationService();
        var request = CreateRequest(sourceFile, targetFolder, AttachmentTransferMode.Move);

        var result = service.Prepare(request);

        Assert.True(result.Success);
        Assert.False(File.Exists(sourceFile));
        Assert.True(File.Exists(result.TargetPath!));
        Assert.Equal(AttachmentTransferMode.Move, result.TransferMode);
        Assert.Contains(result.ExportFields, field => field.FieldCode == "6305" && field.Value == result.TargetPath);
    }

    [Fact]
    public void Prepare_ShouldUseDefaultMoveTransferMode()
    {
        var sourceFile = CreateSourceFile("report.pdf", "attachment");
        var targetFolder = CreateTempFolder();
        var service = new AttachmentExternalLinkPreparationService();
        var request = CreateRequest(sourceFile, targetFolder, transferMode: null);

        var result = service.Prepare(request);

        Assert.True(result.Success);
        Assert.Equal(AttachmentTransferMode.Move, result.TransferMode);
        Assert.False(File.Exists(sourceFile));
    }

    [Fact]
    public void Prepare_ShouldUseCollisionSafeTargetFileName()
    {
        var sourceFile = CreateSourceFile("report.pdf", "new");
        var targetFolder = CreateTempFolder();
        File.WriteAllText(Path.Combine(targetFolder, "11253_07052026_221723.PDF"), "existing");
        var service = new AttachmentExternalLinkPreparationService();
        var request = CreateRequest(sourceFile, targetFolder, AttachmentTransferMode.Copy);

        var result = service.Prepare(request);

        Assert.True(result.Success);
        Assert.Equal("11253_07052026_221723_001.PDF", result.TargetFileName);
        Assert.Equal("existing", File.ReadAllText(Path.Combine(targetFolder, "11253_07052026_221723.PDF")));
        Assert.Equal("new", File.ReadAllText(result.TargetPath!));
    }

    [Fact]
    public void Prepare_ShouldUseDefaultDocumentNameAndDerivedFileFormat()
    {
        var sourceFile = CreateSourceFile("image.jpg", "attachment");
        var targetFolder = CreateTempFolder();
        var service = new AttachmentExternalLinkPreparationService();
        var options = CreateOptions(targetFolder, AttachmentTransferMode.Copy) with
        {
            AttachmentExternalLinkDocumentName = "",
            AttachmentExternalLinkFileFormat = "",
            AttachmentExternalLinkDescription = ""
        };

        var result = service.Prepare(CreateRequest(sourceFile, options));

        Assert.True(result.Success);
        Assert.Contains(result.ExportFields, field => field.FieldCode == "6302" && field.Value == "Datei");
        Assert.Contains(result.ExportFields, field => field.FieldCode == "6303" && field.Value == "JPG");
        Assert.DoesNotContain(result.ExportFields, field => field.FieldCode == "6304");
        Assert.Contains(result.ExportFields, field => field.FieldCode == "6305" && field.Value == result.TargetPath);
    }

    [Fact]
    public void Prepare_ShouldReturnErrorForMissingSourceFile()
    {
        var sourceFile = Path.Combine(CreateTempFolder(), "missing.pdf");
        var targetFolder = CreateTempFolder();
        var service = new AttachmentExternalLinkPreparationService();

        var result = service.Prepare(CreateRequest(sourceFile, targetFolder, AttachmentTransferMode.Copy));

        Assert.False(result.Success);
        Assert.Empty(result.ExportFields);
        Assert.Null(result.ExternalAisLinkFieldSet);
        Assert.Contains("does not exist", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Prepare_ShouldReturnErrorWhenSourceIsFolder()
    {
        var sourceFolder = CreateTempFolder();
        var targetFolder = CreateTempFolder();
        var service = new AttachmentExternalLinkPreparationService();

        var result = service.Prepare(CreateRequest(sourceFolder, targetFolder, AttachmentTransferMode.Copy));

        Assert.False(result.Success);
        Assert.Empty(result.ExportFields);
        Assert.Contains("not a folder", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Prepare_ShouldReturnErrorForEmptyTargetFolder()
    {
        var sourceFile = CreateSourceFile("report.pdf", "attachment");
        var service = new AttachmentExternalLinkPreparationService();

        var result = service.Prepare(CreateRequest(sourceFile, "", AttachmentTransferMode.Copy));

        Assert.False(result.Success);
        Assert.Empty(result.ExportFields);
        Assert.Contains("export folder must not be empty", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Prepare_ShouldReturnErrorForUnsafeTargetFolder()
    {
        var sourceFile = CreateSourceFile("report.pdf", "attachment");
        var unsafeTargetFolder = Path.GetPathRoot(Path.GetTempPath())!;
        var service = new AttachmentExternalLinkPreparationService();

        var result = service.Prepare(CreateRequest(sourceFile, unsafeTargetFolder, AttachmentTransferMode.Copy));

        Assert.False(result.Success);
        Assert.Empty(result.ExportFields);
        Assert.Contains("unsafe", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Prepare_ShouldReturnErrorWhenTransferFailsAndShouldNotCreateLinkFields()
    {
        var sourceFile = CreateSourceFile("report.pdf", "attachment");
        var targetFolder = CreateTempFolder();
        var transferService = new StubAttachmentTransferService(new AttachmentTransferResult(
            Success: false,
            SourcePath: sourceFile,
            TargetPath: null,
            FileName: null,
            TransferMode: AttachmentTransferMode.Copy,
            ErrorMessage: "Simulated transfer failure.",
            WasCopied: false,
            WasMoved: false));
        var service = CreateService(transferService);

        var result = service.Prepare(CreateRequest(sourceFile, targetFolder, AttachmentTransferMode.Copy));

        Assert.False(result.Success);
        Assert.Empty(result.ExportFields);
        Assert.Null(result.ExternalAisLinkFieldSet);
        Assert.Contains("Simulated transfer failure", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Prepare_ShouldReturnErrorWhenTransferReturnsNoTargetPath()
    {
        var sourceFile = CreateSourceFile("report.pdf", "attachment");
        var targetFolder = CreateTempFolder();
        var transferService = new StubAttachmentTransferService(new AttachmentTransferResult(
            Success: true,
            SourcePath: sourceFile,
            TargetPath: "",
            FileName: null,
            TransferMode: AttachmentTransferMode.Copy,
            ErrorMessage: null,
            WasCopied: true,
            WasMoved: false));
        var service = CreateService(transferService);

        var result = service.Prepare(CreateRequest(sourceFile, targetFolder, AttachmentTransferMode.Copy));

        Assert.False(result.Success);
        Assert.Empty(result.ExportFields);
        Assert.Contains("final target path", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    private static AttachmentExternalLinkPreparationService CreateService(IAttachmentTransferService transferService)
    {
        return new AttachmentExternalLinkPreparationService(
            new AttachmentFileNameBuilder(),
            transferService,
            new ExternalAisLinkFieldBuilder(),
            new ExternalAisLinkXdtFieldAdapter());
    }

    private static AttachmentExternalLinkPreparationRequest CreateRequest(
        string sourceFile,
        string targetFolder,
        AttachmentTransferMode? transferMode)
    {
        return CreateRequest(sourceFile, CreateOptions(targetFolder, transferMode));
    }

    private static AttachmentExternalLinkPreparationRequest CreateRequest(
        string sourceFile,
        InterfaceFolderOptions options)
    {
        return new AttachmentExternalLinkPreparationRequest(
            FolderOptions: options,
            SourceAttachmentPath: sourceFile,
            Patient: CreatePatient(),
            ProcessingTimestamp: Timestamp);
    }

    private static InterfaceFolderOptions CreateOptions(
        string targetFolder,
        AttachmentTransferMode? transferMode)
    {
        var options = new InterfaceFolderOptions(
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
            AttachmentExportFolder: targetFolder,
            AttachmentExternalLinkDocumentName: "PDF-Befund",
            AttachmentExternalLinkFileFormat: "",
            AttachmentExternalLinkDescription: "Messprotokoll");

        return transferMode is null
            ? options
            : options with { AttachmentTransferMode = transferMode.Value };
    }

    private static PatientData CreatePatient()
    {
        return new PatientData(
            PatientNumber: "11253",
            LastName: "Muster",
            FirstName: "Mara",
            BirthDate: "1980-05-31",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: null,
            TargetSystem: null,
            GdtVersion: null,
            ExaminationType: null);
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

    private sealed class StubAttachmentTransferService : IAttachmentTransferService
    {
        private readonly AttachmentTransferResult _result;

        public StubAttachmentTransferService(AttachmentTransferResult result)
        {
            _result = result;
        }

        public AttachmentTransferResult Transfer(
            string sourceAttachmentPath,
            string targetFolder,
            string desiredFileName,
            AttachmentTransferMode transferMode)
        {
            return _result;
        }
    }
}
