using System.Globalization;
using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record UserDefinedAisProfileCreationRequest(
    string ProfileName,
    string SystemName,
    string DefaultEncoding);

public sealed record UserDefinedDeviceProfileCreationRequest(
    string ProfileName,
    string Manufacturer,
    string Model,
    string DeviceType,
    string ParserMode,
    bool IsBidirectional = false,
    string DeviceImagePath = "");

public sealed record UserDefinedExportProfileCreationRequest(
    string ProfileName,
    string TargetAisProfileId,
    string SourceDeviceProfileId,
    string OutputEncoding,
    IReadOnlyList<ExportRuleDefinition> Rules);

public sealed record UserDefinedProfileCreationResult<TProfile>(
    TProfile? Profile,
    IReadOnlyList<string> Issues)
{
    public bool Success => Profile is not null && Issues.Count == 0;
}

public sealed class UserDefinedProfileCreationService
{
    private const string DefaultEncoding = "Windows-1252";
    private const string GenericSystemName = "Generisch";
    private const string MedistarSystemName = "MEDISTAR";

    public UserDefinedProfileCreationResult<AisProfile> CreateAisProfile(
        ProfileCatalog catalog,
        UserDefinedAisProfileCreationRequest request,
        DateTimeOffset timestamp,
        string? createdBy,
        Func<string>? idFactory = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(request);

        var issues = new List<string>();
        var profileName = NormalizeRequiredName(request.ProfileName, issues);
        var systemName = NormalizeOptionalText(request.SystemName, GenericSystemName);
        var defaultEncoding = NormalizeOptionalText(request.DefaultEncoding, DefaultEncoding);
        var metadata = CreateMetadata(
            catalog.AisProfiles.Select(profile => profile.Metadata),
            ProfileKind.AisProfile,
            "ais",
            profileName,
            Description: $"Benutzerdefiniertes AIS-Profil {profileName}.",
            Vendor: systemName.Equals(MedistarSystemName, StringComparison.OrdinalIgnoreCase)
                ? "MEDISTAR Praxiscomputer GmbH"
                : systemName,
            Product: systemName,
            timestamp,
            createdBy,
            issues,
            idFactory);

        if (metadata is null)
        {
            return new UserDefinedProfileCreationResult<AisProfile>(null, issues);
        }

        var profile = systemName.Equals(MedistarSystemName, StringComparison.OrdinalIgnoreCase)
            ? CreateMedistarBasedAisProfile(metadata, profileName, defaultEncoding)
            : new AisProfile(
                Metadata: metadata,
                Name: profileName,
                Vendor: systemName,
                DefaultEncoding: defaultEncoding,
                RequiredStaticFields: new Dictionary<string, string>(),
                RequiredPatientFieldCodes: Array.Empty<string>(),
                SupportedOutputFieldCodes: Array.Empty<string>(),
                SupportsResultTextField6228: false,
                SupportsCategoryValuePairs: false,
                RequiresExaminationType8402: false);

        issues.AddRange(AisProfileValidator.Validate(profile));

        return issues.Count == 0
            ? new UserDefinedProfileCreationResult<AisProfile>(profile, Array.Empty<string>())
            : new UserDefinedProfileCreationResult<AisProfile>(null, issues);
    }

    public UserDefinedProfileCreationResult<DeviceProfileDefinition> CreateDeviceProfile(
        ProfileCatalog catalog,
        UserDefinedDeviceProfileCreationRequest request,
        DateTimeOffset timestamp,
        string? createdBy,
        Func<string>? idFactory = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(request);

        var issues = new List<string>();
        var profileName = NormalizeRequiredName(request.ProfileName, issues);
        var manufacturer = NormalizeRequiredField(request.Manufacturer, "Bitte geben Sie einen Hersteller ein.", issues);
        var model = NormalizeRequiredField(request.Model, "Bitte geben Sie ein Modell ein.", issues);
        var parserMode = NormalizeRequiredField(request.ParserMode, "Bitte wählen Sie eine Parser-/Formatbasis aus.", issues);
        var deviceType = NormalizeOptionalText(request.DeviceType, GenericSystemName);
        var deviceImagePath = NormalizeOptionalText(request.DeviceImagePath, string.Empty);
        var metadata = CreateMetadata(
            catalog.DeviceProfiles.Select(profile => profile.Metadata),
            ProfileKind.DeviceProfile,
            "device",
            profileName,
            Description: $"Benutzerdefiniertes Geräteprofil {profileName}.",
            Vendor: manufacturer,
            Product: model,
            timestamp,
            createdBy,
            issues,
            idFactory);

        if (metadata is null)
        {
            return new UserDefinedProfileCreationResult<DeviceProfileDefinition>(null, issues);
        }

        var profile = new DeviceProfileDefinition(
            Metadata: metadata,
            Manufacturer: manufacturer,
            Model: model,
            DeviceType: deviceType,
            ParserMode: parserMode,
            Measurements: Array.Empty<DeviceMeasurementDefinition>(),
            SupportedExaminationTypes: Array.Empty<string>(),
            CanContainMultipleExaminationTypes: false,
            IsBidirectional: request.IsBidirectional,
            DeviceImagePath: deviceImagePath);

        issues.AddRange(DeviceProfileDefinitionValidator.Validate(profile));

        return issues.Count == 0
            ? new UserDefinedProfileCreationResult<DeviceProfileDefinition>(profile, Array.Empty<string>())
            : new UserDefinedProfileCreationResult<DeviceProfileDefinition>(null, issues);
    }

