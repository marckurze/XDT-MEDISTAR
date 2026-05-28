using System.Text;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class XdtBaukastenPreviewServiceTests
{
    [Fact]
    public void BuildPreview_ShouldSeparateAisViewAndDiagnostics()
    {
        using var temp = new TempFolder();
        var aisPath = WriteGdt(temp.Path);
        var devicePath = CopyLm7Fixture(temp.Path);
        var state = CreateLm7State(aisPath, devicePath);
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm7Default());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.Contains("R.:S=+ 6.25 Z=- 3.25*  3", result.Output.AisView);
        Assert.DoesNotContain("Patient:", result.Output.AisView);
        Assert.DoesNotContain("Untersuchungsart:", result.Output.AisView);
        Assert.DoesNotContain("Karteikartenansicht", result.Output.AisView);
        Assert.DoesNotContain("8402:", result.Output.AisView);
        Assert.DoesNotContain("6228:", result.Output.AisView);
        Assert.DoesNotContain("Measure[@Type='LM']", result.Output.AisView);
        Assert.Contains("Messwerte erkannt:", result.Output.Diagnostics);
        Assert.Contains("Measure[@Type='LM']", result.Output.Diagnostics);
        Assert.Contains("6228", result.Output.RawXdt);
    }

    [Fact]
    public void BuildPreview_ShouldRenderLiteralRuleWithoutSourcePath()
    {
        using var temp = new TempFolder();
        var aisPath = WriteGdt(temp.Path);
        var devicePath = CopyLm7Fixture(temp.Path);
        var state = CreateLm7State(aisPath, devicePath);
        state.AddWorkingRule(new ExportRuleDefinition(
            "literal-note",
            "6228",
            "Feste Überschrift",
            ExportRuleType.Template,
            null,
            "Phoropter finaler Verordnungswert",
            999,
            true,
            null));
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm7Default());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.Contains("6228Phoropter finaler Verordnungswert", result.Output.RawXdt);
        Assert.Contains("Phoropter finaler Verordnungswert", result.Output.AisView);
    }

    [Fact]
    public void BuildPreview_ShouldKeepAr360BaukastenReferenceWorking()
    {
        using var temp = new TempFolder();
        var aisPath = WriteGdt(temp.Path, "AR360");
        var devicePath = CopyFixture(temp.Path, "Devices", "Nidek", "AR360", "AR360.xml");
        var state = CreateState(
            aisPath,
            devicePath,
            DefaultDeviceProfileDefinitions.CreateNidekAr360Default(),
            DefaultExportProfileDefinitions.CreateMedistarNidekAr360Default());
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarNidekAr360Default());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.Contains("6228", result.Output.RawXdt);
        Assert.Contains("R.:", result.Output.AisView);
        Assert.DoesNotContain("passt nicht", string.Join(Environment.NewLine, result.Messages), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildPreview_ShouldCreateCv5000PhoropterPreview()
    {
        using var temp = new TempFolder();
        var aisPath = CopyFixture(temp.Path, "Devices", "Topcon", "CV5000", "Patient_mit_Phoropter_Daten.XDT");
        var devicePath = CopyFixture(temp.Path, "Devices", "Topcon", "CV5000", "M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml");
        var state = CreateState(
            aisPath,
            devicePath,
            DefaultDeviceProfileDefinitions.CreateTopconCv5000Default(),
            DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default());
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.Contains("6228Phoropter finaler Verordnungswert", result.Output.RawXdt);
        Assert.Contains("6227Phoropter Maximalwert", result.Output.RawXdt);
        Assert.DoesNotContain("6330", result.Output.RawXdt);
        Assert.Contains("Phoropter finaler Verordnungswert", result.Output.AisView);
        Assert.DoesNotContain("6228:", result.Output.AisView);
        Assert.Contains("<Ophthalmology", result.Output.DeviceOutput);
    }

    private static XdtBaukastenState CreateLm7State(string aisPath, string devicePath)
    {
        return CreateState(
            aisPath,
            devicePath,
            DefaultDeviceProfileDefinitions.CreateNidekLm7Default(),
            DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default());
    }

    private static XdtBaukastenState CreateState(
        string aisPath,
        string devicePath,
        DeviceProfileDefinition deviceProfile,
        ExportProfileDefinition exportProfile)
    {
        var state = new XdtBaukastenState();
        state.SetAisProfile(DefaultAisProfiles.CreateMedistarDefault());
        state.SetDeviceProfile(deviceProfile);
        state.SetExportProfile(exportProfile);
        state.SetAisInput(new XdtBaukastenLoadedInput(aisPath, Path.GetFileName(aisPath), File.ReadAllText(aisPath, Encoding.UTF8)));
        state.SetDeviceInput(new XdtBaukastenLoadedInput(devicePath, Path.GetFileName(devicePath), File.ReadAllText(devicePath, Encoding.UTF8)));
        return state;
    }

    private static string WriteGdt(string folder, string examType = "LM7")
    {
        var path = Path.Combine(folder, "patient.gdt");
        File.WriteAllText(path, string.Concat(
            BuildGdtLine("3000", "4701-1"),
            BuildGdtLine("3101", "Testfrau"),
            BuildGdtLine("3102", "Anna"),
            BuildGdtLine("3103", "12061955"),
            BuildGdtLine("8402", examType)), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return path;
    }

    private static string BuildGdtLine(string fieldCode, string value)
    {
        var length = 3 + fieldCode.Length + value.Length;
        return $"{length:D3}{fieldCode}{value}\n";
    }

    private static string CopyLm7Fixture(string folder)
    {
        return CopyFixture(folder, "Devices", "Nidek", "LM7", "NIDEK LM7.xml");
    }

    private static string CopyFixture(string folder, params string[] pathParts)
    {
        var source = Path.Combine(new[] { AppContext.BaseDirectory, "TestData" }.Concat(pathParts).ToArray());
        var target = Path.Combine(folder, Path.GetFileName(source));
        File.Copy(source, target);
        return target;
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
