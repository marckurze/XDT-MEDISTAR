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
        var monitoringCard = CreateMonitoringCard(profile);

        return new ActiveInterfaceProfileStatusRow(
            Name: profile.Metadata.Name,
            AisName: GetAisProfileName(profile.AisProfileId, aisProfilesById),
            DeviceName: GetDeviceProfileName(profile.DeviceProfileId, deviceProfilesById),
            ExportProfileName: GetExportProfileName(profile.ExportProfileId, exportProfilesById),
            AisImportFolder: profile.FolderOptions.AisImportFolder,
            DeviceImportFolder: profile.FolderOptions.DeviceImportFolder,
            ExportFolder: profile.FolderOptions.ExportFolder,
            AttachmentImportFolder: monitoringCard.AttachmentImportFolder,
            AttachmentExportFolder: monitoringCard.AttachmentExportFolder,
            AttachmentConfigurationStatus: monitoringCard.AttachmentConfigurationStatus,
            LicenseStatus: licenseStatus,
            FolderStatus: folderStatus,
            ProcessingStatus: processingStatus,
            MonitoringCard: monitoringCard);
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

    private static InterfaceMonitoringCardDisplay CreateMonitoringCard(InterfaceProfileDefinition profile)
    {
        var folderOptions = profile.FolderOptions;
        var attachmentImportFolder = CreateAttachmentFolderDisplay(folderOptions, folderOptions.AttachmentImportFolder, "XDT-Anhang Importordner fehlt");
        var attachmentExportFolder = CreateAttachmentFolderDisplay(folderOptions, folderOptions.AttachmentExportFolder, "XDT-Anhang Exportordner fehlt");
        var attachmentStatus = CreateAttachmentConfigurationStatus(folderOptions);

        var expectedInputs = new List<ExpectedInputDisplayItem>
        {
            new("AIS-Datei", DisplayOrMissing(folderOptions.AisImportFolder, "AIS-Importordner fehlt"), string.IsNullOrWhiteSpace(folderOptions.AisImportFolder) ? "fehlt" : "konfiguriert"),
            new("Gerätedatei", DisplayOrMissing(folderOptions.DeviceImportFolder, "Geräte-Importordner fehlt"), string.IsNullOrWhiteSpace(folderOptions.DeviceImportFolder) ? "fehlt" : "konfiguriert"),
            new("XDT-Anhang", attachmentImportFolder, attachmentStatus)
        };

        return new InterfaceMonitoringCardDisplay(
            InterfaceProfileId: profile.Metadata.Id,
            InterfaceProfileName: profile.Metadata.Name,
            ExpectedInputs: expectedInputs,
            AttachmentImportFolder: attachmentImportFolder,
            AttachmentExportFolder: attachmentExportFolder,
            AttachmentConfigurationStatus: attachmentStatus);
    }

    private static string CreateAttachmentFolderDisplay(
        InterfaceFolderOptions folderOptions,
        string? folderPath,
        string missingMessage)
    {
        if (!HasAttachmentConfiguration(folderOptions))
        {
            return "kein Anhang konfiguriert";
        }

        return DisplayOrMissing(folderPath, missingMessage);
    }

    private static string CreateAttachmentConfigurationStatus(InterfaceFolderOptions folderOptions)
    {
        if (!HasAttachmentConfiguration(folderOptions))
        {
            return "kein Anhang konfiguriert";
        }

        var missingImportFolder = string.IsNullOrWhiteSpace(folderOptions.AttachmentImportFolder);
        var missingExportFolder = string.IsNullOrWhiteSpace(folderOptions.AttachmentExportFolder);
        if (missingImportFolder || missingExportFolder)
        {
            return folderOptions.IsAttachmentProcessingEnabled
                ? "XDT-Anhang aktiv, Ordner unvollständig"
                : "XDT-Anhang konfiguriert, Ordner unvollständig";
        }

        return folderOptions.IsAttachmentProcessingEnabled
            ? $"XDT-Anhang aktiv ({FormatRequirementMode(folderOptions.AttachmentRequirementMode)})"
            : "XDT-Anhang konfiguriert, Automatik aus";
    }

    private static string FormatRequirementMode(AttachmentRequirementMode mode)
    {
        return mode == AttachmentRequirementMode.Required
            ? "Pflicht"
            : "Optional";
    }

    private static bool HasAttachmentConfiguration(InterfaceFolderOptions folderOptions)
    {
        return folderOptions.IsAttachmentProcessingEnabled
            || !string.IsNullOrWhiteSpace(folderOptions.AttachmentImportFolder)
            || !string.IsNullOrWhiteSpace(folderOptions.AttachmentExportFolder);
    }

    private static string DisplayOrMissing(string? value, string missingMessage)
    {
        return string.IsNullOrWhiteSpace(value)
            ? missingMessage
            : value;
    }
}
