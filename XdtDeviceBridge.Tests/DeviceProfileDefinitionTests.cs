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

        AssertRequiredMeasurement(profile, "lm7-r-prism-horizontal", "R/PrismX");
        AssertRequiredMeasurement(profile, "lm7-r-prism-horizontal-base", "R/PrismX/@base");
        AssertRequiredMeasurement(profile, "lm7-r-prism-vertical", "R/PrismY");
        AssertRequiredMeasurement(profile, "lm7-r-prism-vertical-base", "R/PrismY/@base");
        Assert.Contains(profile.Measurements, m => m.Id == "lm7-l-prism-horizontal");
        Assert.Contains(profile.Measurements, m => m.Id == "lm7-l-prism-horizontal-base");
        Assert.Contains(profile.Measurements, m => m.Id == "lm7-l-prism-vertical");
        Assert.Contains(profile.Measurements, m => m.Id == "lm7-l-prism-vertical-base");
    }

    [Fact]
    public void CreateNidekLm7Default_ShouldUseValidatedRightSourcePaths()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekLm7Default();

        AssertRequiredMeasurement(profile, "lm7-r-sphere", "R/Sphare");
        AssertRequiredMeasurement(profile, "lm7-r-cylinder", "R/Cylinder");
        AssertRequiredMeasurement(profile, "lm7-r-axis", "R/Axis");
        AssertRequiredMeasurement(profile, "lm7-r-prism-horizontal", "R/PrismX");
        AssertRequiredMeasurement(profile, "lm7-r-prism-horizontal-base", "R/PrismX/@base");
        AssertRequiredMeasurement(profile, "lm7-r-prism-vertical", "R/PrismY");
        AssertRequiredMeasurement(profile, "lm7-r-prism-vertical-base", "R/PrismY/@base");
    }

    [Fact]
    public void CreateNidekLm7Default_ShouldContainPdMeasurement()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekLm7Default();

        AssertUnvalidatedOptionalMeasurement(profile, "lm7-pd", "PD/Distance");
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
        Assert.Equal("NT530P", profile.Model);
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

        AssertRequiredMeasurement(profile, "nt530p-r-iop-1", "Data/R/NT/NTList[@No='1']/mmHg");
        AssertRequiredMeasurement(profile, "nt530p-r-iop-2", "Data/R/NT/NTList[@No='2']/mmHg");
        AssertRequiredMeasurement(profile, "nt530p-r-iop-3", "Data/R/NT/NTList[@No='3']/mmHg");
        AssertRequiredMeasurement(profile, "nt530p-r-iop-average", "Data/R/NT/NTAverage/mmHg");
        AssertRequiredMeasurement(profile, "nt530p-l-iop-1", "Data/L/NT/NTList[@No='1']/mmHg");
        AssertRequiredMeasurement(profile, "nt530p-l-iop-2", "Data/L/NT/NTList[@No='2']/mmHg");
        AssertRequiredMeasurement(profile, "nt530p-l-iop-3", "Data/L/NT/NTList[@No='3']/mmHg");
        AssertRequiredMeasurement(profile, "nt530p-l-iop-average", "Data/L/NT/NTAverage/mmHg");
    }

    [Fact]
    public void CreateNidekNt530PDefault_ShouldContainCorrectedIopAndPachyMeasurements()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault();

        AssertOptionalMeasurement(profile, "nt530p-r-corrected-iop-corrected", "Data/R/NT/CorrectedIOP/Corrected/mmHg");
        AssertOptionalMeasurement(profile, "nt530p-l-corrected-iop-corrected", "Data/L/NT/CorrectedIOP/Corrected/mmHg");
        AssertRequiredMeasurement(profile, "nt530p-r-pachy-average", "Data/R/PACHY/PACHYAverage/Thickness");
        AssertRequiredMeasurement(profile, "nt530p-l-pachy-average", "Data/L/PACHY/PACHYAverage/Thickness");
        AssertRequiredMeasurement(profile, "nt530p-measurement-date", "Data/Date");
        AssertRequiredMeasurement(profile, "nt530p-measurement-time", "Data/Time");
    }

    [Fact]
    public void CreateNidekNt530PDefault_ShouldContainOptionalPachyImageMeasurements()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault();

        AssertOptionalMeasurement(profile, "nt530p-r-pachy-image", "Data/R/PACHY/PACHYImage");
        AssertOptionalMeasurement(profile, "nt530p-l-pachy-image", "Data/L/PACHY/PACHYImage");
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
        Assert.Equal("CL300", profile.Model);
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

        AssertRequiredMeasurement(profile, "cl300-r-sphere", "Ophthalmology/Measure[@type='LM']/LM/R/Sphere");
        AssertRequiredMeasurement(profile, "cl300-r-cylinder", "Ophthalmology/Measure[@type='LM']/LM/R/Cylinder");
        AssertRequiredMeasurement(profile, "cl300-r-axis", "Ophthalmology/Measure[@type='LM']/LM/R/Axis");
        AssertRequiredMeasurement(profile, "cl300-l-sphere", "Ophthalmology/Measure[@type='LM']/LM/L/Sphere");
        AssertRequiredMeasurement(profile, "cl300-l-cylinder", "Ophthalmology/Measure[@type='LM']/LM/L/Cylinder");
        AssertRequiredMeasurement(profile, "cl300-l-axis", "Ophthalmology/Measure[@type='LM']/LM/L/Axis");
    }

    [Fact]
    public void CreateTopconCl300Default_ShouldContainOptionalPdAndPrismMeasurements()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconCl300Default();

        AssertOptionalMeasurement(profile, "cl300-pd-distance", "Ophthalmology/Measure[@type='LM']/PD/B/Distance");
        AssertOptionalMeasurement(profile, "cl300-r-prism-horizontal", "Ophthalmology/Measure[@type='LM']/LM/R/H");
        AssertOptionalMeasurement(profile, "cl300-r-prism-vertical", "Ophthalmology/Measure[@type='LM']/LM/R/V");
        AssertOptionalMeasurement(profile, "cl300-l-prism-horizontal", "Ophthalmology/Measure[@type='LM']/LM/L/H");
        AssertOptionalMeasurement(profile, "cl300-l-prism-vertical", "Ophthalmology/Measure[@type='LM']/LM/L/V");
    }

    [Fact]
    public void CreateTopconCl300Default_ShouldDocumentNamespaceRequirement()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconCl300Default();

        Assert.Contains("Namespace-Normalisierung", profile.Metadata.Description);
        Assert.Contains(profile.Measurements, measurement =>
            (measurement.Description ?? string.Empty).Contains("Namespace-Normalisierung", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ShouldAcceptTopconCl300Profile()
    {
        var issues = DeviceProfileDefinitionValidator.Validate(DefaultDeviceProfileDefinitions.CreateTopconCl300Default());

        Assert.Empty(issues);
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
