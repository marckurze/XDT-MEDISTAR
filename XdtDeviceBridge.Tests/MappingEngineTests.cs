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
            new MappingRule
            {
                Id = "1",
                TargetFieldCode = "3000",
                TargetName = "Patientennummer",
                SourcePath = "AIS.PatientNumber",
                OutputTemplate = "{value}",
                SortOrder = 1,
                IsEnabled = true
            }
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
            new MappingRule
            {
                Id = "1",
                TargetFieldCode = "9001",
                TargetName = "Rechts Sphäre",
                SourcePath = "Device.R/AR/ARMedian/Sphere",
                OutputTemplate = "{value}",
                SortOrder = 1,
                IsEnabled = true
            }
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
            new MappingRule
            {
                Id = "1",
                TargetFieldCode = "9001",
                TargetName = "Rechts Sphäre",
                SourcePath = "Device.R/AR/ARMedian/Sphere",
                OutputTemplate = "",
                SortOrder = 1,
                IsEnabled = true
            }
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
            new MappingRule
            {
                Id = "1",
                TargetFieldCode = "9999",
                TargetName = "Text",
                SourcePath = "Device.R/AR/ARMedian/Sphere",
                OutputTemplate = "Patient {patient.LastName}, R Sphäre {Device.R/AR/ARMedian/Sphere}, R Cyl {Device.R/AR/ARMedian/Cylinder}",
                SortOrder = 1,
                IsEnabled = true
            }
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
            new MappingRule
            {
                Id = "1",
                TargetFieldCode = "9001",
                TargetName = "Rechts Sphäre",
                SourcePath = "Device.R/AR/ARMedian/Sphere",
                OutputTemplate = "{value}",
                SortOrder = 1,
                IsEnabled = false
            }
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
            new MappingRule
            {
                Id = "1",
                TargetFieldCode = "9001",
                TargetName = "Fehlender Wert",
                SourcePath = "Device.R/AR/ARMedian/DoesNotExist",
                OutputTemplate = "{value}",
                SortOrder = 1,
                IsEnabled = true
            }
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
            new MappingRule
            {
                Id = "1",
                TargetFieldCode = "",
                TargetName = "Rechts Sphäre",
                SourcePath = "Device.R/AR/ARMedian/Sphere",
                OutputTemplate = "{value}",
                SortOrder = 1,
                IsEnabled = true
            }
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
            new MappingRule
            {
                Id = "2",
                TargetFieldCode = "3102",
                TargetName = "Vorname",
                SourcePath = "AIS.FirstName",
                OutputTemplate = "{value}",
                SortOrder = 2,
                IsEnabled = true
            },
            new MappingRule
            {
                Id = "1",
                TargetFieldCode = "3101",
                TargetName = "Nachname",
                SourcePath = "AIS.LastName",
                OutputTemplate = "{value}",
                SortOrder = 1,
                IsEnabled = true
            }
        };

        var result = engine.Map(patient, measurements, rules);

        Assert.False(result.HasErrors);
        Assert.Equal("3101", result.Records[0].FieldCode);
        Assert.Equal("3102", result.Records[1].FieldCode);
    }

    private static PatientData CreatePatient()
    {
        return new PatientData
        {
            PatientNumber = "4701-1",
            LastName = "Testfrau",
            FirstName = "Anna",
            BirthDate = new DateOnly(1955, 6, 12),
            Street = "Marktplatz 11",
            PostalCodeCity = "91207 Lauf",
            GenderCode = "2",
            SourceSystem = "MEDISTAR Praxiscomputer GmbH Hannover",
            TargetSystem = "Geräteanbindung GA_XDT",
            GdtVersion = "02.10"
        };
    }

    private static List<MeasurementValue> CreateMeasurements()
    {
        return new List<MeasurementValue>
        {
            new MeasurementValue { SourcePath = "R/AR/ARMedian/Sphere", DisplayName = "Sphere", Value = "-0.25", Eye = "R", Group = "ARMedian" },
            new MeasurementValue { SourcePath = "R/AR/ARMedian/Cylinder", DisplayName = "Cylinder", Value = "-0.50", Eye = "R", Group = "ARMedian" },
            new MeasurementValue { SourcePath = "R/AR/ARMedian/Axis", DisplayName = "Axis", Value = "49", Eye = "R", Group = "ARMedian" },
            new MeasurementValue { SourcePath = "R/AR/ARMedian/SE", DisplayName = "SE", Value = "-0.25", Eye = "R", Group = "ARMedian" },
            new MeasurementValue { SourcePath = "L/AR/ARMedian/Sphere", DisplayName = "Sphere", Value = "+0.00", Eye = "L", Group = "ARMedian" },
            new MeasurementValue { SourcePath = "L/AR/ARMedian/Cylinder", DisplayName = "Cylinder", Value = "-0.50", Eye = "L", Group = "ARMedian" },
            new MeasurementValue { SourcePath = "L/AR/ARMedian/Axis", DisplayName = "Axis", Value = "63", Eye = "L", Group = "ARMedian" },
            new MeasurementValue { SourcePath = "L/AR/ARMedian/SE", DisplayName = "SE", Value = "-0.25", Eye = "L", Group = "ARMedian" },
            new MeasurementValue { SourcePath = "PD/PDList/FarPD", DisplayName = "FarPD", Value = "61", Group = "PDList" },
            new MeasurementValue { SourcePath = "PD/PDList/NearPD", DisplayName = "NearPD", Value = "57", Group = "PDList" }
        };
    }
}
