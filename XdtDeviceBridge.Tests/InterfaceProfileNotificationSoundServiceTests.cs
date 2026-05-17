using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileNotificationSoundServiceTests
{
    private static readonly DateTime BaseTime = new(2026, 5, 17, 11, 0, 0, DateTimeKind.Local);

    [Fact]
    public void TryPlay_ShouldPlayForDeviceDetectedEvent()
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService();
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            var result = service.TryPlay(Event("interface-ar360", "scan-device-detected", BaseTime), soundFilePath, player);

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
    public void TryPlayForDeviceFileDetected_ShouldPlayForNewDeviceFile()
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService();
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            var result = service.TryPlayForDeviceFileDetected(
                "interface-ar360",
                @"C:\Import\Device\ar360.xml",
                BaseTime.ToUniversalTime(),
                BaseTime,
                soundFilePath,
                player);

            Assert.True(result.ShouldPlay);
            Assert.True(result.WasPlayed);
            Assert.False(result.IsSuppressedByCooldown);
            Assert.Equal(1, player.PlayCount);
        }
        finally
        {
            File.Delete(soundFilePath);
        }
    }

    [Theory]
    [InlineData("interface-ar360", "scan-ais-detected", "AIS-Datei erkannt.")]
    [InlineData("interface-ar360", "scan-ready-pair", "AIS-/Geräte-Paar vollständig.")]
    [InlineData("interface-ark1s", "pair:patient-device:message:XDT-Anhang vorbereitet", "XDT-Anhang vorbereitet: report.pdf")]
    [InlineData("interface-ar360", "pair:patient-device:status", "MEDISTAR + NIDEK AR360: automatisch verarbeitet. Exportdatei: out.gdt")]
    [InlineData("interface-ar360", "pair:patient-device:status", "Schnittstelle: automatische Verarbeitung fehlgeschlagen.")]
    [InlineData("interface-ar360", "package-state", "Warte auf Gerätedatei.")]
    [InlineData("monitoring", "manual-scan-started", "Einmaliger Scan gestartet.")]
    [InlineData("interface-ar360", "monitoring-card-detached", "Gerätekarte abgedockt.")]
    public void TryPlay_ShouldIgnoreNonDeviceEvents(string scopeId, string eventKey, string message)
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
    public void TryPlayForDeviceFileDetected_ShouldSuppressOnlyInsideCooldownAndAllowLaterDeviceFiles()
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService();
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            var first = service.TryPlayForDeviceFileDetected(
                "interface-ar360",
                @"C:\Import\Device\first.xml",
                BaseTime.ToUniversalTime(),
                BaseTime,
                soundFilePath,
                player);
            var second = service.TryPlayForDeviceFileDetected(
                "interface-ar360",
                @"C:\Import\Device\second.xml",
                BaseTime.AddSeconds(1).ToUniversalTime(),
                BaseTime.AddSeconds(1),
                soundFilePath,
                player);
            var third = service.TryPlayForDeviceFileDetected(
                "interface-ar360",
                @"C:\Import\Device\third.xml",
                BaseTime.AddSeconds(3).ToUniversalTime(),
                BaseTime.AddSeconds(3),
                soundFilePath,
                player);

            Assert.True(first.WasPlayed);
            Assert.True(second.ShouldPlay);
            Assert.True(second.IsSuppressedByCooldown);
            Assert.False(second.WasPlayed);
            Assert.True(third.WasPlayed);
            Assert.False(third.IsSuppressedByCooldown);
            Assert.Equal(2, player.PlayCount);
        }
        finally
        {
            File.Delete(soundFilePath);
        }
    }

    [Fact]
    public void TryPlayForDeviceFileDetected_ShouldNotReplaySameDeviceFileAfterCooldown()
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService();
        var soundFilePath = CreateExistingSoundFile();
        var detectedAt = BaseTime.ToUniversalTime();
        try
        {
            _ = service.TryPlayForDeviceFileDetected(
                "interface-ar360",
                @"C:\Import\Device\same.xml",
                detectedAt,
                BaseTime,
                soundFilePath,
                player);
            var repeated = service.TryPlayForDeviceFileDetected(
                "interface-ar360",
                @"C:\Import\Device\same.xml",
                detectedAt,
                BaseTime.AddSeconds(5),
                soundFilePath,
                player);

            Assert.False(repeated.ShouldPlay);
            Assert.False(repeated.WasPlayed);
            Assert.Equal(1, player.PlayCount);
        }
        finally
        {
            File.Delete(soundFilePath);
        }
    }

    [Fact]
    public void TryPlayForDeviceFileDetected_ShouldKeepCooldownPerProfile()
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService();
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            _ = service.TryPlayForDeviceFileDetected(
                "interface-ar360",
                @"C:\Import\Device\ar360.xml",
                BaseTime.ToUniversalTime(),
                BaseTime,
                soundFilePath,
                player);
            var ark1s = service.TryPlayForDeviceFileDetected(
                "interface-ark1s",
                @"C:\Import\Device\ark1s.xml",
                BaseTime.AddSeconds(1).ToUniversalTime(),
                BaseTime.AddSeconds(1),
                soundFilePath,
                player);

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
    public void TryPlayForDeviceFileDetected_ShouldNotBeBlockedByDetachOrRedockState()
    {
        var player = new FakeSoundPlayer();
        var soundService = new InterfaceProfileNotificationSoundService();
        var redockService = new InterfaceProfileAutoRedockService();
        var floatingState = new InterfaceProfileFloatingWindowState("interface-ar360", IsDetached: true);
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            redockService.MarkAutoDetached("interface-ar360", floatingState, BaseTime);
            _ = redockService.RecordMonitoringEvent(
                Event("interface-ar360", "pair:patient-device:status", BaseTime, "MEDISTAR + NIDEK AR360: automatisch verarbeitet. Exportdatei: out.gdt"),
                floatingState);
            Assert.True(redockService.EvaluateDue("interface-ar360", floatingState, BaseTime.AddSeconds(5)).ShouldRedockNow);

            var nextRun = soundService.TryPlayForDeviceFileDetected(
                "interface-ar360",
                @"C:\Import\Device\next.xml",
                BaseTime.AddSeconds(6).ToUniversalTime(),
                BaseTime.AddSeconds(6),
                soundFilePath,
                player);

            Assert.True(nextRun.WasPlayed);
            Assert.Equal(1, player.PlayCount);
        }
        finally
        {
            File.Delete(soundFilePath);
        }
    }

    [Fact]
    public void TryPlayForDeviceFileDetected_ShouldNotThrowWhenSoundFileIsMissing()
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService();
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.wav");

        var result = service.TryPlayForDeviceFileDetected(
            "interface-ar360",
            @"C:\Import\Device\ar360.xml",
            BaseTime.ToUniversalTime(),
            BaseTime,
            missingPath,
            player);

        Assert.True(result.ShouldPlay);
        Assert.False(result.WasPlayed);
        Assert.False(result.IsSuppressedByCooldown);
        Assert.Equal("Signalton-Datei fehlt.", result.Message);
        Assert.Equal(0, player.PlayCount);
    }

    [Fact]
    public void TryPlayForDeviceFileDetected_ShouldNotThrowWhenPlayerFails()
    {
        var player = new FakeSoundPlayer(throwOnPlay: true);
        var service = new InterfaceProfileNotificationSoundService();
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            var result = service.TryPlayForDeviceFileDetected(
                "interface-ar360",
                @"C:\Import\Device\ar360.xml",
                BaseTime.ToUniversalTime(),
                BaseTime,
                soundFilePath,
                player);

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
    public void TryPlayForDeviceFileDetected_ShouldNotPlayWhenDisabled()
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService(isEnabled: false);
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            var result = service.TryPlayForDeviceFileDetected(
                "interface-ar360",
                @"C:\Import\Device\ar360.xml",
                BaseTime.ToUniversalTime(),
                BaseTime,
                soundFilePath,
                player);

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
    public void FakePlayer_ShouldAllowMultipleAllowedPlays()
    {
        var player = new FakeSoundPlayer();
        var service = new InterfaceProfileNotificationSoundService();
        var soundFilePath = CreateExistingSoundFile();
        try
        {
            _ = service.TryPlayForDeviceFileDetected(
                "interface-ar360",
                @"C:\Import\Device\first.xml",
                BaseTime.ToUniversalTime(),
                BaseTime,
                soundFilePath,
                player);
            _ = service.TryPlayForDeviceFileDetected(
                "interface-ar360",
                @"C:\Import\Device\second.xml",
                BaseTime.AddSeconds(3).ToUniversalTime(),
                BaseTime.AddSeconds(3),
                soundFilePath,
                player);

            Assert.Equal(2, player.PlayCount);
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
