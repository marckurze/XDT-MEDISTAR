using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class MappingEngineTests
{
    [Fact]
    public void Map_ShouldMapPatientField()
    {
        var engine = new MappingEngine();
        var result = engine.Map(CreatePatient(), CreateMeasurements(), new[]
        {
            new MappingRule("1", "3101", "LastName", "AIS.LastName", "{value}", 1, true)
        });

        var record = Assert.Single(result.Records);
        Assert.Equal("Müller", record.Value);
    }

    [Fact]
    public void Map_ShouldMapDeviceField()
    {
        var engine = new MappingEngine();
        var result = engine.Map(CreatePatient(), CreateMeasurements(), new[]
        {
            new MappingRule("1", "8201", "SphereR", "Device.R/AR/ARMedian/Sphere", "{value}", 1, true)
        });

        var record = Assert.Single(result.Records);
        Assert.Equal("-1.25", record.Value);
    }


    [Fact]
    public void Map_EmptyTemplate_ShouldDefaultToValuePlaceholder()
    {
        var engine = new MappingEngine();
        var result = engine.Map(CreatePatient(), CreateMeasurements(), new[]
        {
            new MappingRule("1", "3102", "FirstName", "AIS.FirstName", "", 1, true)
        });

        var record = Assert.Single(result.Records);
        Assert.Equal("Anna", record.Value);
    }

    [Fact]
    public void Map_ShouldApplyTemplateWithMultiplePlaceholders()
    {
        var engine = new MappingEngine();
        var result = engine.Map(CreatePatient(), CreateMeasurements(), new[]
        {
            new MappingRule("1", "9999", "Composite", "AIS.LastName", "{patient.LastName};{Device.R/AR/ARMedian/Sphere};{value}", 1, true)
        });

        var record = Assert.Single(result.Records);
        Assert.Equal("Müller;-1.25;Müller", record.Value);
    }

    [Fact]
    public void Map_ShouldIgnoreDisabledRules()
    {
        var engine = new MappingEngine();
        var result = engine.Map(CreatePatient(), CreateMeasurements(), new[]
        {
            new MappingRule("1", "3101", "LastName", "AIS.LastName", "{value}", 1, false)
        });

        Assert.Empty(result.Records);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Map_MissingSource_ShouldCreateError()
    {
        var engine = new MappingEngine();
        var result = engine.Map(CreatePatient(), CreateMeasurements(), new[]
        {
            new MappingRule("1", "3101", "Missing", "Device.R/AR/ARMedian/Unknown", "{value}", 1, true)
        });

        Assert.Empty(result.Records);
        Assert.True(result.HasErrors);
    }

    [Fact]
    public void Map_EmptyTargetFieldCode_ShouldCreateError()
    {
        var engine = new MappingEngine();
        var result = engine.Map(CreatePatient(), CreateMeasurements(), new[]
        {
            new MappingRule("1", "", "Invalid", "AIS.LastName", "{value}", 1, true)
        });

        Assert.Empty(result.Records);
        Assert.True(result.HasErrors);
    }

    [Fact]
    public void Map_ShouldRespectSortOrder()
    {
        var engine = new MappingEngine();
        var result = engine.Map(CreatePatient(), CreateMeasurements(), new[]
        {
            new MappingRule("1", "B", "Second", "AIS.FirstName", "{value}", 2, true),
            new MappingRule("2", "A", "First", "AIS.LastName", "{value}", 1, true)
        });

        Assert.Equal(new[] { "A", "B" }, result.Records.Select(r => r.FieldCode));
    }

    private static PatientData CreatePatient() => new(
        PatientNumber: "PAT-100",
        LastName: "Müller",
        FirstName: "Anna",
        BirthDate: "01011980",
        PostalCodeCity: "12345 Berlin",
        Street: "Musterstraße 1",
        GenderCode: "W",
        SourceSystem: "MEDISTAR",
        TargetSystem: "NIDEK",
        GdtVersion: "03.00");

    private static MeasurementValue[] CreateMeasurements() =>
    {
        new("R/AR/ARMedian/Sphere", "Sphere", "-1.25", "D", "R", "ARMedian"),
        new("R/AR/ARMedian/Cylinder", "Cylinder", "-0.50", "D", "R", "ARMedian"),
        new("R/AR/ARMedian/Axis", "Axis", "090", "°", "R", "ARMedian"),
        new("L/AR/ARMedian/Sphere", "Sphere", "-1.00", "D", "L", "ARMedian"),
        new("L/AR/ARMedian/Cylinder", "Cylinder", "-0.25", "D", "L", "ARMedian"),
        new("L/AR/ARMedian/Axis", "Axis", "085", "°", "L", "ARMedian"),
        new("PD/PDList/FarPD", "FarPD", "62.0", "mm", null, "PDList"),
        new("PD/PDList/NearPD", "NearPD", "59.0", "mm", null, "PDList")
    };
}
