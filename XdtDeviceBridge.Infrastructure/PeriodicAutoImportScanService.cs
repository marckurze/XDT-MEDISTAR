using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class PeriodicAutoImportScanService
{
    private readonly IAutoImportScanner _scanner;

    public PeriodicAutoImportScanService()
        : this(new AutoImportScannerService())
    {
    }

    public PeriodicAutoImportScanService(IAutoImportScanner scanner)
    {
        _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
    }

    public async Task StartAsync(
        IReadOnlyList<InterfaceProfileDefinition> activeProfiles,
        TimeSpan interval,
        TimeSpan stabilityDuration,
        Action<AutoImportScanResult> onScanResult,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(activeProfiles);
        ArgumentNullException.ThrowIfNull(onScanResult);

        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentException("Interval must be greater than zero.", nameof(interval));
        }

        if (stabilityDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(stabilityDuration), "Stability duration must not be negative.");
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var profile in activeProfiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                AutoImportScanResult result;
                try
                {
                    result = await _scanner
                        .ScanOnceAsync(profile, stabilityDuration, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    result = new AutoImportScanResult(
                        InterfaceProfileId: profile.Metadata.Id,
                        AisFilesDetected: 0,
                        DeviceFilesDetected: 0,
                        FilesQueued: 0,
                        ReadyPairs: 0,
                        Messages: new[] { $"Scan-Fehler: {ex.Message}" },
                        Queue: new PendingImportQueue());
                }

                onScanResult(result);
            }

            try
            {
                await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}
