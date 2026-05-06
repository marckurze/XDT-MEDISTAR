using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public interface IAutoImportScanner
{
    Task<AutoImportScanResult> ScanOnceAsync(
        InterfaceProfileDefinition profile,
        TimeSpan stabilityDuration,
        CancellationToken cancellationToken = default);
}
