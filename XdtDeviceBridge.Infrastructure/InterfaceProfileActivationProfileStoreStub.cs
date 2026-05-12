using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileActivationProfileStoreStub : IInterfaceProfileActivationProfileStore
{
    private readonly IReadOnlyList<InterfaceProfileDefinition> _profiles;

    public InterfaceProfileActivationProfileStoreStub()
        : this(Array.Empty<InterfaceProfileDefinition>())
    {
    }

    public InterfaceProfileActivationProfileStoreStub(IEnumerable<InterfaceProfileDefinition> profiles)
    {
        ArgumentNullException.ThrowIfNull(profiles);

        _profiles = profiles.ToList();
    }

    public InterfaceProfileActivationProfileLoadResult LoadFreshUserDefinedProfile(string profileId)
    {
        var preconditions = CreateProfilePreconditions(profileId, profile: null);
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return CreateLoadResult(
                InterfaceProfileActivationProfileStoreStatus.NotAvailable,
                success: false,
                profile: null,
                profileId,
                "Schnittstellenprofil-ID fehlt.",
                preconditions);
        }

        var profile = _profiles.FirstOrDefault(item =>
            string.Equals(item.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase));

        preconditions = CreateProfilePreconditions(profileId, profile);
        if (profile is null)
        {
            return CreateLoadResult(
                InterfaceProfileActivationProfileStoreStatus.NotFound,
                success: false,
                profile: null,
                profileId,
                "Schnittstellenprofil wurde im Store-Stub nicht gefunden.",
                preconditions);
        }

        if (profile.Metadata.IsBuiltIn)
        {
            return CreateLoadResult(
                InterfaceProfileActivationProfileStoreStatus.BuiltInBlocked,
                success: false,
                profile,
                profileId,
                "BuiltIn-Schnittstellenprofile duerfen nicht geladen werden, um sie produktiv zu veraendern.",
                preconditions);
        }

        if (!profile.Metadata.IsUserDefined)
        {
            return CreateLoadResult(
                InterfaceProfileActivationProfileStoreStatus.UserDefinedRequired,
                success: false,
                profile,
                profileId,
                "Aktivierungsspeicherung ist nur fuer UserDefined-Schnittstellenprofile vorgesehen.",
                preconditions);
        }

        return CreateLoadResult(
            InterfaceProfileActivationProfileStoreStatus.LoadedUserDefined,
            success: true,
            profile,
            profileId,
            "UserDefined-Schnittstellenprofil wurde im nicht-produktiven Store-Stub gefunden.",
            preconditions);
    }

    public InterfaceProfileActivationProfileSaveResult SaveUserDefinedProfile(
        InterfaceProfileActivationProfileSaveRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var profile = request.Profile;
        var profileId = profile?.Metadata.Id ?? string.Empty;
        var preconditions = CreateProfilePreconditions(profileId, profile);

        if (profile is null)
        {
            return CreateSaveResult(
                InterfaceProfileActivationProfileStoreStatus.NotAvailable,
                profile,
                wouldSave: false,
                "Kein Schnittstellenprofil zum Speichern uebergeben.",
                preconditions);
        }

        if (profile.Metadata.IsBuiltIn)
        {
            return CreateSaveResult(
                InterfaceProfileActivationProfileStoreStatus.BuiltInBlocked,
                profile,
                wouldSave: false,
                "BuiltIn-Schnittstellenprofile duerfen nicht gespeichert oder ueberschrieben werden.",
                preconditions);
        }

        if (!profile.Metadata.IsUserDefined)
        {
            return CreateSaveResult(
                InterfaceProfileActivationProfileStoreStatus.UserDefinedRequired,
                profile,
                wouldSave: false,
                "Sichere Speicherung ist nur fuer UserDefined-Schnittstellenprofile vorgesehen.",
                preconditions);
        }

        return CreateSaveResult(
            InterfaceProfileActivationProfileStoreStatus.SaveNotImplemented,
            profile,
            wouldSave: true,
            "Der Store-Stub hat die UserDefined-Speicherung nur modelliert. Es wurde nichts gespeichert.",
            preconditions);
    }

    private static InterfaceProfileActivationProfileLoadResult CreateLoadResult(
        InterfaceProfileActivationProfileStoreStatus status,
        bool success,
        InterfaceProfileDefinition? profile,
        string requestedProfileId,
        string message,
        IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> preconditions)
    {
        return new InterfaceProfileActivationProfileLoadResult(
            Status: status,
            Success: success,
            Profile: profile,
            ProfileId: profile?.Metadata.Id ?? requestedProfileId,
            ProfileName: profile?.Metadata.Name ?? string.Empty,
            Found: profile is not null,
            IsUserDefined: profile?.Metadata.IsUserDefined == true,
            IsBuiltIn: profile?.Metadata.IsBuiltIn == true,
            Message: message,
            Preconditions: preconditions);
    }

    private static InterfaceProfileActivationProfileSaveResult CreateSaveResult(
        InterfaceProfileActivationProfileStoreStatus status,
        InterfaceProfileDefinition? profile,
        bool wouldSave,
        string message,
        IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> preconditions)
    {
        return new InterfaceProfileActivationProfileSaveResult(
            Status: status,
            Success: false,
            ProfileId: profile?.Metadata.Id ?? string.Empty,
            ProfileName: profile?.Metadata.Name ?? string.Empty,
            WouldSave: wouldSave,
            WasSaved: false,
            ProfileChanged: false,
            IsUserDefined: profile?.Metadata.IsUserDefined == true,
            IsBuiltIn: profile?.Metadata.IsBuiltIn == true,
            Message: message,
            Preconditions: preconditions);
    }

    private static IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> CreateProfilePreconditions(
        string profileId,
        InterfaceProfileDefinition? profile)
    {
        return new[]
        {
            Precondition(
                "profile.id.present",
                "Zielprofil-ID vorhanden",
                "Das Zielprofil muss eindeutig per ID ladbar sein.",
                !string.IsNullOrWhiteSpace(profileId)),
            Precondition(
                "profile.present",
                "Schnittstellenprofil vorhanden",
                "Das Zielprofil muss im Store gefunden werden.",
                profile is not null),
            Precondition(
                "profile.userDefined",
                "Profil ist UserDefined",
                "Nur UserDefined-Schnittstellenprofile duerfen spaeter gespeichert werden.",
                profile?.Metadata.IsUserDefined == true),
            Precondition(
                "profile.notBuiltIn",
                "Profil ist nicht BuiltIn",
                "BuiltIn-Schnittstellenprofile bleiben strikt geschuetzt.",
                profile?.Metadata.IsBuiltIn == false),
            Precondition(
                "store.nonProductiveStub",
                "Nicht-produktiver Store-Stub",
                "Diese Store-Stufe fuehrt kein produktives Speichern aus.",
                isSatisfied: false)
        };
    }

    private static InterfaceProfileActivationExecutorPrecondition Precondition(
        string code,
        string title,
        string description,
        bool isSatisfied)
    {
        return new InterfaceProfileActivationExecutorPrecondition(
            code,
            title,
            description,
            IsRequired: true,
            IsSatisfied: isSatisfied,
            Severity: isSatisfied
                ? InterfaceProfileActivationSeverity.Info
                : InterfaceProfileActivationSeverity.Blocker);
    }
}
