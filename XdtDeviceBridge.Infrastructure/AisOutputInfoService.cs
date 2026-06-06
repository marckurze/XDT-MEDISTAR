using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record AisOutputInfo(
    string InterfaceProfileName,
    string AisProfileName,
    string DeviceProfileName,
    string ExportProfileName,
    string Manufacturer,
    string DeviceType,
    string ConnectionKind,
    bool IsActive,
    string DefaultExaminationType,
    string ExaminationTypeHint,
    IReadOnlyList<AisOutputFieldInfo> Fields,
    IReadOnlyList<MedistarCardLineInfo> StandardCardLineInfos);

public sealed record AisOutputFieldInfo(
    string FieldCode,
    string Meaning,
    string AisRelevance,
    string CardVisibility,
    string Hint,
    bool IsCardField,
    bool IsOptional);

public enum MedistarCardLineCategory
{
    Lensmeter,
    Autorefractor,
    Phoropter,
    SubjectiveRefraction,
    Keratometer,
    Biometry,
    Tonometry,
    Pachymetry
}

public sealed record MedistarCardLineInfo(
    string LineCode,
    string Meaning,
    MedistarCardLineCategory Category);

public sealed class AisOutputInfoService
{
    private static readonly StringComparer CodeComparer = StringComparer.OrdinalIgnoreCase;
    private readonly MedistarCardLineInfoService _cardLineInfoService = new();

    public AisOutputInfo Create(ProfileCatalog catalog, InterfaceProfileDefinition interfaceProfile)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(interfaceProfile);

        var aisProfile = FindAisProfile(catalog, interfaceProfile.AisProfileId);
        var deviceProfile = FindDeviceProfile(catalog, interfaceProfile.DeviceProfileId);
        var exportProfile = FindExportProfile(catalog, interfaceProfile.ExportProfileId);
        var defaultExaminationType = AisExaminationTypeDefaults.Resolve(deviceProfile);
        var fields = CreateFields(interfaceProfile, deviceProfile, exportProfile, defaultExaminationType);
        var standardCardLineInfos = _cardLineInfoService.Create(deviceProfile, exportProfile);

