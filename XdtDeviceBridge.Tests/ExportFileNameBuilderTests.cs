using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class ExportFileNameBuilderTests
{
    [Fact]
    public void Build_ShouldCreateExpectedStandardFileName()
    {
        var builder = new ExportFileNameBuilder();
        var profile = DefaultDeviceProfiles.CreateNidekArk1sDefault();
        var patient = CreatePatient("4701-1");
        var timestamp = new DateTime(2026, 4, 29, 23, 23, 18);

        var fileName = builder.Build(profile, patient, timestamp);

        Assert.Equal("NIDEK_ARK1S_4701-1_20260429_232318.XDT", fileName);
    }

    [Fact]
    public void Build_ShouldReplacePatientNumberPlaceholder()
    {
        var builder = new ExportFileNameBuilder();
        var profile = DefaultDeviceProfiles.CreateNidekArk1sDefault() with { ExportFileNamePattern = "EXP_{PatientNumber}.XDT" };

        var fileName = builder.Build(profile, CreatePatient("ABC-99"), new DateTime(2026, 1, 1, 1, 2, 3));

        Assert.Equal("EXP_ABC-99.XDT", fileName);
    }

    [Fact]
    public void Build_ShouldReplaceDateTimePlaceholders()
    {
        var builder = new ExportFileNameBuilder();
        var profile = DefaultDeviceProfiles.CreateNidekArk1sDefault() with { ExportFileNamePattern = "A_{yyyyMMdd}_{HHmmss}_{yyyyMMdd_HHmmss}" };
        var timestamp = new DateTime(2026, 5, 1, 9, 8, 7);

        var fileName = builder.Build(profile, CreatePatient("1"), timestamp);

        Assert.Equal("A_20260501_090807_20260501_090807.XDT", fileName);
    }

    [Fact]
    public void Build_ShouldReplaceInvalidWindowsCharacters()
    {
        var builder = new ExportFileNameBuilder();
        var profile = DefaultDeviceProfiles.CreateNidekArk1sDefault() with { ExportFileNamePattern = "EXP_{LastName}_{FirstName}" };
        var patient = new PatientData("1", "Mül:ler", "A/na", "01011980", null, null, null, null, null, null);

        var fileName = builder.Build(profile, patient, new DateTime(2026, 1, 1));

        Assert.Equal("EXP_Mül_ler_A_na.XDT", fileName);
    }

    [Fact]
    public void Build_ShouldAddXdtExtensionIfMissing()
    {
        var builder = new ExportFileNameBuilder();
        var profile = DefaultDeviceProfiles.CreateNidekArk1sDefault() with { ExportFileNamePattern = "FILE_{PatientNumber}" };

        var fileName = builder.Build(profile, CreatePatient("100"), new DateTime(2026, 1, 1));

        Assert.EndsWith(".XDT", fileName);
    }

    [Fact]
    public void Build_ShouldReplaceSpaces()
    {
        var builder = new ExportFileNameBuilder();
        var profile = DefaultDeviceProfiles.CreateNidekArk1sDefault() with { ExportFileNamePattern = "FILE {LastName} {FirstName}" };
        var patient = new PatientData("1", "von Test", "Anna Maria", "01011980", null, null, null, null, null, null);

        var fileName = builder.Build(profile, patient, new DateTime(2026, 1, 1));

        Assert.Equal("FILE_von_Test_Anna_Maria.XDT", fileName);
    }

    [Fact]
    public void Build_EmptyPattern_ShouldUseFallback()
    {
        var builder = new ExportFileNameBuilder();
        var profile = DefaultDeviceProfiles.CreateNidekArk1sDefault() with { ExportFileNamePattern = "" };
        var timestamp = new DateTime(2026, 4, 29, 23, 23, 18);

        var fileName = builder.Build(profile, CreatePatient("100"), timestamp);

        Assert.Equal("EXPORT_20260429_232318.XDT", fileName);
    }

    private static PatientData CreatePatient(string patientNumber)
        => new(patientNumber, "Müller", "Jörg", "01011980", null, null, null, null, null, null);
}
