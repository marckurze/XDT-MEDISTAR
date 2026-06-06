using System.Reflection;
using System.Text.Json;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class DeviceTechnicalProfileServiceTests
{
    [Fact]
    public void OriginalCatalog_ShouldContainExactlyOneProfileForEveryBuiltInDevice()
    {
        var builtIns = CreateAllBuiltInDeviceProfiles();
        var profiles = LoadOriginalProfiles();

        Assert.Equal(builtIns.Count, profiles.Count);
        Assert.DoesNotContain(
            profiles.GroupBy(profile => profile.DeviceProfileId, StringComparer.OrdinalIgnoreCase),
            group => group.Count() > 1);

        foreach (var builtIn in builtIns)
        {
            Assert.Contains(profiles, profile => string.Equals(
                profile.DeviceProfileId,
                builtIn.Metadata.Id,
                StringComparison.OrdinalIgnoreCase));
        }

        foreach (var profile in profiles)
        {
            Assert.Contains(builtIns, builtIn => string.Equals(
                builtIn.Metadata.Id,
                profile.DeviceProfileId,
                StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void OriginalCatalog_ShouldContainRequiredPublicFields()
    {
        foreach (var profile in LoadOriginalProfiles())
        {
            Assert.False(string.IsNullOrWhiteSpace(profile.DeviceProfileId), profile.DeviceProfileId);
            Assert.False(string.IsNullOrWhiteSpace(profile.Manufacturer), profile.DeviceProfileId);
            Assert.False(string.IsNullOrWhiteSpace(profile.Model), profile.DeviceProfileId);
            Assert.False(string.IsNullOrWhiteSpace(profile.DeviceType), profile.DeviceProfileId);
            Assert.False(string.IsNullOrWhiteSpace(profile.ShortDescription), profile.DeviceProfileId);

            if (!profile.DeviceType.Contains("Dokument", StringComparison.OrdinalIgnoreCase))
            {
                Assert.NotEmpty(profile.Measurements);
                Assert.All(profile.Measurements, measurement =>
                {
                    Assert.False(string.IsNullOrWhiteSpace(measurement.Name), profile.DeviceProfileId);
                    Assert.False(string.IsNullOrWhiteSpace(measurement.Description), profile.DeviceProfileId);
                    Assert.False(string.IsNullOrWhiteSpace(measurement.UsedFor), profile.DeviceProfileId);
                });
            }
        }
    }

    [Fact]
    public void OriginalCatalog_ShouldReferenceExistingBuiltInDeviceImages()
    {
        foreach (var profile in LoadOriginalProfiles())
        {
            Assert.StartsWith(InterfaceProfileUiPolicy.BuiltInDeviceImageRoot, profile.ImagePath, StringComparison.Ordinal);
            var fileName = profile.ImagePath[InterfaceProfileUiPolicy.BuiltInDeviceImageRoot.Length..];
            Assert.True(
                File.Exists(FindWorkspaceFile("XdtDeviceBridge.App", "Assets", "Devices", fileName)),
                $"Device image missing for {profile.DeviceProfileId}: {fileName}");
        }
    }

    [Fact]
    public void OriginalCatalogAndMarkdown_ShouldNotContainInternalOutputSyntaxOrBlockedTerms()
    {
        var content = File.ReadAllText(GetOriginalCatalogPath())
            + Environment.NewLine
            + File.ReadAllText(FindWorkspaceFile("docs", "GERAETE_STECKBRIEFE.md"));

        var blockedTerms = new[]
        {
            string.Concat("Middle", "ware"),
            string.Concat("Team", "2", "Work"),
            string.Concat("T", "2", "W"),
            string.Concat("Zirle", "wagen"),
            string.Concat("Kai ", "Zirle", "wagen")
        };

        foreach (var term in blockedTerms)
        {
            Assert.DoesNotContain(term, content, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var term in new[] { "MEDISTAR", "6228", "6227", "6221", "6205", "6220", "6302", "6303", "6305", "8402" })
        {
            Assert.DoesNotContain(term, content, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var term in new[] { "vermutlich", "wahrscheinlich", "eventuell", "könnte", "unklar", "nicht sicher", "TODO", "C:\\", "AppData", "%LocalAppData%" })
        {
            Assert.DoesNotContain(term, content, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Service_ShouldSaveLoadAndResetLocalOverride()
    {
        using var temp = new TempFolder();
        var overridePath = Path.Combine(temp.Path, DeviceTechnicalProfileService.DefaultOverrideFileName);
        var service = new DeviceTechnicalProfileService(GetOriginalCatalogPath(), overridePath);
        var profileId = "device-nidek-lm7-default";

        service.SaveOverride(new DeviceTechnicalProfileOverride(
            profileId,
            ShortDescription: "Lokale Beschreibung",
            TechnicalNotes: new[] { "Lokaler Hinweis" },
            TechnicianNote: "Techniker-Notiz"));

        var merged = service.LoadProfile(profileId);

        Assert.NotNull(merged);
        Assert.Equal("Lokale Beschreibung", merged!.ShortDescription);
        Assert.Contains("Lokaler Hinweis", merged.TechnicalNotes!);
        Assert.Equal("Techniker-Notiz", merged.TechnicianNote);
        Assert.True(File.Exists(overridePath));

        service.ResetOverride(profileId);

        var reset = service.LoadProfile(profileId);
        Assert.NotNull(reset);
        Assert.NotEqual("Lokale Beschreibung", reset!.ShortDescription);
        Assert.Equal(string.Empty, reset.TechnicianNote);
    }

    [Fact]
    public void Service_ShouldSaveTechnicianNoteWithoutChangingOriginalCatalog()
    {
        using var temp = new TempFolder();
        var overridePath = Path.Combine(temp.Path, DeviceTechnicalProfileService.DefaultOverrideFileName);
        var originalBefore = File.ReadAllText(GetOriginalCatalogPath());
        var service = new DeviceTechnicalProfileService(GetOriginalCatalogPath(), overridePath);

        service.SaveTechnicianNote("device-topcon-cv5000-default", "steht im Refraktionsraum");

        var profile = service.LoadProfile("device-topcon-cv5000-default");
        Assert.NotNull(profile);
        Assert.Equal("steht im Refraktionsraum", profile!.TechnicianNote);
        Assert.Equal(originalBefore, File.ReadAllText(GetOriginalCatalogPath()));
    }

    [Fact]
    public void Service_ShouldIgnoreBrokenOverrideFile()
    {
        using var temp = new TempFolder();
        var overridePath = Path.Combine(temp.Path, DeviceTechnicalProfileService.DefaultOverrideFileName);
        File.WriteAllText(overridePath, "{broken");
        var service = new DeviceTechnicalProfileService(GetOriginalCatalogPath(), overridePath);

        var profile = service.LoadProfile("device-nidek-lm7-default");

        Assert.NotNull(profile);
        Assert.Equal(string.Empty, profile!.TechnicianNote);
    }

    private static IReadOnlyList<DeviceTechnicalProfile> LoadOriginalProfiles()
    {
        using var stream = File.OpenRead(GetOriginalCatalogPath());
        return new DeviceTechnicalProfileService(GetOriginalCatalogPath(), Path.Combine(Path.GetTempPath(), "unused-device-info-overrides.json"))
            .LoadOriginalProfiles(stream);
    }

    private static IReadOnlyList<DeviceProfileDefinition> CreateAllBuiltInDeviceProfiles()
    {
        return typeof(DefaultDeviceProfileDefinitions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.ReturnType == typeof(DeviceProfileDefinition)
                && method.GetParameters().Length == 0)
            .Select(method => (DeviceProfileDefinition)method.Invoke(null, null)!)
            .Where(profile => profile.Metadata.IsBuiltIn)
            .ToArray();
    }

    private static string GetOriginalCatalogPath()
    {
        return FindWorkspaceFile("XdtDeviceBridge.App", "Assets", "DeviceInfo", "device-technical-profiles.de.json");
    }

    private static string FindWorkspaceFile(params string[] relativeSegments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(relativeSegments).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Workspace file not found: {Path.Combine(relativeSegments)}");
    }

    private sealed class TempFolder : IDisposable
    {
        public TempFolder()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"xdtbox-device-info-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
