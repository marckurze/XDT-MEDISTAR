using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class DeviceProfileDefinitionTests
{
    [Fact]
    public void CreateNidekArk1sDefault_ShouldCreateProfile()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault();

        Assert.Equal("NIDEK", profile.Manufacturer);
        Assert.Equal("ARK1S", profile.Model);
        Assert.Equal("Autorefractor", profile.DeviceType);
        Assert.Equal("Xml", profile.ParserMode);
        Assert.False(profile.CanContainMultipleExaminationTypes);
        Assert.Contains("Refraktion", profile.SupportedExaminationTypes);
        Assert.Contains("PD", profile.SupportedExaminationTypes);
        Assert.Equal(10, profile.Measurements.Count);
    }

    [Fact]
    public void Validate_ShouldAcceptNidekArk1sProfile()
    {
        var issues = DeviceProfileDefinitionValidator.Validate(DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault());

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ShouldReportWrongProfileKind()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() with
        {
            Metadata = CreateMetadata(ProfileKind.AisProfile)
        };

        var issues = DeviceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("Metadata.ProfileKind must be DeviceProfile.", issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptyManufacturer()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() with { Manufacturer = "" };

        var issues = DeviceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("Manufacturer must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptyModel()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() with { Model = " " };

        var issues = DeviceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("Model must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptyParserMode()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() with { ParserMode = "" };

        var issues = DeviceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("ParserMode must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportDuplicateMeasurementIds()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault();
        var duplicate = profile.Measurements[0] with { SourcePath = "Duplicate/Path" };

        profile = profile with
        {
            Measurements = profile.Measurements.Concat(new[] { duplicate }).ToList()
        };

        var issues = DeviceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("Measurements contains duplicate Id: r-sphere", issues);
    }

    [Fact]
    public void CreateNidekArk1sDefault_ShouldContainRequiredMeasurements()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault();

        AssertRequiredMeasurement(profile, "r-sphere", "R/AR/ARMedian/Sphere");
        AssertRequiredMeasurement(profile, "r-cylinder", "R/AR/ARMedian/Cylinder");
        AssertRequiredMeasurement(profile, "r-axis", "R/AR/ARMedian/Axis");
        AssertRequiredMeasurement(profile, "r-se", "R/AR/ARMedian/SE");
        AssertRequiredMeasurement(profile, "l-sphere", "L/AR/ARMedian/Sphere");
        AssertRequiredMeasurement(profile, "l-cylinder", "L/AR/ARMedian/Cylinder");
        AssertRequiredMeasurement(profile, "l-axis", "L/AR/ARMedian/Axis");
        AssertRequiredMeasurement(profile, "l-se", "L/AR/ARMedian/SE");
        AssertRequiredMeasurement(profile, "far-pd", "PD/PDList[@No='1']/FarPD");
    }

    [Fact]
    public void CreateNidekArk1sDefault_ShouldContainCorrectPdPaths()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault();

        Assert.Contains(profile.Measurements, m => m.Id == "far-pd" && m.SourcePath == "PD/PDList[@No='1']/FarPD");
        Assert.Contains(profile.Measurements, m => m.Id == "near-pd" && m.SourcePath == "PD/PDList[@No='1']/NearPD");
    }

    [Fact]
    public void CreateNidekLm7Default_ShouldCreateProfile()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekLm7Default();

        Assert.Equal("NIDEK", profile.Manufacturer);
        Assert.Equal("LM7", profile.Model);
        Assert.Equal("Lensmeter", profile.DeviceType);
        Assert.Equal("Xml", profile.ParserMode);
        Assert.False(profile.CanContainMultipleExaminationTypes);
        Assert.Contains("Lensmeter", profile.SupportedExaminationTypes);
        Assert.Contains("PD", profile.SupportedExaminationTypes);
        Assert.Contains("Prism", profile.SupportedExaminationTypes);
    }

    [Fact]
    public void CreateNidekLm7Default_ShouldContainPrismMeasurements()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekLm7Default();

        AssertOptionalMeasurement(profile, "lm7-lan-r-prism-x", "Measure[@Type='LM']/LM/R/PrismX");
        AssertOptionalMeasurement(profile, "lm7-lan-r-prism-x-base", "Measure[@Type='LM']/LM/R/PrismX/@base");
        AssertOptionalMeasurement(profile, "lm7-lan-r-prism-y", "Measure[@Type='LM']/LM/R/PrismY");
        AssertOptionalMeasurement(profile, "lm7-lan-r-prism-y-base", "Measure[@Type='LM']/LM/R/PrismY/@base");
        AssertOptionalMeasurement(profile, "lm7-lan-l-prism-x", "Measure[@Type='LM']/LM/L/PrismX");
        AssertOptionalMeasurement(profile, "lm7-lan-l-prism-x-base", "Measure[@Type='LM']/LM/L/PrismX/@base");
        AssertOptionalMeasurement(profile, "lm7-lan-l-prism-y", "Measure[@Type='LM']/LM/L/PrismY");
        AssertOptionalMeasurement(profile, "lm7-lan-l-prism-y-base", "Measure[@Type='LM']/LM/L/PrismY/@base");
    }

    [Fact]
    public void CreateNidekLm7Default_ShouldKeepLegacyFragmentPathsOptional()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekLm7Default();

        AssertOptionalMeasurement(profile, "lm7-r-sphere", "R/Sphare");
        AssertOptionalMeasurement(profile, "lm7-r-cylinder", "R/Cylinder");
        AssertOptionalMeasurement(profile, "lm7-r-axis", "R/Axis");
        AssertOptionalMeasurement(profile, "lm7-r-prism-horizontal", "R/PrismX");
        AssertOptionalMeasurement(profile, "lm7-r-prism-horizontal-base", "R/PrismX/@base");
        AssertOptionalMeasurement(profile, "lm7-r-prism-vertical", "R/PrismY");
        AssertOptionalMeasurement(profile, "lm7-r-prism-vertical-base", "R/PrismY/@base");
    }

    [Fact]
    public void CreateNidekLm7Default_ShouldContainPdMeasurement()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekLm7Default();

        AssertUnvalidatedOptionalMeasurement(profile, "lm7-pd", "PD/Distance");
    }

    [Fact]
    public void CreateNidekLm7Default_ShouldContainLanXmlSourcePathsFromInterfaceManual()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekLm7Default();

        AssertRequiredMeasurement(profile, "lm7-lan-r-sphere", "Measure[@Type='LM']/LM/R/Sphere");
        AssertRequiredMeasurement(profile, "lm7-lan-r-cylinder", "Measure[@Type='LM']/LM/R/Cylinder");
        AssertRequiredMeasurement(profile, "lm7-lan-r-axis", "Measure[@Type='LM']/LM/R/Axis");
        AssertRequiredMeasurement(profile, "lm7-lan-l-sphere", "Measure[@Type='LM']/LM/L/Sphere");
        AssertRequiredMeasurement(profile, "lm7-lan-l-cylinder", "Measure[@Type='LM']/LM/L/Cylinder");
        AssertRequiredMeasurement(profile, "lm7-lan-l-axis", "Measure[@Type='LM']/LM/L/Axis");
        AssertOptionalMeasurement(profile, "lm7-lan-r-add2", "Measure[@Type='LM']/LM/R/ADD2");
        AssertOptionalMeasurement(profile, "lm7-lan-r-near-sphere", "Measure[@Type='LM']/LM/R/NearSphere");
        AssertOptionalMeasurement(profile, "lm7-lan-r-prism-x-base", "Measure[@Type='LM']/LM/R/PrismX/@base");
        AssertOptionalMeasurement(profile, "lm7-lan-r-uv-transmittance", "Measure[@Type='LM']/LM/R/UVTransmittance");
        AssertOptionalMeasurement(profile, "lm7-lan-r-confidence-index", "Measure[@Type='LM']/LM/R/ConfidenceIndex");
        AssertOptionalMeasurement(profile, "lm7-lan-r-error", "Measure[@Type='LM']/LM/R/Error");
        AssertOptionalMeasurement(profile, "lm7-lan-pd-distance", "Measure[@Type='LM']/PD/Distance");
        AssertOptionalMeasurement(profile, "lm7-lan-pd-distance-r", "Measure[@Type='LM']/PD/DistanceR");
        AssertOptionalMeasurement(profile, "lm7-lan-pd-distance-l", "Measure[@Type='LM']/PD/DistanceL");
        AssertOptionalMeasurement(profile, "lm7-medistar-r-line", "Measure[@Type='LM']/LM/R/MedistarLine");
        AssertOptionalMeasurement(profile, "lm7-medistar-l-line", "Measure[@Type='LM']/LM/L/MedistarLine");
        Assert.Contains(profile.Measurements, measurement => measurement.SourcePath == "R/Sphare");
    }

    [Fact]
    public void CreateNidekLm7Default_ShouldMarkLeftValuesAsUnvalidatedAndOptional()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekLm7Default();

        AssertUnvalidatedOptionalMeasurement(profile, "lm7-l-sphere", "L/Sphare");
        AssertUnvalidatedOptionalMeasurement(profile, "lm7-l-cylinder", "L/Cylinder");
        AssertUnvalidatedOptionalMeasurement(profile, "lm7-l-axis", "L/Axis");
        AssertUnvalidatedOptionalMeasurement(profile, "lm7-l-prism-horizontal", "L/PrismX");
        AssertUnvalidatedOptionalMeasurement(profile, "lm7-l-prism-horizontal-base", "L/PrismX/@base");
        AssertUnvalidatedOptionalMeasurement(profile, "lm7-l-prism-vertical", "L/PrismY");
        AssertUnvalidatedOptionalMeasurement(profile, "lm7-l-prism-vertical-base", "L/PrismY/@base");
    }

    [Fact]
    public void Validate_ShouldAcceptNidekLm7Profile()
    {
        var issues = DeviceProfileDefinitionValidator.Validate(DefaultDeviceProfileDefinitions.CreateNidekLm7Default());

        Assert.Empty(issues);
    }

    [Fact]
    public void CreateNidekNt530PDefault_ShouldCreateProfile()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault();

        Assert.Equal("NIDEK", profile.Manufacturer);
        Assert.Equal("NT-530P", profile.Model);
        Assert.Contains("Tonometer", profile.DeviceType);
        Assert.Contains("Pachymeter", profile.DeviceType);
        Assert.Equal("Xml", profile.ParserMode);
        Assert.True(profile.CanContainMultipleExaminationTypes);
        Assert.Contains("Tonometrie", profile.SupportedExaminationTypes);
        Assert.Contains("Pachymetrie", profile.SupportedExaminationTypes);
        Assert.Contains("CorrectedIOP", profile.SupportedExaminationTypes);
        Assert.Contains("Attachment", profile.SupportedExaminationTypes);
    }

    [Fact]
    public void CreateNidekNt530PDefault_ShouldContainTonometryMeasurements()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault();

        AssertRequiredMeasurement(profile, "nt530p-r-iop-1", "R/NT/NTList[@No='1']/mmHg");
        AssertOptionalMeasurement(profile, "nt530p-r-iop-2", "R/NT/NTList[@No='2']/mmHg");
        AssertRequiredMeasurement(profile, "nt530p-r-iop-average", "R/NT/NTAverage/mmHg");
        AssertRequiredMeasurement(profile, "nt530p-l-iop-1", "L/NT/NTList[@No='1']/mmHg");
        AssertOptionalMeasurement(profile, "nt530p-l-iop-2", "L/NT/NTList[@No='2']/mmHg");
        AssertRequiredMeasurement(profile, "nt530p-l-iop-average", "L/NT/NTAverage/mmHg");
    }

    [Fact]
    public void CreateNidekNt530PDefault_ShouldContainCorrectedIopAndPachyMeasurements()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault();

        AssertOptionalMeasurement(profile, "nt530p-r-corrected-iop-corrected", "R/NT/CorrectedIOP/Corrected/mmHg");
        AssertOptionalMeasurement(profile, "nt530p-l-corrected-iop-corrected", "L/NT/CorrectedIOP/Corrected/mmHg");
        AssertRequiredMeasurement(profile, "nt530p-r-pachy-average", "R/PACHY/PACHYAverage/Thickness");
        AssertRequiredMeasurement(profile, "nt530p-l-pachy-average", "L/PACHY/PACHYAverage/Thickness");
        AssertRequiredMeasurement(profile, "nt530p-measurement-date", "Date");
        AssertRequiredMeasurement(profile, "nt530p-measurement-time", "Time");
        AssertRequiredMeasurement(profile, "nt530p-pachy-header-line", "Measure[@Type='NT530P']/Pachy/HeaderLine");
        AssertRequiredMeasurement(profile, "nt530p-pachy-medistar-line", "Measure[@Type='NT530P']/Pachy/MedistarLine");
        AssertRequiredMeasurement(profile, "nt530p-tono-header-line", "Measure[@Type='NT530P']/Tono/HeaderLine");
        AssertRequiredMeasurement(profile, "nt530p-tono-pachy-right-line", "Measure[@Type='NT530P']/Tono/PachyRightLine");
        AssertRequiredMeasurement(profile, "nt530p-tono-pachy-left-line", "Measure[@Type='NT530P']/Tono/PachyLeftLine");
        AssertRequiredMeasurement(profile, "nt530p-tono-measured-right-line", "Measure[@Type='NT530P']/Tono/MeasuredRightLine");
        AssertRequiredMeasurement(profile, "nt530p-tono-corrected-right-line", "Measure[@Type='NT530P']/Tono/CorrectedRightLine");
        AssertRequiredMeasurement(profile, "nt530p-tono-right-cct-left-measured-line", "Measure[@Type='NT530P']/Tono/RightCctLeftMeasuredLine");
        AssertRequiredMeasurement(profile, "nt530p-tono-parameter-left-line", "Measure[@Type='NT530P']/Tono/ParameterLeftLine");
        AssertRequiredMeasurement(profile, "nt530p-tono-list-line", "Measure[@Type='NT530P']/Tono/TonoListLine");
        AssertOptionalMeasurement(profile, "nt530p-tono-medistar-line", "Measure[@Type='NT530P']/Tono/MedistarLine");
    }

    [Fact]
    public void CreateNidekNt530PDefault_ShouldContainOptionalPachyImageMeasurements()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault();

        AssertOptionalMeasurement(profile, "nt530p-r-pachy-image", "R/PACHY/PACHYImage");
        AssertOptionalMeasurement(profile, "nt530p-l-pachy-image", "L/PACHY/PACHYImage");
    }

    [Fact]
    public void Validate_ShouldAcceptNidekNt530PProfile()
    {
        var issues = DeviceProfileDefinitionValidator.Validate(DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault());

        Assert.Empty(issues);
    }

    [Fact]
    public void CreateTopconCl300Default_ShouldCreateProfile()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconCl300Default();

        Assert.Equal("TOPCON", profile.Manufacturer);
        Assert.Equal("CL-300", profile.Model);
        Assert.Equal("Lensmeter", profile.DeviceType);
        Assert.Equal("Xml", profile.ParserMode);
        Assert.False(profile.CanContainMultipleExaminationTypes);
        Assert.Contains("Lensmeter", profile.SupportedExaminationTypes);
        Assert.Contains("PD", profile.SupportedExaminationTypes);
        Assert.Contains("Prism", profile.SupportedExaminationTypes);
    }

    [Fact]
    public void CreateTopconCl300Default_ShouldContainRequiredLensmeterMeasurements()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconCl300Default();

        AssertRequiredMeasurement(profile, "cl300-r-sphere", "Measure[@Type='LM']/LM/R/Sphere");
        AssertRequiredMeasurement(profile, "cl300-r-cylinder", "Measure[@Type='LM']/LM/R/Cylinder");
        AssertRequiredMeasurement(profile, "cl300-r-axis", "Measure[@Type='LM']/LM/R/Axis");
        AssertRequiredMeasurement(profile, "cl300-l-sphere", "Measure[@Type='LM']/LM/L/Sphere");
        AssertRequiredMeasurement(profile, "cl300-l-cylinder", "Measure[@Type='LM']/LM/L/Cylinder");
        AssertRequiredMeasurement(profile, "cl300-l-axis", "Measure[@Type='LM']/LM/L/Axis");
    }

    [Fact]
    public void CreateTopconCl300Default_ShouldContainOptionalPdAndPrismMeasurements()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconCl300Default();

        AssertOptionalMeasurement(profile, "cl300-pd-distance", "Measure[@Type='LM']/PD/B/Distance");
        AssertOptionalMeasurement(profile, "cl300-r-prism-horizontal", "Measure[@Type='LM']/LM/R/H");
        AssertOptionalMeasurement(profile, "cl300-r-prism-vertical", "Measure[@Type='LM']/LM/R/V");
        AssertOptionalMeasurement(profile, "cl300-l-prism-horizontal", "Measure[@Type='LM']/LM/L/H");
        AssertOptionalMeasurement(profile, "cl300-l-prism-vertical", "Measure[@Type='LM']/LM/L/V");
    }

    [Fact]
    public void CreateTopconCl300Default_ShouldUseNamespaceAgnosticParserPaths()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconCl300Default();

        Assert.Contains("nsCommon/nsLM", profile.Metadata.Description);
        Assert.DoesNotContain(profile.Measurements, measurement =>
            measurement.SourcePath.Contains("Ophthalmology/", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(profile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/R/MedistarLine");
        Assert.Contains(profile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/L/MedistarLine");
    }

    [Fact]
    public void Validate_ShouldAcceptTopconCl300Profile()
    {
        var issues = DeviceProfileDefinitionValidator.Validate(DefaultDeviceProfileDefinitions.CreateTopconCl300Default());

        Assert.Empty(issues);
    }

    [Fact]
    public void CreateTopconKr800Default_ShouldCreateProfile()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconKr800Default();

        Assert.Equal("TOPCON", profile.Manufacturer);
        Assert.Equal("KR-800S", profile.Model);
        Assert.Contains("Autorefractor", profile.DeviceType);
        Assert.Contains("Keratometer", profile.DeviceType);
        Assert.Contains("Subjective", profile.DeviceType);
        Assert.Equal("Xml", profile.ParserMode);
        Assert.True(profile.CanContainMultipleExaminationTypes);
        Assert.Contains("REF", profile.SupportedExaminationTypes);
        Assert.Contains("KM", profile.SupportedExaminationTypes);
        Assert.Contains("SBJ", profile.SupportedExaminationTypes);
    }

    [Fact]
    public void CreateTopconKr800Default_ShouldContainRequiredRefMeasurements()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconKr800Default();

        AssertRequiredMeasurement(profile, "kr800-ref-r-sphere", "Measure[@Type='REF']/REF/R/Median/Sphere");
        AssertRequiredMeasurement(profile, "kr800-ref-r-cylinder", "Measure[@Type='REF']/REF/R/Median/Cylinder");
        AssertRequiredMeasurement(profile, "kr800-ref-r-axis", "Measure[@Type='REF']/REF/R/Median/Axis");
        AssertRequiredMeasurement(profile, "kr800-ref-l-sphere", "Measure[@Type='REF']/REF/L/Median/Sphere");
        AssertRequiredMeasurement(profile, "kr800-ref-l-cylinder", "Measure[@Type='REF']/REF/L/Median/Cylinder");
        AssertRequiredMeasurement(profile, "kr800-ref-l-axis", "Measure[@Type='REF']/REF/L/Median/Axis");
        AssertOptionalMeasurement(profile, "kr800-ref-r-medistar-line", "Measure[@Type='REF']/REF/R/MedistarLine");
        AssertOptionalMeasurement(profile, "kr800-ref-l-medistar-line", "Measure[@Type='REF']/REF/L/MedistarLine");
    }

    [Fact]
    public void CreateTopconKr800Default_ShouldContainOptionalKmMeasurements()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconKr800Default();

        AssertOptionalMeasurement(profile, "kr800-km-r-k1-power", "Measure[@Type='KM']/KM/R/Median/R1/Power");
        AssertOptionalMeasurement(profile, "kr800-km-r-k2-power", "Measure[@Type='KM']/KM/R/Median/R2/Power");
        AssertOptionalMeasurement(profile, "kr800-km-l-k1-power", "Measure[@Type='KM']/KM/L/Median/R1/Power");
        AssertOptionalMeasurement(profile, "kr800-km-l-k2-power", "Measure[@Type='KM']/KM/L/Median/R2/Power");
        AssertOptionalMeasurement(profile, "kr800-km-line1", "Measure[@Type='KM']/KM/MedistarLine1");
        AssertOptionalMeasurement(profile, "kr800-km-line2", "Measure[@Type='KM']/KM/MedistarLine2");
    }

    [Fact]
    public void CreateTopconKr800Default_ShouldContainUnvalidatedSbjMeasurements()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconKr800Default();

        AssertOptionalMeasurement(profile, "kr800-sbj-far-r-sphere", "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/R/Sph");
        AssertOptionalMeasurement(profile, "kr800-sbj-far-l-sphere", "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/L/Sph");
        AssertOptionalMeasurement(profile, "kr800-sbj-pd-b", "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/PD/B");
        AssertOptionalMeasurement(profile, "kr800-sbj-line1", "Measure[@Type='SBJ']/MedistarLine1");
        AssertOptionalMeasurement(profile, "kr800-sbj-line2", "Measure[@Type='SBJ']/MedistarLine2");
    }

    [Fact]
    public void CreateTopconKr800Default_ShouldUseNamespaceTolerantRootlessPaths()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconKr800Default();

        Assert.DoesNotContain("Namespace-Normalisierung", profile.Metadata.Description);
        Assert.DoesNotContain(profile.Measurements, measurement =>
            measurement.SourcePath.Contains("Ophthalmology/", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ShouldAcceptTopconKr800Profile()
    {
        var issues = DeviceProfileDefinitionValidator.Validate(DefaultDeviceProfileDefinitions.CreateTopconKr800Default());

        Assert.Empty(issues);
    }

    [Fact]
    public void CreateTopconTrk2PDefault_ShouldCreateProfile()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault();

        Assert.Equal("TOPCON", profile.Manufacturer);
        Assert.Equal("TRK2P", profile.Model);
        Assert.Contains("Tonometer", profile.DeviceType);
        Assert.Contains("Pachymeter", profile.DeviceType);
        Assert.Equal("Xml", profile.ParserMode);
        Assert.True(profile.CanContainMultipleExaminationTypes);
        Assert.Contains("TM", profile.SupportedExaminationTypes);
        Assert.Contains("CCT", profile.SupportedExaminationTypes);
        Assert.Contains("Tonometrie", profile.SupportedExaminationTypes);
        Assert.Contains("Pachymetrie", profile.SupportedExaminationTypes);
    }

    [Fact]
    public void CreateTopconTrk2PDefault_ShouldContainRequiredIopAverages()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault();

        AssertRequiredMeasurement(profile, "trk2p-r-iop-average", "Ophthalmology/Measure[@type='TM']/TM/R/Average/IOP_mmHg");
        AssertRequiredMeasurement(profile, "trk2p-l-iop-average", "Ophthalmology/Measure[@type='TM']/TM/L/Average/IOP_mmHg");
    }

    [Fact]
    public void CreateTopconTrk2PDefault_ShouldContainOptionalIopSingleValues()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault();

        AssertOptionalMeasurement(profile, "trk2p-r-iop-1", "Ophthalmology/Measure[@type='TM']/TM/R/List[@No='1']/IOP_mmHg");
        AssertOptionalMeasurement(profile, "trk2p-r-iop-2", "Ophthalmology/Measure[@type='TM']/TM/R/List[@No='2']/IOP_mmHg");
        AssertOptionalMeasurement(profile, "trk2p-r-iop-3", "Ophthalmology/Measure[@type='TM']/TM/R/List[@No='3']/IOP_mmHg");
        AssertOptionalMeasurement(profile, "trk2p-l-iop-1", "Ophthalmology/Measure[@type='TM']/TM/L/List[@No='1']/IOP_mmHg");
        AssertOptionalMeasurement(profile, "trk2p-l-iop-2", "Ophthalmology/Measure[@type='TM']/TM/L/List[@No='2']/IOP_mmHg");
        AssertOptionalMeasurement(profile, "trk2p-l-iop-3", "Ophthalmology/Measure[@type='TM']/TM/L/List[@No='3']/IOP_mmHg");
    }

    [Fact]
    public void CreateTopconTrk2PDefault_ShouldContainOptionalCctPachyMeasurements()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault();

        AssertUnvalidatedOptionalMeasurement(profile, "trk2p-r-cct-3", "Ophthalmology/Measure[@type='TM']/CCT/R/List[@No='3']/CCT_mm");
        AssertUnvalidatedOptionalMeasurement(profile, "trk2p-r-cct-4", "Ophthalmology/Measure[@type='TM']/CCT/R/List[@No='4']/CCT_mm");
        AssertUnvalidatedOptionalMeasurement(profile, "trk2p-l-cct-1", "Ophthalmology/Measure[@type='TM']/CCT/L/List[@No='1']/CCT_mm");
        AssertUnvalidatedOptionalMeasurement(profile, "trk2p-l-cct-2", "Ophthalmology/Measure[@type='TM']/CCT/L/List[@No='2']/CCT_mm");
        AssertUnvalidatedOptionalMeasurement(profile, "trk2p-l-cct-3", "Ophthalmology/Measure[@type='TM']/CCT/L/List[@No='3']/CCT_mm");
    }

    [Fact]
    public void Validate_ShouldAcceptTopconTrk2PProfile()
    {
        var issues = DeviceProfileDefinitionValidator.Validate(DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault());

        Assert.Empty(issues);
    }

    [Fact]
    public void CreateDocumentAttachmentDefault_ShouldCreateAttachmentOnlyProfile()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateDocumentAttachmentDefault();

        Assert.Equal("device-document-attachment-default", profile.Metadata.Id);
        Assert.Equal("Generisches Dokumentgerät", profile.Metadata.Name);
        Assert.Equal("AttachmentOnly", profile.ParserMode);
        Assert.Equal("Dokument/Anhang", profile.DeviceType);
        Assert.Empty(profile.Measurements);
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(profile));
    }

    [Fact]
    public void CreateManualDocumentSelectionDefault_ShouldCreateAttachmentOnlyManualProfile()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateManualDocumentSelectionDefault();

        Assert.Equal("device-manual-document-selection-default", profile.Metadata.Id);
        Assert.Equal("Manuelle Dokumentauswahl", profile.Metadata.Name);
        Assert.Equal("AttachmentOnlyManual", profile.ParserMode);
        Assert.Equal("Dokument/Manuell", profile.DeviceType);
        Assert.Empty(profile.Measurements);
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(profile));
    }

    private static void AssertRequiredMeasurement(DeviceProfileDefinition profile, string id, string sourcePath)
    {
        Assert.Contains(profile.Measurements, m => m.Id == id && m.SourcePath == sourcePath && m.IsRequired);
    }

    private static void AssertOptionalMeasurement(DeviceProfileDefinition profile, string id, string sourcePath)
    {
        Assert.Contains(profile.Measurements, m => m.Id == id && m.SourcePath == sourcePath && !m.IsRequired);
    }

    private static void AssertUnvalidatedOptionalMeasurement(DeviceProfileDefinition profile, string id, string sourcePath)
    {
        Assert.Contains(profile.Measurements, m =>
            m.Id == id
            && m.SourcePath == sourcePath
            && !m.IsRequired
            && (m.Description ?? string.Empty).Contains("noch zu validieren", StringComparison.OrdinalIgnoreCase));
    }

    private static ProfileMetadata CreateMetadata(ProfileKind profileKind)
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new ProfileMetadata(
            Id: "metadata-test",
            Name: "Metadata Test",
            ProfileKind: profileKind,
            Description: null,
            Vendor: null,
            Product: null,
            Version: "1.0.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: "XdtDeviceBridge",
            IsBuiltIn: false,
            IsUserDefined: true);
    }
}
