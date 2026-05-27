using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class XdtBoxAppSettingsRepositoryTests
{
    [Fact]
    public void LoadOrDefault_WhenFileMissing_ReturnsProductiveDefaults()
    {
        var tempFolder = CreateTempFolder();
        var repository = new XdtBoxAppSettingsRepository();

        var settings = repository.LoadOrDefault(Path.Combine(tempFolder, "ui", "app-settings.json"));

        Assert.False(settings.StartMinimizedToTray);
        Assert.True(settings.AutoStartMonitoringOnAppStart);
        Assert.True(settings.CloseToTrayInsteadOfExit);
        Assert.True(settings.ConfirmExitWhileMonitoring);
    }

    [Fact]
    public void SaveAndLoad_ShouldRoundtripStartupAndTraySettings()
    {
        var tempFolder = CreateTempFolder();
        var filePath = Path.Combine(tempFolder, "ui", "app-settings.json");
        var repository = new XdtBoxAppSettingsRepository();
        var expected = new XdtBoxAppSettings
        {
            StartMinimizedToTray = true,
            AutoStartMonitoringOnAppStart = false,
            CloseToTrayInsteadOfExit = false,
            ConfirmExitWhileMonitoring = true
        };

        repository.Save(filePath, expected);
        var actual = repository.LoadOrDefault(filePath);

        Assert.True(actual.StartMinimizedToTray);
        Assert.False(actual.AutoStartMonitoringOnAppStart);
        Assert.False(actual.CloseToTrayInsteadOfExit);
        Assert.True(actual.ConfirmExitWhileMonitoring);
    }

    [Fact]
    public void LoadOrDefault_WhenJsonIsInvalid_ReturnsDefaults()
    {
        var tempFolder = CreateTempFolder();
        var filePath = Path.Combine(tempFolder, "ui", "app-settings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, "{ invalid");
        var repository = new XdtBoxAppSettingsRepository();

        var settings = repository.LoadOrDefault(filePath);

        Assert.True(settings.AutoStartMonitoringOnAppStart);
        Assert.True(settings.CloseToTrayInsteadOfExit);
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "xdtbox-app-settings-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }
}
