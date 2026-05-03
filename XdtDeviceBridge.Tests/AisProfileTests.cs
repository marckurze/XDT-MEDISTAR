using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class AisProfileTests
{
    [Fact]
    public void CreateMedistarDefault_ShouldCreateValidMedistarProfile()
    {
        var profile = DefaultAisProfiles.CreateMedistarDefault();

        Assert.Equal("MEDISTAR", profile.Name);
        Assert.Equal("MEDISTAR Praxiscomputer GmbH", profile.Vendor);
        Assert.Equal("Windows-1252", profile.DefaultEncoding);
        Assert.Equal("6310", profile.RequiredStaticFields["8000"]);
        Assert.Equal(new[] { "3000", "3101", "3102", "3103" }, profile.RequiredPatientFieldCodes);
        Assert.Contains("6228", profile.SupportedOutputFieldCodes);
        Assert.Contains("8402", profile.SupportedOutputFieldCodes);
        Assert.True(profile.SupportsResultTextField6228);
        Assert.True(profile.SupportsCategoryValuePairs);
        Assert.True(profile.RequiresExaminationType8402);
    }

    [Fact]
    public void Validate_ShouldAcceptMedistarProfile()
    {
        var issues = AisProfileValidator.Validate(DefaultAisProfiles.CreateMedistarDefault());

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ShouldReportWrongProfileKind()
    {
        var profile = DefaultAisProfiles.CreateMedistarDefault() with
        {
            Metadata = CreateMetadata(ProfileKind.DeviceProfile)
        };

        var issues = AisProfileValidator.Validate(profile);

        Assert.Contains("Metadata.ProfileKind must be AisProfile.", issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptyName()
    {
        var profile = DefaultAisProfiles.CreateMedistarDefault() with { Name = "" };

        var issues = AisProfileValidator.Validate(profile);

        Assert.Contains("Name must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptyDefaultEncoding()
    {
        var profile = DefaultAisProfiles.CreateMedistarDefault() with { DefaultEncoding = " " };

        var issues = AisProfileValidator.Validate(profile);

        Assert.Contains("DefaultEncoding must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldRequire8402WhenExaminationTypeIsRequired()
    {
        var profile = DefaultAisProfiles.CreateMedistarDefault() with
        {
            SupportedOutputFieldCodes = new[] { "6228" }
        };

        var issues = AisProfileValidator.Validate(profile);

        Assert.Contains("SupportedOutputFieldCodes must contain 8402 when RequiresExaminationType8402 is true.", issues);
    }

    [Fact]
    public void Validate_ShouldRequire6228WhenResultTextFieldIsSupported()
    {
        var profile = DefaultAisProfiles.CreateMedistarDefault() with
        {
            SupportedOutputFieldCodes = new[] { "8402" }
        };

        var issues = AisProfileValidator.Validate(profile);

        Assert.Contains("SupportedOutputFieldCodes must contain 6228 when SupportsResultTextField6228 is true.", issues);
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
