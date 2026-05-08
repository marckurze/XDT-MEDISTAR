using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class BuilderTestExportServiceTests
{
    private static readonly DateTime Timestamp = new(2026, 5, 8, 20, 48, 06, DateTimeKind.Utc);

    [Fact]
    public void BuildPreview_WithoutAttachmentShouldNotContainExternalLinkFields()
    {
        var service = new BuilderTestExportService();
        var rules = CreateExportRules();

        var result = service.BuildPreview(new BuilderTestExportPreviewRequest(
            ExportProfileName: "Testprofil",
            ExportRules: rules,
            Patient: CreatePatient(),
            Measurements: CreateMeasurements(),
            TransientAttachmentFields: Array.Empty<ExportFieldRecord>()));

        Assert.True(result.Success);
        Assert.DoesNotContain(result.ExportRecords, record => IsExternalLinkField(record.FieldCode));
        Assert.DoesNotContain("6302", result.ExportContent, StringComparison.Ordinal);
        Assert.Equal(3, rules.Count);
    }

    [Fact]
    public void BuildPreview_WithAttachmentShouldAppendTransientFieldsWithoutChangingRules()
    {
        var service = new BuilderTestExportService();
        var rules = CreateExportRules();
        var attachmentFields = CreateAttachmentFields(@"C:\Temp\XdtBridgeTest\11253_08052026_204806.PDF");

        var result = service.BuildPreview(new BuilderTestExportPreviewRequest(
            ExportProfileName: "Testprofil",
            ExportRules: rules,
            Patient: CreatePatient(),
            Measurements: CreateMeasurements(),
            TransientAttachmentFields: attachmentFields));

        Assert.True(result.Success);
        Assert.Equal(
            new[] { "8000", "3000", "6228", "6302", "6303", "6304", "6305" },
            result.ExportRecords.Select(record => record.FieldCode));
        Assert.Contains("6302", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains("6303", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains("6304", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains("6305C:\\Temp\\XdtBridgeTest\\11253_08052026_204806.PDF", result.ExportContent, StringComparison.Ordinal);
        Assert.DoesNotContain(rules, rule => rule.TargetFieldCode is "6302" or "6303" or "6304" or "6305");
    }

    [Fact]
    public void BuildPreview_ShouldSkipEmptyOptionalAttachmentDescription()
    {
        var service = new BuilderTestExportService();
        var fieldsWithoutDescription = new[]
        {
            new ExportFieldRecord("6302", "Datei", 1),
            new ExportFieldRecord("6303", "PDF", 2),
            new ExportFieldRecord("6305", @"C:\Temp\XdtBridgeTest\11253.PDF", 4)
        };

        var result = service.BuildPreview(new BuilderTestExportPreviewRequest(
            ExportProfileName: "Testprofil",
            ExportRules: CreateExportRules(),
            Patient: CreatePatient(),
            Measurements: CreateMeasurements(),
            TransientAttachmentFields: fieldsWithoutDescription));

        Assert.True(result.Success);
        Assert.Equal(
            new[] { "8000", "3000", "6228", "6302", "6303", "6305" },
            result.ExportRecords.Select(record => record.FieldCode));
        Assert.DoesNotContain("6304", result.ExportRecords.Select(record => record.FieldCode));
    }

    [Fact]
    public void Export_WithoutAttachmentShouldWriteOnlyXdtFileWithoutExternalLinkFields()
    {
        var service = new BuilderTestExportService();
        var targetFolder = CreateTempFolder();

        var result = service.Export(new BuilderTestExportRequest(
            TargetFolder: targetFolder,
            ExportFileName: "Testexport.XDT",
            OutputEncoding: "UTF-8",
            ExportProfileName: "Testprofil",
            ExportRules: CreateExportRules(),
            Patient: CreatePatient(),
            Measurements: CreateMeasurements(),
            FolderOptions: null,
            SourceAttachmentPath: null,
            IsSourceAttachmentStable: null,
            ProcessingTimestamp: Timestamp));

        Assert.True(result.Success);
        Assert.True(File.Exists(result.ExportFilePath!));
        Assert.Null(result.AttachmentTargetPath);
        Assert.DoesNotContain(result.ExportRecords, record => IsExternalLinkField(record.FieldCode));
        Assert.Single(Directory.EnumerateFiles(targetFolder));
    }

    [Fact]
    public void Export_WithAttachmentShouldWriteXdtFileAndRenamedAttachmentToSelectedTargetFolder()
    {
        var service = new BuilderTestExportService();
        var sourceAttachment = CreateSourceFile("original.pdf", "PDF");
        var targetFolder = CreateTempFolder();
        var simulatedProfileFolder = CreateTempFolder();
        var options = CreateOptions(simulatedProfileFolder) with
        {
            AttachmentTransferMode = AttachmentTransferMode.Move
        };

        var result = service.Export(new BuilderTestExportRequest(
            TargetFolder: targetFolder,
            ExportFileName: "Testexport.XDT",
            OutputEncoding: "UTF-8",
            ExportProfileName: "Testprofil",
            ExportRules: CreateExportRules(),
            Patient: CreatePatient(),
            Measurements: CreateMeasurements(),
            FolderOptions: options,
            SourceAttachmentPath: sourceAttachment,
            IsSourceAttachmentStable: true,
            ProcessingTimestamp: Timestamp));

        Assert.True(result.Success);
        Assert.True(File.Exists(result.ExportFilePath!));
        Assert.True(File.Exists(sourceAttachment));
        Assert.NotNull(result.AttachmentTargetPath);
        Assert.Equal(Path.Combine(targetFolder, "11253_08052026_204806.PDF"), result.AttachmentTargetPath);
        Assert.True(File.Exists(result.AttachmentTargetPath));
        Assert.Equal(Path.Combine(simulatedProfileFolder, "11253_08052026_204806.PDF"), result.AttachmentSimulatedTargetPath);
        Assert.Contains(result.ExportRecords, record => record.FieldCode == "6305" && record.Value == result.AttachmentSimulatedTargetPath);
        Assert.Contains("6302", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains("6303", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains("6304", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains($"6305{result.AttachmentSimulatedTargetPath}", result.ExportContent, StringComparison.Ordinal);
        Assert.DoesNotContain($"6305{targetFolder}", result.ExportContent, StringComparison.Ordinal);
        Assert.DoesNotContain(sourceAttachment, result.ExportContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Export_WithAttachmentShouldUseCollisionSafeAttachmentFileName()
    {
        var service = new BuilderTestExportService();
        var sourceAttachment = CreateSourceFile("original.pdf", "PDF");
        var targetFolder = CreateTempFolder();
        var simulatedProfileFolder = CreateTempFolder();
        File.WriteAllText(Path.Combine(targetFolder, "11253_08052026_204806.PDF"), "existing");

        var result = service.Export(new BuilderTestExportRequest(
            TargetFolder: targetFolder,
            ExportFileName: "Testexport.XDT",
            OutputEncoding: "UTF-8",
            ExportProfileName: "Testprofil",
            ExportRules: CreateExportRules(),
            Patient: CreatePatient(),
            Measurements: CreateMeasurements(),
            FolderOptions: CreateOptions(simulatedProfileFolder),
            SourceAttachmentPath: sourceAttachment,
            IsSourceAttachmentStable: true,
            ProcessingTimestamp: Timestamp));

        Assert.True(result.Success);
        Assert.Equal(Path.Combine(targetFolder, "11253_08052026_204806_001.PDF"), result.AttachmentTargetPath);
        Assert.Equal(Path.Combine(simulatedProfileFolder, "11253_08052026_204806_001.PDF"), result.AttachmentSimulatedTargetPath);
        Assert.Equal("existing", File.ReadAllText(Path.Combine(targetFolder, "11253_08052026_204806.PDF")));
        Assert.Contains($"6305{result.AttachmentSimulatedTargetPath}", result.ExportContent, StringComparison.Ordinal);
        Assert.DoesNotContain($"6305{targetFolder}", result.ExportContent, StringComparison.Ordinal);
    }

    private static IReadOnlyList<ExportRuleDefinition> CreateExportRules()
    {
        return new[]
        {
            new ExportRuleDefinition("1", "8000", "MessageType", ExportRuleType.StaticValue, null, "6310", 1, true, null),
            new ExportRuleDefinition("2", "3000", "PatientNumber", ExportRuleType.AisField, "AIS.PatientNumber", "{value}", 2, true, null),
            new ExportRuleDefinition("3", "6228", "Sphere", ExportRuleType.Template, "Device.R/Sphere", "R={value}", 3, true, null)
        };
    }

    private static IReadOnlyList<MeasurementValue> CreateMeasurements()
    {
        return new[]
        {
            new MeasurementValue("R/Sphere", "Sphere", "1.25", "D", "R", "LM")
        };
    }

    private static IReadOnlyList<ExportFieldRecord> CreateAttachmentFields(string targetPath)
    {
        return new[]
        {
            new ExportFieldRecord("6302", "Datei", 1),
            new ExportFieldRecord("6303", "PDF", 2),
            new ExportFieldRecord("6304", "Test M.Kurze", 3),
            new ExportFieldRecord("6305", targetPath, 4)
        };
    }

    private static InterfaceFolderOptions CreateOptions(string targetFolder)
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
            AttachmentExportFolder: targetFolder,
            AttachmentExternalLinkDocumentName: "Datei",
            AttachmentExternalLinkFileFormat: "{ExtensionUpperWithoutDot}",
            AttachmentExternalLinkDescription: "Test M.Kurze",
            AttachmentExternalLinkPathTemplate: "{Attachment.TargetFullPath}");
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
            ExaminationType: "ARK");
    }

    private static bool IsExternalLinkField(string fieldCode)
    {
        return fieldCode is "6302" or "6303" or "6304" or "6305";
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