        return new AisOutputInfo(
            InterfaceProfileName: interfaceProfile.Metadata.Name,
            AisProfileName: aisProfile?.Name ?? interfaceProfile.AisProfileId,
            DeviceProfileName: deviceProfile?.Metadata.Name ?? interfaceProfile.DeviceProfileId,
            ExportProfileName: exportProfile?.Metadata.Name ?? interfaceProfile.ExportProfileId,
            Manufacturer: deviceProfile?.Manufacturer ?? "-",
            DeviceType: deviceProfile?.DeviceType ?? "-",
            ConnectionKind: FormatConnectionKind(deviceProfile?.ConnectionKind),
            IsActive: interfaceProfile.IsActive,
            DefaultExaminationType: defaultExaminationType,
            ExaminationTypeHint: "Die Feldkennung 8402 enthält die Untersuchungsart. Sie wird häufig im AIS-System für die XDT-Einstellungen benötigt. XDTBox akzeptiert auch abweichende Untersuchungsarten aus dem AIS; wichtig ist, dass die Feldkennung 8402 im AIS korrekt eingerichtet ist.",
            Fields: fields,
            StandardCardLineInfos: standardCardLineInfos);
    }

    private static IReadOnlyList<AisOutputFieldInfo> CreateFields(
        InterfaceProfileDefinition interfaceProfile,
        DeviceProfileDefinition? deviceProfile,
        ExportProfileDefinition? exportProfile,
        string defaultExaminationType)
    {
        var fieldCodes = new SortedSet<string>(CodeComparer);

        if (exportProfile is not null)
        {
            foreach (var rule in exportProfile.Rules
                         .Where(rule => rule.IsEnabled && !string.IsNullOrWhiteSpace(rule.TargetFieldCode)))
            {
                fieldCodes.Add(rule.TargetFieldCode.Trim());
            }
        }

        fieldCodes.Add("8402");

        if (UsesAttachmentFields(interfaceProfile, exportProfile))
        {
            fieldCodes.Add("6302");
            fieldCodes.Add("6303");
            fieldCodes.Add("6305");
        }

        var attachmentOnlyProfile = IsRequiredAttachmentOnlyProfile(interfaceProfile, deviceProfile);

        return fieldCodes
            .OrderBy(GetFieldSortKey)
            .ThenBy(code => code, CodeComparer)
            .Select(code => CreateFieldInfo(code, deviceProfile, defaultExaminationType, attachmentOnlyProfile))
            .ToArray();
    }

    private static bool UsesAttachmentFields(InterfaceProfileDefinition interfaceProfile, ExportProfileDefinition? exportProfile)
    {
        if (interfaceProfile.FolderOptions.IsAttachmentOnlyMode
            || interfaceProfile.FolderOptions.IsAttachmentProcessingEnabled
            || !string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.AttachmentExportFolder)
            || !string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.AttachmentExternalLinkPathTemplate))
        {
            return true;
        }

        return exportProfile?.Rules.Any(rule =>
            rule.TargetFieldCode is "6302" or "6303" or "6304" or "6305") == true;
    }

    private static AisOutputFieldInfo CreateFieldInfo(
        string fieldCode,
        DeviceProfileDefinition? deviceProfile,
        string defaultExaminationType,
        bool attachmentOnlyProfile)
    {
        var deviceMeaning = CreateDeviceResultMeaning(deviceProfile);
        return fieldCode switch
        {
            "8000" => Technical(fieldCode, "XDT-Nachrichtentyp", "Technisches Steuerfeld für die AIS-Übernahme."),
            "3000" => Technical(fieldCode, "Patientennummer", "Patientenbezug aus der eingehenden AIS-Datei."),
            "3101" => Technical(fieldCode, "Nachname", "Patientenbezug aus der eingehenden AIS-Datei."),
            "3102" => Technical(fieldCode, "Vorname", "Patientenbezug aus der eingehenden AIS-Datei."),
            "3103" => Technical(fieldCode, "Geburtsdatum", "Patientenbezug aus der eingehenden AIS-Datei."),
            "8402" => new AisOutputFieldInfo(
                fieldCode,
                "Untersuchungsart",
                "AIS muss 8402 annehmen, wenn Untersuchungsarten zurückgeschrieben werden sollen.",
                "Nein",
                $"Eingehender Wert wird unverändert übernommen. Empfohlener Default für neue Profile: {defaultExaminationType}.",
                IsCardField: false,
                IsOptional: false),
            "6228" => Card(fieldCode, deviceMeaning, "Hauptausgabe für Mess- oder Befundwerte."),
            "6227" => Card(fieldCode, CreateSecondaryResultMeaning(deviceProfile), "Zusatz-/Subjektivwert je nach Exportprofil."),
            "6221" => Card(fieldCode, "Keratometerwerte", "Karteikartentext für Keratometrie."),
            "6220" => Card(fieldCode, "Pachymetrie / CCT", "Karteikartentext für Hornhautdicke."),
            "6205" => Card(fieldCode, "Tonometrie / Augeninnendruck", "Karteikartentext für Druckwerte."),
            "6302" => Attachment(fieldCode, "Dokumentenname", "AIS-Anhang: Anzeige-/Dokumentenname.", attachmentOnlyProfile),
            "6303" => Attachment(fieldCode, "Dateiformat", "AIS-Anhang: Dateityp.", attachmentOnlyProfile),
            "6304" => Optional(fieldCode, "Beschreibung", "AIS-Anhang: optionale Beschreibung."),
            "6305" => Attachment(fieldCode, "Vollständiger Dateipfad", "AIS-Anhang: Pfad zur vorbereiteten Datei.", attachmentOnlyProfile),
            _ => Card(fieldCode, "AIS-Ausgabefeld", "Ausgabe gemäß Exportprofil.")
        };
    }

    private static AisOutputFieldInfo Technical(string fieldCode, string meaning, string hint)
    {
        return new AisOutputFieldInfo(
            fieldCode,
            meaning,
            "Technisch erforderlich",
            "Nein",
            hint,
            IsCardField: false,
            IsOptional: false);
    }

    private static AisOutputFieldInfo Card(string fieldCode, string meaning, string hint)
    {
        return new AisOutputFieldInfo(
            fieldCode,
            meaning,
            "AIS-Feldkennung für Karteikartenausgabe aktivieren",
            "Ja",
            hint,
            IsCardField: true,
            IsOptional: false);
    }

    private static AisOutputFieldInfo Optional(string fieldCode, string meaning, string hint)
    {
        return new AisOutputFieldInfo(
            fieldCode,
            meaning,
            "Optional, nur für Dokument-/Anhang-Workflows erforderlich",
            "Optional",
            hint,
            IsCardField: false,
            IsOptional: true);
    }

    private static AisOutputFieldInfo Attachment(string fieldCode, string meaning, string hint, bool attachmentOnlyProfile)
    {
        return attachmentOnlyProfile
            ? Card(fieldCode, meaning, $"{hint} Bei reinen Dokumentgeräten ist dieses Feld Bestandteil der Karteikartenübergabe.")
            : Optional(fieldCode, meaning, hint);
    }

    private static string CreateDeviceResultMeaning(DeviceProfileDefinition? deviceProfile)
    {
        var text = CreateDeviceClassifier(deviceProfile);
        if (IsIolMasterDeviceText(text))
        {
            return "Keratometerwerte R1/R2";
        }

        if (IsPhoropterDeviceText(text))
        {
            return "Phoropter finaler Verordnungswert";
        }

        if (ContainsAny(text, "lens", "lensmeter", "scheitel", "lm"))
        {
            return "Lensmeterwerte";
        }

        if (ContainsAny(text, "autorefr", "refrak", "ark", "ar-"))
        {
            return "Autorefraktorwerte";
        }

        if (ContainsAny(text, "tonometer", "nct", "nt-", "ct-"))
        {
            return "Tonometrie / Augeninnendruck";
        }

        if (ContainsAny(text, "pachy", "cct"))
        {
            return "Pachymetrie / CCT";
        }

        if (ContainsAny(text, "endo", "em-"))
        {
            return "Endothelzellmessung";
        }

        return "Mess- oder Befundwerte";
    }

    private static string CreateSecondaryResultMeaning(DeviceProfileDefinition? deviceProfile)
    {
        var text = CreateDeviceClassifier(deviceProfile);
        if (IsIolMasterDeviceText(text))
        {
            return "Biometrie / VKT und Achslaenge";
        }

        if (IsPhoropterDeviceText(text))
        {
            return "Phoropter Maximalwert / subjektive Refraktion";
        }

        return "Zusatztext / ergänzende Messausgabe";
    }

    private static string CreateDeviceClassifier(DeviceProfileDefinition? deviceProfile)
    {
        var identity = CreateDeviceIdentityClassifier(deviceProfile);
        if (deviceProfile is null)
        {
            return identity;
        }

        return string.Join(
            ' ',
            identity,
            string.Join(' ', deviceProfile.SupportedExaminationTypes ?? Array.Empty<string>()));
    }

    private static string CreateDeviceIdentityClassifier(DeviceProfileDefinition? deviceProfile)
    {
        if (deviceProfile is null)
        {
            return string.Empty;
        }

        return string.Join(
            ' ',
            deviceProfile.Metadata.Name,
            deviceProfile.Metadata.Product ?? string.Empty,
            deviceProfile.Manufacturer,
            deviceProfile.DeviceType,
            deviceProfile.Model,
            deviceProfile.ParserMode);
    }

    private static bool IsRequiredAttachmentOnlyProfile(
        InterfaceProfileDefinition interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        if (interfaceProfile.FolderOptions.IsAttachmentOnlyMode)
        {
            return true;
        }

        var text = CreateDeviceIdentityClassifier(deviceProfile);
        return IsDocumentOnlyDeviceText(text);
    }

    private static bool IsDocumentOnlyDeviceText(string text)
    {
        return ContainsAny(
            text,
            "document",
            "dokument",
            "attachmentonly",
            "attachment only",
            "anhang",
            "manual document",
            "manualdocument",
            "dokumentauswahl",
            "dokumentübergabe");
    }

    private static bool ContainsAny(string text, params string[] needles)
    {
        return needles.Any(needle => text.Contains(needle, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPhoropterDeviceText(string text)
    {
        if (text.Contains("autorefractor", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ContainsAny(text, "phoropter", "refractor", "rt-");
    }

    private static bool IsIolMasterDeviceText(string text)
    {
        return ContainsAny(text, "iolmaster", "iol master", "biometrie", "biometry", "achslaenge", "achsl", "axial");
    }

    private static int GetFieldSortKey(string fieldCode)
    {
        return fieldCode switch
        {
            "8000" => 0,
            "3000" => 10,
            "3101" => 11,
            "3102" => 12,
            "3103" => 13,
            "8402" => 20,
            "6228" => 30,
            "6227" => 31,
            "6221" => 33,
            "6220" => 34,
            "6205" => 35,
            "6302" => 40,
            "6303" => 41,
            "6304" => 42,
            "6305" => 43,
            _ => 100
        };
    }

    private static AisProfile? FindAisProfile(ProfileCatalog catalog, string profileId)
    {
        return catalog.AisProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase));
    }

    private static DeviceProfileDefinition? FindDeviceProfile(ProfileCatalog catalog, string profileId)
    {
        return catalog.DeviceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase));
    }

    private static ExportProfileDefinition? FindExportProfile(ProfileCatalog catalog, string profileId)
    {
        return catalog.ExportProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase));
    }

    private static string FormatConnectionKind(DeviceConnectionKind? connectionKind)
    {
        return connectionKind switch
        {
            DeviceConnectionKind.SerialRs232 => "Seriell RS232",
            DeviceConnectionKind.NetworkLan => "LAN / Datei / UNC",
            null => "-",
            _ => connectionKind.Value.ToString()
        };
    }
}

