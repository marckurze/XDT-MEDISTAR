using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class AttachmentFileNameBuilderTests
{
    private readonly AttachmentFileNameBuilder _builder = new();
    private readonly DateTime _timestamp = new(2026, 5, 7, 22, 17, 23);

    [Fact]
    public void Build_ShouldUseDefaultTemplateWhenTemplateIsEmpty()
    {
        var fileName = _builder.Build("", CreatePatient(patientNumber: "11253"), _timestamp, ".pdf");

        Assert.Equal("11253_07052026_221723.PDF", fileName);
    }

    [Fact]
    public void Build_ShouldInsertPatientNumber()
    {
        var fileName = _builder.Build("{Ais.PatientNumber}{ExtensionUpper}", CreatePatient(patientNumber: "4711"), _timestamp, ".jpg");

        Assert.Equal("4711.JPG", fileName);
    }

    [Fact]
    public void Build_ShouldFormatDateAndTime()
    {
        var fileName = _builder.Build("{Date:yyyyMMdd}_{Date:ddMMyyyy}_{Time:HHmmss}{ExtensionUpper}", CreatePatient(), _timestamp, ".pdf");

        Assert.Equal("20260507_07052026_221723.PDF", fileName);
    }

    [Fact]
    public void Build_ShouldKeepOriginalExtensionCaseLower()
    {
        var fileName = _builder.Build("scan{OriginalExtension}", CreatePatient(), _timestamp, ".pdf");

        Assert.Equal("scan.pdf", fileName);
    }

    [Fact]
    public void Build_ShouldUppercaseExtension()
    {
        var fileName = _builder.Build("scan{ExtensionUpper}", CreatePatient(), _timestamp, ".pdf");

        Assert.Equal("scan.PDF", fileName);
    }

    [Fact]
    public void Build_ShouldSanitizeInvalidFileNameCharacters()
    {
        var fileName = _builder.Build("{Ais.PatientNumber}_{Ais.LastName}{ExtensionUpper}", CreatePatient(patientNumber: "11:253", lastName: "Mül?ler"), _timestamp, ".pdf");

        Assert.Equal("11_253_Mül_ler.PDF", fileName);
    }

    [Fact]
    public void Build_ShouldNotReturnPathParts()
    {
        var fileName = _builder.Build(@"..\{Ais.PatientNumber}\report{ExtensionUpper}", CreatePatient(patientNumber: "11253"), _timestamp, ".pdf");

        Assert.Equal("report.PDF", fileName);
        Assert.DoesNotContain(@"\", fileName);
        Assert.DoesNotContain("/", fileName);
    }

    [Fact]
    public void Build_ShouldTolerateMissingOptionalValues()
    {
        var fileName = _builder.Build("{Ais.LastName}_{Ais.FirstName}_{Ais.BirthDate:ddMMyyyy}{ExtensionUpper}", CreatePatient(), _timestamp, ".pdf");

        Assert.Equal("PDF", fileName);
    }

    [Fact]
    public void Build_ShouldFormatOptionalBirthDateWhenPresent()
    {
        var fileName = _builder.Build("{Ais.BirthDate:ddMMyyyy}{ExtensionUpper}", CreatePatient(birthDate: "1980-05-31"), _timestamp, ".pdf");

        Assert.Equal("31051980.PDF", fileName);
    }

    [Fact]
    public void BuildUniqueFileName_ShouldReturnOriginalNameWhenFileDoesNotExist()
    {
        var folder = CreateTempFolder();

        var fileName = _builder.BuildUniqueFileName(folder, "11253_07052026_221723.PDF");

        Assert.Equal("11253_07052026_221723.PDF", fileName);
    }

    [Fact]
    public void BuildUniqueFileName_ShouldAppendSuffixWhenFileExists()
    {
        var folder = CreateTempFolder();
        File.WriteAllText(Path.Combine(folder, "11253_07052026_221723.PDF"), string.Empty);

        var fileName = _builder.BuildUniqueFileName(folder, "11253_07052026_221723.PDF");

        Assert.Equal("11253_07052026_221723_001.PDF", fileName);
    }

    [Fact]
    public void BuildUniqueFileName_ShouldIncrementSuffix()
    {
        var folder = CreateTempFolder();
        File.WriteAllText(Path.Combine(folder, "11253_07052026_221723.PDF"), string.Empty);
        File.WriteAllText(Path.Combine(folder, "11253_07052026_221723_001.PDF"), string.Empty);

        var fileName = _builder.BuildUniqueFileName(folder, "11253_07052026_221723.PDF");

        Assert.Equal("11253_07052026_221723_002.PDF", fileName);
    }

    private static PatientData CreatePatient(
        string? patientNumber = null,
        string? lastName = null,
        string? firstName = null,
        string? birthDate = null)
    {
        return new PatientData(
            PatientNumber: patientNumber,
            LastName: lastName,
            FirstName: firstName,
            BirthDate: birthDate,
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: null,
            TargetSystem: null,
            GdtVersion: null,
            ExaminationType: null);
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }
}
