using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileScanIntervalUpdateService
{
    public const int MinimumScanIntervalSeconds = PeriodicAutoImportScanService.MinimumScanIntervalSeconds;
    public const int MaximumScanIntervalSeconds = 300;

    private readonly InterfaceProfileConfigurationService _configurationService;

    public InterfaceProfileScanIntervalUpdateService()
        : this(new InterfaceProfileConfigurationService())
    {
    }

    public InterfaceProfileScanIntervalUpdateService(InterfaceProfileConfigurationService configurationService)
    {
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
    }

    public InterfaceProfileScanIntervalUpdateResult ChangeBy(
        InterfaceProfileDefinition profile,
        int deltaSeconds,
        DateTimeOffset timestamp,
        string? createdBy,
        Func<string>? idFactory = null)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var previousInterval = Clamp(profile.FolderOptions.AutoImportScanIntervalSeconds);
        var requestedInterval = previousInterval + deltaSeconds;
        var effectiveInterval = Clamp(requestedInterval);
        var reachedMinimum = requestedInterval < MinimumScanIntervalSeconds;
        var reachedMaximum = requestedInterval > MaximumScanIntervalSeconds;

        if (effectiveInterval == previousInterval)
        {
            var boundaryMessage = reachedMinimum
                ? $"Minimum {MinimumScanIntervalSeconds} Sekunde erreicht."
                : reachedMaximum
                    ? $"Maximum {MaximumScanIntervalSeconds} Sekunden erreicht."
                    : $"Scanintervall bleibt bei {previousInterval} Sekunden.";

            return new InterfaceProfileScanIntervalUpdateResult(
                Profile: profile,
                PreviousIntervalSeconds: previousInterval,
                RequestedIntervalSeconds: requestedInterval,
                EffectiveIntervalSeconds: effectiveInterval,
                Changed: false,
                ReachedMinimum: reachedMinimum,
                ReachedMaximum: reachedMaximum,
                CreatedUserDefinedCopy: false,
                Message: boundaryMessage,
                Issues: Array.Empty<InterfaceProfileConfigurationIssue>());
        }

        var folderOptions = profile.FolderOptions with
        {
            AutoImportScanIntervalSeconds = effectiveInterval
        };

        var result = _configurationService.CreateConfiguredProfile(
            profile,
            folderOptions,
            profile.IsActive,
            profile.IsLicenseRequired,
            profile.DeviceOutput,
            timestamp,
            createdBy,
            idFactory);

        var createdUserDefinedCopy = profile.Metadata.IsBuiltIn
            && result.Profile is not null
            && result.Profile.Metadata.IsUserDefined
            && !string.Equals(result.Profile.Metadata.Id, profile.Metadata.Id, StringComparison.Ordinal);

        var message = createdUserDefinedCopy
            ? $"Scanintervall auf {effectiveInterval} Sekunden geändert; BuiltIn-Profil bleibt unverändert."
            : $"Scanintervall auf {effectiveInterval} Sekunden geändert.";

        return new InterfaceProfileScanIntervalUpdateResult(
            Profile: result.Profile,
            PreviousIntervalSeconds: previousInterval,
            RequestedIntervalSeconds: requestedInterval,
            EffectiveIntervalSeconds: effectiveInterval,
            Changed: true,
            ReachedMinimum: false,
            ReachedMaximum: false,
            CreatedUserDefinedCopy: createdUserDefinedCopy,
            Message: message,
            Issues: result.Issues);
    }

    private static int Clamp(int intervalSeconds)
    {
        return Math.Clamp(intervalSeconds, MinimumScanIntervalSeconds, MaximumScanIntervalSeconds);
    }
}
