using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public enum UserDefinedProfileRenameKind
{
    AisProfile,
    DeviceProfile,
    ExportProfile,
    InterfaceProfile
}

public sealed record UserDefinedProfileRenameResult(
    bool Success,
    bool NoChange,
    string Message,
    string? ProfileId,
    string? OldName,
    string? NewName,
    IReadOnlyList<string> Issues)
{
    public static UserDefinedProfileRenameResult Blocked(
        string message,
        string? profileId = null,
        string? oldName = null,
        string? newName = null)
    {
        return new UserDefinedProfileRenameResult(
            Success: false,
            NoChange: false,
            Message: message,
            ProfileId: profileId,
            OldName: oldName,
            NewName: newName,
            Issues: new[] { message });
    }

    public static UserDefinedProfileRenameResult Ready(
        string profileId,
        string oldName,
        string newName)
    {
        return new UserDefinedProfileRenameResult(
            Success: true,
            NoChange: false,
            Message: "Profil kann umbenannt werden.",
            ProfileId: profileId,
            OldName: oldName,
            NewName: newName,
            Issues: Array.Empty<string>());
    }

    public static UserDefinedProfileRenameResult Unchanged(
        string profileId,
        string name)
    {
        return new UserDefinedProfileRenameResult(
            Success: true,
            NoChange: true,
            Message: "Der neue Name ist identisch mit dem aktuellen Namen.",
            ProfileId: profileId,
            OldName: name,
            NewName: name,
            Issues: Array.Empty<string>());
    }

    public UserDefinedProfileRenameResult Renamed()
    {
        return this with
        {
            Success = true,
            NoChange = false,
            Message = "Der Profilname wurde geändert.",
            Issues = Array.Empty<string>()
        };
    }
}

public sealed class UserDefinedProfileRenameService
{
    private readonly ProfileCatalogService _profileCatalogService;

    public UserDefinedProfileRenameService()
        : this(new ProfileCatalogService())
    {
    }

    public UserDefinedProfileRenameService(ProfileCatalogService profileCatalogService)
    {
        _profileCatalogService = profileCatalogService ?? throw new ArgumentNullException(nameof(profileCatalogService));
    }

    public UserDefinedProfileRenameResult Evaluate(
        ProfileCatalog catalog,
        UserDefinedProfileRenameKind kind,
        string profileId,
        string newName)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        if (string.IsNullOrWhiteSpace(profileId))
        {
            return UserDefinedProfileRenameResult.Blocked("Kein Profil ausgewählt.");
        }

        var profile = FindProfile(catalog, kind, profileId);
        if (profile is null)
        {
            return UserDefinedProfileRenameResult.Blocked($"Profil nicht gefunden: {profileId}.", profileId);
        }

        if (profile.Metadata.IsBuiltIn)
        {
            return UserDefinedProfileRenameResult.Blocked(
                "BuiltIn-Profile können nicht umbenannt werden.",
                profile.Metadata.Id,
                profile.VisibleName);
        }

        if (!profile.Metadata.IsUserDefined)
        {
            return UserDefinedProfileRenameResult.Blocked(
                "Nur UserDefined-Profile können umbenannt werden.",
                profile.Metadata.Id,
                profile.VisibleName);
        }