public sealed class MedistarCardLineInfoService
{
    private static readonly MedistarCardLineInfo Lensmeter = new("V0", "Lensmeter", MedistarCardLineCategory.Lensmeter);
    private static readonly MedistarCardLineInfo Autorefractor = new("V1", "Autorefraktor", MedistarCardLineCategory.Autorefractor);
    private static readonly MedistarCardLineInfo Phoropter = new("V2", "Phoropter", MedistarCardLineCategory.Phoropter);
    private static readonly MedistarCardLineInfo SubjectiveRefraction = new("V4", "subjektive Refraktion", MedistarCardLineCategory.SubjectiveRefraction);
    private static readonly MedistarCardLineInfo Keratometer = new("V7", "Keratometer", MedistarCardLineCategory.Keratometer);
    private static readonly MedistarCardLineInfo Biometry = new("V8", "Biometrie", MedistarCardLineCategory.Biometry);
    private static readonly MedistarCardLineInfo Tonometry = new("Y", "Tonometrie", MedistarCardLineCategory.Tonometry);
    private static readonly MedistarCardLineInfo Pachymetry = new("P", "Pachymetrie", MedistarCardLineCategory.Pachymetry);

    public IReadOnlyList<MedistarCardLineInfo> Create(
        DeviceProfileDefinition? deviceProfile,
        ExportProfileDefinition? exportProfile)
    {
        if (exportProfile is null)
        {
            return Array.Empty<MedistarCardLineInfo>();
        }

        var categories = new HashSet<MedistarCardLineCategory>();
        var deviceText = CreateDeviceClassifier(deviceProfile);
        var isIolMaster = IsIolMasterDeviceText(deviceText);
        var isPhoropter = IsPhoropterDeviceText(deviceText);

        foreach (var rule in exportProfile.Rules.Where(rule => rule.IsEnabled))
        {
            AddRuleCategories(categories, rule, deviceText, isIolMaster, isPhoropter);
        }

        return categories
            .OrderBy(GetSortKey)
            .Select(CreateInfo)
            .ToArray();
    }

