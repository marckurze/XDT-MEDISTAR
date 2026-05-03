using System;
using System.Collections.Generic;
using XdtDeviceBridge.Core;
using Xunit;

namespace XdtDeviceBridge.Tests;

public sealed class MappingEngineTests
{
    [Fact]
    public void Map_MapsPatientDataField()
    {
        var engine = new MappingEngine();
        var patient = CreatePatient();
        var measurements = CreateMeasurements();

        var rules = new[]
        {
            CreateRule("1", "3000", "Patientennummer", "AIS.PatientNumber", "{value}")
        };

        var result = engine.Map(patient, measurements, rules);

        Assert.False(result.HasErrors);
        var record = Assert.Single(result.Records);
        Assert.Equal("3000", record.FieldCode);
        Assert.Equal("4701-1", record.Value);
    }

    [Fact]
    public void Map_MapsDeviceValue()
    {
        var engine = new MappingEngine();
        var patient = CreatePatient();
        var measurements = CreateMeasurements();

        var rules = new[]
        {
            CreateRule("1", "9001", "Rechts Sphäre", "Device.R/AR/ARMedian/Sphere", "{value}")
        };

        var result = engine.Map(patient, measurements, rules);

        Assert.False(result.HasErrors);
        var record = Assert.Single(result.Records);
        Assert.Equal("9001", record.FieldCode);
        Assert.Equal("-0.25", record.Value);
    }

    [Fact]
    public void Map_UsesValueWhenTemplateIsEmpty()
    {
        var engine = new MappingEngine();
        var patient = CreatePatient();
        var measurements = CreateMeasurements();

        var rules = new[]
        {
            CreateRule("1", "9001", "Rechts Sphäre", "Device.R/AR/ARMedian/Sphere", "")
        };

        var result = engine.Map(patient, measurements, rules);

        Assert.False(result.HasErrors);
        Assert.Equal("-0.25", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_ReplacesMultiplePlaceholders()
    {
        var engine = new MappingEngine();
        var patient = CreatePatient();
        var measurements = CreateMeasurements();

        var rules = new[]
        {
            CreateRule(
                "1",
                "9999",
                "Text",
                "Device.R/AR/ARMedian/Sphere",
                "Patient {patient.LastName}, R Sphäre {Device.R/AR/ARMedian/Sphere}, R Cyl {Device.R/AR/ARMedian/Cylinder}")
        };

        var result = engine.Map(patient, measurements, rules);

        Assert.False(result.HasErrors);
        Assert.Equal("Patient Testfrau, R Sphäre -0.25, R Cyl -0.50", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_IgnoresDisabledRule()
    {
        var engine = new MappingEngine();
        var patient = CreatePatient();
        var measurements = CreateMeasurements();

        var rules = new[]
        {
            CreateRule("1", "9001", "Rechts Sphäre", "Device.R/AR/ARMedian/Sphere", "{value}", isEnabled: false)
        };

        var result = engine.Map(patient, measurements, rules);

        Assert.False(result.HasErrors);
        Assert.Empty(result.Records);
    }

    [Fact]
    public void Map_MissingSourceCreatesError()
    {
        var engine = new MappingEngine();
        var patient = CreatePatient();
        var measurements = CreateMeasurements();

        var rules = new[]
        {
            CreateRule("1", "9001", "Fehlender Wert", "Device.R/AR/ARMedian/DoesNotExist", "{value}")
        };

        var result = engine.Map(patient, measurements, rules);

        Assert.True(result.HasErrors);
        Assert.Empty(result.Records);
        Assert.Contains(result.Issues, issue => issue.Severity == MappingIssueSeverity.Error);
    }

    [Fact]
    public void Map_EmptyTargetFieldCodeCreatesError()
    {
        var engine = new MappingEngine();
        var patient = CreatePatient();
        var measurements = CreateMeasurements();

        var rules = new[]
        {
            CreateRule("1", "", "Rechts Sphäre", "Device.R/AR/ARMedian/Sphere", "{value}")
        };

        var result = engine.Map(patient, measurements, rules);

        Assert.True(result.HasErrors);
        Assert.Empty(result.Records);
        Assert.Contains(result.Issues, issue => issue.Severity == MappingIssueSeverity.Error);
    }

    [Fact]
    public void Map_SortsBySortOrder()
    {
        var engine = new MappingEngine();
        var patient = CreatePatient();
        var measurements = CreateMeasurements();

        var rules = new[]
        {
            CreateRule("2", "3102", "Vorname", "AIS.FirstName", "{value}", sortOrder: 2),
            CreateRule("1", "3101", "Nachname", "AIS.LastName", "{value}")
        };

        var result = engine.Map(patient, measurements, rules);

        Assert.False(result.HasErrors);
        Assert.Equal("3101", result.Records[0].FieldCode);
        Assert.Equal("3102", result.Records[1].FieldCode);
    }

    private static PatientData CreatePatient()
    {
        return new PatientData(
            PatientNumber: "4701-1",
            LastName: "Testfrau",
            FirstName: "Anna",
            BirthDate: "12061955",
            PostalCodeCity: "91207 Lauf",
            Street: "Marktplatz 11",
            GenderCode: "2",
            SourceSystem: "MEDISTAR Praxiscomputer GmbH Hannover",
            TargetSystem: "Geräteanbindung GA_XDT",
            GdtVersion: "02.10",
            ExaminationType: null);
    }

    private static List<MeasurementValue> CreateMeasurements()
    {
        return new List<MeasurementValue>
        {
            new("R/AR/ARMedian/Sphere", "Sphere", "-0.25", null, "R", "ARMedian"),
            new("R/AR/ARMedian/Cylinder", "Cylinder", "-0.50", null, "R", "ARMedian"),
            new("R/AR/ARMedian/Axis", "Axis", "49", null, "R", "ARMedian"),
            new("R/AR/ARMedian/SE", "SE", "-0.25", null, "R", "ARMedian"),
            new("L/AR/ARMedian/Sphere", "Sphere", "+0.00", null, "L", "ARMedian"),
            new("L/AR/ARMedian/Cylinder", "Cylinder", "-0.50", null, "L", "ARMedian"),
            new("L/AR/ARMedian/Axis", "Axis", "63", null, "L", "ARMedian"),
            new("L/AR/ARMedian/SE", "SE", "-0.25", null, "L", "ARMedian"),
            new("PD/PDList/FarPD", "FarPD", "61", null, null, "PDList"),
            new("PD/PDList/NearPD", "NearPD", "57", null, null, "PDList")
        };
    }

    private static MappingRule CreateRule(
        string id,
        string targetFieldCode,
        string targetName,
        string sourcePath,
        string outputTemplate,
        int sortOrder = 1,
        bool isEnabled = true)
    {
        return new MappingRule(id, targetFieldCode, targetName, sourcePath, outputTemplate, sortOrder, isEnabled);
    }
}
