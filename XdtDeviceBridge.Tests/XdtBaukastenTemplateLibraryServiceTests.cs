using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class XdtBaukastenTemplateLibraryServiceTests
{
    [Fact]
    public void SaveAndLoad_ShouldPreserveWorkbenchTemplateRules()
    {
        using var temp = new TempFolder();
        var paths = CreatePaths(temp.Path);
        var service = new XdtBaukastenTemplateLibraryService();
        var filePath = service.CreateDefaultFilePath(paths, "MEDISTAR + Testgerät");
        var template = new XdtBaukastenTemplate(
            Id: "baukasten-template-test",
            Name: "MEDISTAR + Testgerät",
            Description: "Arbeitskopie aus dem XDT-Baukasten",
            SavedAt: new DateTimeOffset(2026, 6, 4, 12, 0, 0, TimeSpan.Zero),
            SavedBy: "test",
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-test",
            ExportProfileId: "export-test",
            AisExportRules:
            [
                new ExportRuleDefinition(
                    "ais-rule",
                    "6228",
                    "PrescriptionRight",
                    ExportRuleType.Template,
                    "Device.Measure/R/Sphere",
                    "R {value}",
                    2,
                    true,
                    "AIS-Regel")
            ],
            DeviceOutputRules:
            [
                new ExportRuleDefinition(
                    "device-output-rule",
                    "DeviceOutput/Patient/No",
                    "PatientNumber",
                    ExportRuleType.Template,
                    "AIS.PatientNumber",
                    "{value}",
                    1,
                    true,
                    "Geräteausgabe-Regel")
            ]);

        service.Save(filePath, template, overwriteExisting: false);
        var loaded = service.Load(filePath);

        Assert.Equal(template.Id, loaded.Id);
        Assert.Equal(template.Name, loaded.Name);
        Assert.Equal("ais-medistar-default", loaded.AisProfileId);
        Assert.Single(loaded.AisExportRules);
        Assert.Single(loaded.DeviceOutputRules);
        Assert.Equal("6228", loaded.AisExportRules[0].TargetFieldCode);
        Assert.Equal("DeviceOutput/Patient/No", loaded.DeviceOutputRules[0].TargetFieldCode);
        Assert.Contains(filePath, service.ListTemplateFiles(paths));
    }

    [Fact]
    public void Save_ShouldNotOverwriteExistingTemplateByDefault()
    {
        using var temp = new TempFolder();
        var paths = CreatePaths(temp.Path);
        var service = new XdtBaukastenTemplateLibraryService();
        var filePath = service.CreateDefaultFilePath(paths, "Doppeltes Template");
        var template = CreateMinimalTemplate();

        service.Save(filePath, template, overwriteExisting: false);

        Assert.Throws<InvalidOperationException>(() => service.Save(filePath, template, overwriteExisting: false));
    }

    private static XdtBaukastenTemplate CreateMinimalTemplate()
    {
        return new XdtBaukastenTemplate(
            Id: "baukasten-template-minimal",
            Name: "Minimal",
            Description: null,
            SavedAt: DateTimeOffset.UtcNow,
            SavedBy: null,
            AisProfileId: "ais",
            DeviceProfileId: "device",
            ExportProfileId: "export",
            AisExportRules: Array.Empty<ExportRuleDefinition>(),
            DeviceOutputRules: Array.Empty<ExportRuleDefinition>());
    }

    private static AppDataPaths CreatePaths(string baseFolder)
    {
        return new AppDataPaths(
            BaseFolder: baseFolder,
            ProfilesFolder: Path.Combine(baseFolder, "profiles"),
            TemplatesFolder: Path.Combine(baseFolder, "templates"),
            LicensesFolder: Path.Combine(baseFolder, "licenses"),
            LogsFolder: Path.Combine(baseFolder, "logs"),
            InstallationInfoFile: Path.Combine(baseFolder, "installation.json"),
            LicenseFile: Path.Combine(baseFolder, "license.json"),
            DeviceGracePeriodsFile: Path.Combine(baseFolder, "grace.json"),
            LicenseRequestsFolder: Path.Combine(baseFolder, "license-requests"),
            TemplatePackagesFolder: Path.Combine(baseFolder, "template-packages"));
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
