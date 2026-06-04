using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class ProfileManagementServiceTests
{
    private readonly ProfileCatalogService _catalogService = new();
    private readonly XdtBaukastenTemplateLibraryService _templateLibraryService = new();
    private readonly ProfileManagementService _service = new();

    [Fact]
    public void BuildRows_ShouldIncludeProfileKindsLocalTemplatesAndMaintenance()
    {
        using var temp = new TempFolder();
        var paths = new AppDataPathProvider().GetPaths(temp.Path);
        _catalogService.EnsureDefaultProfiles(paths);
        var catalog = _catalogService.Load(paths);
        Directory.CreateDirectory(paths.TemplatePackagesFolder);
        File.WriteAllText(Path.Combine(paths.TemplatePackagesFolder, "sample.templatepackage.zip"), "template-package");
        var templatePath = _templateLibraryService.CreateDefaultFilePath(paths, "MEDISTAR + Testgeraet");
        _templateLibraryService.Save(templatePath, CreateMinimalTemplate(), overwriteExisting: false);

        var rows = _service.BuildRows(catalog, paths, _templateLibraryService);

        Assert.Contains(rows, row => row.Kind == ProfileManagementRowKind.AisProfile);
        Assert.Contains(rows, row => row.Kind == ProfileManagementRowKind.DeviceProfile);
        Assert.Contains(rows, row => row.Kind == ProfileManagementRowKind.ExportProfile);
        Assert.Contains(rows, row => row.Kind == ProfileManagementRowKind.InterfaceProfile);
        Assert.Contains(rows, row => row.Kind == ProfileManagementRowKind.TemplatePackage);
        Assert.Contains(rows, row => row.Kind == ProfileManagementRowKind.XdtBaukastenTemplate);
        Assert.Contains(rows, row => row.Kind == ProfileManagementRowKind.Maintenance);
        Assert.Contains(rows, row => row.IsBuiltIn && row.CanDuplicate);
        Assert.Contains(rows, row => row.Kind == ProfileManagementRowKind.XdtBaukastenTemplate && row.CanOpenInWorkbench);
    }

    [Fact]
    public void EvaluateDelete_ShouldBlockBuiltInsAndReferencedUserDefinedProfiles()
    {
        using var temp = new TempFolder();
        var paths = new AppDataPathProvider().GetPaths(temp.Path);
        _catalogService.EnsureDefaultProfiles(paths);
        var catalog = _catalogService.Load(paths);
        var builtInAisRow = _service.BuildRows(catalog, paths, _templateLibraryService)
            .First(row => row.Kind == ProfileManagementRowKind.AisProfile && row.IsBuiltIn);

        var builtInEvaluation = _service.EvaluateDelete(catalog, builtInAisRow);

        Assert.False(builtInEvaluation.Success);
        Assert.Contains("BuiltIn", builtInEvaluation.Message);

        var duplicate = _service.Duplicate(
            catalog,
            paths,
            builtInAisRow,
            new DateTimeOffset(2026, 6, 4, 12, 0, 0, TimeSpan.Zero),
            "test");
        Assert.True(duplicate.Success);
        catalog = _catalogService.Load(paths);
        var reference = catalog.InterfaceProfiles.First() with
        {
            Metadata = catalog.InterfaceProfiles.First().Metadata with
            {
                Id = "interface-reference-userdefined-ais",
                Name = "Referenziert UserDefined AIS",
                IsBuiltIn = false,
                IsUserDefined = true
            },
            AisProfileId = duplicate.AisProfileId!,
            IsActive = false
        };
        _catalogService.SaveNewInterfaceProfileDefinition(paths, reference);
        catalog = _catalogService.Load(paths);
        var referencedRow = _service.BuildRows(catalog, paths, _templateLibraryService)
            .First(row => row.Kind == ProfileManagementRowKind.AisProfile && row.Id == duplicate.AisProfileId);

        var referencedEvaluation = _service.EvaluateDelete(catalog, referencedRow);

        Assert.False(referencedEvaluation.Success);
        Assert.Contains("wird noch verwendet", referencedEvaluation.Message);
    }

    [Fact]
    public void DuplicateAndDelete_ShouldCreateAndRemoveUserDefinedDeviceCopy()
    {
        using var temp = new TempFolder();
        var paths = new AppDataPathProvider().GetPaths(temp.Path);
        _catalogService.EnsureDefaultProfiles(paths);
        var catalog = _catalogService.Load(paths);
        var deviceRow = _service.BuildRows(catalog, paths, _templateLibraryService)
            .First(row => row.Kind == ProfileManagementRowKind.DeviceProfile && row.IsBuiltIn);

        var duplicate = _service.Duplicate(
            catalog,
            paths,
            deviceRow,
            new DateTimeOffset(2026, 6, 4, 12, 0, 0, TimeSpan.Zero),
            "test");

        Assert.True(duplicate.Success);
        catalog = _catalogService.Load(paths);
        var duplicateRow = _service.BuildRows(catalog, paths, _templateLibraryService)
            .First(row => row.Kind == ProfileManagementRowKind.DeviceProfile && row.Id == duplicate.DeviceProfileId);
        Assert.False(duplicateRow.IsBuiltIn);
        Assert.True(duplicateRow.IsUserDefined);
        Assert.True(duplicateRow.CanDelete);

        var delete = _service.Delete(catalog, paths, duplicateRow);

        Assert.True(delete.Success);
        catalog = _catalogService.Load(paths);
        Assert.DoesNotContain(catalog.DeviceProfiles, profile => profile.Metadata.Id == duplicate.DeviceProfileId);
    }

    private static XdtBaukastenTemplate CreateMinimalTemplate()
    {
        return new XdtBaukastenTemplate(
            Id: "baukasten-template-minimal",
            Name: "Minimal",
            Description: null,
            SavedAt: DateTimeOffset.UtcNow,
            SavedBy: null,
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-nidek-ark1s-default",
            ExportProfileId: "export-medistar-nidek-ark1s-default",
            AisExportRules: Array.Empty<ExportRuleDefinition>(),
            DeviceOutputRules: Array.Empty<ExportRuleDefinition>());
    }

    private sealed class TempFolder : IDisposable
    {
        public TempFolder()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
