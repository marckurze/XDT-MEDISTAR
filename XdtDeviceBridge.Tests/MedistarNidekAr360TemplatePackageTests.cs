using System.IO.Compression;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class MedistarNidekAr360TemplatePackageTests
{
    private const string PackageId = "package-medistar-nidek-ar360-v1";
    private const string DefaultAisProfileId = "ais-medistar-default";
    private const string DefaultDeviceProfileId = "device-nidek-ar360-default";
    private const string DefaultExportProfileId = "export-medistar-nidek-ar360-default";
    private const string DefaultInterfaceProfileId = "interface-medistar-nidek-ar360-default";

    private static readonly DateTimeOffset Timestamp = new(2026, 5, 17, 12, 0, 0, TimeSpan.Zero);

    private readonly ProfileCatalogService _catalogService = new();
    private readonly TemplatePackageExportSelectionService _selectionExportService = new();
    private readonly TemplatePackageExporter _exporter = new();
    private readonly TemplatePackageImporter _importer = new();
    private readonly TemplatePackageImportValidator _validator = new();
    private readonly TemplatePackageImportConflictAnalyzer _analyzer = new();
    private readonly TemplatePackageImportPlanBuilder _planBuilder = new();
    private readonly TemplatePackageImportSelectionService _selectionService = new();
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
    public void Export_ShouldContainOnlyExpectedReferenceProfiles()
    {
        var importResult = _importer.Import(ExportReferencePackage());

        Assert.Equal(PackageId, importResult.Package.Metadata.Id);
        Assert.Equal("MEDISTAR + NIDEK AR360 Templatepaket V1", importResult.Package.Metadata.Name);
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

        var includedIds = importResult.Package.IncludedProfiles.Select(profile => profile.Id).ToArray();
        Assert.DoesNotContain(includedIds, id => id.Contains("ark1s", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(includedIds, id => id.Contains("lm7", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(includedIds, id => id.Contains("nt530", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(includedIds, id => id.Contains("topcon", StringComparison.OrdinalIgnoreCase));
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
            "4701-1",
            "Testfrau",
            "Anna",
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
        var basePlan = _planBuilder.Build(analysis, Timestamp);
        var plan = _selectionService.Apply(basePlan, CreateCopySelections(basePlan));
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

        Assert.All(basePlan.ProfilePlans, profilePlan => Assert.Equal(TemplatePackageImportAction.Skip, profilePlan.PlannedAction));
        Assert.False(plan.HasBlockingItems);
        Assert.Equal(4, plan.PlannedImportAsCopy);
        Assert.Equal(0, plan.PlannedReplaceExisting);

        Assert.True(dryRun.Success);
        Assert.Empty(dryRun.BlockingItems);
        Assert.Equal(4, dryRun.WouldImportAsCopy);
        Assert.Equal(0, dryRun.WouldReplaceExisting);
        Assert.Contains(dryRun.Warnings, warning => warning.Contains("wird nicht automatisch aktiviert", StringComparison.OrdinalIgnoreCase));

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
        Assert.Empty(preview.Display.DependencyRows);
        Assert.Contains("übersprungen", preview.Display.DependencyEmptyStateMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, preview.DryRunResult.WouldImportAsCopy);
        Assert.Equal(4, preview.DryRunResult.WouldSkip);
        Assert.Equal(0, preview.DryRunResult.WouldReplaceExisting);
        Assert.All(preview.Display.Rows, row =>
        {
            Assert.Equal(TemplatePackageImportAction.Skip, row.SelectedAction);
            Assert.False(row.IsTargetNameEditable);
            Assert.Contains(row.AvailableActions, action => action.Action == TemplatePackageImportAction.ImportAsCopy);
            Assert.Contains(row.AvailableActions, action => action.Action == TemplatePackageImportAction.Skip);
        });
    }

    private string ExportReferencePackage()
    {
        var zipPath = CreateTempZipPath();
        var result = _selectionExportService.CreateForInterfaceProfile(CreateDefaultCatalog(), DefaultInterfaceProfileId, Timestamp);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal("medistar-nidek-ar360-v1.templatepackage.zip", result.SuggestedFileName);
        _exporter.Export(zipPath, result.Request!);
        return zipPath;
    }

    private static ProfileCatalog CreateDefaultCatalog()
    {
        return new ProfileCatalog(
            AisProfiles: new[] { DefaultAisProfiles.CreateMedistarDefault() },
            DeviceProfiles: new[]
            {
                DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault(),
                DefaultDeviceProfileDefinitions.CreateNidekAr360Default(),
                DefaultDeviceProfileDefinitions.CreateNidekLm7Default(),
                DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault(),
                DefaultDeviceProfileDefinitions.CreateTopconCl300Default(),
                DefaultDeviceProfileDefinitions.CreateTopconKr800Default(),
                DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault()
            },
            ExportProfiles: new[]
            {
                DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
                DefaultExportProfileDefinitions.CreateMedistarNidekAr360Default(),
                DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default(),
                DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault(),
                DefaultExportProfileDefinitions.CreateMedistarTopconCl300Default(),
                DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default(),
                DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault()
            },
            InterfaceProfiles: new[]
            {
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekAr360Default()
            });
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
        return Path.Combine(folder, "medistar-nidek-ar360-v1.templatepackage.zip");
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

    private static IReadOnlyList<TemplatePackageImportUserSelection> CreateCopySelections(TemplatePackageImportPlan plan)
    {
        return plan.ProfilePlans
            .Select(profilePlan => new TemplatePackageImportUserSelection(
                ProfileKind: profilePlan.ProfileKind,
                ImportedProfileId: profilePlan.ImportedProfileId,
                SelectedAction: TemplatePackageImportAction.ImportAsCopy,
                TargetProfileId: null,
                TargetProfileName: null,
                IsValid: true,
                ValidationMessage: null))
            .ToList();
    }
}
