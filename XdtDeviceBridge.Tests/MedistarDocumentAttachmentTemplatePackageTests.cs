using System.IO.Compression;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class MedistarDocumentAttachmentTemplatePackageTests
{
    private const string DefaultAisProfileId = "ais-medistar-default";
    private const string DefaultDeviceProfileId = "device-document-attachment-default";
    private const string DefaultExportProfileId = "export-medistar-document-attachment-default";
    private const string DefaultInterfaceProfileId = "interface-medistar-document-attachment-default";

    private static readonly DateTimeOffset Timestamp = new(2026, 5, 20, 12, 0, 0, TimeSpan.Zero);

    private readonly TemplatePackageExportSelectionService _selectionExportService = new();
    private readonly TemplatePackageExporter _exporter = new();
    private readonly TemplatePackageImporter _importer = new();

    [Fact]
    public void Export_ShouldContainOnlyDocumentAttachmentProfileAndDependencies()
    {
        var importResult = _importer.Import(ExportTemplateCandidate());

        Assert.Equal(DefaultAisProfileId, Assert.Single(importResult.AisProfiles).Metadata.Id);
        Assert.Equal(DefaultDeviceProfileId, Assert.Single(importResult.DeviceProfiles).Metadata.Id);
        Assert.Equal(DefaultExportProfileId, Assert.Single(importResult.ExportProfiles).Metadata.Id);

        var interfaceProfile = Assert.Single(importResult.InterfaceProfiles);
        Assert.Equal(DefaultInterfaceProfileId, interfaceProfile.Metadata.Id);
        Assert.True(interfaceProfile.FolderOptions.IsAttachmentOnlyMode);
        Assert.True(interfaceProfile.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Equal(AttachmentRequirementMode.Required, interfaceProfile.FolderOptions.AttachmentRequirementMode);
        Assert.Equal(AttachmentCompletionMode.WaitForQuietPeriod, interfaceProfile.FolderOptions.AttachmentCompletionMode);
        Assert.Equal(10, interfaceProfile.FolderOptions.AttachmentQuietPeriodSeconds);

        var includedIds = importResult.Package.IncludedProfiles.Select(profile => profile.Id).ToArray();
        Assert.DoesNotContain(includedIds, id => id.Contains("ark1s", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(includedIds, id => id.Contains("ar360", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(includedIds, id => id.Contains("lm7", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(includedIds, id => id.Contains("nt530p", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Export_ShouldCreateExpectedZipStructure()
    {
        using var archive = ZipFile.OpenRead(ExportTemplateCandidate());
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

    private string ExportTemplateCandidate()
    {
        var zipPath = CreateTempZipPath();
        var result = _selectionExportService.CreateForInterfaceProfile(CreateDefaultCatalog(), DefaultInterfaceProfileId, Timestamp);

        Assert.True(result.Success, result.ErrorMessage);
        _exporter.Export(zipPath, result.Request!);
        return zipPath;
    }

    private static ProfileCatalog CreateDefaultCatalog()
    {
        return new ProfileCatalog(
            AisProfiles: new[] { DefaultAisProfiles.CreateMedistarDefault() },
            DeviceProfiles: new[] { DefaultDeviceProfileDefinitions.CreateDocumentAttachmentDefault() },
            ExportProfiles: new[] { DefaultExportProfileDefinitions.CreateMedistarDocumentAttachmentDefault() },
            InterfaceProfiles: new[] { DefaultInterfaceProfileDefinitions.CreateMedistarDocumentAttachmentDefault() });
    }

    private static string CreateTempZipPath()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "medistar-dokumentanhang-v1.templatepackage.zip");
    }
}
