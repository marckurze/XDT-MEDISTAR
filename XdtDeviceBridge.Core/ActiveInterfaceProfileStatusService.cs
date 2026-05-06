namespace XdtDeviceBridge.Core;

public sealed class ActiveInterfaceProfileStatusService
{
    public IReadOnlyList<ActiveInterfaceProfileStatusRow> BuildRows(
        IEnumerable<InterfaceProfileDefinition> interfaceProfiles,
        IEnumerable<AisProfile> aisProfiles,
        IEnumerable<DeviceProfileDefinition> deviceProfiles,
        IEnumerable<ExportProfileDefinition> exportProfiles,
        IEnumerable<LicensedDeviceState> licenseStates)
    {
        ArgumentNullException.ThrowIfNull(interfaceProfiles);
        ArgumentNullException.ThrowIfNull(aisProfiles);
        ArgumentNullException.ThrowIfNull(deviceProfiles);
        ArgumentNullException.ThrowIfNull(exportProfiles);
        ArgumentNullException.ThrowIfNull(licenseStates);

        var aisProfilesById = aisProfiles.ToDictionary(profile => profile.Metadata.Id, StringComparer.OrdinalIgnoreCase);
        var deviceProfilesById = deviceProfiles.ToDictionary(profile => profile.Metadata.Id, StringComparer.OrdinalIgnoreCase);
        var exportProfilesById = exportProfiles.ToDictionary(profile => profile.Metadata.Id, StringComparer.OrdinalIgnoreCase);
        var licenseStatesByInterfaceId = licenseStates.ToDictionary(state => state.InterfaceProfileId, StringComparer.OrdinalIgnoreCase);

        return interfaceProfiles
            .Where(profile => profile.IsActive)
            .Select(profile => BuildRow(
                profile,
                aisProfilesById,
                deviceProfilesById,
                exportProfilesById,
                licenseStatesByInterfaceId))
            .ToList();
    }

    private static ActiveInterfaceProfileStatusRow BuildRow(
        InterfaceProfileDefinition profile,
        IReadOnlyDictionary<string, AisProfile> aisProfilesById,
        IReadOnlyDictionary<string, DeviceProfileDefinition> deviceProfilesById,
        IReadOnlyDictionary<string, ExportProfileDefinition> exportProfilesById,
        IReadOnlyDictionary<string, LicensedDeviceState> licenseStatesByInterfaceId)
    {
        var licenseState = licenseStatesByInterfaceId.TryGetValue(profile.Metadata.Id, out var state)
            ? state
            : null;
        var folderStatus = CreateFolderStatus(profile.FolderOptions);
        var licenseStatus = CreateLicenseStatus(profile, licenseState);
        var processingStatus = CreateProcessingStatus(folderStatus, profile, licenseState);

        return new ActiveInterfaceProfileStatusRow(
            Name: profile.Metadata.Name,
            AisName: GetAisProfileName(profile.AisProfileId, aisProfilesById),
            DeviceName: GetDeviceProfileName(profile.DeviceProfileId, deviceProfilesById),
            ExportProfileName: GetExportProfileName(profile.ExportProfileId, exportProfilesById),
            AisImportFolder: profile.FolderOptions.AisImportFolder,
            DeviceImportFolder: profile.FolderOptions.DeviceImportFolder,
            ExportFolder: profile.FolderOptions.ExportFolder,
            LicenseStatus: licenseStatus,
            FolderStatus: folderStatus,
            ProcessingStatus: processingStatus);
    }

    private static string GetAisProfileName(
        string profileId,
        IReadOnlyDictionary<string, AisProfile> aisProfilesById)
    {
        return aisProfilesById.TryGetValue(profileId, out var profile)
            ? profile.Metadata.Name
            : profileId;
    }

    private static string GetDeviceProfileName(
        string profileId,
        IReadOnlyDictionary<string, DeviceProfileDefinition> deviceProfilesById)
    {
        return deviceProfilesById.TryGetValue(profileId, out var profile)
            ? profile.Metadata.Name
            : profileId;
    }

    private static string GetExportProfileName(
        string profileId,
        IReadOnlyDictionary<string, ExportProfileDefinition> exportProfilesById)
    {
        return exportProfilesById.TryGetValue(profileId, out var profile)
            ? profile.Metadata.Name
            : profileId;
    }

    private static string CreateFolderStatus(InterfaceFolderOptions folderOptions)
    {
        var missingFolders = new List<string>();
        if (string.IsNullOrWhiteSpace(folderOptions.AisImportFolder))
        {
            missingFolders.Add("AIS-Importordner fehlt");
        }

        if (string.IsNullOrWhiteSpace(folderOptions.DeviceImportFolder))
        {
            missingFolders.Add("Geräte-Importordner fehlt");
        }

        if (string.IsNullOrWhiteSpace(folderOptions.ExportFolder))
        {
            missingFolders.Add("Exportordner fehlt");
        }

        return missingFolders.Count == 0
            ? "Ordner konfiguriert"
            : string.Join("; ", missingFolders);
    }

    private static string CreateLicenseStatus(
        InterfaceProfileDefinition profile,
        LicensedDeviceState? licenseState)
    {
        if (!profile.IsLicenseRequired)
        {
            return "Nicht lizenzpflichtig";
        }

        if (licenseState?.IsCoveredByLicense == true)
        {
            return "Lizenz gedeckt";
        }

        if (licenseState?.IsInGracePeriod == true && licenseState.GracePeriodEndsAt is not null)
        {
            return $"In Karenzzeit bis {licenseState.GracePeriodEndsAt.Value.ToLocalTime():dd.MM.yyyy}";
        }

        return "Nicht gedeckt";
    }

    private static string CreateProcessingStatus(
        string folderStatus,
        InterfaceProfileDefinition profile,
        LicensedDeviceState? licenseState)
    {
        var hasFolderIssue = !string.Equals(folderStatus, "Ordner konfiguriert", StringComparison.Ordinal);
        var hasLicenseIssue = profile.IsLicenseRequired
            && licenseState?.IsCoveredByLicense != true
            && licenseState?.IsInGracePeriod != true;

        if (hasFolderIssue && hasLicenseIssue)
        {
            return "Prüfen";
        }

        if (hasFolderIssue)
        {
            return "Unvollständig";
        }

        if (hasLicenseIssue)
        {
            return "Lizenzhinweis";
        }

        return "Bereit für spätere Automatik";
    }
}