        var normalizedName = newName.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return UserDefinedProfileRenameResult.Blocked(
                "Bitte geben Sie einen neuen Namen ein.",
                profile.Metadata.Id,
                profile.VisibleName);
        }

        if (string.Equals(profile.VisibleName, normalizedName, StringComparison.Ordinal))
        {
            return UserDefinedProfileRenameResult.Unchanged(profile.Metadata.Id, profile.VisibleName);
        }

        if (HasNameConflict(catalog, kind, profile.Metadata.Id, normalizedName))
        {
            return UserDefinedProfileRenameResult.Blocked(
                "Es existiert bereits ein Profil mit diesem Namen.",
                profile.Metadata.Id,
                profile.VisibleName,
                normalizedName);
        }

        return UserDefinedProfileRenameResult.Ready(profile.Metadata.Id, profile.VisibleName, normalizedName);
    }

    public UserDefinedProfileRenameResult Rename(
        ProfileCatalog catalog,
        AppDataPaths paths,
        UserDefinedProfileRenameKind kind,
        string profileId,
        string newName)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var evaluation = Evaluate(catalog, kind, profileId, newName);
        if (!evaluation.Success || evaluation.NoChange || evaluation.NewName is null)
        {
            return evaluation;
        }

        switch (kind)
        {
            case UserDefinedProfileRenameKind.AisProfile:
                RenameAisProfile(catalog, paths, profileId, evaluation.NewName);
                break;
            case UserDefinedProfileRenameKind.DeviceProfile:
                RenameDeviceProfile(catalog, paths, profileId, evaluation.NewName);
                break;
            case UserDefinedProfileRenameKind.ExportProfile:
                RenameExportProfile(catalog, paths, profileId, evaluation.NewName);
                break;
            case UserDefinedProfileRenameKind.InterfaceProfile:
                RenameInterfaceProfile(catalog, paths, profileId, evaluation.NewName);
                break;
            default:
                throw new InvalidOperationException($"Unsupported rename profile kind: {kind}");
        }

        return evaluation.Renamed();
    }

    private void RenameAisProfile(
        ProfileCatalog catalog,
        AppDataPaths paths,
        string profileId,
        string newName)
    {
        var profile = catalog.AisProfiles.Single(profile =>
            string.Equals(profile.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase));
        var updatedProfile = profile with
        {
            Metadata = profile.Metadata with { Name = newName },
            Name = newName
        };

        _profileCatalogService.SaveAisProfile(paths, updatedProfile, overwriteExisting: true);
    }

    private void RenameDeviceProfile(
        ProfileCatalog catalog,
        AppDataPaths paths,
        string profileId,
        string newName)
    {
        var profile = catalog.DeviceProfiles.Single(profile =>
            string.Equals(profile.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase));
        var updatedProfile = profile with
        {
            Metadata = profile.Metadata with { Name = newName }
        };

        _profileCatalogService.SaveDeviceProfileDefinition(paths, updatedProfile, overwriteExisting: true);
    }

    private void RenameExportProfile(
        ProfileCatalog catalog,
        AppDataPaths paths,
        string profileId,
        string newName)
    {
        var profile = catalog.ExportProfiles.Single(profile =>
            string.Equals(profile.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase));
        var updatedProfile = profile with
        {
            Metadata = profile.Metadata with { Name = newName }
        };

        _profileCatalogService.SaveExportProfileDefinition(paths, updatedProfile, overwriteExisting: true);
    }

    private void RenameInterfaceProfile(
        ProfileCatalog catalog,
        AppDataPaths paths,
        string profileId,
        string newName)
    {
        var profile = catalog.InterfaceProfiles.Single(profile =>
            string.Equals(profile.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase));
        var updatedProfile = profile with
        {
            Metadata = profile.Metadata with { Name = newName }
        };

        _profileCatalogService.SaveInterfaceProfileDefinition(paths, updatedProfile, overwriteExisting: true);
    }

    private static RenameProfileItem? FindProfile(
        ProfileCatalog catalog,
        UserDefinedProfileRenameKind kind,
        string profileId)
    {
        return kind switch
        {
            UserDefinedProfileRenameKind.AisProfile => catalog.AisProfiles
                .Where(profile => string.Equals(profile.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase))
                .Select(profile => new RenameProfileItem(profile.Metadata, profile.Name))
                .FirstOrDefault(),
            UserDefinedProfileRenameKind.DeviceProfile => catalog.DeviceProfiles
                .Where(profile => string.Equals(profile.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase))
                .Select(profile => new RenameProfileItem(profile.Metadata, profile.Metadata.Name))
                .FirstOrDefault(),
            UserDefinedProfileRenameKind.ExportProfile => catalog.ExportProfiles
                .Where(profile => string.Equals(profile.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase))
                .Select(profile => new RenameProfileItem(profile.Metadata, profile.Metadata.Name))
                .FirstOrDefault(),
            UserDefinedProfileRenameKind.InterfaceProfile => catalog.InterfaceProfiles
                .Where(profile => string.Equals(profile.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase))
                .Select(profile => new RenameProfileItem(profile.Metadata, profile.Metadata.Name))
                .FirstOrDefault(),
            _ => null
        };
    }

    private static bool HasNameConflict(
        ProfileCatalog catalog,
        UserDefinedProfileRenameKind kind,
        string currentProfileId,
        string newName)
    {
        return GetProfileNames(catalog, kind)
            .Any(profile =>
                !string.Equals(profile.Id, currentProfileId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(profile.Name, newName, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<(string Id, string Name)> GetProfileNames(
        ProfileCatalog catalog,
        UserDefinedProfileRenameKind kind)
    {
        return kind switch
        {
            UserDefinedProfileRenameKind.AisProfile => catalog.AisProfiles.Select(profile => (profile.Metadata.Id, profile.Name)),
            UserDefinedProfileRenameKind.DeviceProfile => catalog.DeviceProfiles.Select(profile => (profile.Metadata.Id, profile.Metadata.Name)),
            UserDefinedProfileRenameKind.ExportProfile => catalog.ExportProfiles.Select(profile => (profile.Metadata.Id, profile.Metadata.Name)),
            UserDefinedProfileRenameKind.InterfaceProfile => catalog.InterfaceProfiles.Select(profile => (profile.Metadata.Id, profile.Metadata.Name)),
            _ => Array.Empty<(string Id, string Name)>()
        };
    }

    private sealed record RenameProfileItem(ProfileMetadata Metadata, string VisibleName);
}
