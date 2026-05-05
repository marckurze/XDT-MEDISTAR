namespace XdtDeviceBridge.Core;

public sealed class LicensedDeviceStateEvaluator
{
    public IReadOnlyList<LicensedDeviceState> Evaluate(
        IEnumerable<InterfaceProfileDefinition> interfaceProfiles,
        LicenseInfo? license,
        DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(interfaceProfiles);
        _ = nowUtc;

        var licensedDeviceCount = Math.Max(0, license?.LicensedDeviceCount ?? 0);
        var coveredLicenseRequiredProfiles = 0;
        var states = new List<LicensedDeviceState>();

        foreach (var profile in interfaceProfiles)
        {
            var isLicenseRelevant = profile.IsActive && profile.IsLicenseRequired;
            var isCovered = false;
            string statusMessage;

            if (!profile.IsActive)
            {
                statusMessage = "Nicht aktiv - zaehlt nicht als lizenzpflichtige Anbindung.";
            }
            else if (!profile.IsLicenseRequired)
            {
                statusMessage = "Nicht lizenzpflichtig - zaehlt nicht als lizenzpflichtige Anbindung.";
            }
            else if (license is null)
            {
                statusMessage = "Nicht lizenziert: keine Lizenz vorhanden.";
            }
            else if (coveredLicenseRequiredProfiles < licensedDeviceCount)
            {
                coveredLicenseRequiredProfiles++;
                isCovered = true;
                statusMessage = "Durch Lizenz gedeckt.";
            }
            else
            {
                coveredLicenseRequiredProfiles++;
                statusMessage = "Nicht durch Lizenz gedeckt: lizenzierte Anzahl ueberschritten.";
            }

            states.Add(new LicensedDeviceState(
                InterfaceProfileId: profile.Metadata.Id,
                DisplayName: profile.Metadata.Name,
                IsActive: profile.IsActive,
                IsLicenseRequired: profile.IsLicenseRequired,
                IsCoveredByLicense: isLicenseRelevant && isCovered,
                IsInGracePeriod: false,
                GracePeriodStartedAt: null,
                GracePeriodEndsAt: null,
                StatusMessage: statusMessage));
        }

        return states;
    }
}
