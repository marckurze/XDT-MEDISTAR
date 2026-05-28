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
    public void Map_AllowsLiteralTemplateWithoutSourcePath()
    {
        var engine = new MappingEngine();
        var patient = CreatePatient();

        var result = engine.Map(
            patient,
            CreateMeasurements(),
            new[]
            {
                CreateRule("1", "6228", "Hinweis", string.Empty, "Phoropter finaler Verordnungswert")
            });

        Assert.False(result.HasErrors);
        var record = Assert.Single(result.Records);
        Assert.Equal("6228", record.FieldCode);
        Assert.Equal("Phoropter finaler Verordnungswert", record.Value);
    }

    [Fact]
    public void Map_RejectsRuleWithoutSourcePathAndTemplate()
    {
        var engine = new MappingEngine();

        var result = engine.Map(
            CreatePatient(),
            CreateMeasurements(),
            new[]
            {
                CreateRule("1", "6228", "Leer", string.Empty, string.Empty)
            });

        Assert.True(result.HasErrors);
        Assert.Empty(result.Records);
        Assert.Contains(result.Issues, issue => issue.Message == "SourcePath and OutputTemplate are empty.");
    }

    [Fact]
    public void Map_LiteralTemplateCanUseAisAliasPlaceholders()
    {
        var engine = new MappingEngine();

        var result = engine.Map(
            CreatePatient(),
            CreateMeasurements(),
            new[]
            {
                CreateRule("1", "6228", "Patient", string.Empty, "Patient={AIS.PatientNumber}; Geb={AIS.DateOfBirth}; Art={AIS.ExamType}")
            });

        Assert.False(result.HasErrors);
        Assert.Equal("Patient=4701-1; Geb=12061955; Art=", Assert.Single(result.Records).Value);
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
    public void Map_ReplacesAisLastNamePlaceholder()
    {
        var result = MapSingleTemplate("Nachname={AIS.LastName}");

        Assert.False(result.HasErrors);
        Assert.Equal("Nachname=Testfrau", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_ReplacesAisPatientNumberPlaceholder()
    {
        var result = MapSingleTemplate("Patient={AIS.PatientNumber}");

        Assert.False(result.HasErrors);
        Assert.Equal("Patient=4701-1", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_KeepsPatientPlaceholderSyntaxCompatible()
    {
        var result = MapSingleTemplate("Nachname={patient.LastName}");

        Assert.False(result.HasErrors);
        Assert.Equal("Nachname=Testfrau", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_UnknownAisPlaceholderRendersEmptyWithoutCrash()
    {
        var result = MapSingleTemplate("Unbekannt={AIS.Unknown}");

        Assert.False(result.HasErrors);
        Assert.Equal("Unbekannt=", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_FormatsDiopterPlaceholder()
    {
        var result = MapSingleTemplate("S={Device.R/AR/ARMedian/Sphere:Diopter}");

        Assert.False(result.HasErrors);
        Assert.Equal("S=- 0.25", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_FormatsAxisPlaceholder()
    {
        var result = MapSingleTemplate("A={Device.R/AR/ARMedian/Axis:Axis}");

        Assert.False(result.HasErrors);
        Assert.Equal("A= 49", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_FormatsPdPlaceholder()
    {
        var result = MapSingleTemplate("PD={Device.PD/PDList/FarPD:Pd}");

        Assert.False(result.HasErrors);
        Assert.Equal("PD=61", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_FormatsRawPlaceholder()
    {
        var result = MapSingleTemplate("Raw={Device.Formatting/Raw:Raw}");

        Assert.False(result.HasErrors);
        Assert.Equal("Raw=test", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_FormatsIopPlaceholder()
    {
        var result = MapSingleTemplate("IOP={Device.NT/IOP/R:Iop}");

        Assert.False(result.HasErrors);
        Assert.Equal("IOP=12.7", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_FormatsPachyPlaceholder()
    {
        var result = MapSingleTemplate("Pachy={Device.PACHY/R/Median:Pachy}");

        Assert.False(result.HasErrors);
        Assert.Equal("Pachy=559", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_FormatsPrismPlaceholder()
    {
        var result = MapSingleTemplate("Prism={Device.LM/R/Prism:Prism}");

        Assert.False(result.HasErrors);
        Assert.Equal("Prism=0.75", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_FormatsKeratometryPlaceholder()
    {
        var result = MapSingleTemplate("K={Device.KM/R/K1:Keratometry}");

        Assert.False(result.HasErrors);
        Assert.Equal("K=+43.25", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_FormatsTimePlaceholder()
    {
        var result = MapSingleTemplate("Zeit={Device.Time:Time}");

        Assert.False(result.HasErrors);
        Assert.Equal("Zeit=15:01", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_LeavesPlaceholderWithoutFormatUnchanged()
    {
        var result = MapSingleTemplate("S={Device.R/AR/ARMedian/Sphere}");

        Assert.False(result.HasErrors);
        Assert.Equal("S=-0.25", Assert.Single(result.Records).Value);
    }

    [Fact]
    public void Map_UnknownFormatReturnsRawValue()
    {
        var result = MapSingleTemplate("S={Device.R/AR/ARMedian/Sphere:Unknown}");

        Assert.False(result.HasErrors);
        Assert.Equal("S=-0.25", Assert.Single(result.Records).Value);
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
    public void Map_MissingOptionalPreparedLineShouldBeSkippedWhenOtherDeviceLineExists()
    {
        var engine = new MappingEngine();
        var patient = CreatePatient();
        var measurements = CreateMeasurements();
        measurements.Add(new MeasurementValue("Measure[@Type='REF']/REF/R/MedistarLine", "REF R", "R.:S=+ 1.00", null, "R", "REF"));

        var rules = new[]
        {
            CreateRule("1", "6228", "REF R", "Device.Measure[@Type='REF']/REF/R/MedistarLine", "{value}"),
            CreateRule("2", "6227", "SBJ", "Device.Measure[@Type='SBJ']/MedistarLine4", "{value}", sortOrder: 2)
        };

        var result = engine.Map(patient, measurements, rules);

        Assert.False(result.HasErrors);
        var record = Assert.Single(result.Records);
        Assert.Equal("6228", record.FieldCode);
        Assert.Equal("R.:S=+ 1.00", record.Value);
    }

    [Fact]
    public void Map_AllMissingOptionalPreparedDeviceLinesCreatesError()
    {
        var engine = new MappingEngine();
        var patient = CreatePatient();
        var measurements = CreateMeasurements();

        var rules = new[]
        {
            CreateRule("1", "6228", "REF R", "Device.Measure[@Type='REF']/REF/R/MedistarLine", "{value}"),
            CreateRule("2", "6221", "KM", "Device.Measure[@Type='KM']/KM/MedistarLine1", "{value}", sortOrder: 2)
        };

        var result = engine.Map(patient, measurements, rules);

        Assert.True(result.HasErrors);
        Assert.Empty(result.Records);
        Assert.Contains(result.Issues, issue => issue.Message == "No exportable device measurements were found.");
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
            new("PD/PDList/NearPD", "NearPD", "57", null, null, "PDList"),
            new("Formatting/Raw", "Raw", "  test  ", null, null, "Formatting"),
            new("NT/IOP/R", "IOP R", " 12.7 ", "mmHg", "R", "NT"),
            new("PACHY/R/Median", "Pachy R", "559", "um", "R", "PACHY"),
            new("LM/R/Prism", "Prism R", "0.75", null, "R", "LM"),
            new("KM/R/K1", "K1 R", "+43.25", "D", "R", "KM"),
            new("Time", "Time", "150100", null, null, "Meta")
        };
    }

    private static MappingResult MapSingleTemplate(string outputTemplate)
    {
        var engine = new MappingEngine();
        var rules = new[]
        {
            CreateRule("1", "6228", "Result", "Device.R/AR/ARMedian/Sphere", outputTemplate)
        };

        return engine.Map(CreatePatient(), CreateMeasurements(), rules);
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
