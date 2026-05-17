using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileNotificationSoundServiceTests
{
    private static readonly DateTime BaseTime = new(2026, 5, 17, 11, 0, 0, DateTimeKind.Local);

    [Theory]
    [InlineData("scan-ais-detected")]
    [InlineData("scan-device-detected")]
    [InlineData("scan-ready-pair")]
    public void TryPlay_ShouldPlayForRelevantFileEvents(string eventKey)
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService();
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            var result = service.TryPlay(Event("interface-ar360", eventKey, BaseTime), soundFilePath, player);

            Assert.True(result.ShouldPlay);
            Assert.True(result.WasPlayed);
            Assert.False(result.IsSuppressedByCooldown);
            Assert.Equal(1, player.PlayCount);
            Assert.Equal(soundFilePath, player.LastPath);
        }
        finally
        {
            File.Delete(soundFilePath);
        }
    }

    [Fact]
    public void TryPlay_ShouldPlayForAttachmentArrivalMessage()
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService();
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            var result = service.TryPlay(
                Event(
                    "interface-ark1s",
                    "pair:patient-device:message:XDT-Anhang vorbereitet",
                    BaseTime,
                    "XDT-Anhang vorbereitet: report.pdf"),
                soundFilePath,
                player);

            Assert.True(result.ShouldPlay);
            Assert.True(result.WasPlayed);
            Assert.Equal(1, player.PlayCount);
        }
        finally
        {
            File.Delete(soundFilePath);
        }
    }

    [Theory]
    [InlineData("monitoring", "manual-scan-started", "Einmaliger Scan gestartet.")]
    [InlineData("interface-ar360", "monitoring-card-detached", "Gerätekarte abgedockt.")]
    [InlineData("interface-ar360", "package-state", "Warte auf Gerätedatei.")]
    [InlineData("interface-ar360", "scan-message:AIS-Importordner fehlt.", "AIS-Importordner fehlt.")]
    public void TryPlay_ShouldIgnoreNonRelevantEvents(string scopeId, string eventKey, string message)
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService();
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            var result = service.TryPlay(Event(scopeId, eventKey, BaseTime, message), soundFilePath, player);

            Assert.False(result.ShouldPlay);
            Assert.False(result.WasPlayed);
            Assert.Equal(0, player.PlayCount);
        }
        finally
        {
            File.Delete(soundFilePath);
        }
    }

    [Fact]
    public void TryPlay_ShouldSuppressSecondSoundInsideCooldownForSameProfile()
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService();
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            var first = service.TryPlay(Event("interface-ar360", "scan-ais-detected", BaseTime), soundFilePath, player);
            var second = service.TryPlay(Event("interface-ar360", "scan-device-detected", BaseTime.AddSeconds(1)), soundFilePath, player);

            Assert.True(first.WasPlayed);
            Assert.True(second.ShouldPlay);
            Assert.True(second.IsSuppressedByCooldown);
            Assert.False(second.WasPlayed);
            Assert.Equal(1, player.PlayCount);
        }
        finally
        {
            File.Delete(soundFilePath);
        }
    }

    [Fact]
    public void TryPlay_ShouldAllowSoundAfterCooldown()
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService();
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            _ = service.TryPlay(Event("interface-ar360", "scan-ais-detected", BaseTime), soundFilePath, player);
            var second = service.TryPlay(Event("interface-ar360", "scan-device-detected", BaseTime.AddSeconds(3)), soundFilePath, player);

            Assert.True(second.WasPlayed);
            Assert.False(second.IsSuppressedByCooldown);
            Assert.Equal(2, player.PlayCount);
        }
        finally
        {
            File.Delete(soundFilePath);
        }
    }

    [Fact]
    public void TryPlay_ShouldKeepCooldownPerProfile()
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService();
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            _ = service.TryPlay(Event("interface-ar360", "scan-ais-detected", BaseTime), soundFilePath, player);
            var ark1s = service.TryPlay(Event("interface-ark1s", "scan-device-detected", BaseTime.AddSeconds(1)), soundFilePath, player);

            Assert.True(ark1s.WasPlayed);
            Assert.False(ark1s.IsSuppressedByCooldown);
            Assert.Equal(2, player.PlayCount);
        }
        finally
        {
            File.Delete(soundFilePath);
        }
    }

    [Fact]
    public void TryPlay_ShouldNotThrowWhenSoundFileIsMissing()
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService();
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.wav");

        var result = service.TryPlay(Event("interface-ar360", "scan-ais-detected", BaseTime), missingPath, player);

        Assert.True(result.ShouldPlay);
        Assert.False(result.WasPlayed);
        Assert.False(result.IsSuppressedByCooldown);
        Assert.Equal("Signalton-Datei fehlt.", result.Message);
        Assert.Equal(0, player.PlayCount);
    }

    [Fact]
    public void TryPlay_ShouldNotThrowWhenPlayerFails()
    {
        var player = new FakeSoundPlayer(throwOnPlay: true);
        var service = new InterfaceProfileNotificationSoundService();
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            var result = service.TryPlay(Event("interface-ar360", "scan-ais-detected", BaseTime), soundFilePath, player);

            Assert.True(result.ShouldPlay);
            Assert.False(result.WasPlayed);
            Assert.Contains("Signalton konnte nicht abgespielt werden", result.Message);
            Assert.Equal(1, player.PlayCount);
        }
        finally
        {
            File.Delete(soundFilePath);
        }
    }

    [Fact]
    public void TryPlay_ShouldNotPlayWhenDisabled()
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService(isEnabled: false);
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            var result = service.TryPlay(Event("interface-ar360", "scan-ais-detected", BaseTime), soundFilePath, player);

            Assert.False(result.ShouldPlay);
            Assert.False(result.WasPlayed);
            Assert.Equal(0, player.PlayCount);
        }
        finally
        {
            File.Delete(soundFilePath);
        }
    }

    private static InterfaceMonitoringEventEntry Event(
        string scopeId,
        string eventKey,
        DateTime timestamp,
        string? message = null)
    {
        return new InterfaceMonitoringEventEntry(
            timestamp,
            scopeId,
            eventKey,
            message ?? eventKey,
            InterfaceMonitoringEventSeverity.Info);
    }

    private static string CreateExistingSoundFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"xdt-sound-test-{Guid.NewGuid():N}.wav");
        File.WriteAllBytes(path, Array.Empty<byte>());
        return path;
    }

    private sealed class FakeSoundPlayer(bool throwOnPlay = false) : IInterfaceProfileNotificationSoundPlayer
    {
        public int PlayCount { get; private set; }

        public string? LastPath { get; private set; }

        public void Play(string soundFilePath)
        {
            PlayCount++;
            LastPath = soundFilePath;
            if (throwOnPlay)
            {
                throw new InvalidOperationException("Test-Player-Fehler");
            }
        }
    }
}