    private static void AddRuleCategories(
        ISet<MedistarCardLineCategory> categories,
        ExportRuleDefinition rule,
        string deviceText,
        bool isIolMaster,
        bool isPhoropter)
    {
        var fieldCode = rule.TargetFieldCode.Trim();
        if (fieldCode is not ("6228" or "6227" or "6221" or "6220" or "6205"))
        {
            return;
        }

        var ruleText = CreateRuleClassifier(rule);
        if (isIolMaster)
        {
            if (fieldCode == "6227" || ContainsAny(ruleText, "iol", "biometr", "vkt", "al ", "axial", "achs"))
            {
                categories.Add(MedistarCardLineCategory.Biometry);
            }

            if (fieldCode is "6228" or "6221" || ContainsAny(ruleText, "km", "kerato", "r1", "r2"))
            {
                categories.Add(MedistarCardLineCategory.Keratometer);
            }

            return;
        }

        switch (fieldCode)
        {
            case "6221":
                categories.Add(MedistarCardLineCategory.Keratometer);
                return;
            case "6220":
                categories.Add(MedistarCardLineCategory.Pachymetry);
                return;
            case "6205":
                categories.Add(MedistarCardLineCategory.Tonometry);
                return;
            case "6228":
                AddPrimaryResultCategory(categories, ruleText, deviceText, isPhoropter);
                return;
            case "6227":
                AddSecondaryResultCategory(categories, ruleText, isPhoropter);
                return;
        }
    }

