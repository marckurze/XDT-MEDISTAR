using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TopconTrk2PProfileTests
{
    private readonly XmlDeviceParser _parser = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Fact]
    public void ParseSerial0001_ShouldReadRefKmAndTmMeasurements()
    {
        var result = _parser.ParseFile(GetTrk2PFixturePath("M-Serial0001_20190411_113829_TOPCON_TRK-2P_5270367.xml"));

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Common/Company", "TOPCON");
        AssertMeasurement(result, "Common/ModelName", "TRK-2P");
        AssertMeasurement(result, "Common/MachineNo", "01");
        AssertMeasurement(result, "Common/ROMVersion", "1.09.01");
        AssertMeasurement(result, "Common/Version", "1.2");
        AssertMeasurement(result, "Common/Date", "2019-04-11");
        AssertMeasurement(result, "Common/Time", "11:38:29");
        AssertMeasurement(result, "Measure[@Type='REF']/VD", "13.75");
        AssertMeasurement(result, "Measure[@Type='REF']/PD/Distance", "68.0");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/Sphere", "0.25");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/Cylinder", "-0.25");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/Axis", "2");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/Sphere", "0.25");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/Cylinder", "0.0");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/Axis", "0");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/R/Median/R1/Radius", "7.74");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/R/Median/R1/Power", "43.50");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/R/Median/R1/Axis", "3");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/L/Median/R2/Radius", "7.65");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/L/Median/R2/Power", "44.25");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/List[@No='1']/IOP_mmHg", "17.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/List[@No='2']/IOP_mmHg", "18.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/List[@No='3']/IOP_mmHg", "17.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/Average/IOP_mmHg", "17.5");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/List[@No='1']/IOP_mmHg", "17.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/List[@No='2']/IOP_mmHg", "16.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/List[@No='3']/IOP_mmHg", "16.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/Average/IOP_mmHg", "16.3");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/MedistarLine", "R.:S=+ 0.25 Z=- 0.25*  2 PD= 68 VD= 13.75");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/MedistarLine", "L.:S=+ 0.25 Z=+ 0.00*  0");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/MedistarLine1", "R: R1=7.74 43.50 *3 R2=7.67 44.00 *93 // L: R1=7.72 43.75 *175 R2=7.65 44.25 *85");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/MedistarLine2", "R: AV=7.71 43.75 CYL=-0.50 3 // L: AV=7.69 44.00 CYL=-0.50 175");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/HeaderLine", "Tonometrie");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/TonoListLine", "R = 17 18 17 [17.5] // L = 17 16 16 [16.3] mmHg 11:38");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='CCT']/", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='SBJ']/MedistarLine", StringComparison.Ordinal));
    }

    [Fact]
    public void ParseSerial0135_ShouldReadRefKmTmCorrectedIopAndCctFallback()
    {
        var result = _parser.ParseFile(GetTrk2PFixturePath("M-Serial0135_20130809_174556_TOPCON_TRK-2P_.xml"));

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Common/Company", "TOPCON");
        AssertMeasurement(result, "Common/ModelName", "TRK-2P");
        AssertMeasurement(result, "Common/ROMVersion", "0.52.00D");
        AssertMeasurement(result, "Measure[@Type='REF']/VD", "12.00");
        AssertMeasurement(result, "Measure[@Type='REF']/PD/Distance", "65.00");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/Sphere", "-6.53");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/Cylinder", "-0.42");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/Axis", "114");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/Sphere", "-6.53");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/Cylinder", "-0.47");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/Axis", "65");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/R/Median/R1/Radius", "8.56");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/R/Median/R1/Power", "39.44");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/R/Median/Cylinder/Power", "-0.32");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/L/Median/R2/Power", "39.66");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/List[@No='1']/IOP_mmHg", "20.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/List[@No='2']/IOP_mmHg", "21.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/List[@No='3']/IOP_mmHg", "21.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/Average/IOP_mmHg", "21.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/List[@No='1']/IOP_mmHg", "17.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/List[@No='2']/IOP_mmHg", "18.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/List[@No='3']/IOP_mmHg", "19.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/Average/IOP_mmHg", "18.0");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Param1", "0.520");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Param2", "0.012");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/CCT", "0.907");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Measured/IOP_mmHg", "21.0");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Corrected/IOP_mmHg", "16.0");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/CCT", "0.880");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/MedistarLine", "R.:S=- 6.53 Z=- 0.42*114 PD= 65 VD= 12.00");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/MedistarLine", "L.:S=- 6.53 Z=- 0.47* 65");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/MedistarLine1", "R: R1=8.56 39.44 *87 R2=8.49 39.76 *177 // L: R1=8.62 39.16 *71 R2=8.51 39.66 *161");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/MedistarLine2", "R: AV=8.53 39.60 CYL=-0.32 87 // L: AV=8.57 39.41 CYL=-0.50 71");
        AssertMeasurement(result, "Measure[@Type='CCT']/Pachy/HeaderLine", "Pachymetrie");
        AssertMeasurement(result, "Measure[@Type='CCT']/Pachy/MedistarLine", "RA: 0.907   // LA: 0.880");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/HeaderLine", "Tonometrie");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/PachyRightLine", "PR: 907 [907] µm");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/PachyLeftLine", "PL: 880 [880] µm");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/MeasuredRightLine", "PR: Gemessen = 21.0 mmHg; Korrigiert = 16.0 mmHg;");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/ParameterRightLine", "PR: Param1 = 520um; Param2 = 0.012; CCT = 907um");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/MeasuredLeftLine", "PL: Gemessen = 18.0 mmHg; Korrigiert = 14.0 mmHg;");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/ParameterLeftLine", "PL: Param1 = 520um; Param2 = 0.012; CCT = 880um");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/TonoListLine", "R = 20 21 21 [21.0] // L = 17 18 19 [18.0] mmHg 17:45");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/CorrectedLine");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='SBJ']/MedistarLine", StringComparison.Ordinal));
    }

    [Fact]
    public void ParseSerial1165_ShouldReadTmAndCctOnlyMeasurements()
    {
        var result = _parser.ParseFile(GetTrk2PFixturePath("M-Serial1165_20241126_225512_TOPCON_TRK-2P_5284298.xml"));

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Common/Company", "TOPCON");
        AssertMeasurement(result, "Common/ModelName", "TRK-2P");
        AssertMeasurement(result, "Common/ROMVersion", "1.11.00");
        AssertMeasurement(result, "Common/Time", "22:55:12");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/List[@No='1']/IOP_mmHg", "15.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/List[@No='2']/IOP_mmHg", "15.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/List[@No='3']/IOP_mmHg", "15.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/Average/IOP_mmHg", "15.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/List[@No='1']/IOP_mmHg", "13.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/List[@No='2']/IOP_mmHg", "12.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/List[@No='3']/IOP_mmHg", "14.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/Average/IOP_mmHg", "13.0");
        AssertMeasurement(result, "Measure[@Type='TM']/CCT/R/List[@No='3']/CCT_mm", "0.511");
        AssertMeasurement(result, "Measure[@Type='TM']/CCT/R/List[@No='4']/CCT_mm", "0.509");
        AssertMeasurement(result, "Measure[@Type='TM']/CCT/L/List[@No='1']/CCT_mm", "0.516");
        AssertMeasurement(result, "Measure[@Type='TM']/CCT/L/List[@No='2']/CCT_mm", "0.518");
        AssertMeasurement(result, "Measure[@Type='TM']/CCT/L/List[@No='3']/CCT_mm", "0.518");
        AssertMeasurement(result, "Measure[@Type='CCT']/Pachy/HeaderLine", "Pachymetrie");
        AssertMeasurement(result, "Measure[@Type='CCT']/Pachy/MedistarLine", "RA: 0.510   // LA: 0.517");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/HeaderLine", "Tonometrie");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/PachyRightLine", "PR: 511 509 [510] µm");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/PachyLeftLine", "PL: 516 518 518 [517] µm");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/TonoListLine", "R = 15 15 15 [15.0] // L = 13 12 14 [13.0] mmHg 22:55");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='REF']/REF/", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='KM']/KM/MedistarLine", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='SBJ']/MedistarLine", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath.Contains("CorrectedIOP", StringComparison.Ordinal));
    }

    [Fact]
    public void MedistarExportForSerial0001_ShouldUseRefKmAndTonoWithoutPachyOrSbj()
    {
        var result = MapWithTrk2PExport(GetTrk2PFixturePath("M-Serial0001_20190411_113829_TOPCON_TRK-2P_5270367.xml"));
        var exportResult = new XdtExportBuilder().Build(result.Records);

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Empty(exportResult.Issues);
        Assert.Contains("8402TRK2P", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228R.:S=+ 0.25 Z=- 0.25*  2 PD= 68 VD= 13.75", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 0.25 Z=+ 0.00*  0", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6221R: R1=7.74 43.50 *3 R2=7.67 44.00 *93 // L: R1=7.72 43.75 *175 R2=7.65 44.25 *85", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6221R: AV=7.71 43.75 CYL=-0.50 3 // L: AV=7.69 44.00 CYL=-0.50 175", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205Tonometrie", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205R = 17 18 17 [17.5] // L = 17 16 16 [16.3] mmHg 11:38", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6220", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6227", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6302", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("R.:S= Z=*", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("K1=*", exportResult.Content, StringComparison.Ordinal);
        Assert.Equal(2, result.Records.Count(record => record.FieldCode == "6228"));
        Assert.Equal(2, result.Records.Count(record => record.FieldCode == "6221"));
        Assert.Equal(2, result.Records.Count(record => record.FieldCode == "6205"));
    }

    [Fact]
    public void MedistarExportForSerial0135_ShouldUseRefKmPachyAndCorrectedTono()
    {
        var result = MapWithTrk2PExport(GetTrk2PFixturePath("M-Serial0135_20130809_174556_TOPCON_TRK-2P_.xml"));
        var exportResult = new XdtExportBuilder().Build(result.Records);

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Empty(exportResult.Issues);
        Assert.Contains("8402TRK2P", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228R.:S=- 6.53 Z=- 0.42*114 PD= 65 VD= 12.00", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=- 6.53 Z=- 0.47* 65", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6221R: R1=8.56 39.44 *87 R2=8.49 39.76 *177 // L: R1=8.62 39.16 *71 R2=8.51 39.66 *161", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6221R: AV=8.53 39.60 CYL=-0.32 87 // L: AV=8.57 39.41 CYL=-0.50 71", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6220Pachymetrie", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6220RA: 0.907   // LA: 0.880", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205Tonometrie", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PR: 907 [907] µm", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PL: 880 [880] µm", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PR: Gemessen = 21.0 mmHg; Korrigiert = 16.0 mmHg;", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PR: Param1 = 520um; Param2 = 0.012; CCT = 907um", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PL: Gemessen = 18.0 mmHg; Korrigiert = 14.0 mmHg;", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PL: Param1 = 520um; Param2 = 0.012; CCT = 880um", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205R = 20 21 21 [21.0] // L = 17 18 19 [18.0] mmHg 17:45", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("Y  PR:", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("P  R = 20 21 21", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6227", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6302", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("R.:S= Z=*", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("K1=*", exportResult.Content, StringComparison.Ordinal);
        Assert.Equal(2, result.Records.Count(record => record.FieldCode == "6228"));
        Assert.Equal(2, result.Records.Count(record => record.FieldCode == "6221"));
        Assert.Equal(2, result.Records.Count(record => record.FieldCode == "6220"));
        Assert.Equal(8, result.Records.Count(record => record.FieldCode == "6205"));
    }

    [Fact]
    public void MedistarExportForSerial1165_ShouldUseTmAndCctOnlyWithoutRefKmOrSbj()
    {
        var result = MapWithTrk2PExport(GetTrk2PFixturePath("M-Serial1165_20241126_225512_TOPCON_TRK-2P_5284298.xml"));
        var exportResult = new XdtExportBuilder().Build(result.Records);

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Empty(exportResult.Issues);
        Assert.Contains("8402TRK2P", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6220Pachymetrie", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6220RA: 0.510   // LA: 0.517", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205Tonometrie", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PR: 511 509 [510] µm", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PL: 516 518 518 [517] µm", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205R = 15 15 15 [15.0] // L = 13 12 14 [13.0] mmHg 22:55", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6228", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6221", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6227", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("Gemessen", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("Korrigiert", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("ERROR", exportResult.Content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("R.:S= Z=*", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("K1=*", exportResult.Content, StringComparison.Ordinal);
        Assert.Equal(2, result.Records.Count(record => record.FieldCode == "6220"));
        Assert.Equal(4, result.Records.Count(record => record.FieldCode == "6205"));
        Assert.DoesNotContain(result.Records, record => record.FieldCode is "6228" or "6221" or "6227");
    }

    [Fact]
    public void BuiltInTopconTrk2PProfiles_ShouldBePresentAndValid()
    {
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault();
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconTrk2PDefault();

        Assert.Equal("TOPCON", deviceProfile.Manufacturer);
        Assert.Equal("TRK-2P", deviceProfile.Model);
        Assert.Contains("Autorefraktometer", deviceProfile.DeviceType);
        Assert.Contains("Pachymeter", deviceProfile.DeviceType);
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='KM']/KM/MedistarLine1");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/HeaderLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/TonoListLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='CCT']/Pachy/HeaderLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='CCT']/Pachy/MedistarLine");
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(deviceProfile));

        Assert.Equal("device-topcon-trk2p-default", exportProfile.SourceDeviceProfileId);
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6221" && rule.SourcePath == "Device.Measure[@Type='KM']/KM/MedistarLine1");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6220" && rule.SourcePath == "Device.Measure[@Type='CCT']/Pachy/HeaderLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6220" && rule.SourcePath == "Device.Measure[@Type='CCT']/Pachy/MedistarLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6205" && rule.SourcePath == "Device.Measure[@Type='TM']/Tono/HeaderLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6205" && rule.SourcePath == "Device.Measure[@Type='TM']/Tono/MeasuredRightLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6205" && rule.SourcePath == "Device.Measure[@Type='TM']/Tono/ParameterLeftLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6205" && rule.SourcePath == "Device.Measure[@Type='TM']/Tono/TonoListLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6227" && rule.SourcePath == "Device.Measure[@Type='SBJ']/MedistarLine1");
        Assert.DoesNotContain(exportProfile.Rules, rule => rule.TargetFieldCode is "6302" or "6303" or "6304" or "6305");
        Assert.Empty(ExportProfileDefinitionValidator.Validate(exportProfile));

        Assert.Equal("device-topcon-trk2p-default", interfaceProfile.DeviceProfileId);
        Assert.Equal("export-medistar-topcon-trk2p-default", interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.True(interfaceProfile.Metadata.IsBuiltIn);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(interfaceProfile));
    }

    [Fact]
    public void NonTrk2PTopconXml_ShouldNotCreateTrk2PMedistarLines()
    {
        var path = WriteTempXml("""
            <Ophthalmology>
              <Common>
                <Company>TOPCON</Company>
                <ModelName>TRK-1P</ModelName>
              </Common>
              <Measure type="TM">
                <TM>
                  <R>
                    <Average>
                      <IOP_mmHg>17.0</IOP_mmHg>
                    </Average>
                  </R>
                </TM>
              </Measure>
            </Ophthalmology>
            """);

        var parseResult = _parser.ParseFile(path);

        Assert.Empty(parseResult.Issues);
        Assert.DoesNotContain(parseResult.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='TM']/Tono/", StringComparison.Ordinal));
    }

    private MappingResult MapWithTrk2PExport(string xmlPath)
    {
        var measurements = _parser.ParseFile(xmlPath).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault());

        return _mappingEngine.Map(CreatePatientData(), measurements, rules);
    }

    private static PatientData CreatePatientData()
    {
        return new PatientData(
            PatientNumber: "PAT-TRK2P",
            LastName: "Test",
            FirstName: "Person",
            BirthDate: "01012000",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: "XDT",
            GdtVersion: "2.10",
            ExaminationType: "TRK2P");
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string value)
    {
        Assert.Contains(result.Measurements, measurement =>
            string.Equals(measurement.SourcePath, sourcePath, StringComparison.Ordinal)
            && measurement.Value == value);
    }

    private static string GetTrk2PFixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Topcon", "TRK2P", fileName);
    }

    private static string WriteTempXml(string content)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "topcon-trk2p.xml");
        File.WriteAllText(path, content);
        return path;
    }
}
