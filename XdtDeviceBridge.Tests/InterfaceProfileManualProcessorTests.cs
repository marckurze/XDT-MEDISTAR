using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileManualProcessorTests
{
    private readonly InterfaceProfileManualProcessor _processor = new();

    [Fact]
    public void Process_ShouldCreateExportFileFromGdtXmlAndExportProfile()
    {
        var exportFolder = CreateTempFolder();
        var interfaceProfile = CreateInterfaceProfile(exportFolder);
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();

        var result = _processor.Process(
            interfaceProfile,
            exportProfile,
            GetTestDataPath("sample-gdt-utf8.gdt"),
            GetTestDataPath("nidek-ark1s-sample.xml"),
            new DateTime(2026, 6, 1, 12, 0, 0));

        Assert.True(result.Success);
        Assert.NotNull(result.ExportFilePath);
        Assert.True(File.Exists(result.ExportFilePath));
        Assert.NotNull(result.ExportContent);
        Assert.Contains("6310", result.ExportContent);
        Assert.Contains("PAT-100", result.ExportContent);
        Assert.Contains("R.:S=-1.25", result.ExportContent);
        Assert.DoesNotContain("6302", result.ExportContent);
        Assert.DoesNotContain("6303", result.ExportContent);
        Assert.DoesNotContain("6304", result.ExportContent);
        Assert.DoesNotContain("6305", result.ExportContent);
        Assert.StartsWith(exportFolder, result.ExportFilePath, StringComparison.OrdinalIgnoreCase);
        Assert.Null(result.ArchiveResult);
        Assert.Contains("Archivierung ist für dieses Schnittstellenprofil deaktiviert.", result.Messages);
    }

    [Fact]
    public void Process_Cv5000ReturnShouldReadMedistarHistoryAisTolerantly()
    {
        var exportFolder = CreateTempFolder();
        var aisFilePath = CopyCv5000TestDataToTemp("Patient_mit_Phoropter_Daten.XDT", "Patient.XDT");
        var deviceFilePath = CopyCv5000TestDataToTemp(
            "M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml",
            "M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml");

        var result = _processor.Process(
            CreateCv5000InterfaceProfile(exportFolder),
            DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default(),
            aisFilePath,
            deviceFilePath,
            new DateTime(2026, 5, 23, 10, 18, 29));

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.NotNull(result.ExportContent);
        Assert.Contains("8402Phoro", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains("6227Phoropter finaler Verordnungswert", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains("6228R.:S=+ 1.25 Z=- 2.00*  7 PD= 59 VD= 13.75", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 1.25 Z=- 2.00*  7", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains("6227Phoropter Maximalwert (Vollkorrektion)", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains("6330R.:S=+ 1.25 Z=- 2.00*  7 PD= 59 VD= 13.75", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains("6330L.:S=+ 1.25 Z=- 2.00*  7", result.ExportContent, StringComparison.Ordinal);
        Assert.DoesNotContain("6228--", result.ExportContent, StringComparison.Ordinal);
        Assert.DoesNotContain(result.Messages, message =>
            message.Contains("AIS-Datei konnte nicht fehlerfrei gelesen werden", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.PipelineResult!.Issues, issue =>
            issue.Severity == ProcessingIssueSeverity.Warning
            && issue.Stage == ProcessingStage.GdtParsing
            && issue.Message.Contains("CV-5000-Historienparser", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.PipelineResult.Issues, issue => issue.Severity == ProcessingIssueSeverity.Error);
        Assert.True(File.Exists(aisFilePath));
        Assert.True(File.Exists(deviceFilePath));
    }

    [Fact]
    public void Process_Cv5000HistoryAisWithoutRequiredPatientDataShouldReturnSpecificFailure()
    {
        var aisFilePath = CreateTempFile(
            "Patient.XDT",
            "18.05.2026 V0 R.:S=+ 1.00 Z=- 0.25*  7\r\n18.05.2026 V0 L.:S=+ 1.25 Z=- 0.50* 10\r\n");
        var deviceFilePath = CopyCv5000TestDataToTemp(
            "M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml",
            "cv5000-return.xml");

        var result = _processor.Process(
            CreateCv5000InterfaceProfile(CreateTempFolder(), moveFailedFilesToErrorFolder: false),
            DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default(),
            aisFilePath,
            deviceFilePath,
            new DateTime(2026, 5, 23, 10, 18, 29));

        Assert.False(result.Success);
        var combinedMessages = string.Join(Environment.NewLine, result.Messages);
        Assert.Contains("Pflicht-Patientendaten fehlen", combinedMessages, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("3000 Patientennummer", combinedMessages, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("3101 Nachname", combinedMessages, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AIS-Datei konnte nicht fehlerfrei gelesen werden", combinedMessages, StringComparison.OrdinalIgnoreCase);
        Assert.Null(result.ExportContent);
    }

    [Fact]
    public void AisPatientDataReader_ShouldReturnDetailedGdtError()
    {
        var aisFilePath = CreateTempFile("Patient.XDT", "PHOROPTER\r\n");

        var result = new AisPatientDataReader().Read(aisFilePath);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("GDT-/XDT-Parserfehler", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Zeile 1", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AIS-Datei konnte nicht fehlerfrei gelesen werden.", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Process_ShouldAppendAttachmentLinkFieldsAtEndWhenAttachmentPreparationSucceeds()
    {
        var baseline = _processor.Process(
            CreateInterfaceProfile(CreateTempFolder()),
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            GetTestDataPath("nidek-ark1s-sample.xml"),
            new DateTime(2026, 6, 1, 12, 0, 0));
        var targetPath = @"C:\GitHub\AnhangExp\4701-1_08052026_135231.PDF";
        var fields = new[]
        {
            new ExportFieldRecord("6302", "Datei", 1),
            new ExportFieldRecord("6303", "PDF", 2),
            new ExportFieldRecord("6304", "Messprotokoll", 3),
            new ExportFieldRecord("6305", targetPath, 4)
        };

        var result = _processor.Process(
            CreateInterfaceProfile(CreateTempFolder()),
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            GetTestDataPath("nidek-ark1s-sample.xml"),
            new DateTime(2026, 6, 1, 12, 0, 0),
            _ => CreateAttachmentStatus(fields));

        Assert.True(result.Success);
        Assert.NotNull(result.ExportContent);
        Assert.StartsWith(baseline.ExportContent!, result.ExportContent);
        var lines = SplitXdtLines(result.ExportContent!);
        Assert.Equal(ExpectedXdtLineWithoutCrLf("6302", "Datei"), lines[^4]);
        Assert.Equal(ExpectedXdtLineWithoutCrLf("6303", "PDF"), lines[^3]);
        Assert.Equal(ExpectedXdtLineWithoutCrLf("6304", "Messprotokoll"), lines[^2]);
        Assert.Equal(ExpectedXdtLineWithoutCrLf("6305", targetPath), lines[^1]);
        Assert.Contains("XDT-Anhang-Linkfelder wurden in die Exportdatei übernommen.", result.Messages);
        Assert.Contains("6305" + targetPath, File.ReadAllText(result.ExportFilePath!));
    }

    [Fact]
    public void Process_ShouldNotAppendOptional6304WhenDescriptionFieldIsMissing()
    {
        var targetPath = @"C:\GitHub\AnhangExp\4701-1_08052026_135231.PDF";
        var fields = new[]
        {
            new ExportFieldRecord("6302", "Datei", 1),
            new ExportFieldRecord("6303", "PDF", 2),
            new ExportFieldRecord("6305", targetPath, 3)
        };

        var result = _processor.Process(
            CreateInterfaceProfile(CreateTempFolder()),
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            GetTestDataPath("nidek-ark1s-sample.xml"),
            new DateTime(2026, 6, 1, 12, 0, 0),
            _ => CreateAttachmentStatus(fields));

        Assert.True(result.Success);
        Assert.Contains("6302Datei", result.ExportContent);
        Assert.Contains("6303PDF", result.ExportContent);
        Assert.Contains("6305" + targetPath, result.ExportContent);
        Assert.DoesNotContain("6304", result.ExportContent);
    }

    [Fact]
    public void Process_ShouldKeepExportContentUnchangedWhenAttachmentIsSkippedOrFails()
    {
        var timestamp = new DateTime(2026, 6, 1, 12, 0, 0);
        var baseline = _processor.Process(
            CreateInterfaceProfile(CreateTempFolder()),
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            GetTestDataPath("nidek-ark1s-sample.xml"),
            timestamp);

        var skipped = _processor.Process(
            CreateInterfaceProfile(CreateTempFolder()),
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            GetTestDataPath("nidek-ark1s-sample.xml"),
            timestamp,
            _ => CreateSkippedAttachmentStatus());

        var failed = _processor.Process(
            CreateInterfaceProfile(CreateTempFolder()),
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            GetTestDataPath("nidek-ark1s-sample.xml"),
            timestamp,
            _ => CreateFailedAttachmentStatus());

        Assert.Equal(baseline.ExportContent, skipped.ExportContent);
        Assert.Equal(baseline.ExportContent, failed.ExportContent);
        Assert.DoesNotContain("6302", skipped.ExportContent);
        Assert.DoesNotContain("6302", failed.ExportContent);
    }

    [Fact]
    public void Process_AttachmentOnlyShouldCreate6227AndAttachmentFieldsWithoutMeasurementFields()
    {
        var targetPath1 = @"C:\GitHub\AnhangExp\bild1.jpg";
        var targetPath2 = @"C:\GitHub\AnhangExp\befund.pdf";
        var fields = new[]
        {
            new ExportFieldRecord("6302", "Datei", 1),
            new ExportFieldRecord("6303", "JPG", 2),
            new ExportFieldRecord("6305", targetPath1, 3),
            new ExportFieldRecord("6302", "Datei", 4),
            new ExportFieldRecord("6303", "PDF", 5),
            new ExportFieldRecord("6305", targetPath2, 6)
        };

        var result = _processor.Process(
            CreateAttachmentOnlyInterfaceProfile(CreateTempFolder()),
            DefaultExportProfileDefinitions.CreateMedistarDocumentAttachmentDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            CreateDeviceDocumentFile("bild1.jpg"),
            new DateTime(2026, 6, 1, 12, 0, 0),
            _ => CreateAttachmentStatus(fields),
            _ => "Bilddokumentation: vorderer Augenabschnitt.");

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.NotNull(result.ExportContent);
        Assert.Contains("6227Bilddokumentation: vorderer Augenabschnitt.", result.ExportContent);
        Assert.Equal(2, SplitXdtLines(result.ExportContent!).Count(line => line.Contains("6302Datei", StringComparison.Ordinal)));
        Assert.Contains("6305" + targetPath1, result.ExportContent);
        Assert.Contains("6305" + targetPath2, result.ExportContent);
        Assert.DoesNotContain("6228", result.ExportContent);
        Assert.DoesNotContain("6205", result.ExportContent);
        Assert.DoesNotContain("6220", result.ExportContent);
        Assert.Contains(result.PipelineResult!.Measurements, measurement =>
            measurement.SourcePath == "AttachmentOnly/DocumentationText"
            && measurement.Value == "Bilddokumentation: vorderer Augenabschnitt.");
    }

    [Fact]
    public void Process_AttachmentOnlyWithoutDocumentationTextShouldNotCreateEmpty6227()
    {
        var result = _processor.Process(
            CreateAttachmentOnlyInterfaceProfile(CreateTempFolder()),
            DefaultExportProfileDefinitions.CreateMedistarDocumentAttachmentDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            CreateDeviceDocumentFile("befund.pdf"),
            new DateTime(2026, 6, 1, 12, 0, 0),
            _ => CreateAttachmentStatus(new[]
            {
                new ExportFieldRecord("6302", "Datei", 1),
                new ExportFieldRecord("6303", "PDF", 2),
                new ExportFieldRecord("6305", @"C:\GitHub\AnhangExp\befund.pdf", 3)
            }),
            _ => "   ");

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.NotNull(result.ExportContent);
        Assert.DoesNotContain("6227", result.ExportContent);
        Assert.DoesNotContain(result.PipelineResult!.Measurements, measurement => measurement.SourcePath == "AttachmentOnly/DocumentationText");
    }

    [Fact]
    public void Process_AttachmentOnlyShouldFailInsteadOfExportingAisOnlyWhenAttachmentPreparationFails()
    {
        var result = _processor.Process(
            CreateAttachmentOnlyInterfaceProfile(CreateTempFolder()),
            DefaultExportProfileDefinitions.CreateMedistarDocumentAttachmentDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            CreateDeviceDocumentFile("befund.pdf"),
            new DateTime(2026, 6, 1, 12, 0, 0),
            _ => CreateFailedAttachmentStatus(),
            _ => string.Empty);

        Assert.False(result.Success);
        Assert.Null(result.ExportContent);
        Assert.Contains(result.Messages, message => message.Contains("Dokumentanhang konnte nicht", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.Messages, message => message.Contains("Dateipaar erfolgreich verarbeitet", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Process_AttachmentOnlyShouldWriteEachDocumentationLineAsOwn6227Field()
    {
        var result = _processor.Process(
            CreateAttachmentOnlyInterfaceProfile(CreateTempFolder()),
            DefaultExportProfileDefinitions.CreateMedistarDocumentAttachmentDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            CreateDeviceDocumentFile("befund.pdf"),
            new DateTime(2026, 6, 1, 12, 0, 0),
            _ => CreateAttachmentStatus(new[]
            {
                new ExportFieldRecord("6302", "Datei", 1),
                new ExportFieldRecord("6303", "PDF", 2),
                new ExportFieldRecord("6305", @"C:\GitHub\AnhangExp\befund.pdf", 3)
            }),
            _ => "Das ist eine Atlas Datei\r\nHier die zweite Zeile\n\r\nUnd die dritte");

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        var lines = SplitXdtLines(result.ExportContent!);
        var documentationLines = lines.Where(line => line.Contains("6227", StringComparison.Ordinal)).ToArray();
        Assert.Equal(3, documentationLines.Length);
        Assert.Contains(documentationLines, line => line.EndsWith("6227Das ist eine Atlas Datei", StringComparison.Ordinal));
        Assert.Contains(documentationLines, line => line.EndsWith("6227Hier die zweite Zeile", StringComparison.Ordinal));
        Assert.Contains(documentationLines, line => line.EndsWith("6227Und die dritte", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line == "Hier die zweite Zeile" || line == "Und die dritte");
    }

    [Fact]
    public void Process_AttachmentOnlyShouldTreatXmlAsAttachmentWithoutParsing()
    {
        var invalidXmlAttachmentPath = CreateDeviceDocumentFile("dokument.xml", "dies ist kein Messwert-XML");

        var result = _processor.Process(
            CreateAttachmentOnlyInterfaceProfile(CreateTempFolder()),
            DefaultExportProfileDefinitions.CreateMedistarDocumentAttachmentDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            invalidXmlAttachmentPath,
            new DateTime(2026, 6, 1, 12, 0, 0),
            _ => CreateAttachmentStatus(new[]
            {
                new ExportFieldRecord("6302", "Datei", 1),
                new ExportFieldRecord("6303", "XML", 2),
                new ExportFieldRecord("6305", @"C:\GitHub\AnhangExp\dokument.xml", 3)
            }));

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.DoesNotContain(result.PipelineResult!.Issues, issue => issue.Stage == ProcessingStage.DeviceParsing);
        Assert.DoesNotContain("6228", result.ExportContent);
    }

    [Fact]
    public void Process_AttachmentOnlyShouldNotReportMissingDeviceFileWhenAttachmentMoveAlreadyTransferredIt()
    {
        var deviceFilePath = CreateDeviceDocumentFile("bild1.jpg");

        var result = _processor.Process(
            CreateAttachmentOnlyInterfaceProfile(
                CreateTempFolder(),
                clearDeviceImportFolderBeforeProcessing: true),
            DefaultExportProfileDefinitions.CreateMedistarDocumentAttachmentDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            deviceFilePath,
            new DateTime(2026, 6, 1, 12, 0, 0),
            _ =>
            {
                File.Delete(deviceFilePath);
                return CreateAttachmentStatus(new[]
                {
                    new ExportFieldRecord("6302", "Datei", 1),
                    new ExportFieldRecord("6303", "JPG", 2),
                    new ExportFieldRecord("6305", @"C:\GitHub\AnhangExp\bild1.jpg", 3)
                });
            });

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.DoesNotContain(result.Messages, message => message.Contains("Quelldatei fehlt", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Process_ShouldReturnErrorForNonXmlDeviceFile()
    {
        var interfaceProfile = CreateInterfaceProfile(
            CreateTempFolder(),
            errorFolder: CreateTempFolder(),
            moveFailedFilesToErrorFolder: false);
        var deviceFilePath = Path.Combine(CreateTempFolder(), "device.txt");
        File.WriteAllText(deviceFilePath, "device");

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            deviceFilePath,
            DateTime.UtcNow);

        Assert.False(result.Success);
        Assert.Contains("Dieser Dateityp wird für die manuelle Paarverarbeitung noch nicht unterstützt.", result.Messages);
        Assert.Null(result.FailedFileCopyResult);
    }

    [Fact]
    public void Process_ShouldReturnErrorForMissingExportFolder()
    {
        var interfaceProfile = CreateInterfaceProfile(exportFolder: string.Empty);

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            GetTestDataPath("nidek-ark1s-sample.xml"),
            DateTime.UtcNow);

        Assert.False(result.Success);
        Assert.Contains("Exportordner fehlt.", result.Messages);
    }

    [Fact]
    public void Process_ShouldArchiveFilesWhenArchiveProcessedFilesIsEnabled()
    {
        var exportFolder = CreateTempFolder();
        var archiveFolder = CreateTempFolder();
        var aisFilePath = CopyTestDataToTemp("sample-gdt-utf8.gdt", "patient.gdt");
        var deviceFilePath = CopyTestDataToTemp("nidek-ark1s-sample.xml", "device.xml");
        var interfaceProfile = CreateInterfaceProfile(
            exportFolder,
            archiveFolder: archiveFolder,
            archiveProcessedFiles: true);

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            aisFilePath,
            deviceFilePath,
            new DateTime(2026, 6, 1, 12, 0, 0));

        Assert.True(result.Success);
        Assert.NotNull(result.ExportFilePath);
        Assert.NotNull(result.ArchiveResult);
        Assert.False(result.ArchiveResult.HasErrors);
        Assert.Equal(2, result.ArchiveResult.ArchivedFiles.Count);
        Assert.True(File.Exists(aisFilePath));
        Assert.True(File.Exists(deviceFilePath));
        Assert.Contains("Importdateien wurden ins Archiv kopiert. Originale bleiben erhalten:", result.Messages);
    }

    [Fact]
    public void Process_ShouldMoveFilesWhenArchiveModeIsMove()
    {
        var exportFolder = CreateTempFolder();
        var archiveFolder = CreateTempFolder();
        var aisFilePath = CopyTestDataToTemp("sample-gdt-utf8.gdt", "patient.gdt");
        var deviceFilePath = CopyTestDataToTemp("nidek-ark1s-sample.xml", "device.xml");
        var interfaceProfile = CreateInterfaceProfile(
            exportFolder,
            archiveFolder: archiveFolder,
            archiveProcessedFiles: true,
            archiveProcessedFileMode: ArchiveProcessedFileMode.Move);

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            aisFilePath,
            deviceFilePath,
            new DateTime(2026, 6, 1, 12, 0, 0));

        Assert.True(result.Success);
        Assert.NotNull(result.ArchiveResult);
        Assert.False(result.ArchiveResult.HasErrors);
        Assert.False(File.Exists(aisFilePath));
        Assert.False(File.Exists(deviceFilePath));
        Assert.Contains("Importdateien wurden ins Archiv verschoben:", result.Messages);
    }

    [Fact]
    public void Process_ShouldMoveKnownFilesWhenRemoveOptionsAreEnabled()
    {
        var exportFolder = CreateTempFolder();
        var archiveFolder = CreateTempFolder();
        var aisFilePath = CopyTestDataToTemp("sample-gdt-utf8.gdt", "patient.gdt");
        var deviceFilePath = CopyTestDataToTemp("nidek-ark1s-sample.xml", "device.xml");
        var interfaceProfile = CreateInterfaceProfile(
            exportFolder,
            archiveFolder: archiveFolder,
            archiveProcessedFiles: true,
            archiveProcessedFileMode: ArchiveProcessedFileMode.Copy,
            clearAisImportFolderBeforeProcessing: true,
            clearDeviceImportFolderBeforeProcessing: true);

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            aisFilePath,
            deviceFilePath,
            new DateTime(2026, 6, 1, 12, 0, 0));

        Assert.True(result.Success);
        Assert.NotNull(result.ArchiveResult);
        Assert.False(result.ArchiveResult.HasErrors);
        Assert.False(File.Exists(aisFilePath));
        Assert.False(File.Exists(deviceFilePath));
        Assert.True(File.Exists(Path.Combine(archiveFolder, "2026", "06", "01", "MEDISTAR_NIDEK_ARK1S", "AIS", "patient.gdt")));
        Assert.True(File.Exists(Path.Combine(archiveFolder, "2026", "06", "01", "MEDISTAR_NIDEK_ARK1S", "Device", "device.xml")));
        Assert.Contains("Importdateien wurden gemäß Profilregel archiviert; zu entfernende Importdateien wurden verschoben:", result.Messages);
    }

    [Fact]
    public void Process_ShouldRemoveKnownFilesWithoutArchiveWhenRemoveOptionsAreEnabled()
    {
        var exportFolder = CreateTempFolder();
        var aisFilePath = CopyTestDataToTemp("sample-gdt-utf8.gdt", "patient.gdt");
        var deviceFilePath = CopyTestDataToTemp("nidek-ark1s-sample.xml", "device.xml");
        var interfaceProfile = CreateInterfaceProfile(
            exportFolder,
            archiveProcessedFiles: false,
            clearAisImportFolderBeforeProcessing: true,
            clearDeviceImportFolderBeforeProcessing: true);

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            aisFilePath,
            deviceFilePath,
            new DateTime(2026, 6, 1, 12, 0, 0));

        Assert.True(result.Success);
        Assert.Null(result.ArchiveResult);
        Assert.False(File.Exists(aisFilePath));
        Assert.False(File.Exists(deviceFilePath));
        Assert.Contains("Bekannte verarbeitete Importdateien wurden aus dem Importordner entfernt:", result.Messages);
    }

    [Fact]
    public void Process_ShouldKeepExportFileWhenArchivingFails()
    {
        var exportFolder = CreateTempFolder();
        var aisFilePath = CopyTestDataToTemp("sample-gdt-utf8.gdt", "patient.gdt");
        var deviceFilePath = CopyTestDataToTemp("nidek-ark1s-sample.xml", "device.xml");
        var interfaceProfile = CreateInterfaceProfile(
            exportFolder,
            archiveFolder: string.Empty,
            archiveProcessedFiles: true);

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            aisFilePath,
            deviceFilePath,
            new DateTime(2026, 6, 1, 12, 0, 0));

        Assert.True(result.Success);
        Assert.NotNull(result.ExportFilePath);
        Assert.True(File.Exists(result.ExportFilePath));
        Assert.NotNull(result.ArchiveResult);
        Assert.True(result.ArchiveResult.HasErrors);
        Assert.Contains("Archivierung fehlgeschlagen: Archivordner fehlt.", result.Messages);
    }

    [Fact]
    public void Process_ShouldMoveFailedFilesWhenErrorFolderIsEnabled()
    {
        var errorFolder = CreateTempFolder();
        var aisFilePath = CopyTestDataToTemp("sample-gdt-utf8.gdt", "patient.gdt");
        var deviceFilePath = Path.Combine(CreateTempFolder(), "device.txt");
        File.WriteAllText(deviceFilePath, "device");
        var interfaceProfile = CreateInterfaceProfile(
            CreateTempFolder(),
            errorFolder: errorFolder,
            moveFailedFilesToErrorFolder: true);

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            aisFilePath,
            deviceFilePath,
            new DateTime(2026, 6, 1, 12, 0, 0));

        Assert.False(result.Success);
        Assert.Contains("Dieser Dateityp wird für die manuelle Paarverarbeitung noch nicht unterstützt.", result.Messages);
        Assert.NotNull(result.FailedFileCopyResult);
        Assert.False(result.FailedFileCopyResult.HasErrors);
        Assert.Equal(3, result.FailedFileCopyResult.CopiedFiles.Count);
        Assert.False(File.Exists(aisFilePath));
        Assert.False(File.Exists(deviceFilePath));
        Assert.Contains("Fehlerhafte Importdateien wurden in den Fehlerordner verschoben:", result.Messages);
    }

    [Fact]
    public void Process_ShouldReportMissingErrorFolderWhenErrorCopyIsEnabled()
    {
        var deviceFilePath = Path.Combine(CreateTempFolder(), "device.txt");
        File.WriteAllText(deviceFilePath, "device");
        var interfaceProfile = CreateInterfaceProfile(
            CreateTempFolder(),
            errorFolder: string.Empty,
            moveFailedFilesToErrorFolder: true);

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            deviceFilePath,
            DateTime.UtcNow);

        Assert.False(result.Success);
        Assert.Contains("Dieser Dateityp wird für die manuelle Paarverarbeitung noch nicht unterstützt.", result.Messages);
        Assert.NotNull(result.FailedFileCopyResult);
        Assert.True(result.FailedFileCopyResult.HasErrors);
        Assert.Contains("Fehlerordner ist nicht konfiguriert.", result.Messages);
    }

    [Fact]
    public void Process_ShouldRemoveOriginalFilesWhenErrorFolderIsEnabled()
    {
        var aisFilePath = CopyTestDataToTemp("sample-gdt-utf8.gdt", "patient.gdt");
        var deviceFilePath = Path.Combine(CreateTempFolder(), "device.txt");
        File.WriteAllText(deviceFilePath, "device");
        var interfaceProfile = CreateInterfaceProfile(
            CreateTempFolder(),
            errorFolder: CreateTempFolder(),
            moveFailedFilesToErrorFolder: true);

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            aisFilePath,
            deviceFilePath,
            DateTime.UtcNow);

        Assert.False(result.Success);
        Assert.False(File.Exists(aisFilePath));
        Assert.False(File.Exists(deviceFilePath));
    }

    private static InterfaceProfileDefinition CreateInterfaceProfile(
        string exportFolder,
        string archiveFolder = "",
        bool archiveProcessedFiles = false,
        ArchiveProcessedFileMode archiveProcessedFileMode = ArchiveProcessedFileMode.Copy,
        string errorFolder = "",
        bool moveFailedFilesToErrorFolder = true,
        bool clearAisImportFolderBeforeProcessing = false,
        bool clearDeviceImportFolderBeforeProcessing = false)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().Metadata with
            {
                Id = "interface-test",
                Name = "MEDISTAR + NIDEK ARK1S",
                IsBuiltIn = false,
                IsUserDefined = true
            },
            FolderOptions = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().FolderOptions with
            {
                ExportFolder = exportFolder,
                ArchiveFolder = archiveFolder,
                ArchiveProcessedFiles = archiveProcessedFiles,
                ArchiveProcessedFileMode = archiveProcessedFileMode,
                ErrorFolder = errorFolder,
                MoveFailedFilesToErrorFolder = moveFailedFilesToErrorFolder,
                ClearAisImportFolderBeforeProcessing = clearAisImportFolderBeforeProcessing,
                ClearDeviceImportFolderBeforeProcessing = clearDeviceImportFolderBeforeProcessing
            },
            IsActive = true
        };
    }

    private static InterfaceProfileDefinition CreateAttachmentOnlyInterfaceProfile(
        string exportFolder,
        bool clearDeviceImportFolderBeforeProcessing = false)
    {
        var template = DefaultInterfaceProfileDefinitions.CreateMedistarDocumentAttachmentDefault();
        return template with
        {
            Metadata = template.Metadata with
            {
                Id = "interface-document-attachment-test",
                Name = "MEDISTAR + Dokumentanhang",
                IsBuiltIn = false,
                IsUserDefined = true
            },
            FolderOptions = template.FolderOptions with
            {
                ExportFolder = exportFolder,
                AttachmentExportFolder = CreateTempFolder(),
                ClearDeviceImportFolderBeforeProcessing = clearDeviceImportFolderBeforeProcessing,
                IsAttachmentProcessingEnabled = true,
                IsAttachmentOnlyMode = true
            },
            IsActive = true
        };
    }

    private static InterfaceProfileDefinition CreateCv5000InterfaceProfile(
        string exportFolder,
        bool moveFailedFilesToErrorFolder = true)
    {
        var template = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default();
        return template with
        {
            Metadata = template.Metadata with
            {
                Id = "interface-medistar-topcon-cv5000-test",
                Name = "MEDISTAR + TOPCON CV-5000 - Konfiguration",
                IsBuiltIn = false,
                IsUserDefined = true
            },
            FolderOptions = template.FolderOptions with
            {
                ExportFolder = exportFolder,
                ErrorFolder = CreateTempFolder(),
                MoveFailedFilesToErrorFolder = moveFailedFilesToErrorFolder
            },
            IsActive = true
        };
    }

    private static AttachmentProcessingStatus CreateAttachmentStatus(IReadOnlyList<ExportFieldRecord> fields)
    {
        return new AttachmentProcessingStatus(
            WasAttempted: true,
            WasSkipped: false,
            Success: true,
            Reason: AttachmentProcessingStatusReason.PreparationSucceeded,
            Message: "XDT-Anhang vorbereitet.",
            SourcePath: @"C:\Import\report.pdf",
            TargetPath: fields.FirstOrDefault(field => field.FieldCode == "6305")?.Value,
            TargetFileName: "report.pdf",
            PreparedFields: fields);
    }

    private static AttachmentProcessingStatus CreateSkippedAttachmentStatus()
    {
        return new AttachmentProcessingStatus(
            WasAttempted: false,
            WasSkipped: true,
            Success: false,
            Reason: AttachmentProcessingStatusReason.NoSupportedAttachment,
            Message: "XDT-Anhang übersprungen: keine unterstützte Anhangdatei gefunden.",
            SourcePath: null,
            TargetPath: null,
            TargetFileName: null,
            PreparedFields: Array.Empty<ExportFieldRecord>());
    }

    private static AttachmentProcessingStatus CreateFailedAttachmentStatus()
    {
        return new AttachmentProcessingStatus(
            WasAttempted: true,
            WasSkipped: false,
            Success: false,
            Reason: AttachmentProcessingStatusReason.PreparationFailed,
            Message: "XDT-Anhang Vorbereitung fehlgeschlagen.",
            SourcePath: @"C:\Import\report.pdf",
            TargetPath: null,
            TargetFileName: null,
            PreparedFields: Array.Empty<ExportFieldRecord>());
    }

    private static string[] SplitXdtLines(string content)
    {
        return content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
    }

    private static string ExpectedXdtLineWithoutCrLf(string fieldCode, string value)
    {
        var declaredLength = 3 + 4 + value.Length + 2;
        return $"{declaredLength:D3}{fieldCode}{value}";
    }

    private static string CreateTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string GetTestDataPath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
    }

    private static string CopyTestDataToTemp(string testDataFileName, string targetFileName)
    {
        var folder = CreateTempFolder();
        var targetFilePath = Path.Combine(folder, targetFileName);
        File.Copy(GetTestDataPath(testDataFileName), targetFilePath);
        return targetFilePath;
    }

    private static string CopyCv5000TestDataToTemp(string testDataFileName, string targetFileName)
    {
        var folder = CreateTempFolder();
        var targetFilePath = Path.Combine(folder, targetFileName);
        File.Copy(GetCv5000TestDataPath(testDataFileName), targetFilePath);
        return targetFilePath;
    }

    private static string CreateTempFile(string fileName, string content)
    {
        var folder = CreateTempFolder();
        var filePath = Path.Combine(folder, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    private static string GetCv5000TestDataPath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Topcon", "CV5000", fileName);
    }

    private static string CreateDeviceDocumentFile(string fileName, string content = "attachment")
    {
        var folder = CreateTempFolder();
        var filePath = Path.Combine(folder, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }
}
