using System.Text;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class BuilderManualProcessingPreviewServiceTests
{
    [Fact]
    public void BuildPreview_ForRt6100ShouldUseProfileAwarePhoropterHistoryFallback()
    {
        var service = new BuilderManualProcessingPreviewService();
        var result = service.BuildPreview(new BuilderManualProcessingPreviewRequest(
            InterfaceProfile: DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt6100Default(),
            DeviceProfile: DefaultDeviceProfileDefinitions.CreateNidekRt6100Default(),
            ExportProfile: DefaultExportProfileDefinitions.CreateMedistarNidekRt6100Default(),
            AisFilePath: GetCv5000FixturePath("Patient_mit_Phoropter_Daten.XDT"),
            DeviceFilePath: WriteTempXml(CreateRt6100ReturnXml())));

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.NotNull(result.Patient);
        Assert.Equal("4701-1", result.Patient!.PatientNumber);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == ProcessingIssueSeverity.Warning
            && issue.Message.Contains("RT-6100-Rueckweg", StringComparison.Ordinal));
        Assert.Contains(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='RT']/Best/HeaderLine");
        Assert.Contains(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='RT']/Full/R/MedistarLine");
        Assert.Contains("8402Phoro", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains("6228Phoropter finaler Verordnungswert", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains("6227Phoropter Maximalwert (Vollkorrektion)", result.ExportContent, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", result.ExportContent, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildPreview_ForRegularXmlDeviceShouldNotUsePhoropterHistoryFallback()
    {
        var service = new BuilderManualProcessingPreviewService();
        var result = service.BuildPreview(new BuilderManualProcessingPreviewRequest(
            InterfaceProfile: DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            DeviceProfile: DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault(),
            ExportProfile: DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            AisFilePath: GetCv5000FixturePath("Patient_mit_Phoropter_Daten.XDT"),
            DeviceFilePath: Path.Combine(AppContext.BaseDirectory, "TestData", "nidek-ark1s-sample.xml")));

        Assert.True(result.HasErrors);
        Assert.Null(result.Patient);
        Assert.DoesNotContain(result.Issues, issue => issue.Message.Contains("RT-6100-Rueckweg", StringComparison.Ordinal));
        Assert.Contains(result.Issues, issue =>
            issue.Severity == ProcessingIssueSeverity.Error
            && issue.Message.Contains("AIS-Datei konnte wegen GDT-/XDT-Parserfehlern", StringComparison.Ordinal));
    }

    private static string CreateRt6100ReturnXml()
    {
        return """
            <?xml version="1.0" encoding="UTF-16"?>
            <Ophthalmology>
              <Common>
                <Company>NIDEK</Company>
                <ModelName>RT-6100</ModelName>
                <Version>NIDEK_RT_V1.00</Version>
                <Date>2026.05.28</Date>
                <Time>12:34:56</Time>
                <Patient>
                  <No>4711</No>
                  <ID>4711</ID>
                </Patient>
              </Common>
              <Measure Type="RT">
                <Phoropter>
                  <Corrected CorrectionType="Best" Vision="Distant" Situation="Standard">
                    <DisplayName>Final Prescription</DisplayName>
                    <VD>13.75</VD>
                    <R>
                      <Sphere>-8.00</Sphere>
                      <Cylinder>-1.25</Cylinder>
                      <Axis>165</Axis>
                      <ADD>+1.25</ADD>
                      <PD>32.00</PD>
                    </R>
                    <L>
                      <Sphere>-7.50</Sphere>
                      <Cylinder>-0.50</Cylinder>
                      <Axis>25</Axis>
                      <ADD>+1.25</ADD>
                      <PD>33.00</PD>
                    </L>
                    <B>
                      <PD>65.00</PD>
                    </B>
                  </Corrected>
                  <Corrected CorrectionType="Full" Vision="Distant" Situation="Standard">
                    <DisplayName>Full Correction</DisplayName>
                    <VD>13.75</VD>
                    <R>
                      <Sphere>-8.25</Sphere>
                      <Cylinder>-1.50</Cylinder>
                      <Axis>170</Axis>
                      <ADD>+1.50</ADD>
                      <PD>32.50</PD>
                    </R>
                    <L>
                      <Sphere>-7.75</Sphere>
                      <Cylinder>-0.75</Cylinder>
                      <Axis>30</Axis>
                      <ADD>+1.50</ADD>
                      <PD>33.00</PD>
                    </L>
                    <B>
                      <PD>65.50</PD>
                    </B>
                  </Corrected>
                </Phoropter>
              </Measure>
            </Ophthalmology>
            """;
    }

    private static string WriteTempXml(string content)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "nidek-rt6100.xml");
        File.WriteAllText(path, content, Encoding.Unicode);
        return path;
    }

    private static string GetCv5000FixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Topcon", "CV5000", fileName);
    }
}
