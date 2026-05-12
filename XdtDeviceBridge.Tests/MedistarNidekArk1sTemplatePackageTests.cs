using System.IO.Compression;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class MedistarNidekArk1sTemplatePackageTests
{
    private const string PackageId = "package-medistar-nidek-ark1s-v1";
    private const string DefaultAisProfileId = "ais-medistar-default";
    private const string DefaultDeviceProfileId = "device-nidek-ark1s-default";
    private const string DefaultExportProfileId = "export-medistar-nidek-ark1s-default";
    private const string DefaultInterfaceProfileId = "interface-medistar-nidek-ark1s-default";

    private static readonly DateTimeOffset Timestamp = new(2026, 5, 12, 10, 0, 0, TimeSpan.Zero);

    private readonly ProfileCatalogService _catalogService = new();
    private readonly TemplatePackageExporter _exporter = new();
    private readonly TemplatePackageImporter _importer = new();
    private readonly TemplatePackageImportValidator _validator = new();
    private readonly TemplatePackageImportConflictAnalyzer _analyzer = new();
    private readonly TemplatePackageImportPlanBuilder _planBuilder = new();
    private readonly TemplatePackageImportDryRunService _dryRunService = new();
    private readonly TemplatePackageImportPreviewService _previewService = new();
    private readonly TemplatePackageImportExecutor _executor = new();

    [Fact]
    public void Export_ShouldCreateReferencePackageWithExpectedZipStructure()
    {
        var zipPath = ExportReferencePackage();

        using var archive = ZipFile.OpenRead(zipPath);
        var entries = archive.Entries
            .Select(entry => entry.FullName)
            .OrderBy(entry => entry, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal(
            new[]
            {
                $"ais/{DefaultAisProfileId}.json",
                $"devices/{DefaultDeviceProfileId}.json",
                $"exports/{DefaultExportProfileId}.json",
                $"interfaces/{DefaultInterfaceProfileId}.json",
                "package.json"
            },
            entries);
    }

    [Fact]
    public void Export_ShouldContainOnlyExpectedReferenceProfilesAndAttachmentLinkSettings()
    {
        var importResult = _importer.Import(ExportReferencePackage());

        Assert.Equal(PackageId, importResult.Package.Metadata.Id);
        Assert.Equal("MEDISTAR + NIDEK ARK1S Referenzpaket V1", importResult.Package.Metadata.Name);
        Assert.Equal("1.0", importResult.Package.PackageFormatVersion);
        Assert.Equal(4, importResult.Package.IncludedProfiles.Count);

        Assert.Equal(DefaultAisProfileId, Assert.Single(importResult.AisProfiles).Metadata.Id);
        Assert.Equal(DefaultDeviceProfileId, Assert.Single(importResult.DeviceProfiles).Metadata.Id);
        Assert.Equal(DefaultExportProfileId, Assert.Single(importResult.ExportProfiles).Metadata.Id);

        var interfaceProfile = Assert.Single(importResult.InterfaceProfiles);
        Assert.Equal(DefaultInterfaceProfileId, interfaceProfile.Metadata.Id);
        Assert.Equal(DefaultAisProfileId, interfaceProfile.AisProfileId);
        Assert.Equal(DefaultDeviceProfileId, interfaceProfile.DeviceProfileId);
        Assert.Equal(DefaultExportProfileId, interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.False(interfaceProfile.FolderOptions.IsAttachmentProcessingEnabled);
        AssertReferenceAttachmentLinkSettings(interfaceProfile.FolderOptions);
    }

    [Fact]
    public void Export_ShouldNotContainLivePathsCustomerDataOrPatientData()
    {
        var archiveText = ReadAllZipText(ExportReferencePackage());
        var forbiddenMarkers = new[]
        {
            @"C:\",
            @"C:\\",
            @"\\",
            @"C:\MEDISTAR",
            @"C:\\MEDISTAR",
            "11253",
            "4711",
            "Muster",
            "Mara",
            "Mustermann",
            "MarcK",
            "M.Kurze",
            "Kunde",
            "Praxis Dr."
        };

        foreach (var marker in forbiddenMarkers)
        {
            Assert.DoesNotContain(marker, archiveText, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ImportFlow_ShouldProtectBuiltInsAndImportReferencePackageAsInactiveUserDefinedCopy()
    {
        var zipPath = ExportReferencePackage();
        var importResult = _importer.Import(zipPath);
        var paths = CreateAppDataPaths();
        _catalogService.EnsureDefaultProfiles(paths);
        var existingCatalog = _catalogService.Load(paths);

        var validation = _validator.Validate(importResult);
        var analysis = _analyzer.Analyze(importResult, existingCatalog);
        var plan = _planBuilder.Build(analysis, Timestamp);
        var dryRun = _dryRunService.Preview(importResult, plan, existingCatalog);
        var execution = _executor.Execute(importResult, plan, dryRun, paths, Timestamp, "Tests");
        var loadedCatalog = _catalogService.Load(paths);

        Assert.DoesNotContain(validation.Issues, issue => issue.Severity == TemplatePackageImportValidationIssueSeverity.Error);
        Assert.True(analysis.Success);
        Assert.Equal(4, analysis.ProfileDecisions.Count);
        Assert.All(analysis.ProfileDecisions, decision =>
        {
            Assert.Equal(TemplatePackageImportExistingProfileSource.BuiltIn, decision.ExistingProfileSource);
            Assert.Equal(TemplatePackageImportAction.ImportAsCopy, decision.SuggestedAction);
            Assert.False(decision.IsBlocking);
        });

        Assert.False(plan.HasBlockingItems);
        Assert.Equal(4, plan.PlannedImportAsCopy);
        Assert.Equal(0, plan.PlannedReplaceExisting);

        Assert.True(dryRun.Success);
        Assert.Empty(dryRun.BlockingItems);
        Assert.Equal(4, dryRun.WouldImportAsCopy);
        Assert.Equal(0, dryRun.WouldReplaceExisting);
        Assert.Contains(dryRun.Warnings, warning => warning.Contains("will not be activated automatically", StringComparison.OrdinalIgnoreCase));

        Assert.True(execution.Success);
        Assert.Equal(4, execution.ImportedAsCopy);
        Assert.Equal(0, execution.ImportedAsNew);
        Assert.Equal(0, execution.Blocked);
        Assert.Equal(0, execution.Failed);

        AssertBuiltInAndImportedCopy(loadedCatalog.AisProfiles.Select(profile => profile.Metadata), DefaultAisProfileId);
        AssertBuiltInAndImportedCopy(loadedCatalog.DeviceProfiles.Select(profile => profile.Metadata), DefaultDeviceProfileId);
        AssertBuiltInAndImportedCopy(loadedCatalog.ExportProfiles.Select(profile => profile.Metadata), DefaultExportProfileId);

        var builtInInterface = Assert.Single(loadedCatalog.InterfaceProfiles, profile => profile.Metadata.Id == DefaultInterfaceProfileId);
        Assert.True(builtInInterface.Metadata.IsBuiltIn);
        Assert.False(builtInInterface.Metadata.IsUserDefined);
        Assert.False(builtInInterface.IsActive);

        var importedInterface = Assert.Single(loadedCatalog.InterfaceProfiles, profile => profile.Metadata.Id == $"{DefaultInterfaceProfileId}-import");
        Assert.True(importedInterface.Metadata.IsUserDefined);
        Assert.False(importedInterface.Metadata.IsBuiltIn);
        Assert.False(importedInterface.IsActive);
        Assert.False(importedInterface.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Equal($"{DefaultAisProfileId}-import", importedInterface.AisProfileId);
        Assert.Equal($"{DefaultDeviceProfileId}-import", importedInterface.DeviceProfileId);
        Assert.Equal($"{DefaultExportProfileId}-import", importedInterface.ExportProfileId);
        AssertReferenceAttachmentLinkSettings(importedInterface.FolderOptions);
    }

    [Fact]
    public void ImportPreviewService_ShouldCreateUiPreviewForReferencePackageWithoutWritingProfiles()
    {
        var zipPath = ExportReferencePackage();
        var paths = CreateAppDataPaths();
        _catalogService.EnsureDefaultProfiles(paths);
        var existingCatalog = _catalogService.Load(paths);
        var filesBefore = GetProfileFiles(paths);

        var preview = _previewService.Create(zipPath, existingCatalog);

        var filesAfter = GetProfileFiles(paths);
        Assert.Equal(filesBefore, filesAfter);
        Assert.DoesNotContain(filesAfter, path => path.Contains("-import", StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(
            preview.ValidationResult.Issues,
            issue => issue.Severity == TemplatePackageImportValidationIssueSeverity.Error);
        Assert.True(preview.AnalysisResult.Success);
        Assert.True(preview.DryRunResult.Success);
        Assert.Empty(preview.DryRunResult.BlockingItems);
        Assert.Equal(4, preview.Display.Rows.Count);
        Assert.Equal(3, preview.Display.DependencyRows.Count);
        Assert.Equal(4, preview.DryRunResult.WouldImportAsCopy);
        Assert.Equal(0, preview.DryRunResult.WouldReplaceExisting);
        Assert.All(preview.Display.Rows, row =>
        {
            Assert.Equal(TemplatePackageImportAction.ImportAsCopy, row.SelectedAction);
            Assert.Contains(row.AvailableActions, action => action.Action == TemplatePackageImportAction.Skip);
        });
    }

    private string ExportReferencePackage()
    {
        var zipPath = CreateTempZipPath();
        _exporter.Export(zipPath, CreateReferenceRequest());
        return zipPath;
    }

    private static TemplatePackageExportRequest CreateReferenceRequest()
    {
        var aisProfile = DefaultAisProfiles.CreateMedistarDefault();
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var package = new TemplatePackage(
            Metadata: new ProfileMetadata(
                Id: PackageId,
                Name: "MEDISTAR + NIDEK ARK1S Referenzpaket V1",
                ProfileKind: ProfileKind.TemplatePackage,
                Description: "Official V1 reference package for MEDISTAR and NIDEK ARK1S.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/NIDEK ARK1S",
                Version: "1.0.0",
                CreatedAt: Timestamp,
                UpdatedAt: Timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            IncludedProfiles: new[]
            {
                aisProfile.Metadata,
                deviceProfile.Metadata,
                exportProfile.Metadata,
                interfaceProfile.Metadata
            },
            PackageFormatVersion: "1.0",
            CreatedAt: Timestamp.UtcDateTime,
            CreatedBy: "XdtDeviceBridge",
            Description: "Reproducible test package for the validated MEDISTAR + NIDEK ARK1S workflow.");

        return new TemplatePackageExportRequest(
            Package: package,
            AisProfiles: new[] { aisProfile },
            DeviceProfiles: new[] { deviceProfile },
            ExportProfiles: new[] { exportProfile },
            InterfaceProfiles: new[] { interfaceProfile });
    }

    private static void AssertReferenceAttachmentLinkSettings(InterfaceFolderOptions options)
    {
        Assert.Equal("Datei", options.AttachmentExternalLinkDocumentName);
        Assert.Equal("{ExtensionUpperWithoutDot}", options.AttachmentExternalLinkFileFormat);
        Assert.Equal("", options.AttachmentExternalLinkDescription);
        Assert.Equal("{Attachment.TargetFullPath}", options.AttachmentExternalLinkPathTemplate);
        Assert.Equal(AttachmentRequirementMode.Optional, options.AttachmentRequirementMode);
        Assert.Equal("", options.AttachmentImportFolder);
        Assert.Equal("", options.AttachmentExportFolder);
    }

    private static void AssertBuiltInAndImportedCopy(IEnumerable<ProfileMetadata> metadata, string sourceId)
    {
        Assert.Contains(metadata, profile => profile.Id == sourceId && profile.IsBuiltIn && !profile.IsUserDefined);
        Assert.Contains(metadata, profile => profile.Id == $"{sourceId}-import" && profile.IsUserDefined && !profile.IsBuiltIn);
    }

    private static string ReadAllZipText(string zipPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);

        return string.Join(
            Environment.NewLine,
            archive.Entries
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Name))
                .OrderBy(entry => entry.FullName, StringComparer.OrdinalIgnoreCase)
                .Select(ReadEntry));
    }

    private static string ReadEntry(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string CreateTempZipPath()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "medistar-nidek-ark1s-v1.templatepackage.zip");
    }

    private static AppDataPaths CreateAppDataPaths()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return new AppDataPathProvider().GetPaths(baseFolder);
    }

    private static string[] GetProfileFiles(AppDataPaths paths)
    {
        return Directory.Exists(paths.ProfilesFolder)
            ? Directory.GetFiles(paths.ProfilesFolder, "*.json", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : Array.Empty<string>();
    }
}