    public UserDefinedProfileCreationResult<ExportProfileDefinition> CreateExportProfile(
        ProfileCatalog catalog,
        UserDefinedExportProfileCreationRequest request,
        DateTimeOffset timestamp,
        string? createdBy,
        Func<string>? idFactory = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(request);

        var issues = new List<string>();
        var profileName = NormalizeRequiredName(request.ProfileName, issues);
        var targetAisProfileId = NormalizeRequiredField(request.TargetAisProfileId, "Bitte wählen Sie ein AIS-Profil aus.", issues);
        var sourceDeviceProfileId = NormalizeRequiredField(request.SourceDeviceProfileId, "Bitte wählen Sie ein Geräteprofil aus.", issues);
        var outputEncoding = NormalizeRequiredField(request.OutputEncoding, "Bitte geben Sie eine Ausgabe-Codierung an.", issues);
        var metadata = CreateMetadata(
            catalog.ExportProfiles.Select(profile => profile.Metadata),
            ProfileKind.ExportProfile,
            "export",
            profileName,
            Description: $"Benutzerdefiniertes Exportprofil {profileName}.",
            Vendor: null,
            Product: null,
            timestamp,
            createdBy,
            issues,
            idFactory);

        if (!catalog.AisProfiles.Any(profile => string.Equals(profile.Metadata.Id, targetAisProfileId, StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add("Das ausgewählte AIS-Profil wurde nicht gefunden.");
        }

        if (!catalog.DeviceProfiles.Any(profile => string.Equals(profile.Metadata.Id, sourceDeviceProfileId, StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add("Das ausgewählte Geräteprofil wurde nicht gefunden.");
        }

        var exportRules = request.Rules;
        if (exportRules is null)
        {
            issues.Add("Exportregeln dürfen nicht fehlen.");
            exportRules = Array.Empty<ExportRuleDefinition>();
        }

        if (metadata is null || issues.Count > 0)
        {
            return new UserDefinedProfileCreationResult<ExportProfileDefinition>(null, issues);
        }

        var profile = new ExportProfileDefinition(
            Metadata: metadata,
            TargetAisProfileId: targetAisProfileId,
            SourceDeviceProfileId: sourceDeviceProfileId,
            OutputEncoding: outputEncoding,
            Rules: exportRules.ToList());

        issues.AddRange(ExportProfileDefinitionValidator.Validate(profile));

        return issues.Count == 0
            ? new UserDefinedProfileCreationResult<ExportProfileDefinition>(profile, Array.Empty<string>())
            : new UserDefinedProfileCreationResult<ExportProfileDefinition>(null, issues);
    }

    public static bool HasProfileNameOrIdConflict(
        IEnumerable<ProfileMetadata> existingMetadata,
        string candidate)
    {
        ArgumentNullException.ThrowIfNull(existingMetadata);

        var normalizedCandidate = candidate.Trim();
        if (string.IsNullOrWhiteSpace(normalizedCandidate))
        {
            return false;
        }

        return existingMetadata.Any(metadata =>
            string.Equals(metadata.Name, normalizedCandidate, StringComparison.OrdinalIgnoreCase)
            || string.Equals(metadata.Id, normalizedCandidate, StringComparison.OrdinalIgnoreCase));
    }

    public static string CreateAvailableProfileName(
        IEnumerable<string> existingNames,
        string baseName)
    {
        ArgumentNullException.ThrowIfNull(existingNames);

        var normalizedBaseName = string.IsNullOrWhiteSpace(baseName)
            ? "Neues Profil"
            : baseName.Trim();
        var usedNames = new HashSet<string>(existingNames.Where(name => !string.IsNullOrWhiteSpace(name)), StringComparer.OrdinalIgnoreCase);

        if (!usedNames.Contains(normalizedBaseName))
        {
            return normalizedBaseName;
        }

        for (var index = 2; index < 10_000; index++)
        {
            var candidate = $"{normalizedBaseName} {index}";
            if (!usedNames.Contains(candidate))
            {
                return candidate;
            }
        }

        return $"{normalizedBaseName} {Guid.NewGuid():N}";
    }

    public static string CreateUniqueProfileId(
        string prefix,
        string profileName,
        IEnumerable<string> existingProfileIds)
    {
        ArgumentNullException.ThrowIfNull(existingProfileIds);

        var safePrefix = CreateSlug(prefix);
        if (string.IsNullOrWhiteSpace(safePrefix))
        {
            safePrefix = "profile";
        }

        var slug = CreateSlug(profileName);
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = "userdefined";
        }

        var candidate = $"{safePrefix}-{slug}";
        var usedIds = new HashSet<string>(existingProfileIds.Where(id => !string.IsNullOrWhiteSpace(id)), StringComparer.OrdinalIgnoreCase);
        if (!usedIds.Contains(candidate))
        {
            return candidate;
        }

        for (var index = 2; index < 10_000; index++)
        {
            var indexedCandidate = $"{candidate}-{index}";
            if (!usedIds.Contains(indexedCandidate))
            {
                return indexedCandidate;
            }
        }

        return $"{candidate}-{Guid.NewGuid():N}";
    }

    private static AisProfile CreateMedistarBasedAisProfile(
        ProfileMetadata metadata,
        string profileName,
        string defaultEncoding)
    {
        var medistar = DefaultAisProfiles.CreateMedistarDefault();

        return medistar with
        {
            Metadata = metadata,
            Name = profileName,
            DefaultEncoding = defaultEncoding,
            RequiredStaticFields = new Dictionary<string, string>(medistar.RequiredStaticFields),
            RequiredPatientFieldCodes = medistar.RequiredPatientFieldCodes.ToArray(),
            SupportedOutputFieldCodes = medistar.SupportedOutputFieldCodes.ToArray()
        };
    }

    private static ProfileMetadata? CreateMetadata(
        IEnumerable<ProfileMetadata> existingMetadata,
        ProfileKind profileKind,
        string idPrefix,
        string profileName,
        string Description,
        string? Vendor,
        string? Product,
        DateTimeOffset timestamp,
        string? createdBy,
        List<string> issues,
        Func<string>? idFactory)
    {
        var metadataList = existingMetadata.ToList();
        if (HasProfileNameOrIdConflict(metadataList, profileName))
        {
            issues.Add("Es existiert bereits ein Profil mit diesem Namen oder dieser ID.");
        }

        var profileId = idFactory?.Invoke()?.Trim();
        if (string.IsNullOrWhiteSpace(profileId))
        {
            profileId = CreateUniqueProfileId(idPrefix, profileName, metadataList.Select(metadata => metadata.Id));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            issues.Add("Profil-ID konnte nicht erzeugt werden.");
        }
        else if (!IsSafeProfileId(profileId))
        {
            issues.Add("Profil-ID enthält ungültige Zeichen.");
        }
        else if (metadataList.Any(metadata => string.Equals(metadata.Id, profileId, StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add("Es existiert bereits ein Profil mit diesem Namen oder dieser ID.");
        }

        if (issues.Count > 0)
        {
            return null;
        }

        return new ProfileMetadata(
            Id: profileId,
            Name: profileName,
            ProfileKind: profileKind,
            Description: Description,
            Vendor: Vendor,
            Product: Product,
            Version: "1.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: createdBy,
            IsBuiltIn: false,
            IsUserDefined: true);
    }

    private static string NormalizeRequiredName(string value, List<string> issues)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            issues.Add("Bitte geben Sie einen Profilnamen ein.");
        }

        return normalized;
    }

    private static string NormalizeRequiredField(string value, string message, List<string> issues)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            issues.Add(message);
        }

        return normalized;
    }

    private static string NormalizeOptionalText(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static bool IsSafeProfileId(string profileId)
    {
        return profileId.All(character =>
            char.IsAsciiLetterOrDigit(character)
            || character == '-'
            || character == '_'
            || character == '.');
    }

    private static string CreateSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        var lastWasSeparator = false;

        foreach (var character in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
            if (unicodeCategory == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            var lower = char.ToLowerInvariant(character);
            if (lower is >= 'a' and <= 'z' || lower is >= '0' and <= '9')
            {
                builder.Append(lower);
                lastWasSeparator = false;
                continue;
            }

            if (!lastWasSeparator && builder.Length > 0)
            {
                builder.Append('-');
                lastWasSeparator = true;
            }
        }

        return builder.ToString().Trim('-');
    }
}