    private static void AddPrimaryResultCategory(
        ISet<MedistarCardLineCategory> categories,
        string ruleText,
        string deviceText,
        bool isPhoropter)
    {
        if (ContainsAny(ruleText, "lm", "lens", "lensmeter", "scheitel"))
        {
            categories.Add(MedistarCardLineCategory.Lensmeter);
            return;
        }

        if (ContainsAny(ruleText, "ref", "autorefr", "ark", " ar", "rm"))
        {
            categories.Add(MedistarCardLineCategory.Autorefractor);
            return;
        }

        if (ContainsAny(ruleText, "km", "kerato", "r1", "r2"))
        {
            categories.Add(MedistarCardLineCategory.Keratometer);
            return;
        }

        if (isPhoropter || ContainsAny(ruleText, "phoropter", "final", "prescription", "rts"))
        {
            categories.Add(MedistarCardLineCategory.Phoropter);
            return;
        }

        if (ContainsAny(deviceText, "lens", "lensmeter", "scheitel"))
        {
            categories.Add(MedistarCardLineCategory.Lensmeter);
            return;
        }

        if (ContainsAny(deviceText, "autorefr", "refrak", "ark", "ar-"))
        {
            categories.Add(MedistarCardLineCategory.Autorefractor);
        }
    }

    private static void AddSecondaryResultCategory(
        ISet<MedistarCardLineCategory> categories,
        string ruleText,
        bool isPhoropter)
    {
        if (isPhoropter || ContainsAny(ruleText, "subjective", "subjekt", "sbj", "full", "correction", "vollkorrektion"))
        {
            categories.Add(MedistarCardLineCategory.SubjectiveRefraction);
        }
    }

    private static string CreateRuleClassifier(ExportRuleDefinition rule)
    {
        return string.Join(
            ' ',
            rule.TargetFieldCode,
            rule.TargetName,
            rule.SourcePath ?? string.Empty,
            rule.Description ?? string.Empty);
    }

    private static string CreateDeviceClassifier(DeviceProfileDefinition? deviceProfile)
    {
        if (deviceProfile is null)
        {
            return string.Empty;
        }

        return string.Join(
            ' ',
            deviceProfile.Metadata.Name,
            deviceProfile.Metadata.Product ?? string.Empty,
            deviceProfile.Manufacturer,
            deviceProfile.DeviceType,
            deviceProfile.Model,
            deviceProfile.ParserMode,
            string.Join(' ', deviceProfile.SupportedExaminationTypes ?? Array.Empty<string>()));
    }

    private static MedistarCardLineInfo CreateInfo(MedistarCardLineCategory category)
    {
        return category switch
        {
            MedistarCardLineCategory.Lensmeter => Lensmeter,
            MedistarCardLineCategory.Autorefractor => Autorefractor,
            MedistarCardLineCategory.Phoropter => Phoropter,
            MedistarCardLineCategory.SubjectiveRefraction => SubjectiveRefraction,
            MedistarCardLineCategory.Keratometer => Keratometer,
            MedistarCardLineCategory.Biometry => Biometry,
            MedistarCardLineCategory.Tonometry => Tonometry,
            MedistarCardLineCategory.Pachymetry => Pachymetry,
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };
    }

    private static int GetSortKey(MedistarCardLineCategory category)
    {
        return category switch
        {
            MedistarCardLineCategory.Lensmeter => 10,
            MedistarCardLineCategory.Autorefractor => 20,
            MedistarCardLineCategory.Phoropter => 30,
            MedistarCardLineCategory.SubjectiveRefraction => 40,
            MedistarCardLineCategory.Keratometer => 50,
            MedistarCardLineCategory.Biometry => 60,
            MedistarCardLineCategory.Tonometry => 70,
            MedistarCardLineCategory.Pachymetry => 80,
            _ => 100
        };
    }

