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

    private static XdtBaukastenState CreateLm7State(string aisPath, string devicePath)
    {
        var state = new XdtBaukastenState();
        state.SetAisProfile(DefaultAisProfiles.CreateMedistarDefault());
        state.SetDeviceProfile(DefaultDeviceProfileDefinitions.CreateNidekLm7Default());
        state.SetExportProfile(DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default());
        state.SetAisInput(new XdtBaukastenLoadedInput(aisPath, Path.GetFileName(aisPath), File.ReadAllText(aisPath, Encoding.UTF8)));
        state.SetDeviceInput(new XdtBaukastenLoadedInput(devicePath, Path.GetFileName(devicePath), File.ReadAllText(devicePath, Encoding.UTF8)));
        return state;
    }

    private static string WriteGdt(string folder)
    {
        var path = Path.Combine(folder, "patient.gdt");
        File.WriteAllText(path, string.Concat(
            BuildGdtLine("3000", "4701-1"),
            BuildGdtLine("3101", "Testfrau"),
            BuildGdtLine("3102", "Anna"),
            BuildGdtLine("3103", "12061955"),
            BuildGdtLine("8402", "LM7")), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return path;
    }

    private static string BuildGdtLine(string fieldCode, string value)
    {
        var length = 3 + fieldCode.Length + value.Length;
        return $"{length:D3}{fieldCode}{value}\n";
    }

    private static string CopyLm7Fixture(string folder)
    {
        var source = Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Nidek", "LM7", "NIDEK LM7.xml");
        var target = Path.Combine(folder, "NIDEK LM7.xml");
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
