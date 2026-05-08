using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class PeriodicAutoImportScanServiceTests
{
    [Fact]
    public async Task StartAsync_ShouldThrowArgumentNullExceptionForNullProfiles()
    {
        var service = new PeriodicAutoImportScanService(new FakeScanner());

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.StartAsync(
            null!,
            TimeSpan.FromMilliseconds(10),
            TimeSpan.Zero,
            _ => { },
            CancellationToken.None));
    }

    [Fact]
    public async Task StartAsync_ShouldThrowArgumentExceptionForNonPositiveInterval()
    {
        var service = new PeriodicAutoImportScanService(new FakeScanner());

        await Assert.ThrowsAsync<ArgumentException>(() => service.StartAsync(
            new[] { CreateProfile() },
            TimeSpan.Zero,
            TimeSpan.Zero,
            _ => { },
            CancellationToken.None));
    }

    [Fact]
    public async Task StartAsync_ShouldThrowArgumentNullExceptionForNullCallback()
    {
        var service = new PeriodicAutoImportScanService(new FakeScanner());

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.StartAsync(
            new[] { CreateProfile() },
            TimeSpan.FromMilliseconds(10),
            TimeSpan.Zero,
            null!,
            CancellationToken.None));
    }

    [Fact]
    public async Task StartAsync_ShouldInvokeCallbackAtLeastOnce()
    {
        var scanner = new FakeScanner();
        var service = new PeriodicAutoImportScanService(scanner);
        using var cancellationTokenSource = new CancellationTokenSource();
        var callbackCount = 0;

        await service.StartAsync(
            new[] { CreateProfile() },
            TimeSpan.FromMilliseconds(10),
            TimeSpan.Zero,
            _ =>
            {
                callbackCount++;
                cancellationTokenSource.Cancel();
            },
            cancellationTokenSource.Token);

        Assert.True(callbackCount >= 1);
        Assert.Equal(1, scanner.CallCount);
    }

    [Fact]
    public async Task StartAsync_ShouldStopWhenCancellationTokenIsCancelled()
    {
        var scanner = new FakeScanner();
        var service = new PeriodicAutoImportScanService(scanner);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await service.StartAsync(
            new[] { CreateProfile() },
            TimeSpan.FromMilliseconds(10),
            TimeSpan.Zero,
            _ => throw new InvalidOperationException("Callback should not run."),
            cancellationTokenSource.Token);

        Assert.Equal(0, scanner.CallCount);
    }

    [Fact]
    public async Task StartAsync_ShouldConvertScannerExceptionToScanResultMessage()
    {
        var scanner = new FakeScanner { ExceptionToThrow = new InvalidOperationException("kaputt") };
        var service = new PeriodicAutoImportScanService(scanner);
        using var cancellationTokenSource = new CancellationTokenSource();
        AutoImportScanResult? receivedResult = null;

        await service.StartAsync(
            new[] { CreateProfile() },
            TimeSpan.FromMilliseconds(10),
            TimeSpan.Zero,
            result =>
            {
                receivedResult = result;
                cancellationTokenSource.Cancel();
            },
            cancellationTokenSource.Token);

        Assert.NotNull(receivedResult);
        Assert.Contains(receivedResult.Messages, message => message.Contains("kaputt", StringComparison.Ordinal));
    }

    [Fact]
    public void GetEffectiveInterval_ShouldUseDefaultFromFolderOptions()
    {
        var profile = CreateProfile();

        var interval = PeriodicAutoImportScanService.GetEffectiveInterval(profile);

        Assert.Equal(TimeSpan.FromSeconds(5), interval);
    }

    [Fact]
    public void GetEffectiveInterval_ShouldUseConfiguredProfileInterval()
    {
        var profile = CreateProfile() with
        {
            FolderOptions = CreateProfile().FolderOptions with { AutoImportScanIntervalSeconds = 7 }
        };

        var interval = PeriodicAutoImportScanService.GetEffectiveInterval(profile);

        Assert.Equal(TimeSpan.FromSeconds(7), interval);
    }

    [Fact]
    public void GetEffectiveInterval_ShouldClampTooSmallProfileInterval()
    {
        var profile = CreateProfile() with
        {
            FolderOptions = CreateProfile().FolderOptions with { AutoImportScanIntervalSeconds = -3 }
        };

        var interval = PeriodicAutoImportScanService.GetEffectiveInterval(profile);

        Assert.Equal(TimeSpan.FromSeconds(1), interval);
    }

    [Fact]
    public void GetEffectiveInterval_ShouldUseSmallestActiveProfileInterval()
    {
        var profiles = new[]
        {
            CreateProfile() with { FolderOptions = CreateProfile().FolderOptions with { AutoImportScanIntervalSeconds = 9 } },
            CreateProfile() with { FolderOptions = CreateProfile().FolderOptions with { AutoImportScanIntervalSeconds = 3 } }
        };

        var interval = PeriodicAutoImportScanService.GetEffectiveInterval(profiles);

        Assert.Equal(TimeSpan.FromSeconds(3), interval);
    }

    private static InterfaceProfileDefinition CreateProfile()
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            IsActive = true
        };
    }

    private sealed class FakeScanner : IAutoImportScanner
    {
        public int CallCount { get; private set; }

        public Exception? ExceptionToThrow { get; set; }

        public Task<AutoImportScanResult> ScanOnceAsync(
            InterfaceProfileDefinition profile,
            TimeSpan stabilityDuration,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(new AutoImportScanResult(
                InterfaceProfileId: profile.Metadata.Id,
                AisFilesDetected: 1,
                DeviceFilesDetected: 1,
                FilesQueued: 2,
                ReadyPairs: 1,
                Messages: Array.Empty<string>(),
                Queue: new PendingImportQueue()));
        }
    }
}