    private static bool ContainsAny(string text, params string[] needles)
    {
        return needles.Any(needle => text.Contains(needle, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPhoropterDeviceText(string text)
    {
        if (text.Contains("autorefractor", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ContainsAny(text, "phoropter", "refractor", "rt-");
    }

    private static bool IsIolMasterDeviceText(string text)
    {
        return ContainsAny(text, "iolmaster", "iol master", "biometrie", "biometry", "achslaenge", "achsl", "axial");
    }
}

public static class AisExaminationTypeDefaults
{
    public const string Fallback = "MESS";

    public static string Resolve(DeviceProfileDefinition? deviceProfile)
    {
        if (deviceProfile is null)
        {
            return Fallback;
        }

        var identityText = string.Join(
            ' ',
            deviceProfile.Metadata.Name,
            deviceProfile.Metadata.Product ?? string.Empty,
            deviceProfile.Manufacturer,
            deviceProfile.DeviceType,
            deviceProfile.Model,
            deviceProfile.ParserMode);
        var supportedText = string.Join(' ', deviceProfile.SupportedExaminationTypes ?? Array.Empty<string>());
        var text = string.Join(
            ' ',
            identityText,
            supportedText);

        if (IsDocumentOnlyDeviceText(identityText))
        {
            return "DOKU";
        }

        if (ContainsAny(text, "oct"))
        {
            return "OCT";
        }

        if (ContainsAny(text, "endo", "em-"))
        {
            return "ENDO";
        }

        if (IsIolMasterDeviceText(text))
        {
            return "IOL";
        }

        if (IsCombinationDevice(text))
        {
            return "KOMB";
        }

        if (ContainsAny(text, "tonometer", "nct", "nt-", "tm", "ct-"))
        {
            return "TONO";
        }

        if (ContainsAny(text, "pachy", "cct"))
        {
            return "PACHY";
        }

        if (ContainsAny(text, "kerato", "keratometer", "km"))
        {
            if (ContainsAny(text, "ref", "refrak", "auto"))
            {
                return "KOMB";
            }

            return "KERA";
        }

        if (IsPhoropterDeviceText(text))
        {
            return "PHORO";
        }

        if (ContainsAny(text, "lens", "lensmeter", "scheitel", "lm"))
        {
            return "LENS";
        }

        if (ContainsAny(text, "autorefr", "refrak", "ark", "ar-"))
        {
            return "AUTO";
        }

        return Fallback;
    }

    private static bool IsDocumentOnlyDeviceText(string text)
    {
        return ContainsAny(
            text,
            "document",
            "dokument",
            "attachmentonly",
            "attachment only",
            "anhang",
            "manual document",
            "manualdocument",
            "dokumentauswahl",
            "dokumentübergabe");
    }

    private static bool IsCombinationDevice(string text)
    {
        if (ContainsAny(text, "combo", "kombi", "komb", "trk-", "tonoref", "ref/km", "ref-km"))
        {
            return true;
        }

        var hasRef = ContainsAny(text, "autorefr", "refrak", "ref", "ark", "ar-");
        var hasKerato = ContainsAny(text, "kerato", "keratometer", "km");
        var hasTono = ContainsAny(text, "tonometer", "nct", "nt-", "tm");
        var hasPachy = ContainsAny(text, "pachy", "cct");

        return (hasRef && hasKerato)
            || (hasRef && hasTono)
            || (hasRef && hasPachy)
            || (hasKerato && hasTono)
            || (hasKerato && hasPachy);
    }

    private static bool ContainsAny(string text, params string[] needles)
    {
        return needles.Any(needle => text.Contains(needle, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPhoropterDeviceText(string text)
    {
        if (text.Contains("autorefractor", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ContainsAny(text, "phoropter", "refractor", "rt-");
    }

    private static bool IsIolMasterDeviceText(string text)
    {
        return ContainsAny(text, "iolmaster", "iol master", "biometrie", "biometry", "achslaenge", "achsl", "axial");
    }
}
