using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TabProtectionServiceTests
{
    private readonly TabProtectionService _service = new();

    [Fact]
    public void CreateEnabledSettings_ShouldStoreSaltAndHashWithoutPlainPassword()
    {
        var password = "Techniker-123";

        var settings = _service.CreateEnabledSettings(password);

        Assert.True(settings.HasPassword);
        Assert.NotEqual(password, settings.PasswordHashBase64);
        Assert.NotEqual(password, settings.SaltBase64);
        Assert.True(settings.Iterations >= 100_000);
        Assert.True(_service.VerifyPassword(settings, password));
        Assert.False(_service.VerifyPassword(settings, "falsch"));
    }

    [Fact]
    public void SaveAndLoad_ShouldRoundTripSettings()
    {
        using var temp = new TempFolder();
        var filePath = Path.Combine(temp.Path, "tab-protection.json");
        var settings = _service.CreateEnabledSettings("Techniker-123");

        _service.Save(filePath, settings);
        var loaded = _service.LoadOrDefault(filePath);

        Assert.True(loaded.HasPassword);
        Assert.True(_service.VerifyPassword(loaded, "Techniker-123"));
        Assert.DoesNotContain("Techniker-123", File.ReadAllText(filePath));
    }

    [Fact]
    public void GeneralPassword_ShouldOnlyBeAcceptedWhenUserPasswordExists()
    {
        var generalPassword = string.Concat("##", "XDT", "Box", "Admin", "##");
        var disabled = TabProtectionSettings.Disabled;
        var enabled = _service.CreateEnabledSettings("Praxis-Passwort");

        Assert.False(_service.VerifyGeneralPassword(disabled, generalPassword));
        Assert.True(_service.VerifyGeneralPassword(enabled, generalPassword));
    }

    private sealed class TempFolder : IDisposable
    {
        public TempFolder()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"xdtbox-tab-protection-tests-{Guid.NewGuid():N}");
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
