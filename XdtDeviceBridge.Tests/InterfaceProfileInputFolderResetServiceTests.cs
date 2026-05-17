using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileInputFolderResetServiceTests
{
    [Fact]
    public void ClearInputFolders_ShouldClearAisDeviceAndAttachmentImportFolders()
    {
        var folders = CreateProfileFolders();
        var profile = CreateProfile(folders.AisImport, folders.DeviceImport, attachmentImportFolder: folders.AttachmentImport);
        File.WriteAllText(Path.Combine(folders.AisImport, "patient.gdt"), "ais");
        File.WriteAllText(Path.Combine(folders.DeviceImport, "device.xml"), "device");
        File.WriteAllText(Path.Combine(folders.AttachmentImport, "attachment.pdf"), "attachment");

        var result = new InterfaceProfileInputFolderResetService().ClearInputFolders(profile);

        Assert.Equal(3, result.DeletedFileCount);
        Assert.Empty(Directory.EnumerateFiles(folders.AisImport));
        Assert.Empty(Directory.EnumerateFiles(folders.DeviceImport));
        Assert.Empty(Directory.EnumerateFiles(folders.AttachmentImport));
    }

    [Fact]
    public void ClearInputFolders_ShouldNotClearAttachmentImportFolderWhenNotConfigured()
    {
        var folders = CreateProfileFolders();
        var profile = CreateProfile(folders.AisImport, folders.DeviceImport);
        var attachmentFile = Path.Combine(folders.AttachmentImport, "attachment.pdf");
        File.WriteAllText(Path.Combine(folders.AisImport, "patient.gdt"), "ais");
        File.WriteAllText(Path.Combine(folders.DeviceImport, "device.xml"), "device");
        File.WriteAllText(attachmentFile, "attachment");

        var result = new InterfaceProfileInputFolderResetService().ClearInputFolders(profile);

        Assert.Equal(2, result.DeletedFileCount);
        Assert.True(File.Exists(attachmentFile));
    }

    [Fact]
    public void ClearInputFolders_ShouldNotClearExportArchiveOrErrorFolders()
    {
        var folders = CreateProfileFolders();
        var profile = CreateProfile(
            folders.AisImport,
            folders.DeviceImport,
            attachmentImportFolder: folders.AttachmentImport,
            exportFolder: folders.Export,
            archiveFolder: folders.Archive,
            errorFolder: folders.Error,
            attachmentExportFolder: folders.AttachmentExport);
        var exportFile = Path.Combine(folders.Export, "export.xdt");
        var archiveFile = Path.Combine(folders.Archive, "archive.gdt");
        var errorFile = Path.Combine(folders.Error, "error.xml");
        var attachmentExportFile = Path.Combine(folders.AttachmentExport, "linked.pdf");
        File.WriteAllText(Path.Combine(folders.AisImport, "patient.gdt"), "ais");
        File.WriteAllText(Path.Combine(folders.DeviceImport, "device.xml"), "device");
        File.WriteAllText(Path.Combine(folders.AttachmentImport, "attachment.pdf"), "attachment");
        File.WriteAllText(exportFile, "export");
        File.WriteAllText(archiveFile, "archive");
        File.WriteAllText(errorFile, "error");
        File.WriteAllText(attachmentExportFile, "linked");

        var result = new InterfaceProfileInputFolderResetService().ClearInputFolders(profile);

        Assert.Equal(3, result.DeletedFileCount);
        Assert.True(File.Exists(exportFile));
        Assert.True(File.Exists(archiveFile));
        Assert.True(File.Exists(errorFile));
        Assert.True(File.Exists(attachmentExportFile));
    }

    [Fact]
    public void ClearInputFolders_ShouldSkipInputFolderThatEqualsProtectedFolder()
    {
        var folders = CreateProfileFolders();
        var protectedFile = Path.Combine(folders.Export, "export.xdt");
        File.WriteAllText(protectedFile, "export");
        var profile = CreateProfile(
            folders.Export,
            folders.DeviceImport,
            exportFolder: folders.Export);
        File.WriteAllText(Path.Combine(folders.DeviceImport, "device.xml"), "device");

        var result = new InterfaceProfileInputFolderResetService().ClearInputFolders(profile);

        Assert.Equal(1, result.DeletedFileCount);
        Assert.Equal(1, result.SkippedFolderCount);
        Assert.True(File.Exists(protectedFile));
    }

    [Fact]
    public void ClearInputFolders_ShouldNotDeleteSubfoldersRecursively()
    {
        var folders = CreateProfileFolders();
        var nestedFolder = Path.Combine(folders.AisImport, "nested");
        Directory.CreateDirectory(nestedFolder);
        var nestedFile = Path.Combine(nestedFolder, "patient.gdt");
        File.WriteAllText(Path.Combine(folders.AisImport, "top.gdt"), "ais");
        File.WriteAllText(nestedFile, "nested");
        var profile = CreateProfile(folders.AisImport, folders.DeviceImport);

        var result = new InterfaceProfileInputFolderResetService().ClearInputFolders(profile);

        Assert.Equal(1, result.DeletedFileCount);
        Assert.True(Directory.Exists(nestedFolder));
        Assert.True(File.Exists(nestedFile));
    }

    [Fact]
    public void ClearInputFolders_ShouldTreatDuplicateInputFoldersOnce()
    {
        var folders = CreateProfileFolders();
        File.WriteAllText(Path.Combine(folders.AisImport, "patient.gdt"), "ais");
        var profile = CreateProfile(
            folders.AisImport,
            folders.AisImport,
            attachmentImportFolder: folders.AisImport);

        var result = new InterfaceProfileInputFolderResetService().ClearInputFolders(profile);

        Assert.Equal(1, result.ProcessedFolderCount);
        Assert.Equal(1, result.DeletedFileCount);
    }

    [Fact]
    public void ClearInputFolders_ShouldReportMissingFolderWithoutThrowing()
    {
        var missingFolder = Path.Combine(CreateTempFolder(), "missing");
        var profile = CreateProfile(missingFolder, "");

        var result = new InterfaceProfileInputFolderResetService().ClearInputFolders(profile);

        Assert.Equal(1, result.MissingFolderCount);
        Assert.Equal(0, result.DeletedFileCount);
    }

    [Fact]
    public void ClearInputFolders_ShouldReportDeleteFailureWithoutThrowing()
    {
        var service = new InterfaceProfileInputFolderResetService(
            new FolderSafetyValidator(),
            _ => true,
            _ => new[] { @"C:\Import\locked.gdt" },
            _ => throw new IOException("Datei ist gesperrt."));
        var profile = CreateProfile(@"C:\Import", "");

        var result = service.ClearInputFolders(profile);

        Assert.Equal(1, result.FailedFileCount);
        Assert.Contains(result.Messages, message => message.Contains("Datei konnte nicht gelöscht werden", StringComparison.Ordinal));
    }

    [Fact]
    public void ClearInputFolders_ShouldAffectOnlySelectedProfileFolders()
    {
        var ar360 = CreateProfileFolders();
        var ark1s = CreateProfileFolders();
        File.WriteAllText(Path.Combine(ar360.AisImport, "patient.gdt"), "ais");
        File.WriteAllText(Path.Combine(ar360.DeviceImport, "device.xml"), "device");
        var ark1sAis = Path.Combine(ark1s.AisImport, "patient.gdt");
        var ark1sDevice = Path.Combine(ark1s.DeviceImport, "device.xml");
        File.WriteAllText(ark1sAis, "ais");
        File.WriteAllText(ark1sDevice, "device");
        var ar360Profile = CreateProfile(ar360.AisImport, ar360.DeviceImport);

        var result = new InterfaceProfileInputFolderResetService().ClearInputFolders(ar360Profile);

        Assert.Equal(2, result.DeletedFileCount);
        Assert.True(File.Exists(ark1sAis));
        Assert.True(File.Exists(ark1sDevice));
    }

    private static InterfaceProfileDefinition CreateProfile(
        string aisImportFolder,
        string deviceImportFolder,
        string attachmentImportFolder = "",
        string exportFolder = "",
        string archiveFolder = "",
        string errorFolder = "",
        string attachmentExportFolder = "")
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekAr360Default() with
        {
            FolderOptions = new InterfaceFolderOptions(
                AisImportFolder: aisImportFolder,
                DeviceImportFolder: deviceImportFolder,
                ExportFolder: exportFolder,
                ArchiveFolder: archiveFolder,
                ErrorFolder: errorFolder,
                ClearAisImportFolderBeforeProcessing: false,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: false,
                AttachmentImportFolder: attachmentImportFolder,
                AttachmentExportFolder: attachmentExportFolder)
        };
    }

    private static ProfileFolders CreateProfileFolders()
    {
        var root = CreateTempFolder();
        var folders = new ProfileFolders(
            Path.Combine(root, "ais-in"),
            Path.Combine(root, "device-in"),
            Path.Combine(root, "attachment-in"),
            Path.Combine(root, "export"),
            Path.Combine(root, "archive"),
            Path.Combine(root, "error"),
            Path.Combine(root, "attachment-export"));
        foreach (var folder in folders.All)
        {
            Directory.CreateDirectory(folder);
        }

        return folders;
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }

    private sealed record ProfileFolders(
        string AisImport,
        string DeviceImport,
        string AttachmentImport,
        string Export,
        string Archive,
        string Error,
        string AttachmentExport)
    {
        public IReadOnlyList<string> All => new[]
        {
            AisImport,
            DeviceImport,
            AttachmentImport,
            Export,
            Archive,
            Error,
            AttachmentExport
        };
    }
}
