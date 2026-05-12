using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileActivationProfileCatalogStore : IInterfaceProfileActivationProfileStore
{
    private readonly ProfileCatalogService _profileCatalogService;
    private readonly AppDataPaths _paths;

    public InterfaceProfileActivationProfileCatalogStore(AppDataPaths paths)
        : this(new ProfileCatalogService(), paths)
    {
    }

    public InterfaceProfileActivationProfileCatalogStore(
        ProfileCatalogService profileCatalogService,
        AppDataPaths paths)
    {
        _profileCatalogService = profileCatalogService ?? throw new ArgumentNullException(nameof(profileCatalogService));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
    }

    public InterfaceProfileActivationProfileLoadResult LoadFreshUserDefinedProfile(string profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return CreateLoadResult(
                InterfaceProfileActivationProfileStoreStatus.NotAvailable,
                success: false,
                profile: null,
                requestedProfileId: profileId,
                message: "Schnittstellenprofil-ID fehlt.",
                loadSucceeded: false);
        }

        try
        {
            var catalog = _profileCatalogService.Load(_paths);
            var profile = catalog.InterfaceProfiles.FirstOrDefault(item =>
                string.Equals(item.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase));

            if (profile is null)
            {
                return CreateLoadResult(
                    InterfaceProfileActivationProfileStoreStatus.NotFound,
                    success: false,
                    profile: null,
                    requestedProfileId: profileId,
                    message: "Schnittstellenprofil wurde im Profilkatalog nicht gefunden.",
                    loadSucceeded: true);
            }

            if (profile.Metadata.IsBuiltIn)
            {
                return CreateLoadResult(
                    InterfaceProfileActivationProfileStoreStatus.BuiltInBlocked,
                    success: false,
                    profile,
                    requestedProfileId: profileId,
                    message: "BuiltIn-Schnittstellenprofile sind fuer Aktivierungs-Persistenz gesperrt.",
                    loadSucceeded: true);
            }

            if (!profile.Metadata.IsUserDefined)
            {
                return CreateLoadResult(
                    InterfaceProfileActivationProfileStoreStatus.NonUserDefinedBlocked,
                    success: false,
                    profile,
                    requestedProfileId: profileId,
                    message: "Schnittstellenprofil ist nicht UserDefined und darf nicht ueber den Aktivierungs-Store gespeichert werden.",
                    loadSucceeded: true);
            }

            return CreateLoadResult(
                InterfaceProfileActivationProfileStoreStatus.LoadedUserDefined,
                success: true,
                profile,
                requestedProfileId: profileId,
                message: "UserDefined-Schnittstellenprofil wurde frisch aus dem Profilkatalog geladen.",
                loadSucceeded: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            return CreateLoadResult(
                InterfaceProfileActivationProfileStoreStatus.Failed,
                success: false,
                profile: null,
                requestedProfileId: profileId,
                message: $"Schnittstellenprofil konnte nicht aus dem Profilkatalog geladen werden: {ex.Message}",
                loadSucceeded: false);
        }
    }

    public InterfaceProfileActivationProfileSaveResult SaveUserDefinedProfile(
        InterfaceProfileActivationProfileSaveRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var profile = request.Profile;
        var profileId = profile?.Metadata.Id ?? string.Empty;
        var preconditions = CreateSavePreconditions(request);

        if (profile is null)
        {
            return CreateSaveResult(
                InterfaceProfileActivationProfileStoreStatus.NotAvailable,
                profile,
                wouldSave: false,
                success: false,
                "Kein Schnittstellenprofil zum Speichern uebergeben.",
                preconditions);
        }

        if (profile.Metadata.IsBuiltIn)
        {
            return CreateSaveResult(
                InterfaceProfileActivationProfileStoreStatus.BuiltInBlocked,
                profile,
                wouldSave: false,
                success: false,
                "BuiltIn-Schnittstellenprofile duerfen nicht gespeichert oder ueberschrieben werden.",
                preconditions);
        }

        if (!profile.Metadata.IsUserDefined)
        {
            return CreateSaveResult(
                InterfaceProfileActivationProfileStoreStatus.NonUserDefinedBlocked,
                profile,
                wouldSave: false,
                success: false,
                "Sichere Speicherung ist nur fuer UserDefined-Schnittstellenprofile vorgesehen.",
                preconditions);
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            return CreateSaveResult(
                InterfaceProfileActivationProfileStoreStatus.NotAvailable,
                profile,
                wouldSave: false,
                success: false,
                "Schnittstellenprofil-ID fehlt.",
                preconditions);
        }

        if (!request.FinalReEvaluationCompleted)
        {
            return CreateSaveResult(
                InterfaceProfileActivationProfileStoreStatus.MissingCapability,
                profile,
                wouldSave: false,
                success: false,
                "Finale Re-Evaluation ist nicht nachgewiesen; DryRun blockiert produktives Speichern.",
                preconditions);
        }

        if (request.OperationMode != InterfaceProfileActivationExecutorOperationMode.ValidateOnly)
        {
            return CreateSaveResult(
                InterfaceProfileActivationProfileStoreStatus.SaveNotImplemented,
                profile,
                wouldSave: true,
                success: false,
                "Produktives Speichern ist in dieser Adapter-Stufe deaktiviert.",
                preconditions);
        }

        return CreateSaveResult(
            InterfaceProfileActivationProfileStoreStatus.SaveWouldBeAllowed,
            profile,
            wouldSave: true,
            success: true,
            "ValidateOnly: UserDefined-Speicherung waere nach den lokalen Preconditions grundsaetzlich zulaessig; es wurde nichts gespeichert.",
            preconditions);
    }

    private static InterfaceProfileActivationProfileLoadResult CreateLoadResult(
        InterfaceProfileActivationProfileStoreStatus status,
        bool success,
        InterfaceProfileDefinition? profile,
        string requestedProfileId,
        string message,
        bool loadSucceeded)
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
            Preconditions: CreateLoadPreconditions(requestedProfileId, profile, loadSucceeded));
    }

    private static InterfaceProfileActivationProfileSaveResult CreateSaveResult(
        InterfaceProfileActivationProfileStoreStatus status,
        InterfaceProfileDefinition? profile,
        bool wouldSave,
        bool success,
        string message,
        IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> preconditions)
    {
        return new InterfaceProfileActivationProfileSaveResult(
            Status: status,
            Success: success,
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

    private static IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> CreateLoadPreconditions(
        string profileId,
        InterfaceProfileDefinition? profile,
        bool loadSucceeded)
    {
        return new[]
        {
            Precondition(
                "profile.id.present",
                "Zielprofil-ID vorhanden",
                "Das Zielprofil muss eindeutig per ID ladbar sein.",
                !string.IsNullOrWhiteSpace(profileId)),
            Precondition(
                "store.catalog.load",
                "Profilkatalog gelesen",
                "Der bestehende ProfileCatalogService muss den Katalog lesen koennen.",
                loadSucceeded),
            Precondition(
                "profile.present",
                "Schnittstellenprofil vorhanden",
                "Das Zielprofil muss im Profilkatalog gefunden werden.",
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
                profile?.Metadata.IsBuiltIn == false)
        };
    }

    private static IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> CreateSavePreconditions(
        InterfaceProfileActivationProfileSaveRequest request)
    {
        var profile = request.Profile;
        return new[]
        {
            Precondition(
                "profile.id.present",
                "Zielprofil-ID vorhanden",
                "Das Zielprofil muss eindeutig identifizierbar sein.",
                !string.IsNullOrWhiteSpace(profile?.Metadata.Id)),
            Precondition(
                "profile.present",
                "Schnittstellenprofil vorhanden",
                "Ein Schnittstellenprofil muss zum Speichern uebergeben werden.",
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
                "executor.finalReEvaluation.completed",
                "Finale Re-Evaluation nachgewiesen",
                "Produktives Speichern darf erst nach frischer finaler Evaluation/Guard/Plan erfolgen.",
                request.FinalReEvaluationCompleted),
            Precondition(
                "store.validateOnly",
                "ValidateOnly/DryRun",
                "Diese Adapter-Stufe validiert nur und ruft keine produktive Speichermethode auf.",
                request.OperationMode == InterfaceProfileActivationExecutorOperationMode.ValidateOnly),
            Precondition(
                "store.productiveSave.disabled",
                "Produktives Speichern deaktiviert",
                "SaveInterfaceProfileDefinition wird in dieser Adapter-Stufe nicht aufgerufen.",
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
