namespace XdtDeviceBridge.Core;

public static class DefaultDeviceProfileDefinitions
{
    public static DeviceProfileDefinition CreateNidekArk1sDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-nidek-ark1s-default",
                Name: "NIDEK ARK1S",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for NIDEK ARK1S XML measurement files.",
                Vendor: "NIDEK",
                Product: "ARK1S",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "NIDEK",
            Model: "ARK1S",
            DeviceType: "Autorefractor",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("r-sphere", "R Sphere", "R/AR/ARMedian/Sphere", "ARMedian", "R", "dpt", true, "Right eye sphere."),
                new DeviceMeasurementDefinition("r-cylinder", "R Cylinder", "R/AR/ARMedian/Cylinder", "ARMedian", "R", "dpt", true, "Right eye cylinder."),
                new DeviceMeasurementDefinition("r-axis", "R Axis", "R/AR/ARMedian/Axis", "ARMedian", "R", "deg", true, "Right eye axis."),
                new DeviceMeasurementDefinition("r-se", "R SE", "R/AR/ARMedian/SE", "ARMedian", "R", "dpt", true, "Right eye spherical equivalent."),
                new DeviceMeasurementDefinition("l-sphere", "L Sphere", "L/AR/ARMedian/Sphere", "ARMedian", "L", "dpt", true, "Left eye sphere."),
                new DeviceMeasurementDefinition("l-cylinder", "L Cylinder", "L/AR/ARMedian/Cylinder", "ARMedian", "L", "dpt", true, "Left eye cylinder."),
                new DeviceMeasurementDefinition("l-axis", "L Axis", "L/AR/ARMedian/Axis", "ARMedian", "L", "deg", true, "Left eye axis."),
                new DeviceMeasurementDefinition("l-se", "L SE", "L/AR/ARMedian/SE", "ARMedian", "L", "dpt", true, "Left eye spherical equivalent."),
                new DeviceMeasurementDefinition("far-pd", "FarPD", "PD/PDList[@No='1']/FarPD", "PDList", string.Empty, "mm", true, "Far pupillary distance."),
                new DeviceMeasurementDefinition("near-pd", "NearPD", "PD/PDList[@No='1']/NearPD", "PDList", string.Empty, "mm", false, "Near pupillary distance.")
            },
            SupportedExaminationTypes: new[] { "Refraktion", "PD" },
            CanContainMultipleExaminationTypes: false,
            DeviceImagePath: InterfaceProfileUiPolicy.NidekArk1sDeviceImagePath);
    }

    public static DeviceProfileDefinition CreateNidekAr360Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-nidek-ar360-default",
                Name: "NIDEK AR360",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for NIDEK AR360 / AR-360A LAN XML autorefractor measurement files.",
                Vendor: "NIDEK",
                Product: "AR360 / AR-360A",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "NIDEK",
            Model: "AR-360A",
            DeviceType: "Autorefractor",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("ar360-company", "Company", "Company", "Common", string.Empty, string.Empty, true, "NIDEK LAN XML company field."),
                new DeviceMeasurementDefinition("ar360-model-name", "ModelName", "ModelName", "Common", string.Empty, string.Empty, true, "NIDEK LAN XML model name field; expected AR-360A."),
                new DeviceMeasurementDefinition("ar360-date", "Date", "Date", "Common", string.Empty, string.Empty, false, "NIDEK LAN XML measurement date."),
                new DeviceMeasurementDefinition("ar360-time", "Time", "Time", "Common", string.Empty, string.Empty, false, "NIDEK LAN XML measurement time."),
                new DeviceMeasurementDefinition("ar360-patient-no", "Patient No.", "Patient/No.", "Common", string.Empty, string.Empty, false, "NIDEK LAN XML patient number; not exported to MEDISTAR."),
                new DeviceMeasurementDefinition("ar360-vd", "VD", "VD", "ARMedian", string.Empty, "mm", false, "Vertex distance from NIDEK AR360 LAN XML."),
                new DeviceMeasurementDefinition("ar360-r-sphere", "R Sphere", "R/AR/ARMedian/Sphere", "ARMedian", "R", "dpt", true, "Right eye sphere from ARMedian."),
                new DeviceMeasurementDefinition("ar360-r-cylinder", "R Cylinder", "R/AR/ARMedian/Cylinder", "ARMedian", "R", "dpt", true, "Right eye cylinder from ARMedian."),
                new DeviceMeasurementDefinition("ar360-r-axis", "R Axis", "R/AR/ARMedian/Axis", "ARMedian", "R", "deg", true, "Right eye axis from ARMedian."),
                new DeviceMeasurementDefinition("ar360-l-sphere", "L Sphere", "L/AR/ARMedian/Sphere", "ARMedian", "L", "dpt", true, "Left eye sphere from ARMedian."),
                new DeviceMeasurementDefinition("ar360-l-cylinder", "L Cylinder", "L/AR/ARMedian/Cylinder", "ARMedian", "L", "dpt", true, "Left eye cylinder from ARMedian."),
                new DeviceMeasurementDefinition("ar360-l-axis", "L Axis", "L/AR/ARMedian/Axis", "ARMedian", "L", "deg", true, "Left eye axis from ARMedian."),
                new DeviceMeasurementDefinition("ar360-far-pd", "FarPD", "PD/PDList[@No='1']/FarPD", "PDList", string.Empty, "mm", false, "Far pupillary distance from NIDEK AR360 LAN XML.")
            },
            SupportedExaminationTypes: new[] { "Refraktion", "AR", "PD" },
            CanContainMultipleExaminationTypes: false,
            DeviceImagePath: InterfaceProfileUiPolicy.NidekAr360DeviceImagePath);
    }

    public static DeviceProfileDefinition CreateNidekAr1Default()
    {
        return CreateNidekArXmlDefault(
            id: "device-nidek-ar1-default",
            name: "NIDEK AR-1",
            model: "AR-1",
            description: "Built-in NIDEK AR-1 XML autorefractor profile derived from the reference package OPTO01 paths.",
            includeSubjective: false);
    }

    public static DeviceProfileDefinition CreateNidekAr1SDefault()
    {
        return CreateNidekArXmlDefault(
            id: "device-nidek-ar1s-default",
            name: "NIDEK AR-1S",
            model: "AR-1S",
            description: "Built-in NIDEK AR-1S XML autorefractor profile derived from the reference package OPTO01 and SUBJ01 paths.",
            includeSubjective: true);
    }

    public static DeviceProfileDefinition CreateNidekAr310ADefault()
    {
        return CreateNidekArXmlDefault(
            id: "device-nidek-ar310a-default",
            name: "NIDEK AR-310A",
            model: "AR-310A",
            description: "Built-in NIDEK AR-310A XML autorefractor profile derived from the reference package OPTO01 paths.",
            includeSubjective: false);
    }

    private static DeviceProfileDefinition CreateNidekArXmlDefault(
        string id,
        string name,
        string model,
        string description,
        bool includeSubjective)
    {
        var timestamp = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);
        var measurements = new List<DeviceMeasurementDefinition>
        {
            new($"{id}-company", "Company", "Company", "Common", string.Empty, string.Empty, true, "NIDEK XML company field."),
            new($"{id}-model-name", "ModelName", "ModelName", "Common", string.Empty, string.Empty, true, $"NIDEK XML model name field; expected {model}."),
            new($"{id}-date", "Date", "Date", "Common", string.Empty, string.Empty, false, "NIDEK XML measurement date."),
            new($"{id}-time", "Time", "Time", "Common", string.Empty, string.Empty, false, "NIDEK XML measurement time."),
            new($"{id}-vd", "VD", "VD", "ARMedian", string.Empty, "mm", false, "Vertex distance from NIDEK XML."),
            new($"{id}-r-sphere", "R Sphere", "R/AR/ARMedian/Sphere", "ARMedian", "R", "dpt", true, "Right eye sphere from ARMedian."),
            new($"{id}-r-cylinder", "R Cylinder", "R/AR/ARMedian/Cylinder", "ARMedian", "R", "dpt", true, "Right eye cylinder from ARMedian."),
            new($"{id}-r-axis", "R Axis", "R/AR/ARMedian/Axis", "ARMedian", "R", "deg", true, "Right eye axis from ARMedian."),
            new($"{id}-l-sphere", "L Sphere", "L/AR/ARMedian/Sphere", "ARMedian", "L", "dpt", true, "Left eye sphere from ARMedian."),
            new($"{id}-l-cylinder", "L Cylinder", "L/AR/ARMedian/Cylinder", "ARMedian", "L", "dpt", true, "Left eye cylinder from ARMedian."),
            new($"{id}-l-axis", "L Axis", "L/AR/ARMedian/Axis", "ARMedian", "L", "deg", true, "Left eye axis from ARMedian."),
            new($"{id}-far-pd", "FarPD", "PD/PDList[@No='1']/FarPD", "PDList", string.Empty, "mm", false, "Far pupillary distance from NIDEK XML.")
        };

        if (includeSubjective)
        {
            measurements.AddRange(new[]
            {
                new DeviceMeasurementDefinition($"{id}-subj-r-sphere", "Subjective R Sphere", "R/SR/Sphere", "SR", "R", "dpt", false, "Right subjective sphere from SUBJ01."),
                new DeviceMeasurementDefinition($"{id}-subj-r-cylinder", "Subjective R Cylinder", "R/SR/Cylinder", "SR", "R", "dpt", false, "Right subjective cylinder from SUBJ01."),
                new DeviceMeasurementDefinition($"{id}-subj-r-axis", "Subjective R Axis", "R/SR/Axis", "SR", "R", "deg", false, "Right subjective axis from SUBJ01."),
                new DeviceMeasurementDefinition($"{id}-subj-l-sphere", "Subjective L Sphere", "L/SR/Sphere", "SR", "L", "dpt", false, "Left subjective sphere from SUBJ01."),
                new DeviceMeasurementDefinition($"{id}-subj-l-cylinder", "Subjective L Cylinder", "L/SR/Cylinder", "SR", "L", "dpt", false, "Left subjective cylinder from SUBJ01."),
                new DeviceMeasurementDefinition($"{id}-subj-l-axis", "Subjective L Axis", "L/SR/Axis", "SR", "L", "deg", false, "Left subjective axis from SUBJ01.")
            });
        }

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.DeviceProfile,
                Description: description,
                Vendor: "NIDEK",
                Product: model,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "NIDEK",
            Model: model,
            DeviceType: includeSubjective ? "Autorefractor/Subjective" : "Autorefractor",
            ParserMode: "Xml",
            Measurements: measurements,
            SupportedExaminationTypes: includeSubjective ? new[] { "Refraktion", "AR", "SR", "PD" } : new[] { "Refraktion", "AR", "PD" },
            CanContainMultipleExaminationTypes: includeSubjective,
            DeviceImagePath: InterfaceProfileUiPolicy.NidekAr360DeviceImagePath);
    }

    public static DeviceProfileDefinition CreateNidekArk510ADefault()
    {
        return CreateNidekArk5xxDefault(
            id: "device-nidek-ark510a-default",
            name: "NIDEK ARK-510A",
            model: "ARK-510A",
            description: "Default device profile definition for NIDEK ARK-510A XML autorefractor measurement files.");
    }

    public static DeviceProfileDefinition CreateNidekArk560ADefault()
    {
        return CreateNidekArk5xxDefault(
            id: "device-nidek-ark560a-default",
            name: "NIDEK ARK-560A",
            model: "ARK-560A",
            description: "Default device profile definition for NIDEK ARK-560A XML autorefractor measurement files.");
    }

    private static DeviceProfileDefinition CreateNidekArk5xxDefault(
        string id,
        string name,
        string model,
        string description)
    {
        var timestamp = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.DeviceProfile,
                Description: description,
                Vendor: "NIDEK",
                Product: model,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "NIDEK",
            Model: model,
            DeviceType: "Autorefractor",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition($"{id}-company", "Company", "Company", "Common", string.Empty, string.Empty, true, "NIDEK XML company field."),
                new DeviceMeasurementDefinition($"{id}-model-name", "ModelName", "ModelName", "Common", string.Empty, string.Empty, true, $"NIDEK XML model name field; expected {model}."),
                new DeviceMeasurementDefinition($"{id}-date", "Date", "Date", "Common", string.Empty, string.Empty, false, "NIDEK XML measurement date."),
                new DeviceMeasurementDefinition($"{id}-time", "Time", "Time", "Common", string.Empty, string.Empty, false, "NIDEK XML measurement time."),
                new DeviceMeasurementDefinition($"{id}-vd", "VD", "VD", "ARMedian", string.Empty, "mm", false, "Vertex distance from NIDEK XML."),
                new DeviceMeasurementDefinition($"{id}-r-sphere", "R Sphere", "R/AR/ARMedian/Sphere", "ARMedian", "R", "dpt", true, "Right eye sphere from ARMedian."),
                new DeviceMeasurementDefinition($"{id}-r-cylinder", "R Cylinder", "R/AR/ARMedian/Cylinder", "ARMedian", "R", "dpt", true, "Right eye cylinder from ARMedian."),
                new DeviceMeasurementDefinition($"{id}-r-axis", "R Axis", "R/AR/ARMedian/Axis", "ARMedian", "R", "deg", true, "Right eye axis from ARMedian."),
                new DeviceMeasurementDefinition($"{id}-l-sphere", "L Sphere", "L/AR/ARMedian/Sphere", "ARMedian", "L", "dpt", true, "Left eye sphere from ARMedian."),
                new DeviceMeasurementDefinition($"{id}-l-cylinder", "L Cylinder", "L/AR/ARMedian/Cylinder", "ARMedian", "L", "dpt", true, "Left eye cylinder from ARMedian."),
                new DeviceMeasurementDefinition($"{id}-l-axis", "L Axis", "L/AR/ARMedian/Axis", "ARMedian", "L", "deg", true, "Left eye axis from ARMedian."),
                new DeviceMeasurementDefinition($"{id}-far-pd", "FarPD", "PD/PDList[@No='1']/FarPD", "PDList", string.Empty, "mm", false, "Far pupillary distance from NIDEK XML.")
            },
            SupportedExaminationTypes: new[] { "Refraktion", "AR", "PD" },
            CanContainMultipleExaminationTypes: false);
    }

    public static DeviceProfileDefinition CreateNidekLm1800PDefault()
    {
        var timestamp = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-nidek-lm1800p-default",
                Name: "NIDEK LM-1800P",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for NIDEK LM-1800P XML lensmeter measurement files.",
                Vendor: "NIDEK",
                Product: "LM-1800P",
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "NIDEK",
            Model: "LM-1800P",
            DeviceType: "Lensmeter",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("lm1800p-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "NIDEK LM-1800P XML company field."),
                new DeviceMeasurementDefinition("lm1800p-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "NIDEK LM-1800P XML model name field."),
                new DeviceMeasurementDefinition("lm1800p-date", "Date", "Common/Date", "Common", string.Empty, string.Empty, false, "NIDEK LM-1800P measurement date."),
                new DeviceMeasurementDefinition("lm1800p-time", "Time", "Common/Time", "Common", string.Empty, string.Empty, false, "NIDEK LM-1800P measurement time."),
                new DeviceMeasurementDefinition("lm1800p-r-sphere", "R Sphere", "Measure[@Type='LM']/LM/R/Sphere", "LM", "R", "dpt", false, "Right lens sphere from the R block."),
                new DeviceMeasurementDefinition("lm1800p-r-cylinder", "R Cylinder", "Measure[@Type='LM']/LM/R/Cylinder", "LM", "R", "dpt", false, "Right lens cylinder from the R block."),
                new DeviceMeasurementDefinition("lm1800p-r-axis", "R Axis", "Measure[@Type='LM']/LM/R/Axis", "LM", "R", "deg", false, "Right lens axis from the R block."),
                new DeviceMeasurementDefinition("lm1800p-i-sphere", "I Sphere", "Measure[@Type='LM']/LM/I/Sphere", "LM", "R", "dpt", false, "Right lens sphere from the LM-1800P I block."),
                new DeviceMeasurementDefinition("lm1800p-i-cylinder", "I Cylinder", "Measure[@Type='LM']/LM/I/Cylinder", "LM", "R", "dpt", false, "Right lens cylinder from the LM-1800P I block."),
                new DeviceMeasurementDefinition("lm1800p-i-axis", "I Axis", "Measure[@Type='LM']/LM/I/Axis", "LM", "R", "deg", false, "Right lens axis from the LM-1800P I block."),
                new DeviceMeasurementDefinition("lm1800p-l-sphere", "L Sphere", "Measure[@Type='LM']/LM/L/Sphere", "LM", "L", "dpt", false, "Left lens sphere from the L block."),
                new DeviceMeasurementDefinition("lm1800p-l-cylinder", "L Cylinder", "Measure[@Type='LM']/LM/L/Cylinder", "LM", "L", "dpt", false, "Left lens cylinder from the L block."),
                new DeviceMeasurementDefinition("lm1800p-l-axis", "L Axis", "Measure[@Type='LM']/LM/L/Axis", "LM", "L", "deg", false, "Left lens axis from the L block."),
                new DeviceMeasurementDefinition("lm1800p-s-sphere", "S Sphere", "Measure[@Type='LM']/LM/S/Sphere", "LM", "L", "dpt", false, "Left lens sphere from the LM-1800P S block."),
                new DeviceMeasurementDefinition("lm1800p-s-cylinder", "S Cylinder", "Measure[@Type='LM']/LM/S/Cylinder", "LM", "L", "dpt", false, "Left lens cylinder from the LM-1800P S block."),
                new DeviceMeasurementDefinition("lm1800p-s-axis", "S Axis", "Measure[@Type='LM']/LM/S/Axis", "LM", "L", "deg", false, "Left lens axis from the LM-1800P S block."),
                new DeviceMeasurementDefinition("lm1800p-r-add", "R ADD", "Measure[@Type='LM']/LM/R/ADD", "LM", "R", "dpt", false, "Right addition from the R block."),
                new DeviceMeasurementDefinition("lm1800p-i-add", "I ADD", "Measure[@Type='LM']/LM/I/ADD", "LM", "R", "dpt", false, "Right addition from the I block."),
                new DeviceMeasurementDefinition("lm1800p-l-add", "L ADD", "Measure[@Type='LM']/LM/L/ADD", "LM", "L", "dpt", false, "Left addition from the L block."),
                new DeviceMeasurementDefinition("lm1800p-s-add", "S ADD", "Measure[@Type='LM']/LM/S/ADD", "LM", "L", "dpt", false, "Left addition from the S block."),
                new DeviceMeasurementDefinition("lm1800p-pd-distance", "PD Distance", "Measure[@Type='LM']/PD/Distance", "PD", string.Empty, "mm", false, "Lensmeter pupillary distance."),
                new DeviceMeasurementDefinition("lm1800p-medistar-r-line", "R MEDISTAR Lensmeter-Zeile", "Measure[@Type='LM']/LM/R/MedistarLine", "LM", "R", string.Empty, false, "Computed MEDISTAR lensmeter line for the right lens."),
                new DeviceMeasurementDefinition("lm1800p-medistar-l-line", "L MEDISTAR Lensmeter-Zeile", "Measure[@Type='LM']/LM/L/MedistarLine", "LM", "L", string.Empty, false, "Computed MEDISTAR lensmeter line for the left lens.")
            },
            SupportedExaminationTypes: new[] { "Lensmeter", "PD", "Prism" },
            CanContainMultipleExaminationTypes: false);
    }

    public static DeviceProfileDefinition CreateNidekLm1800PdDefault()
    {
        var profile = CreateNidekLm1800PDefault();
        var timestamp = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

        return profile with
        {
            Metadata = new ProfileMetadata(
                Id: "device-nidek-lm1800pd-default",
                Name: "NIDEK LM-1800PD",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Built-in NIDEK LM-1800PD XML lensmeter profile derived from the reference package LM family paths.",
                Vendor: "NIDEK",
                Product: "LM-1800PD",
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Model = "LM-1800PD"
        };
    }

    public static DeviceProfileDefinition CreateNidekLm7Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-nidek-lm7-default",
                Name: "NIDEK LM7",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for NIDEK LM7 lensmeter measurement files.",
                Vendor: "NIDEK",
                Product: "LM7",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "NIDEK",
            Model: "LM7",
            DeviceType: "Lensmeter",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("lm7-r-sphere", "R Sphere", "R/Sphare", "LM", "R", "dpt", false, "Legacy right lens sphere path from an early LM7 XML fragment."),
                new DeviceMeasurementDefinition("lm7-r-cylinder", "R Cylinder", "R/Cylinder", "LM", "R", "dpt", false, "Legacy right lens cylinder path from an early LM7 XML fragment."),
                new DeviceMeasurementDefinition("lm7-r-axis", "R Axis", "R/Axis", "LM", "R", "deg", false, "Legacy right lens axis path from an early LM7 XML fragment."),
                new DeviceMeasurementDefinition("lm7-r-prism-horizontal", "R PrismHorizontal", "R/PrismX", "LM", "R", "prism dpt", false, "Legacy right horizontal prism path from an early LM7 XML fragment."),
                new DeviceMeasurementDefinition("lm7-r-prism-horizontal-base", "R PrismHorizontalBase", "R/PrismX/@base", "LM", "R", string.Empty, false, "Legacy right horizontal prism base path from an early LM7 XML fragment."),
                new DeviceMeasurementDefinition("lm7-r-prism-vertical", "R PrismVertical", "R/PrismY", "LM", "R", "prism dpt", false, "Legacy right vertical prism path from an early LM7 XML fragment."),
                new DeviceMeasurementDefinition("lm7-r-prism-vertical-base", "R PrismVerticalBase", "R/PrismY/@base", "LM", "R", string.Empty, false, "Legacy right vertical prism base path from an early LM7 XML fragment."),
                new DeviceMeasurementDefinition("lm7-l-sphere", "L Sphere", "L/Sphare", "LM", "L", "dpt", false, "Left lens sphere, noch zu validieren."),
                new DeviceMeasurementDefinition("lm7-l-cylinder", "L Cylinder", "L/Cylinder", "LM", "L", "dpt", false, "Left lens cylinder, noch zu validieren."),
                new DeviceMeasurementDefinition("lm7-l-axis", "L Axis", "L/Axis", "LM", "L", "deg", false, "Left lens axis, noch zu validieren."),
                new DeviceMeasurementDefinition("lm7-l-prism-horizontal", "L PrismHorizontal", "L/PrismX", "LM", "L", "prism dpt", false, "Left horizontal prism, noch zu validieren."),
                new DeviceMeasurementDefinition("lm7-l-prism-horizontal-base", "L PrismHorizontalBase", "L/PrismX/@base", "LM", "L", string.Empty, false, "Left horizontal prism base direction, noch zu validieren."),
                new DeviceMeasurementDefinition("lm7-l-prism-vertical", "L PrismVertical", "L/PrismY", "LM", "L", "prism dpt", false, "Left vertical prism, noch zu validieren."),
                new DeviceMeasurementDefinition("lm7-l-prism-vertical-base", "L PrismVerticalBase", "L/PrismY/@base", "LM", "L", string.Empty, false, "Left vertical prism base direction, noch zu validieren."),
                new DeviceMeasurementDefinition("lm7-pd", "PD", "PD/Distance", "PD", string.Empty, "mm", false, "Lensmeter pupillary distance, noch zu validieren."),
                new DeviceMeasurementDefinition("lm7-common-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, false, "LM7 LAN XML common company field."),
                new DeviceMeasurementDefinition("lm7-common-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, false, "LM7 LAN XML common model name field."),
                new DeviceMeasurementDefinition("lm7-common-machine-no", "MachineNo", "Common/MachineNo", "Common", string.Empty, string.Empty, false, "LM7 LAN XML machine number."),
                new DeviceMeasurementDefinition("lm7-common-rom-version", "ROMVersion", "Common/ROMVersion", "Common", string.Empty, string.Empty, false, "LM7 LAN XML ROM version."),
                new DeviceMeasurementDefinition("lm7-common-version", "Version", "Common/Version", "Common", string.Empty, string.Empty, false, "LM7 LAN XML format version, e.g. NIDEK_V1.00 or NIDEK_V1.01."),
                new DeviceMeasurementDefinition("lm7-common-date", "Date", "Common/Date", "Common", string.Empty, string.Empty, false, "LM7 LAN XML measurement date."),
                new DeviceMeasurementDefinition("lm7-common-time", "Time", "Common/Time", "Common", string.Empty, string.Empty, false, "LM7 LAN XML measurement time."),
                new DeviceMeasurementDefinition("lm7-common-patient-no", "Patient No.", "Common/Patient/No.", "Common", string.Empty, string.Empty, false, "LM7 LAN XML patient number."),
                new DeviceMeasurementDefinition("lm7-common-patient-id", "Patient ID", "Common/Patient/ID", "Common", string.Empty, string.Empty, false, "LM7 LAN XML patient ID."),
                new DeviceMeasurementDefinition("lm7-common-operator-id", "Operator ID", "Common/Operator/ID", "Common", string.Empty, string.Empty, false, "LM7 LAN XML operator ID."),
                new DeviceMeasurementDefinition("lm7-lan-measure-mode", "MeasureMode", "Measure[@Type='LM']/MeasureMode", "LM", string.Empty, string.Empty, false, "LM7 LAN XML measure mode."),
                new DeviceMeasurementDefinition("lm7-lan-diopter-step", "DiopterStep", "Measure[@Type='LM']/DiopterStep", "LM", string.Empty, "dpt", false, "LM7 LAN XML diopter step."),
                new DeviceMeasurementDefinition("lm7-lan-axis-step", "AxisStep", "Measure[@Type='LM']/AxisStep", "LM", string.Empty, "deg", false, "LM7 LAN XML axis step."),
                new DeviceMeasurementDefinition("lm7-lan-cylinder-mode", "CylinderMode", "Measure[@Type='LM']/CylinderMode", "LM", string.Empty, string.Empty, false, "LM7 LAN XML cylinder mode."),
                new DeviceMeasurementDefinition("lm7-lan-prism-diopter-step", "PrismDiopterStep", "Measure[@Type='LM']/PrismDiopterStep", "LM", string.Empty, "prism dpt", false, "LM7 LAN XML prism diopter step."),
                new DeviceMeasurementDefinition("lm7-lan-prism-base-step", "PrismBaseStep", "Measure[@Type='LM']/PrismBaseStep", "LM", string.Empty, "deg", false, "LM7 LAN XML prism base step."),
                new DeviceMeasurementDefinition("lm7-lan-prism-mode", "PrismMode", "Measure[@Type='LM']/PrismMode", "LM", string.Empty, string.Empty, false, "LM7 LAN XML prism mode."),
                new DeviceMeasurementDefinition("lm7-lan-add-mode", "AddMode", "Measure[@Type='LM']/AddMode", "LM", string.Empty, string.Empty, false, "LM7 LAN XML addition mode."),
                new DeviceMeasurementDefinition("lm7-lan-r-sphere", "R Sphere", "Measure[@Type='LM']/LM/R/Sphere", "LM", "R", "dpt", true, "Right lens sphere from LM7 LAN XML according to NIDEK interface manual."),
                new DeviceMeasurementDefinition("lm7-lan-r-cylinder", "R Cylinder", "Measure[@Type='LM']/LM/R/Cylinder", "LM", "R", "dpt", true, "Right lens cylinder from LM7 LAN XML according to NIDEK interface manual."),
                new DeviceMeasurementDefinition("lm7-lan-r-axis", "R Axis", "Measure[@Type='LM']/LM/R/Axis", "LM", "R", "deg", true, "Right lens axis from LM7 LAN XML according to NIDEK interface manual."),
                new DeviceMeasurementDefinition("lm7-lan-r-se", "R SE", "Measure[@Type='LM']/LM/R/SE", "LM", "R", "dpt", false, "Right spherical equivalent from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-r-add", "R ADD", "Measure[@Type='LM']/LM/R/ADD", "LM", "R", "dpt", false, "Right first addition from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-r-add2", "R ADD2", "Measure[@Type='LM']/LM/R/ADD2", "LM", "R", "dpt", false, "Right second addition from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-r-near-sphere", "R NearSphere", "Measure[@Type='LM']/LM/R/NearSphere", "LM", "R", "dpt", false, "Right first near sphere from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-r-near-sphere2", "R NearSphere2", "Measure[@Type='LM']/LM/R/NearSphere2", "LM", "R", "dpt", false, "Right second near sphere from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-r-prism", "R Prism", "Measure[@Type='LM']/LM/R/Prism", "LM", "R", "prism dpt", false, "Right polar prism from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-r-prism-base", "R PrismBase", "Measure[@Type='LM']/LM/R/PrismBase", "LM", "R", "deg", false, "Right polar prism base from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-r-prism-x", "R PrismX", "Measure[@Type='LM']/LM/R/PrismX", "LM", "R", "prism dpt", false, "Right horizontal prism from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-r-prism-x-base", "R PrismX base", "Measure[@Type='LM']/LM/R/PrismX/@base", "LM", "R", string.Empty, false, "Right horizontal prism base direction from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-r-prism-y", "R PrismY", "Measure[@Type='LM']/LM/R/PrismY", "LM", "R", "prism dpt", false, "Right vertical prism from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-r-prism-y-base", "R PrismY base", "Measure[@Type='LM']/LM/R/PrismY/@base", "LM", "R", string.Empty, false, "Right vertical prism base direction from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-r-uv-transmittance", "R UVTransmittance", "Measure[@Type='LM']/LM/R/UVTransmittance", "LM", "R", "%", false, "Right UV transmittance from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-r-confidence-index", "R ConfidenceIndex", "Measure[@Type='LM']/LM/R/ConfidenceIndex", "LM", "R", string.Empty, false, "Right confidence index from LM7 LAN XML NIDEK_V1.01."),
                new DeviceMeasurementDefinition("lm7-lan-r-error", "R Error", "Measure[@Type='LM']/LM/R/Error", "LM", "R", string.Empty, false, "Right error information from LM7 LAN XML NIDEK_V1.01."),
                new DeviceMeasurementDefinition("lm7-lan-l-sphere", "L Sphere", "Measure[@Type='LM']/LM/L/Sphere", "LM", "L", "dpt", true, "Left lens sphere from LM7 LAN XML according to NIDEK interface manual."),
                new DeviceMeasurementDefinition("lm7-lan-l-cylinder", "L Cylinder", "Measure[@Type='LM']/LM/L/Cylinder", "LM", "L", "dpt", true, "Left lens cylinder from LM7 LAN XML according to NIDEK interface manual."),
                new DeviceMeasurementDefinition("lm7-lan-l-axis", "L Axis", "Measure[@Type='LM']/LM/L/Axis", "LM", "L", "deg", true, "Left lens axis from LM7 LAN XML according to NIDEK interface manual."),
                new DeviceMeasurementDefinition("lm7-lan-l-se", "L SE", "Measure[@Type='LM']/LM/L/SE", "LM", "L", "dpt", false, "Left spherical equivalent from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-l-add", "L ADD", "Measure[@Type='LM']/LM/L/ADD", "LM", "L", "dpt", false, "Left first addition from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-l-add2", "L ADD2", "Measure[@Type='LM']/LM/L/ADD2", "LM", "L", "dpt", false, "Left second addition from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-l-near-sphere", "L NearSphere", "Measure[@Type='LM']/LM/L/NearSphere", "LM", "L", "dpt", false, "Left first near sphere from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-l-near-sphere2", "L NearSphere2", "Measure[@Type='LM']/LM/L/NearSphere2", "LM", "L", "dpt", false, "Left second near sphere from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-l-prism", "L Prism", "Measure[@Type='LM']/LM/L/Prism", "LM", "L", "prism dpt", false, "Left polar prism from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-l-prism-base", "L PrismBase", "Measure[@Type='LM']/LM/L/PrismBase", "LM", "L", "deg", false, "Left polar prism base from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-l-prism-x", "L PrismX", "Measure[@Type='LM']/LM/L/PrismX", "LM", "L", "prism dpt", false, "Left horizontal prism from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-l-prism-x-base", "L PrismX base", "Measure[@Type='LM']/LM/L/PrismX/@base", "LM", "L", string.Empty, false, "Left horizontal prism base direction from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-l-prism-y", "L PrismY", "Measure[@Type='LM']/LM/L/PrismY", "LM", "L", "prism dpt", false, "Left vertical prism from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-l-prism-y-base", "L PrismY base", "Measure[@Type='LM']/LM/L/PrismY/@base", "LM", "L", string.Empty, false, "Left vertical prism base direction from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-l-uv-transmittance", "L UVTransmittance", "Measure[@Type='LM']/LM/L/UVTransmittance", "LM", "L", "%", false, "Left UV transmittance from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-l-confidence-index", "L ConfidenceIndex", "Measure[@Type='LM']/LM/L/ConfidenceIndex", "LM", "L", string.Empty, false, "Left confidence index from LM7 LAN XML NIDEK_V1.01."),
                new DeviceMeasurementDefinition("lm7-lan-l-error", "L Error", "Measure[@Type='LM']/LM/L/Error", "LM", "L", string.Empty, false, "Left error information from LM7 LAN XML NIDEK_V1.01."),
                new DeviceMeasurementDefinition("lm7-lan-s-sphere", "S Sphere", "Measure[@Type='LM']/LM/S/Sphere", "LM", string.Empty, "dpt", false, "Single state sphere from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-s-cylinder", "S Cylinder", "Measure[@Type='LM']/LM/S/Cylinder", "LM", string.Empty, "dpt", false, "Single state cylinder from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-s-axis", "S Axis", "Measure[@Type='LM']/LM/S/Axis", "LM", string.Empty, "deg", false, "Single state axis from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-s-se", "S SE", "Measure[@Type='LM']/LM/S/SE", "LM", string.Empty, "dpt", false, "Single state spherical equivalent from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-s-add", "S ADD", "Measure[@Type='LM']/LM/S/ADD", "LM", string.Empty, "dpt", false, "Single state first addition from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-s-add2", "S ADD2", "Measure[@Type='LM']/LM/S/ADD2", "LM", string.Empty, "dpt", false, "Single state second addition from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-s-near-sphere", "S NearSphere", "Measure[@Type='LM']/LM/S/NearSphere", "LM", string.Empty, "dpt", false, "Single state first near sphere from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-s-near-sphere2", "S NearSphere2", "Measure[@Type='LM']/LM/S/NearSphere2", "LM", string.Empty, "dpt", false, "Single state second near sphere from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-s-prism", "S Prism", "Measure[@Type='LM']/LM/S/Prism", "LM", string.Empty, "prism dpt", false, "Single state polar prism from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-s-prism-base", "S PrismBase", "Measure[@Type='LM']/LM/S/PrismBase", "LM", string.Empty, "deg", false, "Single state polar prism base from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-s-prism-x", "S PrismX", "Measure[@Type='LM']/LM/S/PrismX", "LM", string.Empty, "prism dpt", false, "Single state horizontal prism from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-s-prism-x-base", "S PrismX base", "Measure[@Type='LM']/LM/S/PrismX/@base", "LM", string.Empty, string.Empty, false, "Single state horizontal prism base direction from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-s-prism-y", "S PrismY", "Measure[@Type='LM']/LM/S/PrismY", "LM", string.Empty, "prism dpt", false, "Single state vertical prism from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-s-prism-y-base", "S PrismY base", "Measure[@Type='LM']/LM/S/PrismY/@base", "LM", string.Empty, string.Empty, false, "Single state vertical prism base direction from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-s-uv-transmittance", "S UVTransmittance", "Measure[@Type='LM']/LM/S/UVTransmittance", "LM", string.Empty, "%", false, "Single state UV transmittance from LM7 LAN XML."),
                new DeviceMeasurementDefinition("lm7-lan-s-confidence-index", "S ConfidenceIndex", "Measure[@Type='LM']/LM/S/ConfidenceIndex", "LM", string.Empty, string.Empty, false, "Single state confidence index from LM7 LAN XML NIDEK_V1.01."),
                new DeviceMeasurementDefinition("lm7-lan-s-error", "S Error", "Measure[@Type='LM']/LM/S/Error", "LM", string.Empty, string.Empty, false, "Single state error information from LM7 LAN XML NIDEK_V1.01."),
                new DeviceMeasurementDefinition("lm7-lan-pd-distance", "PD Distance", "Measure[@Type='LM']/PD/Distance", "PD", string.Empty, "mm", false, "LM7 LAN XML far pupillary distance."),
                new DeviceMeasurementDefinition("lm7-lan-pd-distance-r", "PD DistanceR", "Measure[@Type='LM']/PD/DistanceR", "PD", "R", "mm", false, "LM7 LAN XML right far pupillary distance."),
                new DeviceMeasurementDefinition("lm7-lan-pd-distance-l", "PD DistanceL", "Measure[@Type='LM']/PD/DistanceL", "PD", "L", "mm", false, "LM7 LAN XML left far pupillary distance."),
                new DeviceMeasurementDefinition("lm7-lan-pd-near", "PD Near", "Measure[@Type='LM']/PD/Near", "PD", string.Empty, "mm", false, "LM7 LAN XML near pupillary distance."),
                new DeviceMeasurementDefinition("lm7-lan-pd-near-r", "PD NearR", "Measure[@Type='LM']/PD/NearR", "PD", "R", "mm", false, "LM7 LAN XML right near pupillary distance."),
                new DeviceMeasurementDefinition("lm7-lan-pd-near-l", "PD NearL", "Measure[@Type='LM']/PD/NearL", "PD", "L", "mm", false, "LM7 LAN XML left near pupillary distance."),
                new DeviceMeasurementDefinition("lm7-medistar-r-line", "R MEDISTAR Lensmeter-Zeile", "Measure[@Type='LM']/LM/R/MedistarLine", "LM", "R", string.Empty, false, "Computed MEDISTAR lensmeter line for right lens; optional values are omitted when absent."),
                new DeviceMeasurementDefinition("lm7-medistar-l-line", "L MEDISTAR Lensmeter-Zeile", "Measure[@Type='LM']/LM/L/MedistarLine", "LM", "L", string.Empty, false, "Computed MEDISTAR lensmeter line for left lens; optional values are omitted when absent.")
            },
            SupportedExaminationTypes: new[] { "Lensmeter", "PD", "Prism" },
            CanContainMultipleExaminationTypes: false,
            DeviceImagePath: InterfaceProfileUiPolicy.NidekLm7DeviceImagePath);
    }

    public static DeviceProfileDefinition CreateNidekNt530PDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 18, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-nidek-nt530p-default",
                Name: "NIDEK NT530P",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for NIDEK NT-530P tonometry and pachymetry XML measurement files.",
                Vendor: "NIDEK",
                Product: "NT-530P",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "NIDEK",
            Model: "NT-530P",
            DeviceType: "Tonometer/Pachymeter",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("nt530p-company", "Company", "Company", "Metadata", string.Empty, string.Empty, true, "NIDEK NT-530P XML company field."),
                new DeviceMeasurementDefinition("nt530p-model-name", "ModelName", "ModelName", "Metadata", string.Empty, string.Empty, true, "NIDEK NT-530P XML model name field."),
                new DeviceMeasurementDefinition("nt530p-rom-version", "ROMVersion", "ROMVersion", "Metadata", string.Empty, string.Empty, false, "NIDEK NT-530P ROM version."),
                new DeviceMeasurementDefinition("nt530p-version", "Version", "Version", "Metadata", string.Empty, string.Empty, false, "NIDEK NT-530P XML format version."),
                new DeviceMeasurementDefinition("nt530p-measurement-date", "MeasurementDate", "Date", "Metadata", string.Empty, string.Empty, true, "Measurement date from NT-530P XML."),
                new DeviceMeasurementDefinition("nt530p-measurement-time", "MeasurementTime", "Time", "Metadata", string.Empty, string.Empty, true, "Measurement time from NT-530P XML."),
                new DeviceMeasurementDefinition("nt530p-patient-no", "Patient No.", "Patient/No.", "Metadata", string.Empty, string.Empty, false, "Patient number stored by the NT-530P XML file; not exported to MEDISTAR."),
                new DeviceMeasurementDefinition("nt530p-patient-id", "Patient ID", "Patient/ID", "Metadata", string.Empty, string.Empty, false, "Patient ID stored by the NT-530P XML file; not exported to MEDISTAR."),
                new DeviceMeasurementDefinition("nt530p-comment", "Comment", "Comment", "Metadata", string.Empty, string.Empty, false, "Optional NT-530P comment field."),
                new DeviceMeasurementDefinition("nt530p-r-iop-1", "R IOP 1", "R/NT/NTList[@No='1']/mmHg", "NT", "R", "mmHg", true, "Right eye tonometry value 1 from NT-530P XML."),
                new DeviceMeasurementDefinition("nt530p-r-iop-2", "R IOP 2", "R/NT/NTList[@No='2']/mmHg", "NT", "R", "mmHg", false, "Right eye tonometry value 2 from NT-530P XML."),
                new DeviceMeasurementDefinition("nt530p-r-iop-average", "R IOP Average", "R/NT/NTAverage/mmHg", "NT", "R", "mmHg", true, "Right eye tonometry average from NT-530P XML."),
                new DeviceMeasurementDefinition("nt530p-l-iop-1", "L IOP 1", "L/NT/NTList[@No='1']/mmHg", "NT", "L", "mmHg", true, "Left eye tonometry value 1 from NT-530P XML."),
                new DeviceMeasurementDefinition("nt530p-l-iop-2", "L IOP 2", "L/NT/NTList[@No='2']/mmHg", "NT", "L", "mmHg", false, "Left eye tonometry value 2 from NT-530P XML."),
                new DeviceMeasurementDefinition("nt530p-l-iop-average", "L IOP Average", "L/NT/NTAverage/mmHg", "NT", "L", "mmHg", true, "Left eye tonometry average from NT-530P XML."),
                new DeviceMeasurementDefinition("nt530p-r-corrected-iop-measured", "R CorrectedIOP Measured", "R/NT/CorrectedIOP/Measured/mmHg", "CorrectedIOP", "R", "mmHg", false, "Right measured IOP for corrected IOP block."),
                new DeviceMeasurementDefinition("nt530p-r-corrected-iop-corrected", "R CorrectedIOP Corrected", "R/NT/CorrectedIOP/Corrected/mmHg", "CorrectedIOP", "R", "mmHg", false, "Right corrected IOP value."),
                new DeviceMeasurementDefinition("nt530p-r-corrected-iop-param1", "R CorrectedIOP Param1", "R/NT/CorrectedIOP/Param1", "CorrectedIOP", "R", string.Empty, false, "Right Param1 value used for corrected IOP."),
                new DeviceMeasurementDefinition("nt530p-r-corrected-iop-param2", "R CorrectedIOP Param2", "R/NT/CorrectedIOP/Param2", "CorrectedIOP", "R", string.Empty, false, "Right Param2 value used for corrected IOP."),
                new DeviceMeasurementDefinition("nt530p-r-corrected-iop-cct", "R CorrectedIOP CCT", "R/NT/CorrectedIOP/CCT", "CorrectedIOP", "R", "um", false, "Right CCT value used for corrected IOP."),
                new DeviceMeasurementDefinition("nt530p-l-corrected-iop-measured", "L CorrectedIOP Measured", "L/NT/CorrectedIOP/Measured/mmHg", "CorrectedIOP", "L", "mmHg", false, "Left measured IOP for corrected IOP block."),
                new DeviceMeasurementDefinition("nt530p-l-corrected-iop-corrected", "L CorrectedIOP Corrected", "L/NT/CorrectedIOP/Corrected/mmHg", "CorrectedIOP", "L", "mmHg", false, "Left corrected IOP value."),
                new DeviceMeasurementDefinition("nt530p-l-corrected-iop-param1", "L CorrectedIOP Param1", "L/NT/CorrectedIOP/Param1", "CorrectedIOP", "L", string.Empty, false, "Left Param1 value used for corrected IOP."),
                new DeviceMeasurementDefinition("nt530p-l-corrected-iop-param2", "L CorrectedIOP Param2", "L/NT/CorrectedIOP/Param2", "CorrectedIOP", "L", string.Empty, false, "Left Param2 value used for corrected IOP."),
                new DeviceMeasurementDefinition("nt530p-l-corrected-iop-cct", "L CorrectedIOP CCT", "L/NT/CorrectedIOP/CCT", "CorrectedIOP", "L", "um", false, "Left CCT value used for corrected IOP."),
                new DeviceMeasurementDefinition("nt530p-r-pachy-1", "R Pachy 1", "R/PACHY/PACHYList[@No='1']/Thickness", "PACHY", "R", "um", true, "Right pachymetry value 1 from NT-530P XML."),
                new DeviceMeasurementDefinition("nt530p-r-pachy-2", "R Pachy 2", "R/PACHY/PACHYList[@No='2']/Thickness", "PACHY", "R", "um", false, "Right pachymetry value 2 from NT-530P XML."),
                new DeviceMeasurementDefinition("nt530p-r-pachy-average", "R Pachy Average", "R/PACHY/PACHYAverage/Thickness", "PACHY", "R", "um", true, "Right pachymetry average from NT-530P XML."),
                new DeviceMeasurementDefinition("nt530p-l-pachy-1", "L Pachy 1", "L/PACHY/PACHYList[@No='1']/Thickness", "PACHY", "L", "um", true, "Left pachymetry value 1 from NT-530P XML."),
                new DeviceMeasurementDefinition("nt530p-l-pachy-2", "L Pachy 2", "L/PACHY/PACHYList[@No='2']/Thickness", "PACHY", "L", "um", false, "Left pachymetry value 2 from NT-530P XML."),
                new DeviceMeasurementDefinition("nt530p-l-pachy-average", "L Pachy Average", "L/PACHY/PACHYAverage/Thickness", "PACHY", "L", "um", true, "Left pachymetry average from NT-530P XML."),
                new DeviceMeasurementDefinition("nt530p-r-pachy-image", "R PACHYImage", "R/PACHY/PACHYImage", "Attachment", "R", string.Empty, false, "Right pachymetry image reference for future attachment handling; optional because JPG files can be missing."),
                new DeviceMeasurementDefinition("nt530p-l-pachy-image", "L PACHYImage", "L/PACHY/PACHYImage", "Attachment", "L", string.Empty, false, "Left pachymetry image reference for future attachment handling; optional because JPG files can be missing."),
                new DeviceMeasurementDefinition("nt530p-pachy-header-line", "MEDISTAR Pachymetrie-Überschrift", "Measure[@Type='NT530P']/Pachy/HeaderLine", "PACHY", string.Empty, string.Empty, true, "Computed MEDISTAR pachymetry heading for field 6220."),
                new DeviceMeasurementDefinition("nt530p-pachy-medistar-line", "MEDISTAR Pachymetrie-Zeile", "Measure[@Type='NT530P']/Pachy/MedistarLine", "PACHY", string.Empty, string.Empty, true, "Computed MEDISTAR pachymetry line for field 6220."),
                new DeviceMeasurementDefinition("nt530p-tono-header-line", "MEDISTAR Tonometrie-Überschrift", "Measure[@Type='NT530P']/Tono/HeaderLine", "NT", string.Empty, string.Empty, true, "Computed MEDISTAR tonometry heading for field 6205."),
                new DeviceMeasurementDefinition("nt530p-tono-pachy-right-line", "MEDISTAR Tonometrie Pachymetrie rechts", "Measure[@Type='NT530P']/Tono/PachyRightLine", "NT", "R", string.Empty, true, "Computed MEDISTAR tonometry pachymetry line for the right eye."),
                new DeviceMeasurementDefinition("nt530p-tono-pachy-left-line", "MEDISTAR Tonometrie Pachymetrie links", "Measure[@Type='NT530P']/Tono/PachyLeftLine", "NT", "L", string.Empty, true, "Computed MEDISTAR tonometry pachymetry line for the left eye."),
                new DeviceMeasurementDefinition("nt530p-tono-measured-right-line", "MEDISTAR Tonometrie Messung rechts", "Measure[@Type='NT530P']/Tono/MeasuredRightLine", "NT", "R", string.Empty, true, "Computed MEDISTAR tonometry measured IOP line for the right eye."),
                new DeviceMeasurementDefinition("nt530p-tono-corrected-right-line", "MEDISTAR Tonometrie Korrektur rechts", "Measure[@Type='NT530P']/Tono/CorrectedRightLine", "NT", "R", string.Empty, true, "Computed MEDISTAR tonometry corrected IOP and parameter line for the right eye."),
                new DeviceMeasurementDefinition("nt530p-tono-right-cct-left-measured-line", "MEDISTAR Tonometrie CCT rechts und Messung links", "Measure[@Type='NT530P']/Tono/RightCctLeftMeasuredLine", "NT", string.Empty, string.Empty, true, "Computed MEDISTAR tonometry line for right CCT and left measured/corrected IOP."),
                new DeviceMeasurementDefinition("nt530p-tono-parameter-left-line", "MEDISTAR Tonometrie Parameter links", "Measure[@Type='NT530P']/Tono/ParameterLeftLine", "NT", "L", string.Empty, true, "Computed MEDISTAR tonometry parameter line for the left eye."),
                new DeviceMeasurementDefinition("nt530p-tono-list-line", "MEDISTAR Tonometrie Einzelwerte", "Measure[@Type='NT530P']/Tono/TonoListLine", "NT", string.Empty, string.Empty, true, "Computed MEDISTAR tonometry single values, averages and measurement time."),
                new DeviceMeasurementDefinition("nt530p-tono-medistar-line", "MEDISTAR Tonometrie-Zeile", "Measure[@Type='NT530P']/Tono/MedistarLine", "NT", string.Empty, string.Empty, false, "Computed compact MEDISTAR tonometry line; default export uses the split 6205 lines.")
            },
            SupportedExaminationTypes: new[] { "Tonometrie", "Pachymetrie", "CorrectedIOP", "Attachment" },
            CanContainMultipleExaminationTypes: true,
            DeviceImagePath: InterfaceProfileUiPolicy.NidekNt530PDeviceImagePath);
    }

    public static DeviceProfileDefinition CreateNidekNt1Default()
    {
        return CreateNidekNt530PVariantDefault(
            id: "device-nidek-nt1-default",
            name: "NIDEK NT-1",
            model: "NT-1",
            description: "Built-in NIDEK NT-1 XML tonometry profile derived from the reference package OPTO06 paths.");
    }

    public static DeviceProfileDefinition CreateNidekNt1EDefault()
    {
        return CreateNidekNt530PVariantDefault(
            id: "device-nidek-nt1e-default",
            name: "NIDEK NT-1E",
            model: "NT-1E",
            description: "Built-in NIDEK NT-1E XML tonometry profile derived from the reference package OPTO06 paths.");
    }

    public static DeviceProfileDefinition CreateNidekNt1PDefault()
    {
        return CreateNidekNt530PVariantDefault(
            id: "device-nidek-nt1p-default",
            name: "NIDEK NT-1P",
            model: "NT-1P",
            description: "Built-in NIDEK NT-1P XML tonometry and pachymetry profile derived from the reference package OPTO06 paths.");
    }

    public static DeviceProfileDefinition CreateNidekNt510Default()
    {
        return CreateNidekNt530PVariantDefault(
            id: "device-nidek-nt510-default",
            name: "NIDEK NT-510",
            model: "NT-510",
            description: "Built-in NIDEK NT-510 XML tonometry and pachymetry profile derived from the reference package OPTO06 paths.");
    }

    public static DeviceProfileDefinition CreateNidekNt530Default()
    {
        return CreateNidekNt530PVariantDefault(
            id: "device-nidek-nt530-default",
            name: "NIDEK NT-530",
            model: "NT-530",
            description: "Built-in NIDEK NT-530 XML tonometry and pachymetry profile derived from the reference package OPTO06 paths.");
    }

    private static DeviceProfileDefinition CreateNidekNt530PVariantDefault(
        string id,
        string name,
        string model,
        string description)
    {
        var profile = CreateNidekNt530PDefault();
        var timestamp = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

        return profile with
        {
            Metadata = new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.DeviceProfile,
                Description: description,
                Vendor: "NIDEK",
                Product: model,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Model = model,
            DeviceImagePath = InterfaceProfileUiPolicy.NidekNt530PDeviceImagePath
        };
    }

    public static DeviceProfileDefinition CreateNidekRt6100Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-nidek-rt6100-default",
                Name: "NIDEK RT-6100",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for bidirectional NIDEK RT-6100 LAN/MEM-200 phoropter XML workflows. LM_Base and REF_Base can be written to the device; returned Best values are exported with 6228 and Full values with 6227.",
                Vendor: "NIDEK",
                Product: "RT-6100",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "NIDEK",
            Model: "RT-6100",
            DeviceType: "Phoropter",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("rt6100-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "NIDEK RT-6100 common company field."),
                new DeviceMeasurementDefinition("rt6100-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "NIDEK RT-6100 common model field."),
                new DeviceMeasurementDefinition("rt6100-version", "Version", "Common/Version", "Common", string.Empty, string.Empty, true, "NIDEK RT-6100 XML format version."),
                new DeviceMeasurementDefinition("rt6100-date", "Date", "Common/Date", "Common", string.Empty, string.Empty, false, "NIDEK RT-6100 measurement date."),
                new DeviceMeasurementDefinition("rt6100-time", "Time", "Common/Time", "Common", string.Empty, string.Empty, false, "NIDEK RT-6100 measurement time."),
                new DeviceMeasurementDefinition("rt6100-patient-no", "Patient No", "Common/Patient/No", "Common", string.Empty, string.Empty, false, "Patient number stored by the RT-6100 XML file; not exported to MEDISTAR."),
                new DeviceMeasurementDefinition("rt6100-patient-id", "Patient ID", "Common/Patient/ID", "Common", string.Empty, string.Empty, false, "Patient ID stored by the RT-6100 XML file; not exported to MEDISTAR."),
                new DeviceMeasurementDefinition("rt6100-corrected-type", "Corrected CorrectionType", "Measure[@Type='RT']/Phoropter/Corrected/@CorrectionType", "RT", string.Empty, string.Empty, false, "RT-6100 corrected block type such as LM_Base, REF_Base, Full or Best."),
                new DeviceMeasurementDefinition("rt6100-best-header", "Best MEDISTAR-Header", "Measure[@Type='RT']/Best/HeaderLine", "RT", string.Empty, string.Empty, false, "Computed MEDISTAR 6228 header for NIDEK RT-6100 Best / final prescription values."),
                new DeviceMeasurementDefinition("rt6100-best-r-line", "Best R MEDISTAR-Zeile", "Measure[@Type='RT']/Best/R/MedistarLine", "RT", "R", string.Empty, false, "Computed MEDISTAR 6228 right-eye line for NIDEK RT-6100 Best."),
                new DeviceMeasurementDefinition("rt6100-best-l-line", "Best L MEDISTAR-Zeile", "Measure[@Type='RT']/Best/L/MedistarLine", "RT", "L", string.Empty, false, "Computed MEDISTAR 6228 left-eye line for NIDEK RT-6100 Best."),
                new DeviceMeasurementDefinition("rt6100-full-header", "Full MEDISTAR-Header", "Measure[@Type='RT']/Full/HeaderLine", "RT", string.Empty, string.Empty, false, "Computed MEDISTAR 6227 header for NIDEK RT-6100 Full / full correction values."),
                new DeviceMeasurementDefinition("rt6100-full-r-line", "Full R MEDISTAR-Zeile", "Measure[@Type='RT']/Full/R/MedistarLine", "RT", "R", string.Empty, false, "Computed MEDISTAR 6227 right-eye line for NIDEK RT-6100 Full."),
                new DeviceMeasurementDefinition("rt6100-full-l-line", "Full L MEDISTAR-Zeile", "Measure[@Type='RT']/Full/L/MedistarLine", "RT", "L", string.Empty, false, "Computed MEDISTAR 6227 left-eye line for NIDEK RT-6100 Full.")
            },
            SupportedExaminationTypes: new[] { "RT", "Phoropter", "Refraktion", "Best", "Full", "LM_Base", "REF_Base" },
            CanContainMultipleExaminationTypes: true,
            IsBidirectional: true,
            DeviceImagePath: InterfaceProfileUiPolicy.NidekRt6100DeviceImagePath,
            ConnectionKind: DeviceConnectionKind.NetworkLan);
    }

    public static DeviceProfileDefinition CreateTopconCl300Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-topcon-cl300-default",
                Name: "TOPCON CL300",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for TOPCON CL-300 Ophthalmology XML lensmeter files with nsCommon/nsLM namespace handling.",
                Vendor: "TOPCON",
                Product: "CL-300",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "TOPCON",
            Model: "CL-300",
            DeviceType: "Lensmeter",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("cl300-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "TOPCON common company field."),
                new DeviceMeasurementDefinition("cl300-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "TOPCON CL-300 common model field."),
                new DeviceMeasurementDefinition("cl300-machine-no", "MachineNo", "Common/MachineNo", "Common", string.Empty, string.Empty, false, "TOPCON CL-300 machine number."),
                new DeviceMeasurementDefinition("cl300-rom-version", "ROMVersion", "Common/ROMVersion", "Common", string.Empty, string.Empty, false, "TOPCON CL-300 ROM version."),
                new DeviceMeasurementDefinition("cl300-version", "Version", "Common/Version", "Common", string.Empty, string.Empty, false, "TOPCON CL-300 XML version."),
                new DeviceMeasurementDefinition("cl300-date", "Date", "Common/Date", "Common", string.Empty, string.Empty, false, "TOPCON CL-300 measurement date."),
                new DeviceMeasurementDefinition("cl300-time", "Time", "Common/Time", "Common", string.Empty, string.Empty, false, "TOPCON CL-300 measurement time."),
                new DeviceMeasurementDefinition("cl300-patient-no", "Patient No.", "Common/Patient/No.", "Common", string.Empty, string.Empty, false, "TOPCON CL-300 patient number."),
                new DeviceMeasurementDefinition("cl300-patient-id", "Patient ID", "Common/Patient/ID", "Common", string.Empty, string.Empty, false, "TOPCON CL-300 patient ID."),
                new DeviceMeasurementDefinition("cl300-diopter-step", "DiopterStep", "Measure[@Type='LM']/DiopterStep", "LM", string.Empty, "dpt", false, "Diopter step from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-axis-step", "AxisStep", "Measure[@Type='LM']/AxisStep", "LM", string.Empty, "deg", false, "Axis step from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-prism-step", "PrismStep", "Measure[@Type='LM']/PrismStep", "LM", string.Empty, "prism dpt", false, "Prism step from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-cylinder-mode", "CylinderMode", "Measure[@Type='LM']/CylinderMode", "LM", string.Empty, string.Empty, false, "Cylinder mode from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-lens-type", "LensType", "Measure[@Type='LM']/LensType", "LM", string.Empty, string.Empty, false, "Lens type from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-abbe-number", "AbbeNumber", "Measure[@Type='LM']/AbbeNumber", "LM", string.Empty, string.Empty, false, "Abbe number from TOPCON CL-300 XML when present."),
                new DeviceMeasurementDefinition("cl300-wavelength", "Wavelength", "Measure[@Type='LM']/Wavelength", "LM", string.Empty, string.Empty, false, "Wavelength from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-r-sphere", "R Sphere", "Measure[@Type='LM']/LM/R/Sphere", "LM", "R", "dpt", true, "Right lens sphere from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-r-cylinder", "R Cylinder", "Measure[@Type='LM']/LM/R/Cylinder", "LM", "R", "dpt", true, "Right lens cylinder from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-r-axis", "R Axis", "Measure[@Type='LM']/LM/R/Axis", "LM", "R", "deg", true, "Right lens axis from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-r-add1", "R Add1", "Measure[@Type='LM']/LM/R/Add1", "LM", "R", "dpt", false, "Right first addition from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-r-add2", "R Add2", "Measure[@Type='LM']/LM/R/Add2", "LM", "R", "dpt", false, "Right second addition from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-r-prism-horizontal", "R PrismHorizontal", "Measure[@Type='LM']/LM/R/H", "LM", "R", "prism dpt", false, "Right horizontal prism from TOPCON CL-300 XML; basis direction remains open."),
                new DeviceMeasurementDefinition("cl300-r-prism-vertical", "R PrismVertical", "Measure[@Type='LM']/LM/R/V", "LM", "R", "prism dpt", false, "Right vertical prism from TOPCON CL-300 XML; basis direction remains open."),
                new DeviceMeasurementDefinition("cl300-l-sphere", "L Sphere", "Measure[@Type='LM']/LM/L/Sphere", "LM", "L", "dpt", true, "Left lens sphere from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-l-cylinder", "L Cylinder", "Measure[@Type='LM']/LM/L/Cylinder", "LM", "L", "dpt", true, "Left lens cylinder from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-l-axis", "L Axis", "Measure[@Type='LM']/LM/L/Axis", "LM", "L", "deg", true, "Left lens axis from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-l-add1", "L Add1", "Measure[@Type='LM']/LM/L/Add1", "LM", "L", "dpt", false, "Left first addition from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-l-add2", "L Add2", "Measure[@Type='LM']/LM/L/Add2", "LM", "L", "dpt", false, "Left second addition from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-l-prism-horizontal", "L PrismHorizontal", "Measure[@Type='LM']/LM/L/H", "LM", "L", "prism dpt", false, "Left horizontal prism from TOPCON CL-300 XML; basis direction remains open."),
                new DeviceMeasurementDefinition("cl300-l-prism-vertical", "L PrismVertical", "Measure[@Type='LM']/LM/L/V", "LM", "L", "prism dpt", false, "Left vertical prism from TOPCON CL-300 XML; basis direction remains open."),
                new DeviceMeasurementDefinition("cl300-pd-distance", "PD Distance", "Measure[@Type='LM']/PD/B/Distance", "PD", string.Empty, "mm", false, "Binocular PD distance from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-pd-r-distance", "R PD Distance", "Measure[@Type='LM']/PD/R/Distance", "PD", "R", "mm", false, "Right PD distance from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-pd-l-distance", "L PD Distance", "Measure[@Type='LM']/PD/L/Distance", "PD", "L", "mm", false, "Left PD distance from TOPCON CL-300 XML."),
                new DeviceMeasurementDefinition("cl300-medistar-r-line", "R MEDISTAR Lensmeter-Zeile", "Measure[@Type='LM']/LM/R/MedistarLine", "LM", "R", string.Empty, false, "Computed MEDISTAR lensmeter line for TOPCON CL-300 right lens; optional values are omitted when absent."),
                new DeviceMeasurementDefinition("cl300-medistar-l-line", "L MEDISTAR Lensmeter-Zeile", "Measure[@Type='LM']/LM/L/MedistarLine", "LM", "L", string.Empty, false, "Computed MEDISTAR lensmeter line for TOPCON CL-300 left lens; optional values are omitted when absent.")
            },
            SupportedExaminationTypes: new[] { "Lensmeter", "PD", "Prism" },
            CanContainMultipleExaminationTypes: false,
            DeviceImagePath: InterfaceProfileUiPolicy.TopconCl300DeviceImagePath);
    }

    public static DeviceProfileDefinition CreateTopconCl300PdlDefault()
    {
        var timestamp = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);
        var profile = CreateTopconCl300Default();

        return profile with
        {
            Metadata = new ProfileMetadata(
                Id: "device-topcon-cl300pdl-default",
                Name: "TOPCON CL-300PDL",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for TOPCON CL-300PDL Ophthalmology XML lensmeter files. The reference data confirms the same LM/PD XML structure as the CL-300 family.",
                Vendor: "TOPCON",
                Product: "CL-300PDL",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Model = "CL-300PDL",
            DeviceType = "Lensmeter",
            DeviceImagePath = InterfaceProfileUiPolicy.TopconCl300DeviceImagePath
        };
    }

    public static DeviceProfileDefinition CreateTopconSolosDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 24, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-topcon-solos-default",
                Name: "TOPCON Solos",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for TOPCON SOLOS Ophthalmology XML lensmeter files with nsCommon/nsLM namespace handling. Transmission values are read as optional measurements but not exported to MEDISTAR yet.",
                Vendor: "TOPCON",
                Product: "SOLOS",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "TOPCON",
            Model: "SOLOS",
            DeviceType: "Lensmeter",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("solos-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "TOPCON common company field."),
                new DeviceMeasurementDefinition("solos-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "TOPCON SOLOS common model field."),
                new DeviceMeasurementDefinition("solos-machine-no", "MachineNo", "Common/MachineNo", "Common", string.Empty, string.Empty, false, "TOPCON SOLOS machine number."),
                new DeviceMeasurementDefinition("solos-rom-version", "ROMVersion", "Common/ROMVersion", "Common", string.Empty, string.Empty, false, "TOPCON SOLOS ROM version."),
                new DeviceMeasurementDefinition("solos-version", "Version", "Common/Version", "Common", string.Empty, string.Empty, false, "TOPCON SOLOS XML version."),
                new DeviceMeasurementDefinition("solos-date", "Date", "Common/Date", "Common", string.Empty, string.Empty, false, "TOPCON SOLOS measurement date."),
                new DeviceMeasurementDefinition("solos-time", "Time", "Common/Time", "Common", string.Empty, string.Empty, false, "TOPCON SOLOS measurement time."),
                new DeviceMeasurementDefinition("solos-patient-no", "Patient No.", "Common/Patient/No.", "Common", string.Empty, string.Empty, false, "TOPCON SOLOS patient number."),
                new DeviceMeasurementDefinition("solos-patient-id", "Patient ID", "Common/Patient/ID", "Common", string.Empty, string.Empty, false, "TOPCON SOLOS patient ID."),
                new DeviceMeasurementDefinition("solos-measure-mode", "MeasureMode", "Measure[@Type='LM']/MeasureMode", "LM", string.Empty, string.Empty, false, "Optional SOLOS measure mode from LM schema."),
                new DeviceMeasurementDefinition("solos-diopter-step", "DiopterStep", "Measure[@Type='LM']/DiopterStep", "LM", string.Empty, "dpt", false, "Diopter step from TOPCON SOLOS XML."),
                new DeviceMeasurementDefinition("solos-axis-step", "AxisStep", "Measure[@Type='LM']/AxisStep", "LM", string.Empty, "deg", false, "Axis step from TOPCON SOLOS XML."),
                new DeviceMeasurementDefinition("solos-prism-step", "PrismStep", "Measure[@Type='LM']/PrismStep", "LM", string.Empty, "prism dpt", false, "Prism step from TOPCON SOLOS XML."),
                new DeviceMeasurementDefinition("solos-prism-diopter-step", "PrismDiopterStep", "Measure[@Type='LM']/PrismDiopterStep", "LM", string.Empty, "prism dpt", false, "Optional SOLOS prism diopter step from LM schema."),
                new DeviceMeasurementDefinition("solos-prism-base-step", "PrismBaseStep", "Measure[@Type='LM']/PrismBaseStep", "LM", string.Empty, "deg", false, "Optional SOLOS prism base step from LM schema."),
                new DeviceMeasurementDefinition("solos-prism-mode", "PrismMode", "Measure[@Type='LM']/PrismMode", "LM", string.Empty, string.Empty, false, "Optional SOLOS prism mode from LM schema."),
                new DeviceMeasurementDefinition("solos-add-mode", "AddMode", "Measure[@Type='LM']/AddMode", "LM", string.Empty, string.Empty, false, "Optional SOLOS addition mode from LM schema."),
                new DeviceMeasurementDefinition("solos-cylinder-mode", "CylinderMode", "Measure[@Type='LM']/CylinderMode", "LM", string.Empty, string.Empty, false, "Cylinder mode from TOPCON SOLOS XML."),
                new DeviceMeasurementDefinition("solos-lens-type", "LensType", "Measure[@Type='LM']/LensType", "LM", string.Empty, string.Empty, false, "Lens type from TOPCON SOLOS XML."),
                new DeviceMeasurementDefinition("solos-wavelength", "Wavelength", "Measure[@Type='LM']/Wavelength", "LM", string.Empty, string.Empty, false, "Wavelength from TOPCON SOLOS XML."),
                new DeviceMeasurementDefinition("solos-r-sphere", "R Sphere", "Measure[@Type='LM']/LM/R/Sphere", "LM", "R", "dpt", true, "Right lens sphere from TOPCON SOLOS XML."),
                new DeviceMeasurementDefinition("solos-r-cylinder", "R Cylinder", "Measure[@Type='LM']/LM/R/Cylinder", "LM", "R", "dpt", true, "Right lens cylinder from TOPCON SOLOS XML."),
                new DeviceMeasurementDefinition("solos-r-axis", "R Axis", "Measure[@Type='LM']/LM/R/Axis", "LM", "R", "deg", true, "Right lens axis from TOPCON SOLOS XML."),
                new DeviceMeasurementDefinition("solos-r-se", "R SE", "Measure[@Type='LM']/LM/R/SE", "LM", "R", "dpt", false, "Optional right spherical equivalent from SOLOS XML."),
                new DeviceMeasurementDefinition("solos-r-add1", "R Add1", "Measure[@Type='LM']/LM/R/Add1", "LM", "R", "dpt", false, "Right first addition from SOLOS XML when present."),
                new DeviceMeasurementDefinition("solos-r-add", "R ADD", "Measure[@Type='LM']/LM/R/ADD", "LM", "R", "dpt", false, "Right first addition from SOLOS schema variant when present."),
                new DeviceMeasurementDefinition("solos-r-add2", "R Add2", "Measure[@Type='LM']/LM/R/Add2", "LM", "R", "dpt", false, "Right second addition from SOLOS XML when present."),
                new DeviceMeasurementDefinition("solos-r-add2-upper", "R ADD2", "Measure[@Type='LM']/LM/R/ADD2", "LM", "R", "dpt", false, "Right second addition from SOLOS schema variant when present."),
                new DeviceMeasurementDefinition("solos-r-near-sphere", "R NearSphere", "Measure[@Type='LM']/LM/R/NearSphere", "LM", "R", "dpt", false, "Optional right near sphere from SOLOS XML; not exported as ADD automatically."),
                new DeviceMeasurementDefinition("solos-r-near-sphere2", "R NearSphere2", "Measure[@Type='LM']/LM/R/NearSphere2", "LM", "R", "dpt", false, "Optional right second near sphere from SOLOS XML; not exported as ADD automatically."),
                new DeviceMeasurementDefinition("solos-r-prism-horizontal", "R PrismHorizontal", "Measure[@Type='LM']/LM/R/H", "LM", "R", "prism dpt", false, "Right signed horizontal prism from SOLOS XML."),
                new DeviceMeasurementDefinition("solos-r-prism-vertical", "R PrismVertical", "Measure[@Type='LM']/LM/R/V", "LM", "R", "prism dpt", false, "Right signed vertical prism from SOLOS XML."),
                new DeviceMeasurementDefinition("solos-r-prism-x", "R PrismX", "Measure[@Type='LM']/LM/R/PrismX", "LM", "R", "prism dpt", false, "Optional right horizontal prism from schema variant."),
                new DeviceMeasurementDefinition("solos-r-prism-y", "R PrismY", "Measure[@Type='LM']/LM/R/PrismY", "LM", "R", "prism dpt", false, "Optional right vertical prism from schema variant."),
                new DeviceMeasurementDefinition("solos-r-uv-transmittance", "R UVTransmittance", "Measure[@Type='LM']/LM/R/UVTransmittance", "LM", "R", "%", false, "Optional right UV transmittance from SOLOS schema; not exported to MEDISTAR yet."),
                new DeviceMeasurementDefinition("solos-r-confidence-index", "R ConfidenceIndex", "Measure[@Type='LM']/LM/R/ConfidenceIndex", "LM", "R", string.Empty, false, "Optional right confidence index from SOLOS schema."),
                new DeviceMeasurementDefinition("solos-l-sphere", "L Sphere", "Measure[@Type='LM']/LM/L/Sphere", "LM", "L", "dpt", true, "Left lens sphere from TOPCON SOLOS XML."),
                new DeviceMeasurementDefinition("solos-l-cylinder", "L Cylinder", "Measure[@Type='LM']/LM/L/Cylinder", "LM", "L", "dpt", true, "Left lens cylinder from TOPCON SOLOS XML."),
                new DeviceMeasurementDefinition("solos-l-axis", "L Axis", "Measure[@Type='LM']/LM/L/Axis", "LM", "L", "deg", true, "Left lens axis from TOPCON SOLOS XML."),
                new DeviceMeasurementDefinition("solos-l-se", "L SE", "Measure[@Type='LM']/LM/L/SE", "LM", "L", "dpt", false, "Optional left spherical equivalent from SOLOS XML."),
                new DeviceMeasurementDefinition("solos-l-add1", "L Add1", "Measure[@Type='LM']/LM/L/Add1", "LM", "L", "dpt", false, "Left first addition from SOLOS XML when present."),
                new DeviceMeasurementDefinition("solos-l-add", "L ADD", "Measure[@Type='LM']/LM/L/ADD", "LM", "L", "dpt", false, "Left first addition from SOLOS schema variant when present."),
                new DeviceMeasurementDefinition("solos-l-add2", "L Add2", "Measure[@Type='LM']/LM/L/Add2", "LM", "L", "dpt", false, "Left second addition from SOLOS XML when present."),
                new DeviceMeasurementDefinition("solos-l-add2-upper", "L ADD2", "Measure[@Type='LM']/LM/L/ADD2", "LM", "L", "dpt", false, "Left second addition from SOLOS schema variant when present."),
                new DeviceMeasurementDefinition("solos-l-near-sphere", "L NearSphere", "Measure[@Type='LM']/LM/L/NearSphere", "LM", "L", "dpt", false, "Optional left near sphere from SOLOS XML; not exported as ADD automatically."),
                new DeviceMeasurementDefinition("solos-l-near-sphere2", "L NearSphere2", "Measure[@Type='LM']/LM/L/NearSphere2", "LM", "L", "dpt", false, "Optional left second near sphere from SOLOS XML; not exported as ADD automatically."),
                new DeviceMeasurementDefinition("solos-l-prism-horizontal", "L PrismHorizontal", "Measure[@Type='LM']/LM/L/H", "LM", "L", "prism dpt", false, "Left signed horizontal prism from SOLOS XML."),
                new DeviceMeasurementDefinition("solos-l-prism-vertical", "L PrismVertical", "Measure[@Type='LM']/LM/L/V", "LM", "L", "prism dpt", false, "Left signed vertical prism from SOLOS XML."),
                new DeviceMeasurementDefinition("solos-l-prism-x", "L PrismX", "Measure[@Type='LM']/LM/L/PrismX", "LM", "L", "prism dpt", false, "Optional left horizontal prism from schema variant."),
                new DeviceMeasurementDefinition("solos-l-prism-y", "L PrismY", "Measure[@Type='LM']/LM/L/PrismY", "LM", "L", "prism dpt", false, "Optional left vertical prism from schema variant."),
                new DeviceMeasurementDefinition("solos-l-uv-transmittance", "L UVTransmittance", "Measure[@Type='LM']/LM/L/UVTransmittance", "LM", "L", "%", false, "Optional left UV transmittance from SOLOS schema; not exported to MEDISTAR yet."),
                new DeviceMeasurementDefinition("solos-l-confidence-index", "L ConfidenceIndex", "Measure[@Type='LM']/LM/L/ConfidenceIndex", "LM", "L", string.Empty, false, "Optional left confidence index from SOLOS schema."),
                new DeviceMeasurementDefinition("solos-pd-distance", "PD Distance", "Measure[@Type='LM']/PD/B/Distance", "PD", string.Empty, "mm", false, "Binocular PD distance from TOPCON SOLOS XML."),
                new DeviceMeasurementDefinition("solos-pd-r-distance", "R PD Distance", "Measure[@Type='LM']/PD/R/Distance", "PD", "R", "mm", false, "Right PD distance from TOPCON SOLOS XML."),
                new DeviceMeasurementDefinition("solos-pd-l-distance", "L PD Distance", "Measure[@Type='LM']/PD/L/Distance", "PD", "L", "mm", false, "Left PD distance from TOPCON SOLOS XML."),
                new DeviceMeasurementDefinition("solos-pd-distance-schema", "PD Distance", "Measure[@Type='LM']/PD/Distance", "PD", string.Empty, "mm", false, "Binocular PD distance from SOLOS schema variant."),
                new DeviceMeasurementDefinition("solos-pd-distance-r-schema", "R PD Distance", "Measure[@Type='LM']/PD/DistanceR", "PD", "R", "mm", false, "Right PD distance from SOLOS schema variant."),
                new DeviceMeasurementDefinition("solos-pd-distance-l-schema", "L PD Distance", "Measure[@Type='LM']/PD/DistanceL", "PD", "L", "mm", false, "Left PD distance from SOLOS schema variant."),
                new DeviceMeasurementDefinition("solos-pd-near", "PD Near", "Measure[@Type='LM']/PD/Near", "PD", string.Empty, "mm", false, "Optional near PD from SOLOS schema variant."),
                new DeviceMeasurementDefinition("solos-pd-near-r", "R PD Near", "Measure[@Type='LM']/PD/NearR", "PD", "R", "mm", false, "Optional right near PD from SOLOS schema variant."),
                new DeviceMeasurementDefinition("solos-pd-near-l", "L PD Near", "Measure[@Type='LM']/PD/NearL", "PD", "L", "mm", false, "Optional left near PD from SOLOS schema variant."),
                new DeviceMeasurementDefinition("solos-medistar-r-line", "R MEDISTAR Lensmeter-Zeile", "Measure[@Type='LM']/LM/R/MedistarLine", "LM", "R", string.Empty, false, "Computed MEDISTAR lensmeter line for TOPCON SOLOS right lens; optional values are omitted when absent."),
                new DeviceMeasurementDefinition("solos-medistar-l-line", "L MEDISTAR Lensmeter-Zeile", "Measure[@Type='LM']/LM/L/MedistarLine", "LM", "L", string.Empty, false, "Computed MEDISTAR lensmeter line for TOPCON SOLOS left lens; optional values are omitted when absent.")
            },
            SupportedExaminationTypes: new[] { "Lensmeter", "PD", "Prism", "Transmission" },
            CanContainMultipleExaminationTypes: false,
            DeviceImagePath: InterfaceProfileUiPolicy.TopconSolosDeviceImagePath);
    }

    public static DeviceProfileDefinition CreateTopconKr800Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-topcon-kr800-default",
                Name: "TOPCON KR-800S",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for TOPCON KR-800S XML files with REF, KM and SBJ measurements.",
                Vendor: "TOPCON",
                Product: "KR-800S",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "TOPCON",
            Model: "KR-800S",
            DeviceType: "Autorefractor/Keratometer/Subjective",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("kr800-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "TOPCON common company field."),
                new DeviceMeasurementDefinition("kr800-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "TOPCON common model field; expected KR-800S."),
                new DeviceMeasurementDefinition("kr800-ref-vd", "REF VD", "Measure[@Type='REF']/VD", "REF", string.Empty, "mm", false, "Vertex distance from TOPCON KR-800S REF block."),
                new DeviceMeasurementDefinition("kr800-ref-r-sphere", "REF R Sphere", "Measure[@Type='REF']/REF/R/Median/Sphere", "REF", "R", "dpt", true, "Right autorefractor sphere from TOPCON KR-800S REF median values."),
                new DeviceMeasurementDefinition("kr800-ref-r-cylinder", "REF R Cylinder", "Measure[@Type='REF']/REF/R/Median/Cylinder", "REF", "R", "dpt", true, "Right autorefractor cylinder from TOPCON KR-800S REF median values."),
                new DeviceMeasurementDefinition("kr800-ref-r-axis", "REF R Axis", "Measure[@Type='REF']/REF/R/Median/Axis", "REF", "R", "deg", true, "Right autorefractor axis from TOPCON KR-800S REF median values."),
                new DeviceMeasurementDefinition("kr800-ref-l-sphere", "REF L Sphere", "Measure[@Type='REF']/REF/L/Median/Sphere", "REF", "L", "dpt", true, "Left autorefractor sphere from TOPCON KR-800S REF median values."),
                new DeviceMeasurementDefinition("kr800-ref-l-cylinder", "REF L Cylinder", "Measure[@Type='REF']/REF/L/Median/Cylinder", "REF", "L", "dpt", true, "Left autorefractor cylinder from TOPCON KR-800S REF median values."),
                new DeviceMeasurementDefinition("kr800-ref-l-axis", "REF L Axis", "Measure[@Type='REF']/REF/L/Median/Axis", "REF", "L", "deg", true, "Left autorefractor axis from TOPCON KR-800S REF median values."),
                new DeviceMeasurementDefinition("kr800-ref-pd-distance", "REF PD Distance", "Measure[@Type='REF']/PD/Distance", "REF", string.Empty, "mm", false, "Binocular PD distance from TOPCON KR-800S REF block."),
                new DeviceMeasurementDefinition("kr800-ref-r-medistar-line", "REF R MEDISTAR-Zeile", "Measure[@Type='REF']/REF/R/MedistarLine", "REF", "R", string.Empty, false, "Computed MEDISTAR REF line for TOPCON KR-800S right eye."),
                new DeviceMeasurementDefinition("kr800-ref-l-medistar-line", "REF L MEDISTAR-Zeile", "Measure[@Type='REF']/REF/L/MedistarLine", "REF", "L", string.Empty, false, "Computed MEDISTAR REF line for TOPCON KR-800S left eye."),
                new DeviceMeasurementDefinition("kr800-km-r-k1-radius", "KM R R1 Radius", "Measure[@Type='KM']/KM/R/Median/R1/Radius", "KM", "R", "mm", false, "Right R1 radius from TOPCON KR-800S KM median values."),
                new DeviceMeasurementDefinition("kr800-km-r-k1-power", "KM R R1 Power", "Measure[@Type='KM']/KM/R/Median/R1/Power", "KM", "R", "dpt", false, "Right R1 power from TOPCON KR-800S KM median values."),
                new DeviceMeasurementDefinition("kr800-km-r-k1-axis", "KM R R1 Axis", "Measure[@Type='KM']/KM/R/Median/R1/Axis", "KM", "R", "deg", false, "Right R1 axis from TOPCON KR-800S KM median values."),
                new DeviceMeasurementDefinition("kr800-km-r-k2-radius", "KM R R2 Radius", "Measure[@Type='KM']/KM/R/Median/R2/Radius", "KM", "R", "mm", false, "Right R2 radius from TOPCON KR-800S KM median values."),
                new DeviceMeasurementDefinition("kr800-km-r-k2-power", "KM R R2 Power", "Measure[@Type='KM']/KM/R/Median/R2/Power", "KM", "R", "dpt", false, "Right R2 power from TOPCON KR-800S KM median values."),
                new DeviceMeasurementDefinition("kr800-km-r-k2-axis", "KM R R2 Axis", "Measure[@Type='KM']/KM/R/Median/R2/Axis", "KM", "R", "deg", false, "Right R2 axis from TOPCON KR-800S KM median values."),
                new DeviceMeasurementDefinition("kr800-km-l-k1-radius", "KM L R1 Radius", "Measure[@Type='KM']/KM/L/Median/R1/Radius", "KM", "L", "mm", false, "Left R1 radius from TOPCON KR-800S KM median values."),
                new DeviceMeasurementDefinition("kr800-km-l-k1-power", "KM L R1 Power", "Measure[@Type='KM']/KM/L/Median/R1/Power", "KM", "L", "dpt", false, "Left R1 power from TOPCON KR-800S KM median values."),
                new DeviceMeasurementDefinition("kr800-km-l-k1-axis", "KM L R1 Axis", "Measure[@Type='KM']/KM/L/Median/R1/Axis", "KM", "L", "deg", false, "Left R1 axis from TOPCON KR-800S KM median values."),
                new DeviceMeasurementDefinition("kr800-km-l-k2-radius", "KM L R2 Radius", "Measure[@Type='KM']/KM/L/Median/R2/Radius", "KM", "L", "mm", false, "Left R2 radius from TOPCON KR-800S KM median values."),
                new DeviceMeasurementDefinition("kr800-km-l-k2-power", "KM L R2 Power", "Measure[@Type='KM']/KM/L/Median/R2/Power", "KM", "L", "dpt", false, "Left R2 power from TOPCON KR-800S KM median values."),
                new DeviceMeasurementDefinition("kr800-km-l-k2-axis", "KM L R2 Axis", "Measure[@Type='KM']/KM/L/Median/R2/Axis", "KM", "L", "deg", false, "Left R2 axis from TOPCON KR-800S KM median values."),
                new DeviceMeasurementDefinition("kr800-km-line1", "KM MEDISTAR R1/R2-Zeile", "Measure[@Type='KM']/KM/MedistarLine1", "KM", string.Empty, string.Empty, false, "Computed MEDISTAR keratometry R1/R2 line for TOPCON KR-800S."),
                new DeviceMeasurementDefinition("kr800-km-line2", "KM MEDISTAR AV/CYL-Zeile", "Measure[@Type='KM']/KM/MedistarLine2", "KM", string.Empty, string.Empty, false, "Computed MEDISTAR keratometry AV/CYL line for TOPCON KR-800S."),
                new DeviceMeasurementDefinition("kr800-sbj-far-r-sphere", "SBJ FAR R Sphere", "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/R/Sph", "SBJ", "R", "dpt", false, "Subjective Full Correction FAR right sphere from TOPCON KR-800S."),
                new DeviceMeasurementDefinition("kr800-sbj-far-l-sphere", "SBJ FAR L Sphere", "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/L/Sph", "SBJ", "L", "dpt", false, "Subjective Full Correction FAR left sphere from TOPCON KR-800S."),
                new DeviceMeasurementDefinition("kr800-sbj-pd-b", "SBJ PD B", "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/PD/B", "SBJ", string.Empty, "mm", false, "Subjective binocular PD."),
                new DeviceMeasurementDefinition("kr800-sbj-line1", "SBJ MEDISTAR-Zeile 1", "Measure[@Type='SBJ']/MedistarLine1", "SBJ", string.Empty, string.Empty, false, "Computed MEDISTAR subjective refraction line 1."),
                new DeviceMeasurementDefinition("kr800-sbj-line2", "SBJ MEDISTAR-Zeile 2", "Measure[@Type='SBJ']/MedistarLine2", "SBJ", string.Empty, string.Empty, false, "Computed MEDISTAR subjective refraction line 2."),
                new DeviceMeasurementDefinition("kr800-sbj-line3", "SBJ MEDISTAR-Zeile 3", "Measure[@Type='SBJ']/MedistarLine3", "SBJ", string.Empty, string.Empty, false, "Computed MEDISTAR subjective refraction line 3, if present."),
                new DeviceMeasurementDefinition("kr800-sbj-line4", "SBJ MEDISTAR-Zeile 4", "Measure[@Type='SBJ']/MedistarLine4", "SBJ", string.Empty, string.Empty, false, "Computed MEDISTAR subjective refraction line 4, if present.")
            },
            SupportedExaminationTypes: new[] { "REF", "KM", "SBJ", "Refraktion", "Keratometer", "Subjektiv" },
            CanContainMultipleExaminationTypes: true,
            DeviceImagePath: InterfaceProfileUiPolicy.TopconKr800DeviceImagePath);
    }

    public static DeviceProfileDefinition CreateTopconKr1Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 24, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-topcon-kr1-default",
                Name: "TOPCON KR-1",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for TOPCON KR-1 XML files. The current fixture validates REF median values; KM/KRT remains prepared for later real fixtures.",
                Vendor: "TOPCON",
                Product: "KR-1",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "TOPCON",
            Model: "KR-1",
            DeviceType: "Keratorefraktometer / Autorefraktor-Keratometer-Kandidat",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("kr1-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "TOPCON common company field."),
                new DeviceMeasurementDefinition("kr1-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "TOPCON common model field; expected KR-1."),
                new DeviceMeasurementDefinition("kr1-machine-no", "MachineNo", "Common/MachineNo", "Common", string.Empty, string.Empty, false, "TOPCON KR-1 machine number."),
                new DeviceMeasurementDefinition("kr1-rom-version", "ROMVersion", "Common/ROMVersion", "Common", string.Empty, string.Empty, false, "TOPCON KR-1 ROM version."),
                new DeviceMeasurementDefinition("kr1-version", "Version", "Common/Version", "Common", string.Empty, string.Empty, false, "TOPCON KR-1 XML version."),
                new DeviceMeasurementDefinition("kr1-date", "Date", "Common/Date", "Common", string.Empty, string.Empty, false, "TOPCON KR-1 measurement date."),
                new DeviceMeasurementDefinition("kr1-time", "Time", "Common/Time", "Common", string.Empty, string.Empty, false, "TOPCON KR-1 measurement time."),
                new DeviceMeasurementDefinition("kr1-patient-no", "Patient No.", "Common/Patient/No.", "Common", string.Empty, string.Empty, false, "TOPCON KR-1 patient number."),
                new DeviceMeasurementDefinition("kr1-patient-id", "Patient ID", "Common/Patient/ID", "Common", string.Empty, string.Empty, false, "TOPCON KR-1 patient ID."),
                new DeviceMeasurementDefinition("kr1-ref-vd", "REF VD", "Measure[@Type='REF']/VD", "REF", string.Empty, "mm", false, "Vertex distance from TOPCON KR-1 REF block."),
                new DeviceMeasurementDefinition("kr1-ref-diopter-step", "REF DiopterStep", "Measure[@Type='REF']/DiopterStep", "REF", string.Empty, "dpt", false, "Diopter step from TOPCON KR-1 REF block."),
                new DeviceMeasurementDefinition("kr1-ref-axis-step", "REF AxisStep", "Measure[@Type='REF']/AxisStep", "REF", string.Empty, "deg", false, "Axis step from TOPCON KR-1 REF block."),
                new DeviceMeasurementDefinition("kr1-ref-cylinder-mode", "REF CylinderMode", "Measure[@Type='REF']/CylinderMode", "REF", string.Empty, string.Empty, false, "Cylinder mode from TOPCON KR-1 REF block."),
                new DeviceMeasurementDefinition("kr1-ref-r-sphere", "REF R Sphere", "Measure[@Type='REF']/REF/R/Median/Sphere", "REF", "R", "dpt", true, "Right autorefractor sphere from TOPCON KR-1 REF median values."),
                new DeviceMeasurementDefinition("kr1-ref-r-cylinder", "REF R Cylinder", "Measure[@Type='REF']/REF/R/Median/Cylinder", "REF", "R", "dpt", true, "Right autorefractor cylinder from TOPCON KR-1 REF median values."),
                new DeviceMeasurementDefinition("kr1-ref-r-axis", "REF R Axis", "Measure[@Type='REF']/REF/R/Median/Axis", "REF", "R", "deg", true, "Right autorefractor axis from TOPCON KR-1 REF median values."),
                new DeviceMeasurementDefinition("kr1-ref-r-se", "REF R SE", "Measure[@Type='REF']/REF/R/Median/SE", "REF", "R", "dpt", false, "Right spherical equivalent from TOPCON KR-1 REF median values; not exported to MEDISTAR."),
                new DeviceMeasurementDefinition("kr1-ref-l-sphere", "REF L Sphere", "Measure[@Type='REF']/REF/L/Median/Sphere", "REF", "L", "dpt", true, "Left autorefractor sphere from TOPCON KR-1 REF median values."),
                new DeviceMeasurementDefinition("kr1-ref-l-cylinder", "REF L Cylinder", "Measure[@Type='REF']/REF/L/Median/Cylinder", "REF", "L", "dpt", true, "Left autorefractor cylinder from TOPCON KR-1 REF median values."),
                new DeviceMeasurementDefinition("kr1-ref-l-axis", "REF L Axis", "Measure[@Type='REF']/REF/L/Median/Axis", "REF", "L", "deg", true, "Left autorefractor axis from TOPCON KR-1 REF median values."),
                new DeviceMeasurementDefinition("kr1-ref-l-se", "REF L SE", "Measure[@Type='REF']/REF/L/Median/SE", "REF", "L", "dpt", false, "Left spherical equivalent from TOPCON KR-1 REF median values; not exported to MEDISTAR."),
                new DeviceMeasurementDefinition("kr1-ref-pd-distance", "REF PD Distance", "Measure[@Type='REF']/PD/Distance", "REF", string.Empty, "mm", false, "Binocular distance PD from TOPCON KR-1 REF block."),
                new DeviceMeasurementDefinition("kr1-ref-pd-near", "REF PD Near", "Measure[@Type='REF']/PD/Near", "REF", string.Empty, "mm", false, "Near PD from TOPCON KR-1 REF block; not exported automatically."),
                new DeviceMeasurementDefinition("kr1-ref-r-medistar-line", "REF R MEDISTAR-Zeile", "Measure[@Type='REF']/REF/R/MedistarLine", "REF", "R", string.Empty, false, "Computed MEDISTAR REF line for TOPCON KR-1 right eye."),
                new DeviceMeasurementDefinition("kr1-ref-l-medistar-line", "REF L MEDISTAR-Zeile", "Measure[@Type='REF']/REF/L/MedistarLine", "REF", "L", string.Empty, false, "Computed MEDISTAR REF line for TOPCON KR-1 left eye.")
            },
            SupportedExaminationTypes: new[] { "REF", "Refraktion", "Autorefraktion", "Keratometer-Kandidat" },
            CanContainMultipleExaminationTypes: true,
            DeviceImagePath: InterfaceProfileUiPolicy.TopconKr1DeviceImagePath);
    }

    public static DeviceProfileDefinition CreateTopconRm800Default()
    {
        var timestamp = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);
        var profile = CreateTopconKr1Default();

        return profile with
        {
            Metadata = new ProfileMetadata(
                Id: "device-topcon-rm800-default",
                Name: "TOPCON RM-800",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for TOPCON RM-800 REF-only Ophthalmology XML files. The reference data confirms REF median values on the TOPCON XML family; KM remains disabled until a real fixture is available.",
                Vendor: "TOPCON",
                Product: "RM-800",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Model = "RM-800",
            DeviceType = "Autorefraktor",
            SupportedExaminationTypes = new[] { "REF", "Refraktion", "Autorefraktion" },
            DeviceImagePath = InterfaceProfileUiPolicy.TopconKr800DeviceImagePath
        };
    }

    public static DeviceProfileDefinition CreateTopconTrk2PDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-topcon-trk2p-default",
                Name: "TOPCON TRK2P",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for TOPCON TRK-2P JOIA/Ophthalmology XML files with REF, KM, TM, CCT fallback and optional SBJ data. XML namespaces nsCommon/nsREF/nsKM/nsTM/nsSBJ are read namespace-tolerantly.",
                Vendor: "TOPCON",
                Product: "TRK-2P",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "TOPCON",
            Model: "TRK-2P",
            DeviceType: "Autorefraktometer/Keratometer/Tonometer/Pachymeter/Subjektivtest",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("trk2p-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "TOPCON JOIA common company field."),
                new DeviceMeasurementDefinition("trk2p-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "TOPCON JOIA common model field."),
                new DeviceMeasurementDefinition("trk2p-measurement-date", "MeasurementDate", "Common/Date", "Common", string.Empty, string.Empty, false, "Measurement date from TOPCON TRK-2P common block."),
                new DeviceMeasurementDefinition("trk2p-measurement-time", "MeasurementTime", "Common/Time", "Common", string.Empty, string.Empty, false, "Measurement time from TOPCON TRK-2P common block."),
                new DeviceMeasurementDefinition("trk2p-ref-r-sphere", "REF R Sphere", "Measure[@Type='REF']/REF/R/Median/Sphere", "REF", "R", "dpt", true, "Right sphere from TOPCON TRK-2P REF median values."),
                new DeviceMeasurementDefinition("trk2p-ref-r-cylinder", "REF R Cylinder", "Measure[@Type='REF']/REF/R/Median/Cylinder", "REF", "R", "dpt", true, "Right cylinder from TOPCON TRK-2P REF median values."),
                new DeviceMeasurementDefinition("trk2p-ref-r-axis", "REF R Axis", "Measure[@Type='REF']/REF/R/Median/Axis", "REF", "R", "deg", true, "Right axis from TOPCON TRK-2P REF median values."),
                new DeviceMeasurementDefinition("trk2p-ref-l-sphere", "REF L Sphere", "Measure[@Type='REF']/REF/L/Median/Sphere", "REF", "L", "dpt", true, "Left sphere from TOPCON TRK-2P REF median values."),
                new DeviceMeasurementDefinition("trk2p-ref-l-cylinder", "REF L Cylinder", "Measure[@Type='REF']/REF/L/Median/Cylinder", "REF", "L", "dpt", true, "Left cylinder from TOPCON TRK-2P REF median values."),
                new DeviceMeasurementDefinition("trk2p-ref-l-axis", "REF L Axis", "Measure[@Type='REF']/REF/L/Median/Axis", "REF", "L", "deg", true, "Left axis from TOPCON TRK-2P REF median values."),
                new DeviceMeasurementDefinition("trk2p-ref-pd-distance", "REF PD Distance", "Measure[@Type='REF']/PD/Distance", "REF", string.Empty, "mm", false, "Distance PD from TOPCON TRK-2P REF data."),
                new DeviceMeasurementDefinition("trk2p-ref-vd", "REF VD", "Measure[@Type='REF']/VD", "REF", string.Empty, "mm", false, "Vertex distance from TOPCON TRK-2P REF data."),
                new DeviceMeasurementDefinition("trk2p-ref-r-line", "REF R MEDISTAR-Zeile", "Measure[@Type='REF']/REF/R/MedistarLine", "REF", "R", string.Empty, false, "Computed MEDISTAR REF line for TOPCON TRK-2P right eye."),
                new DeviceMeasurementDefinition("trk2p-ref-l-line", "REF L MEDISTAR-Zeile", "Measure[@Type='REF']/REF/L/MedistarLine", "REF", "L", string.Empty, false, "Computed MEDISTAR REF line for TOPCON TRK-2P left eye."),
                new DeviceMeasurementDefinition("trk2p-km-line1", "KM MEDISTAR R1/R2-Zeile", "Measure[@Type='KM']/KM/MedistarLine1", "KM", string.Empty, string.Empty, false, "Computed MEDISTAR keratometry R1/R2 line for TOPCON TRK-2P."),
                new DeviceMeasurementDefinition("trk2p-km-line2", "KM MEDISTAR AV/CYL-Zeile", "Measure[@Type='KM']/KM/MedistarLine2", "KM", string.Empty, string.Empty, false, "Computed MEDISTAR keratometry AV/CYL line for TOPCON TRK-2P."),
                new DeviceMeasurementDefinition("trk2p-r-iop-1", "TM R IOP 1", "Measure[@Type='TM']/TM/R/List[@No='1']/IOP_mmHg", "TM", "R", "mmHg", false, "Right IOP single value 1."),
                new DeviceMeasurementDefinition("trk2p-r-iop-2", "TM R IOP 2", "Measure[@Type='TM']/TM/R/List[@No='2']/IOP_mmHg", "TM", "R", "mmHg", false, "Right IOP single value 2."),
                new DeviceMeasurementDefinition("trk2p-r-iop-3", "TM R IOP 3", "Measure[@Type='TM']/TM/R/List[@No='3']/IOP_mmHg", "TM", "R", "mmHg", false, "Right IOP single value 3."),
                new DeviceMeasurementDefinition("trk2p-r-iop-average", "TM R IOP Average", "Measure[@Type='TM']/TM/R/Average/IOP_mmHg", "TM", "R", "mmHg", true, "Right IOP average."),
                new DeviceMeasurementDefinition("trk2p-l-iop-1", "TM L IOP 1", "Measure[@Type='TM']/TM/L/List[@No='1']/IOP_mmHg", "TM", "L", "mmHg", false, "Left IOP single value 1."),
                new DeviceMeasurementDefinition("trk2p-l-iop-2", "TM L IOP 2", "Measure[@Type='TM']/TM/L/List[@No='2']/IOP_mmHg", "TM", "L", "mmHg", false, "Left IOP single value 2."),
                new DeviceMeasurementDefinition("trk2p-l-iop-3", "TM L IOP 3", "Measure[@Type='TM']/TM/L/List[@No='3']/IOP_mmHg", "TM", "L", "mmHg", false, "Left IOP single value 3."),
                new DeviceMeasurementDefinition("trk2p-l-iop-average", "TM L IOP Average", "Measure[@Type='TM']/TM/L/Average/IOP_mmHg", "TM", "L", "mmHg", true, "Left IOP average."),
                new DeviceMeasurementDefinition("trk2p-tono-header-line", "TM MEDISTAR Tonometrie-Überschrift", "Measure[@Type='TM']/Tono/HeaderLine", "TM", string.Empty, string.Empty, false, "Computed MEDISTAR tonometry heading for TOPCON TRK-2P when TM values are available."),
                new DeviceMeasurementDefinition("trk2p-tono-pachy-right-line", "TM MEDISTAR Pachy rechts", "Measure[@Type='TM']/Tono/PachyRightLine", "TM", "R", string.Empty, false, "Computed MEDISTAR tonometry pachymetry line for right eye when CCT is available."),
                new DeviceMeasurementDefinition("trk2p-tono-pachy-left-line", "TM MEDISTAR Pachy links", "Measure[@Type='TM']/Tono/PachyLeftLine", "TM", "L", string.Empty, false, "Computed MEDISTAR tonometry pachymetry line for left eye when CCT is available."),
                new DeviceMeasurementDefinition("trk2p-tono-measured-right-line", "TM MEDISTAR Messung rechts", "Measure[@Type='TM']/Tono/MeasuredRightLine", "TM", "R", string.Empty, false, "Computed MEDISTAR measured and corrected IOP line for right eye when CorrectedIOP is available."),
                new DeviceMeasurementDefinition("trk2p-tono-parameter-right-line", "TM MEDISTAR Parameter rechts", "Measure[@Type='TM']/Tono/ParameterRightLine", "TM", "R", string.Empty, false, "Computed MEDISTAR CorrectedIOP parameter line for right eye when CCT is available."),
                new DeviceMeasurementDefinition("trk2p-tono-measured-left-line", "TM MEDISTAR Messung links", "Measure[@Type='TM']/Tono/MeasuredLeftLine", "TM", "L", string.Empty, false, "Computed MEDISTAR measured and corrected IOP line for left eye when CorrectedIOP is available."),
                new DeviceMeasurementDefinition("trk2p-tono-parameter-left-line", "TM MEDISTAR Parameter links", "Measure[@Type='TM']/Tono/ParameterLeftLine", "TM", "L", string.Empty, false, "Computed MEDISTAR CorrectedIOP parameter line for left eye when CCT is available."),
                new DeviceMeasurementDefinition("trk2p-tono-list-line", "TM MEDISTAR IOP-Listen-Zeile", "Measure[@Type='TM']/Tono/TonoListLine", "TM", string.Empty, string.Empty, false, "Computed MEDISTAR tonometry list line."),
                new DeviceMeasurementDefinition("trk2p-pachy-header-line", "CCT MEDISTAR Pachymetrie-Überschrift", "Measure[@Type='CCT']/Pachy/HeaderLine", "CCT", string.Empty, string.Empty, false, "Computed MEDISTAR pachymetry heading when CCT values are available."),
                new DeviceMeasurementDefinition("trk2p-pachy-line", "CCT MEDISTAR Pachymetrie-Zeile", "Measure[@Type='CCT']/Pachy/MedistarLine", "CCT", string.Empty, string.Empty, false, "Computed MEDISTAR pachymetry line from CCT or CorrectedIOP fallback."),
                new DeviceMeasurementDefinition("trk2p-sbj-line1", "SBJ MEDISTAR-Zeile 1", "Measure[@Type='SBJ']/MedistarLine1", "SBJ", string.Empty, string.Empty, false, "Computed MEDISTAR subjective refraction line 1 when SBJ values are present."),
                new DeviceMeasurementDefinition("trk2p-sbj-line2", "SBJ MEDISTAR-Zeile 2", "Measure[@Type='SBJ']/MedistarLine2", "SBJ", string.Empty, string.Empty, false, "Computed MEDISTAR subjective refraction line 2 when SBJ values are present."),
                new DeviceMeasurementDefinition("trk2p-sbj-line3", "SBJ MEDISTAR-Zeile 3", "Measure[@Type='SBJ']/MedistarLine3", "SBJ", string.Empty, string.Empty, false, "Computed MEDISTAR subjective refraction line 3 when SBJ values are present."),
                new DeviceMeasurementDefinition("trk2p-sbj-line4", "SBJ MEDISTAR-Zeile 4", "Measure[@Type='SBJ']/MedistarLine4", "SBJ", string.Empty, string.Empty, false, "Computed MEDISTAR subjective refraction line 4 when SBJ values are present.")
            },
            SupportedExaminationTypes: new[] { "REF", "KM", "TM", "CCT", "SBJ", "Autorefraktion", "Keratometer", "Tonometrie", "Pachymetrie", "Subjektiv" },
            CanContainMultipleExaminationTypes: true,
            DeviceImagePath: InterfaceProfileUiPolicy.TopconTrk2PDeviceImagePath);
    }

    public static DeviceProfileDefinition CreateTopconTrk3OmniaDefault()
    {
        var timestamp = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);
        var profile = CreateTopconTrk2PDefault();

        return profile with
        {
            Metadata = new ProfileMetadata(
                Id: "device-topcon-trk3-omnia-default",
                Name: "TOPCON TRK-3 Omnia",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for TOPCON TRK-3 Omnia JOIA/Ophthalmology XML files. Reference scripts confirm the same REF/KM/TM/CCT paths used by the TRK-2P XML family.",
                Vendor: "TOPCON",
                Product: "TRK-3 Omnia",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Model = "TRK-3 Omnia",
            DeviceType = "Autorefraktometer/Keratometer/Tonometer/Pachymeter",
            DeviceImagePath = InterfaceProfileUiPolicy.TopconTrk2PDeviceImagePath
        };
    }

    public static DeviceProfileDefinition CreateTopconCt1PDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 22, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-topcon-ct1p-default",
                Name: "TOPCON CT1P",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for TOPCON CT-1P JOIA/Ophthalmology XML files with TM and per-eye CorrectedIOP/CCT pachymetry support. XML namespaces nsCommon/nsTM are read namespace-tolerantly.",
                Vendor: "TOPCON",
                Product: "CT-1P",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "TOPCON",
            Model: "CT-1P",
            DeviceType: "Tonometer/Pachymeter",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("ct1p-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "TOPCON JOIA common company field."),
                new DeviceMeasurementDefinition("ct1p-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "TOPCON JOIA common model field; expected CT-1P."),
                new DeviceMeasurementDefinition("ct1p-machine-no", "MachineNo", "Common/MachineNo", "Common", string.Empty, string.Empty, false, "TOPCON CT-1P machine number."),
                new DeviceMeasurementDefinition("ct1p-rom-version", "ROMVersion", "Common/ROMVersion", "Common", string.Empty, string.Empty, false, "TOPCON CT-1P ROM version."),
                new DeviceMeasurementDefinition("ct1p-version", "Version", "Common/Version", "Common", string.Empty, string.Empty, false, "TOPCON CT-1P XML version."),
                new DeviceMeasurementDefinition("ct1p-measurement-date", "MeasurementDate", "Common/Date", "Common", string.Empty, string.Empty, false, "Measurement date from TOPCON CT-1P common block."),
                new DeviceMeasurementDefinition("ct1p-measurement-time", "MeasurementTime", "Common/Time", "Common", string.Empty, string.Empty, false, "Measurement time from TOPCON CT-1P common block."),
                new DeviceMeasurementDefinition("ct1p-patient-no", "Patient No.", "Common/Patient/No.", "Common", string.Empty, string.Empty, false, "TOPCON CT-1P patient number."),
                new DeviceMeasurementDefinition("ct1p-patient-id", "Patient ID", "Common/Patient/ID", "Common", string.Empty, string.Empty, false, "TOPCON CT-1P patient ID."),
                new DeviceMeasurementDefinition("ct1p-r-iop-1", "TM R IOP 1", "Measure[@Type='TM']/TM/R/List[@No='1']/IOP_mmHg", "TM", "R", "mmHg", false, "Right IOP single value 1."),
                new DeviceMeasurementDefinition("ct1p-r-iop-2", "TM R IOP 2", "Measure[@Type='TM']/TM/R/List[@No='2']/IOP_mmHg", "TM", "R", "mmHg", false, "Right IOP single value 2."),
                new DeviceMeasurementDefinition("ct1p-r-iop-3", "TM R IOP 3", "Measure[@Type='TM']/TM/R/List[@No='3']/IOP_mmHg", "TM", "R", "mmHg", false, "Right IOP single value 3."),
                new DeviceMeasurementDefinition("ct1p-r-iop-5", "TM R IOP 5", "Measure[@Type='TM']/TM/R/List[@No='5']/IOP_mmHg", "TM", "R", "mmHg", false, "Right IOP single value 5."),
                new DeviceMeasurementDefinition("ct1p-r-iop-average", "TM R IOP Average", "Measure[@Type='TM']/TM/R/Average/IOP_mmHg", "TM", "R", "mmHg", false, "Right IOP average."),
                new DeviceMeasurementDefinition("ct1p-l-iop-4", "TM L IOP 4", "Measure[@Type='TM']/TM/L/List[@No='4']/IOP_mmHg", "TM", "L", "mmHg", false, "Left IOP single value 4."),
                new DeviceMeasurementDefinition("ct1p-l-iop-5", "TM L IOP 5", "Measure[@Type='TM']/TM/L/List[@No='5']/IOP_mmHg", "TM", "L", "mmHg", false, "Left IOP single value 5."),
                new DeviceMeasurementDefinition("ct1p-l-iop-6", "TM L IOP 6", "Measure[@Type='TM']/TM/L/List[@No='6']/IOP_mmHg", "TM", "L", "mmHg", false, "Left IOP single value 6."),
                new DeviceMeasurementDefinition("ct1p-l-iop-average", "TM L IOP Average", "Measure[@Type='TM']/TM/L/Average/IOP_mmHg", "TM", "L", "mmHg", false, "Left IOP average."),
                new DeviceMeasurementDefinition("ct1p-corrected-r-measured", "R CorrectedIOP Measured", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Measured/IOP_mmHg", "CorrectedIOP", "R", "mmHg", false, "Right measured IOP from CorrectedIOP."),
                new DeviceMeasurementDefinition("ct1p-corrected-r-corrected", "R CorrectedIOP Corrected", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Corrected/IOP_mmHg", "CorrectedIOP", "R", "mmHg", false, "Right corrected IOP from CorrectedIOP."),
                new DeviceMeasurementDefinition("ct1p-corrected-r-param1", "R CorrectedIOP Param1", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Param1", "CorrectedIOP", "R", "mm", false, "Right Param1 for CorrectedIOP."),
                new DeviceMeasurementDefinition("ct1p-corrected-r-param2", "R CorrectedIOP Param2", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Param2", "CorrectedIOP", "R", string.Empty, false, "Right Param2 for CorrectedIOP."),
                new DeviceMeasurementDefinition("ct1p-corrected-r-cct", "R CorrectedIOP CCT", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/CCT", "CorrectedIOP", "R", "mm", false, "Right CCT fallback from CorrectedIOP."),
                new DeviceMeasurementDefinition("ct1p-corrected-l-measured", "L CorrectedIOP Measured", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/Measured/IOP_mmHg", "CorrectedIOP", "L", "mmHg", false, "Left measured IOP from CorrectedIOP when present."),
                new DeviceMeasurementDefinition("ct1p-corrected-l-corrected", "L CorrectedIOP Corrected", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/Corrected/IOP_mmHg", "CorrectedIOP", "L", "mmHg", false, "Left corrected IOP from CorrectedIOP when present."),
                new DeviceMeasurementDefinition("ct1p-corrected-l-param1", "L CorrectedIOP Param1", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/Param1", "CorrectedIOP", "L", "mm", false, "Left Param1 for CorrectedIOP when usable CCT or measured values are present."),
                new DeviceMeasurementDefinition("ct1p-corrected-l-param2", "L CorrectedIOP Param2", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/Param2", "CorrectedIOP", "L", string.Empty, false, "Left Param2 for CorrectedIOP when usable CCT or measured values are present."),
                new DeviceMeasurementDefinition("ct1p-corrected-l-cct", "L CorrectedIOP CCT", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/CCT", "CorrectedIOP", "L", "mm", false, "Left CCT fallback from CorrectedIOP when present."),
                new DeviceMeasurementDefinition("ct1p-pachy-header-line", "CCT MEDISTAR Pachymetrie-Überschrift", "Measure[@Type='CCT']/Pachy/HeaderLine", "CCT", string.Empty, string.Empty, false, "Computed MEDISTAR pachymetry heading when CCT values are available."),
                new DeviceMeasurementDefinition("ct1p-pachy-line", "CCT MEDISTAR Pachymetrie-Zeile", "Measure[@Type='CCT']/Pachy/MedistarLine", "CCT", string.Empty, string.Empty, false, "Computed MEDISTAR pachymetry line from CorrectedIOP/CCT fallback."),
                new DeviceMeasurementDefinition("ct1p-tono-header-line", "TM MEDISTAR Tonometrie-Überschrift", "Measure[@Type='TM']/Tono/HeaderLine", "TM", string.Empty, string.Empty, false, "Computed MEDISTAR tonometry heading when TM values are available."),
                new DeviceMeasurementDefinition("ct1p-tono-pachy-right-line", "TM MEDISTAR Pachy rechts", "Measure[@Type='TM']/Tono/PachyRightLine", "TM", "R", string.Empty, false, "Computed MEDISTAR tonometry pachymetry line for right eye when CCT is available."),
                new DeviceMeasurementDefinition("ct1p-tono-pachy-left-line", "TM MEDISTAR Pachy links", "Measure[@Type='TM']/Tono/PachyLeftLine", "TM", "L", string.Empty, false, "Computed MEDISTAR tonometry pachymetry line for left eye when CCT is available."),
                new DeviceMeasurementDefinition("ct1p-tono-measured-right-line", "TM MEDISTAR Messung rechts", "Measure[@Type='TM']/Tono/MeasuredRightLine", "TM", "R", string.Empty, false, "Computed MEDISTAR measured and corrected IOP line for right eye when available."),
                new DeviceMeasurementDefinition("ct1p-tono-parameter-right-line", "TM MEDISTAR Parameter rechts", "Measure[@Type='TM']/Tono/ParameterRightLine", "TM", "R", string.Empty, false, "Computed MEDISTAR CorrectedIOP parameter line for right eye when CCT or measured values are available."),
                new DeviceMeasurementDefinition("ct1p-tono-measured-left-line", "TM MEDISTAR Messung links", "Measure[@Type='TM']/Tono/MeasuredLeftLine", "TM", "L", string.Empty, false, "Computed MEDISTAR measured and corrected IOP line for left eye when available."),
                new DeviceMeasurementDefinition("ct1p-tono-parameter-left-line", "TM MEDISTAR Parameter links", "Measure[@Type='TM']/Tono/ParameterLeftLine", "TM", "L", string.Empty, false, "Computed MEDISTAR CorrectedIOP parameter line for left eye when CCT or measured values are available."),
                new DeviceMeasurementDefinition("ct1p-tono-list-line", "TM MEDISTAR IOP-Listen-Zeile", "Measure[@Type='TM']/Tono/TonoListLine", "TM", string.Empty, string.Empty, false, "Computed MEDISTAR tonometry list line.")
            },
            SupportedExaminationTypes: new[] { "TM", "CCT", "Tonometrie", "Pachymetrie", "CorrectedIOP" },
            CanContainMultipleExaminationTypes: true,
            DeviceImagePath: InterfaceProfileUiPolicy.TopconCt1PDeviceImagePath);
    }

    public static DeviceProfileDefinition CreateTopconCt800ADefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 24, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-topcon-ct800a-default",
                Name: "TOPCON CT-800A",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for TOPCON CT-800A Ophthalmology XML files with nsCommon/nsTM namespace handling. TM tonometry is exported via MEDISTAR 6205; incomplete CorrectedIOP/CCT blocks are ignored.",
                Vendor: "TOPCON",
                Product: "CT-800A",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "TOPCON",
            Model: "CT-800A",
            DeviceType: "Tonometer / Non-Contact-Tonometer",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("ct800a-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "TOPCON JOIA common company field."),
                new DeviceMeasurementDefinition("ct800a-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "TOPCON JOIA common model field; expected CT-800A."),
                new DeviceMeasurementDefinition("ct800a-machine-no", "MachineNo", "Common/MachineNo", "Common", string.Empty, string.Empty, false, "TOPCON CT-800A machine number."),
                new DeviceMeasurementDefinition("ct800a-rom-version", "ROMVersion", "Common/ROMVersion", "Common", string.Empty, string.Empty, false, "TOPCON CT-800A ROM version."),
                new DeviceMeasurementDefinition("ct800a-version", "Version", "Common/Version", "Common", string.Empty, string.Empty, false, "TOPCON CT-800A XML version."),
                new DeviceMeasurementDefinition("ct800a-measurement-date", "MeasurementDate", "Common/Date", "Common", string.Empty, string.Empty, false, "Measurement date from TOPCON CT-800A common block."),
                new DeviceMeasurementDefinition("ct800a-measurement-time", "MeasurementTime", "Common/Time", "Common", string.Empty, string.Empty, false, "Measurement time from TOPCON CT-800A common block."),
                new DeviceMeasurementDefinition("ct800a-patient-no", "Patient No.", "Common/Patient/No.", "Common", string.Empty, string.Empty, false, "TOPCON CT-800A patient number."),
                new DeviceMeasurementDefinition("ct800a-patient-id", "Patient ID", "Common/Patient/ID", "Common", string.Empty, string.Empty, false, "TOPCON CT-800A patient ID."),
                new DeviceMeasurementDefinition("ct800a-r-iop-1", "TM R IOP 1", "Measure[@Type='TM']/TM/R/List[@No='1']/IOP_mmHg", "TM", "R", "mmHg", false, "Right IOP single value 1."),
                new DeviceMeasurementDefinition("ct800a-r-iop-2", "TM R IOP 2", "Measure[@Type='TM']/TM/R/List[@No='2']/IOP_mmHg", "TM", "R", "mmHg", false, "Right IOP single value 2."),
                new DeviceMeasurementDefinition("ct800a-r-iop-3", "TM R IOP 3", "Measure[@Type='TM']/TM/R/List[@No='3']/IOP_mmHg", "TM", "R", "mmHg", false, "Right IOP single value 3."),
                new DeviceMeasurementDefinition("ct800a-r-iop-average", "TM R IOP Average", "Measure[@Type='TM']/TM/R/Average/IOP_mmHg", "TM", "R", "mmHg", false, "Right IOP average."),
                new DeviceMeasurementDefinition("ct800a-l-iop-1", "TM L IOP 1", "Measure[@Type='TM']/TM/L/List[@No='1']/IOP_mmHg", "TM", "L", "mmHg", false, "Left IOP single value 1."),
                new DeviceMeasurementDefinition("ct800a-l-iop-2", "TM L IOP 2", "Measure[@Type='TM']/TM/L/List[@No='2']/IOP_mmHg", "TM", "L", "mmHg", false, "Left IOP single value 2."),
                new DeviceMeasurementDefinition("ct800a-l-iop-3", "TM L IOP 3", "Measure[@Type='TM']/TM/L/List[@No='3']/IOP_mmHg", "TM", "L", "mmHg", false, "Left IOP single value 3."),
                new DeviceMeasurementDefinition("ct800a-l-iop-average", "TM L IOP Average", "Measure[@Type='TM']/TM/L/Average/IOP_mmHg", "TM", "L", "mmHg", false, "Left IOP average."),
                new DeviceMeasurementDefinition("ct800a-corrected-r-measured", "R CorrectedIOP Measured", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Measured/IOP_mmHg", "CorrectedIOP", "R", "mmHg", false, "Right measured IOP from CorrectedIOP."),
                new DeviceMeasurementDefinition("ct800a-corrected-r-corrected", "R CorrectedIOP Corrected", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Corrected/IOP_mmHg", "CorrectedIOP", "R", "mmHg", false, "Right corrected IOP from CorrectedIOP."),
                new DeviceMeasurementDefinition("ct800a-corrected-r-param1", "R CorrectedIOP Param1", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Param1", "CorrectedIOP", "R", "mm", false, "Right Param1 for CorrectedIOP."),
                new DeviceMeasurementDefinition("ct800a-corrected-r-param2", "R CorrectedIOP Param2", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Param2", "CorrectedIOP", "R", string.Empty, false, "Right Param2 for CorrectedIOP."),
                new DeviceMeasurementDefinition("ct800a-corrected-r-cct", "R CorrectedIOP CCT", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/CCT", "CorrectedIOP", "R", "mm", false, "Right CCT from complete CorrectedIOP."),
                new DeviceMeasurementDefinition("ct800a-corrected-l-measured", "L CorrectedIOP Measured", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/Measured/IOP_mmHg", "CorrectedIOP", "L", "mmHg", false, "Left measured IOP from CorrectedIOP."),
                new DeviceMeasurementDefinition("ct800a-corrected-l-corrected", "L CorrectedIOP Corrected", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/Corrected/IOP_mmHg", "CorrectedIOP", "L", "mmHg", false, "Left corrected IOP from CorrectedIOP."),
                new DeviceMeasurementDefinition("ct800a-corrected-l-param1", "L CorrectedIOP Param1", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/Param1", "CorrectedIOP", "L", "mm", false, "Left Param1 for CorrectedIOP."),
                new DeviceMeasurementDefinition("ct800a-corrected-l-param2", "L CorrectedIOP Param2", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/Param2", "CorrectedIOP", "L", string.Empty, false, "Left Param2 for CorrectedIOP."),
                new DeviceMeasurementDefinition("ct800a-corrected-l-cct", "L CorrectedIOP CCT", "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/CCT", "CorrectedIOP", "L", "mm", false, "Left CCT from complete CorrectedIOP."),
                new DeviceMeasurementDefinition("ct800a-tono-header-line", "TM MEDISTAR Tonometrie-Überschrift", "Measure[@Type='TM']/Tono/HeaderLine", "TM", string.Empty, string.Empty, false, "Computed MEDISTAR tonometry heading when TM values are available."),
                new DeviceMeasurementDefinition("ct800a-tono-pachy-right-line", "TM MEDISTAR Pachy rechts", "Measure[@Type='TM']/Tono/PachyRightLine", "TM", "R", string.Empty, false, "Computed MEDISTAR tonometry CCT line for right eye when CorrectedIOP is complete."),
                new DeviceMeasurementDefinition("ct800a-tono-pachy-left-line", "TM MEDISTAR Pachy links", "Measure[@Type='TM']/Tono/PachyLeftLine", "TM", "L", string.Empty, false, "Computed MEDISTAR tonometry CCT line for left eye when CorrectedIOP is complete."),
                new DeviceMeasurementDefinition("ct800a-tono-measured-right-line", "TM MEDISTAR Messung rechts", "Measure[@Type='TM']/Tono/MeasuredRightLine", "TM", "R", string.Empty, false, "Computed MEDISTAR measured and corrected IOP line for right eye when complete."),
                new DeviceMeasurementDefinition("ct800a-tono-parameter-right-line", "TM MEDISTAR Parameter rechts", "Measure[@Type='TM']/Tono/ParameterRightLine", "TM", "R", string.Empty, false, "Computed MEDISTAR CorrectedIOP parameter line for right eye when complete."),
                new DeviceMeasurementDefinition("ct800a-tono-measured-left-line", "TM MEDISTAR Messung links", "Measure[@Type='TM']/Tono/MeasuredLeftLine", "TM", "L", string.Empty, false, "Computed MEDISTAR measured and corrected IOP line for left eye when complete."),
                new DeviceMeasurementDefinition("ct800a-tono-parameter-left-line", "TM MEDISTAR Parameter links", "Measure[@Type='TM']/Tono/ParameterLeftLine", "TM", "L", string.Empty, false, "Computed MEDISTAR CorrectedIOP parameter line for left eye when complete."),
                new DeviceMeasurementDefinition("ct800a-tono-list-line", "TM MEDISTAR IOP-Listen-Zeile", "Measure[@Type='TM']/Tono/TonoListLine", "TM", string.Empty, string.Empty, false, "Computed MEDISTAR tonometry list line.")
            },
            SupportedExaminationTypes: new[] { "TM", "Tonometrie", "CorrectedIOP" },
            CanContainMultipleExaminationTypes: false,
            DeviceImagePath: InterfaceProfileUiPolicy.TopconCt800ADeviceImagePath);
    }

    public static DeviceProfileDefinition CreateTopconCv5000Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 23, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-topcon-cv5000-default",
                Name: "TOPCON CV-5000 / CV-5000S",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for bidirectional TOPCON CV-5000 / CV-5000S phoropter workflows. Incoming SBJ XML is read namespace-tolerantly: Prescription is returned with 6228 header and values; Full Correction is returned with 6227 header and values; AIS history can be written as TOPCON CV-5000 XML import candidate.",
                Vendor: "TOPCON",
                Product: "CV-5000 / CV-5000S",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "TOPCON",
            Model: "CV-5000 / CV-5000S",
            DeviceType: "Phoropter",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("cv5000-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "TOPCON common company field."),
                new DeviceMeasurementDefinition("cv5000-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "TOPCON CV-5000/CV-5000S common model field."),
                new DeviceMeasurementDefinition("cv5000-machine-no", "MachineNo", "Common/MachineNo", "Common", string.Empty, string.Empty, false, "TOPCON CV-5000 machine number."),
                new DeviceMeasurementDefinition("cv5000-rom-version", "ROMVersion", "Common/ROMVersion", "Common", string.Empty, string.Empty, false, "TOPCON CV-5000 ROM version."),
                new DeviceMeasurementDefinition("cv5000-version", "Version", "Common/Version", "Common", string.Empty, string.Empty, false, "TOPCON CV-5000 XML version."),
                new DeviceMeasurementDefinition("cv5000-date", "Date", "Common/Date", "Common", string.Empty, string.Empty, false, "TOPCON CV-5000 measurement date."),
                new DeviceMeasurementDefinition("cv5000-time", "Time", "Common/Time", "Common", string.Empty, string.Empty, false, "TOPCON CV-5000 measurement time."),
                new DeviceMeasurementDefinition("cv5000-patient-no", "Patient No.", "Common/Patient/No.", "Common", string.Empty, string.Empty, false, "TOPCON CV-5000 patient number."),
                new DeviceMeasurementDefinition("cv5000-patient-id", "Patient ID", "Common/Patient/ID", "Common", string.Empty, string.Empty, false, "TOPCON CV-5000 patient ID."),
                new DeviceMeasurementDefinition("cv5000-sbj-type1-name", "SBJ Type 1 Name", "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/TypeName", "SBJ", string.Empty, string.Empty, true, "First TOPCON CV-5000 subjective result type name."),
                new DeviceMeasurementDefinition("cv5000-sbj-type1-r-sph", "SBJ Type 1 R Sph", "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/R/Sph", "SBJ", "R", "dpt", false, "Right sphere from first CV-5000 result type."),
                new DeviceMeasurementDefinition("cv5000-sbj-type1-r-cyl", "SBJ Type 1 R Cyl", "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/R/Cyl", "SBJ", "R", "dpt", false, "Right cylinder from first CV-5000 result type."),
                new DeviceMeasurementDefinition("cv5000-sbj-type1-r-axis", "SBJ Type 1 R Axis", "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/R/Axis", "SBJ", "R", "deg", false, "Right axis from first CV-5000 result type."),
                new DeviceMeasurementDefinition("cv5000-sbj-type1-l-sph", "SBJ Type 1 L Sph", "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/L/Sph", "SBJ", "L", "dpt", false, "Left sphere from first CV-5000 result type."),
                new DeviceMeasurementDefinition("cv5000-sbj-type1-l-cyl", "SBJ Type 1 L Cyl", "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/L/Cyl", "SBJ", "L", "dpt", false, "Left cylinder from first CV-5000 result type."),
                new DeviceMeasurementDefinition("cv5000-sbj-type1-l-axis", "SBJ Type 1 L Axis", "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/L/Axis", "SBJ", "L", "deg", false, "Left axis from first CV-5000 result type."),
                new DeviceMeasurementDefinition("cv5000-sbj-type1-vd", "SBJ Type 1 VD", "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/VD", "SBJ", string.Empty, "mm", false, "Vertex distance from first CV-5000 result type."),
                new DeviceMeasurementDefinition("cv5000-sbj-type1-pd-b", "SBJ Type 1 PD B", "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/PD/B", "SBJ", string.Empty, "mm", false, "Binocular PD from first CV-5000 result type."),
                new DeviceMeasurementDefinition("cv5000-prescription-header", "Prescription MEDISTAR-Header", "Measure[@Type='SBJ']/Prescription/HeaderLine", "SBJ", string.Empty, string.Empty, false, "Computed MEDISTAR 6228 header for TOPCON CV-5000 Prescription."),
                new DeviceMeasurementDefinition("cv5000-prescription-r-line", "Prescription R MEDISTAR-Zeile", "Measure[@Type='SBJ']/Prescription/R/MedistarLine", "SBJ", "R", string.Empty, false, "Computed MEDISTAR 6228 right-eye line for TOPCON CV-5000 Prescription."),
                new DeviceMeasurementDefinition("cv5000-prescription-l-line", "Prescription L MEDISTAR-Zeile", "Measure[@Type='SBJ']/Prescription/L/MedistarLine", "SBJ", "L", string.Empty, false, "Computed MEDISTAR 6228 left-eye line for TOPCON CV-5000 Prescription."),
                new DeviceMeasurementDefinition("cv5000-full-correction-header", "Full Correction MEDISTAR-Header", "Measure[@Type='SBJ']/FullCorrection/HeaderLine", "SBJ", string.Empty, string.Empty, false, "Computed MEDISTAR 6227 header for TOPCON CV-5000 Full Correction."),
                new DeviceMeasurementDefinition("cv5000-full-correction-r-line", "Full Correction R MEDISTAR-Zeile", "Measure[@Type='SBJ']/FullCorrection/R/MedistarLine", "SBJ", "R", string.Empty, false, "Computed MEDISTAR 6227 right-eye line for TOPCON CV-5000 Full Correction."),
                new DeviceMeasurementDefinition("cv5000-full-correction-l-line", "Full Correction L MEDISTAR-Zeile", "Measure[@Type='SBJ']/FullCorrection/L/MedistarLine", "SBJ", "L", string.Empty, false, "Computed MEDISTAR 6227 left-eye line for TOPCON CV-5000 Full Correction.")
            },
            SupportedExaminationTypes: new[] { "SBJ", "Phoropter", "Refraktion", "Prescription", "Full Correction" },
            CanContainMultipleExaminationTypes: true,
            IsBidirectional: true,
            DeviceImagePath: InterfaceProfileUiPolicy.TopconCv5000DeviceImagePath);
    }

    public static DeviceProfileDefinition CreateNidekRt2100SerialDefault()
    {
        return CreateNidekRtSerialDefault(
            id: "device-nidek-rt2100-serial-default",
            name: "NIDEK RT-2100 RS232",
            product: "RT-2100 RS232",
            model: "RT-2100",
            description: "Default device profile definition for the prepared bidirectional NIDEK RT-2100 serial RS232 phoropter workflow. Final prescription is exported with 6228 and Subjective/Full Correction with 6227 after real practice captures are validated.",
            timestamp: new DateTimeOffset(2026, 5, 29, 12, 0, 0, TimeSpan.Zero));
    }

    public static DeviceProfileDefinition CreateNidekRt3100SerialDefault()
    {
        return CreateNidekRtSerialDefault(
            id: "device-nidek-rt3100-serial-default",
            name: "NIDEK RT-3100 RS232",
            product: "RT-3100 RS232",
            model: "RT-3100",
            description: "Default device profile definition for the prepared bidirectional NIDEK RT-3100 serial RS232 phoropter workflow. Type 1 uses 2400 7E2; Type 2 can be selected on site with 9600 8O1.",
            timestamp: new DateTimeOffset(2026, 5, 29, 12, 0, 0, TimeSpan.Zero));
    }

    public static DeviceProfileDefinition CreateNidekRt5100SerialDefault()
    {
        return CreateNidekRtSerialDefault(
            id: "device-nidek-rt5100-serial-default",
            name: "NIDEK RT-5100 RS232",
            product: "RT-5100 RS232",
            model: "RT-5100",
            description: "Default device profile definition for the prepared bidirectional NIDEK RT-5100 serial RS232 phoropter workflow. The shared NIDEK-RT serial parser reads Final and Subjective refraction candidates; live validation remains open.",
            timestamp: new DateTimeOffset(2026, 5, 29, 12, 0, 0, TimeSpan.Zero));
    }

    private static DeviceProfileDefinition CreateNidekRtSerialDefault(
        string id,
        string name,
        string product,
        string model,
        string description,
        DateTimeOffset timestamp)
    {
        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.DeviceProfile,
                Description: description,
                Vendor: "NIDEK",
                Product: product,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "NIDEK",
            Model: model,
            DeviceType: "Phoropter",
            ParserMode: NidekRtSerialPhoropterConstants.ParserMode,
            Measurements: CreateNidekRtSerialMeasurements(model),
            SupportedExaminationTypes: new[] { "RT", "Phoropter", "Refraktion", "Final", "Subjective", "LM", "RM", "AR" },
            CanContainMultipleExaminationTypes: true,
            IsBidirectional: true,
            DeviceImagePath: InterfaceProfileUiPolicy.GetBuiltInDeviceImagePathForDeviceProfileId(id),
            ConnectionKind: DeviceConnectionKind.SerialRs232,
            SerialSettings: new SerialCommunicationSettings(
                BaudRate: 2400,
                DataBits: 7,
                StopBits: SerialStopBitsSetting.Two,
                Parity: SerialParitySetting.Even,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: true,
                RtsEnable: true,
                IsBidirectional: true,
                LineTerminator: SerialLineTerminatorSetting.CR));
    }

    private static IReadOnlyList<DeviceMeasurementDefinition> CreateNidekRtSerialMeasurements(string model)
    {
        var prefix = model.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase).ToLowerInvariant();
        return new[]
        {
            new DeviceMeasurementDefinition($"{prefix}-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "NIDEK RT serial common company field."),
            new DeviceMeasurementDefinition($"{prefix}-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "NIDEK RT serial model name."),
            new DeviceMeasurementDefinition($"{prefix}-patient-id", "Patient ID", "Common/Patient/ID", "Common", string.Empty, string.Empty, false, "Patient or ID number from the serial header, when present."),
            new DeviceMeasurementDefinition($"{prefix}-date", "Date", "Common/Date", "Common", string.Empty, string.Empty, false, "Measurement date from the serial header, when present."),
            new DeviceMeasurementDefinition($"{prefix}-system-no", "SystemNo", "Common/SystemNo", "Common", string.Empty, string.Empty, false, "System number from RT-3100/RT-5100 header, when present."),
            new DeviceMeasurementDefinition($"{prefix}-source", "Datenquelle", "Measure[@Type='RTSERIAL']/Source", "RTSERIAL", string.Empty, string.Empty, false, "Recognized NIDEK RT serial data source blocks, for example @RT."),
            new DeviceMeasurementDefinition($"{prefix}-final-r-sphere", "Final R Sphere", "Measure[@Type='RTSERIAL']/Final/R/Sphere", "RTSERIAL", "R", "dpt", false, "Right sphere from final prescription."),
            new DeviceMeasurementDefinition($"{prefix}-final-r-cylinder", "Final R Cylinder", "Measure[@Type='RTSERIAL']/Final/R/Cylinder", "RTSERIAL", "R", "dpt", false, "Right cylinder from final prescription."),
            new DeviceMeasurementDefinition($"{prefix}-final-r-axis", "Final R Axis", "Measure[@Type='RTSERIAL']/Final/R/Axis", "RTSERIAL", "R", "deg", false, "Right axis from final prescription."),
            new DeviceMeasurementDefinition($"{prefix}-final-r-add", "Final R ADD", "Measure[@Type='RTSERIAL']/Final/R/ADD", "RTSERIAL", "R", "dpt", false, "Right ADD from final prescription."),
            new DeviceMeasurementDefinition($"{prefix}-final-r-pd", "Final R PD", "Measure[@Type='RTSERIAL']/Final/R/PD", "RTSERIAL", "R", "mm", false, "Right PD from final prescription block."),
            new DeviceMeasurementDefinition($"{prefix}-final-r-va", "Final R VA", "Measure[@Type='RTSERIAL']/Final/R/VA", "RTSERIAL", "R", string.Empty, false, "Right visual acuity from final prescription block."),
            new DeviceMeasurementDefinition($"{prefix}-final-r-wd", "Final R WD", "Measure[@Type='RTSERIAL']/Final/R/WorkingDistance", "RTSERIAL", "R", "cm", false, "Right working distance from final prescription block."),
            new DeviceMeasurementDefinition($"{prefix}-final-l-sphere", "Final L Sphere", "Measure[@Type='RTSERIAL']/Final/L/Sphere", "RTSERIAL", "L", "dpt", false, "Left sphere from final prescription."),
            new DeviceMeasurementDefinition($"{prefix}-final-l-cylinder", "Final L Cylinder", "Measure[@Type='RTSERIAL']/Final/L/Cylinder", "RTSERIAL", "L", "dpt", false, "Left cylinder from final prescription."),
            new DeviceMeasurementDefinition($"{prefix}-final-l-axis", "Final L Axis", "Measure[@Type='RTSERIAL']/Final/L/Axis", "RTSERIAL", "L", "deg", false, "Left axis from final prescription."),
            new DeviceMeasurementDefinition($"{prefix}-final-l-add", "Final L ADD", "Measure[@Type='RTSERIAL']/Final/L/ADD", "RTSERIAL", "L", "dpt", false, "Left ADD from final prescription."),
            new DeviceMeasurementDefinition($"{prefix}-final-l-pd", "Final L PD", "Measure[@Type='RTSERIAL']/Final/L/PD", "RTSERIAL", "L", "mm", false, "Left PD from final prescription block."),
            new DeviceMeasurementDefinition($"{prefix}-final-l-va", "Final L VA", "Measure[@Type='RTSERIAL']/Final/L/VA", "RTSERIAL", "L", string.Empty, false, "Left visual acuity from final prescription block."),
            new DeviceMeasurementDefinition($"{prefix}-final-l-wd", "Final L WD", "Measure[@Type='RTSERIAL']/Final/L/WorkingDistance", "RTSERIAL", "L", "cm", false, "Left working distance from final prescription block."),
            new DeviceMeasurementDefinition($"{prefix}-subjective-r-sphere", "Subjective R Sphere", "Measure[@Type='RTSERIAL']/Subjective/R/Sphere", "RTSERIAL", "R", "dpt", false, "Right sphere from subjective/full correction."),
            new DeviceMeasurementDefinition($"{prefix}-subjective-r-cylinder", "Subjective R Cylinder", "Measure[@Type='RTSERIAL']/Subjective/R/Cylinder", "RTSERIAL", "R", "dpt", false, "Right cylinder from subjective/full correction."),
            new DeviceMeasurementDefinition($"{prefix}-subjective-r-axis", "Subjective R Axis", "Measure[@Type='RTSERIAL']/Subjective/R/Axis", "RTSERIAL", "R", "deg", false, "Right axis from subjective/full correction."),
            new DeviceMeasurementDefinition($"{prefix}-subjective-r-add", "Subjective R ADD", "Measure[@Type='RTSERIAL']/Subjective/R/ADD", "RTSERIAL", "R", "dpt", false, "Right ADD from subjective/full correction."),
            new DeviceMeasurementDefinition($"{prefix}-subjective-l-sphere", "Subjective L Sphere", "Measure[@Type='RTSERIAL']/Subjective/L/Sphere", "RTSERIAL", "L", "dpt", false, "Left sphere from subjective/full correction."),
            new DeviceMeasurementDefinition($"{prefix}-subjective-l-cylinder", "Subjective L Cylinder", "Measure[@Type='RTSERIAL']/Subjective/L/Cylinder", "RTSERIAL", "L", "dpt", false, "Left cylinder from subjective/full correction."),
            new DeviceMeasurementDefinition($"{prefix}-subjective-l-axis", "Subjective L Axis", "Measure[@Type='RTSERIAL']/Subjective/L/Axis", "RTSERIAL", "L", "deg", false, "Left axis from subjective/full correction."),
            new DeviceMeasurementDefinition($"{prefix}-subjective-l-add", "Subjective L ADD", "Measure[@Type='RTSERIAL']/Subjective/L/ADD", "RTSERIAL", "L", "dpt", false, "Left ADD from subjective/full correction."),
            new DeviceMeasurementDefinition($"{prefix}-final-header", "Final MEDISTAR-Header", "Measure[@Type='RTSERIAL']/Final/HeaderLine", "RTSERIAL", string.Empty, string.Empty, false, "Computed MEDISTAR 6228 header for NIDEK RT serial final prescription."),
            new DeviceMeasurementDefinition($"{prefix}-final-r-line", "Final R MEDISTAR-Zeile", "Measure[@Type='RTSERIAL']/Final/R/MedistarLine", "RTSERIAL", "R", string.Empty, false, "Computed MEDISTAR 6228 right-eye final prescription line."),
            new DeviceMeasurementDefinition($"{prefix}-final-l-line", "Final L MEDISTAR-Zeile", "Measure[@Type='RTSERIAL']/Final/L/MedistarLine", "RTSERIAL", "L", string.Empty, false, "Computed MEDISTAR 6228 left-eye final prescription line."),
            new DeviceMeasurementDefinition($"{prefix}-subjective-header", "Subjective MEDISTAR-Header", "Measure[@Type='RTSERIAL']/Subjective/HeaderLine", "RTSERIAL", string.Empty, string.Empty, false, "Computed MEDISTAR 6227 header for NIDEK RT serial subjective/full correction."),
            new DeviceMeasurementDefinition($"{prefix}-subjective-r-line", "Subjective R MEDISTAR-Zeile", "Measure[@Type='RTSERIAL']/Subjective/R/MedistarLine", "RTSERIAL", "R", string.Empty, false, "Computed MEDISTAR 6227 right-eye subjective/full correction line."),
            new DeviceMeasurementDefinition($"{prefix}-subjective-l-line", "Subjective L MEDISTAR-Zeile", "Measure[@Type='RTSERIAL']/Subjective/L/MedistarLine", "RTSERIAL", "L", string.Empty, false, "Computed MEDISTAR 6227 left-eye subjective/full correction line."),
            new DeviceMeasurementDefinition($"{prefix}-lm-r-sphere", "LM R Sphere", "Measure[@Type='RTSERIAL']/Lensmeter/R/Sphere", "RTSERIAL", "R", "dpt", false, "Lensmeter right sphere from serial RT data."),
            new DeviceMeasurementDefinition($"{prefix}-lm-l-sphere", "LM L Sphere", "Measure[@Type='RTSERIAL']/Lensmeter/L/Sphere", "RTSERIAL", "L", "dpt", false, "Lensmeter left sphere from serial RT data."),
            new DeviceMeasurementDefinition($"{prefix}-ar-r-sphere", "AR R Sphere", "Measure[@Type='RTSERIAL']/Objective/R/Sphere", "RTSERIAL", "R", "dpt", false, "Objective/autorefraction right sphere from serial RT data."),
            new DeviceMeasurementDefinition($"{prefix}-ar-l-sphere", "AR L Sphere", "Measure[@Type='RTSERIAL']/Objective/L/Sphere", "RTSERIAL", "L", "dpt", false, "Objective/autorefraction left sphere from serial RT data.")
        };
    }

    public static DeviceProfileDefinition CreateShinNipponAccurefR800Default()
    {
        return CreateShinNipponTextSerialDefault(
            id: "device-shin-nippon-accuref-r800-default",
            name: "Shin-Nippon Accuref R-800",
            product: "Accuref R-800",
            model: "Accuref R-800",
            deviceType: "Autorefraktor",
            description: "Built-in serial text profile for Shin-Nippon Accuref R-800 REF data derived from neutral reference parser rules. REF maps to 6228; practical raw-data validation remains open.",
            baudRate: 115200,
            includeRef: true,
            includeKm: false,
            includeLens: false,
            includeTono: false,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static DeviceProfileDefinition CreateShinNipponAccurefK900Default()
    {
        return CreateShinNipponTextSerialDefault(
            id: "device-shin-nippon-accuref-k900-default",
            name: "Shin-Nippon Accuref K-900",
            product: "Accuref K-900",
            model: "Accuref K-900",
            deviceType: "Autorefraktor/Keratometer",
            description: "Built-in serial text profile for Shin-Nippon Accuref K-900 REF/KM data derived from neutral reference parser rules. REF maps to 6228 and KM maps to 6221; practical raw-data validation remains open.",
            baudRate: 115200,
            includeRef: true,
            includeKm: true,
            includeLens: false,
            includeTono: false,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static DeviceProfileDefinition CreateShinNipponDl1000Default()
    {
        return CreateShinNipponTextSerialDefault(
            id: "device-shin-nippon-dl1000-default",
            name: "Shin-Nippon DL-1000",
            product: "DL-1000",
            model: "DL-1000",
            deviceType: "Lensmeter",
            description: "Built-in serial text profile for Shin-Nippon DL-1000 lensmeter data derived from neutral reference parser rules. Lensmeter lines map to 6228; practical raw-data validation remains open.",
            baudRate: 9600,
            includeRef: false,
            includeKm: false,
            includeLens: true,
            includeTono: false,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static DeviceProfileDefinition CreateShinNipponDl800Default()
    {
        return CreateShinNipponTextSerialDefault(
            id: "device-shin-nippon-dl800-default",
            name: "Shin-Nippon DL-800",
            product: "DL-800",
            model: "DL-800",
            deviceType: "Lensmeter",
            description: "Built-in serial text profile for Shin-Nippon DL-800 lensmeter data derived from neutral reference parser rules. Lensmeter lines map to 6228; practical raw-data validation remains open.",
            baudRate: 9600,
            includeRef: false,
            includeKm: false,
            includeLens: true,
            includeTono: false,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static DeviceProfileDefinition CreateShinNipponDl900Default()
    {
        return CreateShinNipponTextSerialDefault(
            id: "device-shin-nippon-dl900-default",
            name: "Shin-Nippon DL-900",
            product: "DL-900",
            model: "DL-900",
            deviceType: "Lensmeter",
            description: "Built-in serial text profile for Shin-Nippon DL-900 lensmeter data derived from neutral reference parser rules. Lensmeter lines map to 6228; practical raw-data validation remains open.",
            baudRate: 9600,
            includeRef: false,
            includeKm: false,
            includeLens: true,
            includeTono: false,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static DeviceProfileDefinition CreateShinNipponNct200Default()
    {
        return CreateShinNipponTextSerialDefault(
            id: "device-shin-nippon-nct200-default",
            name: "Shin-Nippon NCT-200",
            product: "NCT-200",
            model: "NCT-200",
            deviceType: "Non-Contact-Tonometer",
            description: "Built-in serial text profile for Shin-Nippon NCT-200 tonometry data derived from neutral reference parser rules. IOP maps to 6205; practical raw-data validation remains open.",
            baudRate: 19200,
            includeRef: false,
            includeKm: false,
            includeLens: false,
            includeTono: true,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static DeviceProfileDefinition CreateShinNipponSlm4000Default()
    {
        return CreateShinNipponTextSerialDefault(
            id: "device-shin-nippon-slm4000-default",
            name: "Shin-Nippon SLM-4000",
            product: "SLM-4000",
            model: "SLM-4000",
            deviceType: "Lensmeter",
            description: "Built-in serial text profile for Shin-Nippon SLM-4000 lensmeter data derived from neutral reference parser rules. Lensmeter lines map to 6228; practical raw-data validation remains open.",
            baudRate: 9600,
            includeRef: false,
            includeKm: false,
            includeLens: true,
            includeTono: false,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    private static DeviceProfileDefinition CreateShinNipponTextSerialDefault(
        string id,
        string name,
        string product,
        string model,
        string deviceType,
        string description,
        int baudRate,
        bool includeRef,
        bool includeKm,
        bool includeLens,
        bool includeTono,
        DateTimeOffset timestamp)
    {
        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.DeviceProfile,
                Description: description,
                Vendor: "Shin-Nippon",
                Product: product,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "Shin-Nippon",
            Model: model,
            DeviceType: deviceType,
            ParserMode: ShinNipponDeviceParser.ParserMode,
            Measurements: CreateShinNipponTextMeasurements(product, includeRef, includeKm, includeLens, includeTono),
            SupportedExaminationTypes: CreateShinNipponSupportedExaminationTypes(includeRef, includeKm, includeLens, includeTono),
            CanContainMultipleExaminationTypes: (includeRef && includeKm) || includeTono,
            IsBidirectional: false,
            DeviceImagePath: InterfaceProfileUiPolicy.GetBuiltInDeviceImagePathForDeviceProfileId(id),
            ConnectionKind: DeviceConnectionKind.SerialRs232,
            SerialSettings: new SerialCommunicationSettings(
                BaudRate: baudRate,
                DataBits: 8,
                StopBits: SerialStopBitsSetting.One,
                Parity: SerialParitySetting.None,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: false,
                RtsEnable: false,
                IsBidirectional: false,
                LineTerminator: SerialLineTerminatorSetting.CRLF,
                ReadTimeoutMilliseconds: includeTono ? 30000 : 5000,
                WriteTimeoutMilliseconds: 1000));
    }

    private static IReadOnlyList<string> CreateShinNipponSupportedExaminationTypes(
        bool includeRef,
        bool includeKm,
        bool includeLens,
        bool includeTono)
    {
        var values = new List<string>();
        if (includeRef)
        {
            values.AddRange(new[] { "REF", "Autorefraktor" });
        }

        if (includeKm)
        {
            values.AddRange(new[] { "KM", "Keratometer" });
        }

        if (includeLens)
        {
            values.AddRange(new[] { "LM", "Lensmeter" });
        }

        if (includeTono)
        {
            values.AddRange(new[] { "TM", "Tonometrie" });
        }

        return values;
    }

    private static IReadOnlyList<DeviceMeasurementDefinition> CreateShinNipponTextMeasurements(
        string product,
        bool includeRef,
        bool includeKm,
        bool includeLens,
        bool includeTono)
    {
        var prefix = $"shin-nippon-{product.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase).ToLowerInvariant()}";
        var measurements = new List<DeviceMeasurementDefinition>
        {
            new($"{prefix}-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "Shin-Nippon common company field."),
            new($"{prefix}-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "Shin-Nippon model name.")
        };

        if (includeRef)
        {
            foreach (var eye in new[] { "R", "L" })
            {
                var eyePrefix = $"{prefix}-ref-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{eyePrefix}-sphere", $"REF {eye} Sphere", $"Measure[@Type='REF']/REF/{eye}/Sphere", "REF", eye, "dpt", false, "REF sphere from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-cylinder", $"REF {eye} Cylinder", $"Measure[@Type='REF']/REF/{eye}/Cylinder", "REF", eye, "dpt", false, "REF cylinder from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-axis", $"REF {eye} Axis", $"Measure[@Type='REF']/REF/{eye}/Axis", "REF", eye, "deg", false, "REF axis from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-pd", $"REF {eye} PD", $"Measure[@Type='REF']/REF/{eye}/PD", "REF", eye, "mm", false, "REF PD from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-vd", $"REF {eye} VD", $"Measure[@Type='REF']/REF/{eye}/VD", "REF", eye, "mm", false, "REF VD from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-medistar-line", $"REF {eye} MEDISTAR-Zeile", $"Measure[@Type='REF']/REF/{eye}/MedistarLine", "REF", eye, string.Empty, false, "Prepared MEDISTAR 6228 REF line."));
            }
        }

        if (includeKm)
        {
            foreach (var eye in new[] { "R", "L" })
            {
                var eyePrefix = $"{prefix}-km-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{eyePrefix}-r1-radius", $"KM {eye} R1 Radius", $"Measure[@Type='KM']/KM/{eye}/R1/Radius", "KM", eye, "mm", false, "KM R1 radius from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-r1-axis", $"KM {eye} R1 Axis", $"Measure[@Type='KM']/KM/{eye}/R1/Axis", "KM", eye, "deg", false, "KM R1 axis from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-r2-radius", $"KM {eye} R2 Radius", $"Measure[@Type='KM']/KM/{eye}/R2/Radius", "KM", eye, "mm", false, "KM R2 radius from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-r2-axis", $"KM {eye} R2 Axis", $"Measure[@Type='KM']/KM/{eye}/R2/Axis", "KM", eye, "deg", false, "KM R2 axis from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-cylinder", $"KM {eye} Cylinder", $"Measure[@Type='KM']/KM/{eye}/Cylinder", "KM", eye, "dpt", false, "KM cylinder from Shin-Nippon text."));
            }

            measurements.Add(new($"{prefix}-km-radii-line", "KM MEDISTAR R1/R2-Zeile", "Measure[@Type='KM']/KM/MedistarLine1", "KM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6221 KM R1/R2 line."));
            measurements.Add(new($"{prefix}-km-cylinder-line", "KM MEDISTAR CYL-Zeile", "Measure[@Type='KM']/KM/MedistarLine2", "KM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6221 KM cylinder line."));
        }

        if (includeLens)
        {
            foreach (var eye in new[] { "R", "L" })
            {
                var eyePrefix = $"{prefix}-lm-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{eyePrefix}-sphere", $"LM {eye} Sphere", $"Measure[@Type='LM']/LM/{eye}/Sphere", "LM", eye, "dpt", false, "LM sphere from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-cylinder", $"LM {eye} Cylinder", $"Measure[@Type='LM']/LM/{eye}/Cylinder", "LM", eye, "dpt", false, "LM cylinder from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-axis", $"LM {eye} Axis", $"Measure[@Type='LM']/LM/{eye}/Axis", "LM", eye, "deg", false, "LM axis from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-add", $"LM {eye} ADD", $"Measure[@Type='LM']/LM/{eye}/ADD", "LM", eye, "dpt", false, "LM ADD from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-prism", $"LM {eye} Prism", $"Measure[@Type='LM']/LM/{eye}/Prism", "LM", eye, "pdpt", false, "LM prism from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-pd", $"LM {eye} PD", $"Measure[@Type='LM']/LM/{eye}/PD", "LM", eye, "mm", false, "LM PD from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-medistar-line", $"LM {eye} MEDISTAR-Zeile", $"Measure[@Type='LM']/LM/{eye}/MedistarLine", "LM", eye, string.Empty, false, "Prepared MEDISTAR 6228 lensmeter line."));
            }
        }

        if (includeTono)
        {
            foreach (var eye in new[] { "R", "L" })
            {
                var eyePrefix = $"{prefix}-tono-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{eyePrefix}-value1", $"Tonometrie {eye} Wert 1", $"Measure[@Type='TM']/Tono/{eye}/Value1", "TM", eye, "mmHg", false, "First IOP value from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-value2", $"Tonometrie {eye} Wert 2", $"Measure[@Type='TM']/Tono/{eye}/Value2", "TM", eye, "mmHg", false, "Second IOP value from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-value3", $"Tonometrie {eye} Wert 3", $"Measure[@Type='TM']/Tono/{eye}/Value3", "TM", eye, "mmHg", false, "Third IOP value from Shin-Nippon text."));
                measurements.Add(new($"{eyePrefix}-average", $"Tonometrie {eye} Mittelwert", $"Measure[@Type='TM']/Tono/{eye}/Average", "TM", eye, "mmHg", false, "Average IOP value from Shin-Nippon text."));
            }

            measurements.Add(new($"{prefix}-tono-medistar-line", "Tonometrie MEDISTAR-Zeile", "Measure[@Type='TM']/Tono/TonoListLine", "TM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6205 tonometry line."));
        }

        return measurements;
    }

    public static DeviceProfileDefinition CreateReichert7CrNctDefault()
    {
        return CreateReichertDefault(
            id: "device-reichert-7cr-nct-default",
            name: "Reichert 7CR NCT",
            product: "7CR NCT",
            model: "7CR NCT",
            deviceType: "Tonometer",
            description: "Built-in serial text profile for Reichert 7CR NCT tonometry data derived from neutral reference parser rules. IOP values map to MEDISTAR 6205; practical raw-data validation remains open.",
            baudRate: 19200,
            includeLens: false,
            includeTono: true,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static DeviceProfileDefinition CreateReichertLensChekPlusDefault()
    {
        return CreateReichertDefault(
            id: "device-reichert-lenschek-plus-default",
            name: "Reichert LensChek Plus",
            product: "LensChek Plus",
            model: "LensChek Plus",
            deviceType: "Lensmeter",
            description: "Built-in serial text/XML profile for Reichert LensChek Plus lensmeter data derived from neutral reference parser rules. Lensmeter lines map to MEDISTAR 6228; practical raw-data validation remains open.",
            baudRate: 9600,
            includeLens: true,
            includeTono: false,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    private static DeviceProfileDefinition CreateReichertDefault(
        string id,
        string name,
        string product,
        string model,
        string deviceType,
        string description,
        int baudRate,
        bool includeLens,
        bool includeTono,
        DateTimeOffset timestamp)
    {
        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.DeviceProfile,
                Description: description,
                Vendor: "Reichert",
                Product: product,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "Reichert",
            Model: model,
            DeviceType: deviceType,
            ParserMode: ReichertDeviceParser.ParserMode,
            Measurements: CreateReichertMeasurements(product, includeLens, includeTono),
            SupportedExaminationTypes: CreateReichertSupportedExaminationTypes(includeLens, includeTono),
            CanContainMultipleExaminationTypes: false,
            IsBidirectional: false,
            DeviceImagePath: InterfaceProfileUiPolicy.GetBuiltInDeviceImagePathForDeviceProfileId(id),
            ConnectionKind: DeviceConnectionKind.SerialRs232,
            SerialSettings: new SerialCommunicationSettings(
                BaudRate: baudRate,
                DataBits: 8,
                StopBits: SerialStopBitsSetting.One,
                Parity: SerialParitySetting.None,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: false,
                RtsEnable: false,
                IsBidirectional: false,
                LineTerminator: includeTono ? SerialLineTerminatorSetting.CR : SerialLineTerminatorSetting.CRLF,
                ReadTimeoutMilliseconds: includeTono ? 30000 : 5000,
                WriteTimeoutMilliseconds: 1000));
    }

    private static IReadOnlyList<string> CreateReichertSupportedExaminationTypes(bool includeLens, bool includeTono)
    {
        var values = new List<string>();
        if (includeLens)
        {
            values.AddRange(new[] { "LM", "Lensmeter" });
        }

        if (includeTono)
        {
            values.AddRange(new[] { "TM", "Tonometrie" });
        }

        return values;
    }

    private static IReadOnlyList<DeviceMeasurementDefinition> CreateReichertMeasurements(
        string product,
        bool includeLens,
        bool includeTono)
    {
        var prefix = $"reichert-{product.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase).Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase).ToLowerInvariant()}";
        var measurements = new List<DeviceMeasurementDefinition>
        {
            new($"{prefix}-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "Reichert common company field."),
            new($"{prefix}-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "Reichert model name.")
        };

        if (includeLens)
        {
            foreach (var eye in new[] { "R", "L" })
            {
                var eyePrefix = $"{prefix}-lm-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{eyePrefix}-sphere", $"LM {eye} Sphere", $"Measure[@Type='LM']/LM/{eye}/Sphere", "LM", eye, "dpt", false, "Lensmeter sphere from Reichert data."));
                measurements.Add(new($"{eyePrefix}-cylinder", $"LM {eye} Cylinder", $"Measure[@Type='LM']/LM/{eye}/Cylinder", "LM", eye, "dpt", false, "Lensmeter cylinder from Reichert data."));
                measurements.Add(new($"{eyePrefix}-axis", $"LM {eye} Axis", $"Measure[@Type='LM']/LM/{eye}/Axis", "LM", eye, "deg", false, "Lensmeter axis from Reichert data."));
                measurements.Add(new($"{eyePrefix}-add", $"LM {eye} ADD", $"Measure[@Type='LM']/LM/{eye}/ADD", "LM", eye, "dpt", false, "Lensmeter ADD from Reichert data."));
                measurements.Add(new($"{eyePrefix}-prism", $"LM {eye} Prism", $"Measure[@Type='LM']/LM/{eye}/Prism", "LM", eye, "pdpt", false, "Lensmeter prism from Reichert data."));
                measurements.Add(new($"{eyePrefix}-pd", $"LM {eye} PD", $"Measure[@Type='LM']/LM/{eye}/PD", "LM", eye, "mm", false, "Lensmeter PD from Reichert data."));
                measurements.Add(new($"{eyePrefix}-medistar-line", $"LM {eye} MEDISTAR-Zeile", $"Measure[@Type='LM']/LM/{eye}/MedistarLine", "LM", eye, string.Empty, false, "Prepared MEDISTAR 6228 lensmeter line."));
            }
        }

        if (includeTono)
        {
            foreach (var eye in new[] { "R", "L" })
            {
                var eyePrefix = $"{prefix}-tono-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{eyePrefix}-iop", $"Tonometrie {eye} IOP", $"Measure[@Type='TM']/Tono/{eye}/IOP", "TM", eye, "mmHg", false, "IOP value from Reichert 7CR data."));
                measurements.Add(new($"{eyePrefix}-score", $"Tonometrie {eye} Score", $"Measure[@Type='TM']/Tono/{eye}/Score", "TM", eye, string.Empty, false, "Quality score from Reichert 7CR data."));
            }

            measurements.Add(new($"{prefix}-tono-medistar-line", "Tonometrie MEDISTAR-Zeile", "Measure[@Type='TM']/Tono/TonoListLine", "TM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6205 tonometry line."));
        }

        return measurements;
    }

    public static DeviceProfileDefinition CreateRodenstockCx800Default()
    {
        var id = "device-rodenstock-cx800-default";
        var timestamp = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: "Rodenstock CX 800",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Built-in serial text profile for Rodenstock CX 800 REF/KM data derived from neutral reference parser rules. REF maps to 6228 and KM to 6221; practical raw-data validation remains open.",
                Vendor: "Rodenstock",
                Product: "CX 800",
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "Rodenstock",
            Model: "CX 800",
            DeviceType: "Autorefraktor/Keratometer",
            ParserMode: RodenstockDeviceParser.ParserMode,
            Measurements: CreateRodenstockCx800Measurements(),
            SupportedExaminationTypes: new[] { "REF", "Autorefraktor", "KM", "Keratometer" },
            CanContainMultipleExaminationTypes: true,
            IsBidirectional: false,
            DeviceImagePath: InterfaceProfileUiPolicy.GetBuiltInDeviceImagePathForDeviceProfileId(id),
            ConnectionKind: DeviceConnectionKind.SerialRs232,
            SerialSettings: new SerialCommunicationSettings(
                BaudRate: 9600,
                DataBits: 8,
                StopBits: SerialStopBitsSetting.One,
                Parity: SerialParitySetting.None,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: false,
                RtsEnable: false,
                IsBidirectional: false,
                LineTerminator: SerialLineTerminatorSetting.None,
                ReadTimeoutMilliseconds: 5000,
                WriteTimeoutMilliseconds: 1000));
    }

    private static IReadOnlyList<DeviceMeasurementDefinition> CreateRodenstockCx800Measurements()
    {
        var prefix = "rodenstock-cx800";
        var measurements = new List<DeviceMeasurementDefinition>
        {
            new($"{prefix}-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "Rodenstock common company field."),
            new($"{prefix}-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "Rodenstock model name."),
            new($"{prefix}-ref-pd", "REF PD", "Measure[@Type='REF']/REF/PD", "REF", string.Empty, "mm", false, "REF PD from Rodenstock CX 800 data."),
            new($"{prefix}-ref-vd", "REF VD", "Measure[@Type='REF']/REF/VD", "REF", string.Empty, "mm", false, "REF VD from Rodenstock CX 800 data.")
        };

        foreach (var eye in new[] { "R", "L" })
        {
            var refPrefix = $"{prefix}-ref-{eye.ToLowerInvariant()}";
            measurements.Add(new($"{refPrefix}-sphere", $"REF {eye} Sphere", $"Measure[@Type='REF']/REF/{eye}/Sphere", "REF", eye, "dpt", false, "REF sphere from Rodenstock CX 800 data."));
            measurements.Add(new($"{refPrefix}-cylinder", $"REF {eye} Cylinder", $"Measure[@Type='REF']/REF/{eye}/Cylinder", "REF", eye, "dpt", false, "REF cylinder from Rodenstock CX 800 data."));
            measurements.Add(new($"{refPrefix}-axis", $"REF {eye} Axis", $"Measure[@Type='REF']/REF/{eye}/Axis", "REF", eye, "deg", false, "REF axis from Rodenstock CX 800 data."));
            measurements.Add(new($"{refPrefix}-medistar-line", $"REF {eye} MEDISTAR-Zeile", $"Measure[@Type='REF']/REF/{eye}/MedistarLine", "REF", eye, string.Empty, false, "Prepared MEDISTAR 6228 REF line."));

            var kmPrefix = $"{prefix}-km-{eye.ToLowerInvariant()}";
            measurements.Add(new($"{kmPrefix}-r1-radius", $"KM {eye} R1 Radius", $"Measure[@Type='KM']/KM/{eye}/R1/Radius", "KM", eye, "mm", false, "KM R1 radius from Rodenstock CX 800 data."));
            measurements.Add(new($"{kmPrefix}-r1-power", $"KM {eye} R1 Power", $"Measure[@Type='KM']/KM/{eye}/R1/Power", "KM", eye, "dpt", false, "KM R1 power from Rodenstock CX 800 data."));
            measurements.Add(new($"{kmPrefix}-r1-axis", $"KM {eye} R1 Axis", $"Measure[@Type='KM']/KM/{eye}/R1/Axis", "KM", eye, "deg", false, "KM R1 axis from Rodenstock CX 800 data."));
            measurements.Add(new($"{kmPrefix}-r2-radius", $"KM {eye} R2 Radius", $"Measure[@Type='KM']/KM/{eye}/R2/Radius", "KM", eye, "mm", false, "KM R2 radius from Rodenstock CX 800 data."));
            measurements.Add(new($"{kmPrefix}-r2-power", $"KM {eye} R2 Power", $"Measure[@Type='KM']/KM/{eye}/R2/Power", "KM", eye, "dpt", false, "KM R2 power from Rodenstock CX 800 data."));
            measurements.Add(new($"{kmPrefix}-r2-axis", $"KM {eye} R2 Axis", $"Measure[@Type='KM']/KM/{eye}/R2/Axis", "KM", eye, "deg", false, "KM R2 axis from Rodenstock CX 800 data."));
            measurements.Add(new($"{kmPrefix}-av-radius", $"KM {eye} AV Radius", $"Measure[@Type='KM']/KM/{eye}/AV/Radius", "KM", eye, "mm", false, "KM average radius from Rodenstock CX 800 data."));
            measurements.Add(new($"{kmPrefix}-av-power", $"KM {eye} AV Power", $"Measure[@Type='KM']/KM/{eye}/AV/Power", "KM", eye, "dpt", false, "KM average power from Rodenstock CX 800 data."));
            measurements.Add(new($"{kmPrefix}-cylinder", $"KM {eye} Cylinder", $"Measure[@Type='KM']/KM/{eye}/Cylinder", "KM", eye, "dpt", false, "KM cylinder from Rodenstock CX 800 data."));
            measurements.Add(new($"{kmPrefix}-cylinder-axis", $"KM {eye} Cylinder Axis", $"Measure[@Type='KM']/KM/{eye}/CylinderAxis", "KM", eye, "deg", false, "KM cylinder axis from Rodenstock CX 800 data."));
        }

        measurements.Add(new($"{prefix}-km-radii-line", "KM MEDISTAR R1/R2-Zeile", "Measure[@Type='KM']/KM/MedistarLine1", "KM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6221 KM R1/R2 line."));
        measurements.Add(new($"{prefix}-km-average-line", "KM MEDISTAR AV/CYL-Zeile", "Measure[@Type='KM']/KM/MedistarLine2", "KM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6221 KM average/cylinder line."));
        return measurements;
    }

    public static DeviceProfileDefinition CreateHuvitzHrk8000ADefault()
    {
        return CreateHuvitzTextSerialDefault(
            id: "device-huvitz-hrk8000a-default",
            name: "Huvitz HRK-8000A",
            product: "HRK-8000A",
            model: "HRK-8000A",
            deviceType: "Autorefraktor/Keratometer",
            description: "Built-in serial text profile for Huvitz HRK-8000A REF/KM data derived from neutral reference parser rules. REF is exported to 6228 and KM to 6221; practical raw-data validation remains open.",
            baudRate: 9600,
            includeRefKm: true,
            includeTonoPachy: false,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static DeviceProfileDefinition CreateHuvitzHrk9000ADefault()
    {
        return CreateHuvitzTextSerialDefault(
            id: "device-huvitz-hrk9000a-default",
            name: "Huvitz HRK-9000A",
            product: "HRK-9000A",
            model: "HRK-9000A",
            deviceType: "Autorefraktor/Keratometer",
            description: "Built-in serial text profile for Huvitz HRK-9000A REF/KM data derived from neutral reference parser rules. REF is exported to 6228 and KM to 6221; practical raw-data validation remains open.",
            baudRate: 9600,
            includeRefKm: true,
            includeTonoPachy: false,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static DeviceProfileDefinition CreateHuvitzHnt1PDefault()
    {
        return CreateHuvitzTextSerialDefault(
            id: "device-huvitz-hnt1p-default",
            name: "Huvitz HNT-1P",
            product: "HNT-1P",
            model: "HNT-1P",
            deviceType: "Tonometer/Pachymeter",
            description: "Built-in serial text profile for Huvitz HNT-1P tonometry and pachymetry data derived from neutral reference parser rules. IOP is exported to 6205 and CCT to 6220; practical raw-data validation remains open.",
            baudRate: 115200,
            includeRefKm: false,
            includeTonoPachy: true,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static DeviceProfileDefinition CreateHuvitzHtr1ADefault()
    {
        return CreateHuvitzTextSerialDefault(
            id: "device-huvitz-htr1a-default",
            name: "Huvitz HTR-1A",
            product: "HTR-1A",
            model: "HTR-1A",
            deviceType: "Autorefraktor/Keratometer/Tonometer/Pachymeter",
            description: "Built-in serial text profile for Huvitz HTR-1A combined REF/KM/IOP/CCT data derived from neutral reference parser rules. REF maps to 6228, KM to 6221, IOP to 6205 and CCT to 6220; practical raw-data validation remains open.",
            baudRate: 9600,
            includeRefKm: true,
            includeTonoPachy: true,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    private static DeviceProfileDefinition CreateHuvitzTextSerialDefault(
        string id,
        string name,
        string product,
        string model,
        string deviceType,
        string description,
        int baudRate,
        bool includeRefKm,
        bool includeTonoPachy,
        DateTimeOffset timestamp)
    {
        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.DeviceProfile,
                Description: description,
                Vendor: "Huvitz",
                Product: product,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "Huvitz",
            Model: model,
            DeviceType: deviceType,
            ParserMode: HuvitzTextDeviceParser.ParserMode,
            Measurements: CreateHuvitzTextMeasurements(product, includeRefKm, includeTonoPachy),
            SupportedExaminationTypes: CreateHuvitzSupportedExaminationTypes(includeRefKm, includeTonoPachy),
            CanContainMultipleExaminationTypes: includeRefKm && includeTonoPachy,
            IsBidirectional: false,
            DeviceImagePath: InterfaceProfileUiPolicy.GetBuiltInDeviceImagePathForDeviceProfileId(id),
            ConnectionKind: DeviceConnectionKind.SerialRs232,
            SerialSettings: new SerialCommunicationSettings(
                BaudRate: baudRate,
                DataBits: 8,
                StopBits: SerialStopBitsSetting.One,
                Parity: SerialParitySetting.None,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: false,
                RtsEnable: false,
                IsBidirectional: false,
                LineTerminator: SerialLineTerminatorSetting.CRLF,
                ReadTimeoutMilliseconds: 5000,
                WriteTimeoutMilliseconds: 1000));
    }

    private static IReadOnlyList<string> CreateHuvitzSupportedExaminationTypes(bool includeRefKm, bool includeTonoPachy)
    {
        var values = new List<string>();
        if (includeRefKm)
        {
            values.AddRange(new[] { "REF", "Autorefraktor", "KM", "Keratometer" });
        }

        if (includeTonoPachy)
        {
            values.AddRange(new[] { "TM", "Tonometrie", "CCT", "Pachymetrie" });
        }

        return values;
    }

    private static IReadOnlyList<DeviceMeasurementDefinition> CreateHuvitzTextMeasurements(
        string product,
        bool includeRefKm,
        bool includeTonoPachy)
    {
        var prefix = $"huvitz-{product.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase).ToLowerInvariant()}";
        var measurements = new List<DeviceMeasurementDefinition>
        {
            new($"{prefix}-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "Huvitz common company field."),
            new($"{prefix}-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "Huvitz model name when present in the text payload.")
        };

        if (includeRefKm)
        {
            foreach (var eye in new[] { "R", "L" })
            {
                var eyePrefix = $"{prefix}-ref-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{eyePrefix}-sphere", $"REF {eye} Sphere", $"Measure[@Type='REF']/REF/{eye}/Sphere", "REF", eye, "dpt", false, "REF sphere from Huvitz serial text."));
                measurements.Add(new($"{eyePrefix}-cylinder", $"REF {eye} Cylinder", $"Measure[@Type='REF']/REF/{eye}/Cylinder", "REF", eye, "dpt", false, "REF cylinder from Huvitz serial text."));
                measurements.Add(new($"{eyePrefix}-axis", $"REF {eye} Axis", $"Measure[@Type='REF']/REF/{eye}/Axis", "REF", eye, "deg", false, "REF axis from Huvitz serial text."));
                measurements.Add(new($"{eyePrefix}-pd", $"REF {eye} PD", $"Measure[@Type='REF']/REF/{eye}/PD", "REF", eye, "mm", false, "REF PD from Huvitz serial text when present."));
                measurements.Add(new($"{eyePrefix}-medistar-line", $"REF {eye} MEDISTAR-Zeile", $"Measure[@Type='REF']/REF/{eye}/MedistarLine", "REF", eye, string.Empty, false, "Prepared MEDISTAR 6228 REF line."));

                var kmPrefix = $"{prefix}-km-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{kmPrefix}-r1", $"KM {eye} R1", $"Measure[@Type='KM']/KM/{eye}/Radius1", "KM", eye, "mm", false, "KM radius 1 from Huvitz serial text."));
                measurements.Add(new($"{kmPrefix}-r2", $"KM {eye} R2", $"Measure[@Type='KM']/KM/{eye}/Radius2", "KM", eye, "mm", false, "KM radius 2 from Huvitz serial text."));
                measurements.Add(new($"{kmPrefix}-axis", $"KM {eye} Axis", $"Measure[@Type='KM']/KM/{eye}/Axis", "KM", eye, "deg", false, "KM axis from Huvitz serial text."));
            }

            measurements.Add(new($"{prefix}-km-medistar-line", "KM MEDISTAR-Zeile", "Measure[@Type='KM']/KM/MedistarLine1", "KM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6221 KM line."));
        }

        if (includeTonoPachy)
        {
            foreach (var eye in new[] { "R", "L" })
            {
                var tonoPrefix = $"{prefix}-tono-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{tonoPrefix}-value1", $"Tonometrie {eye} Wert 1", $"Measure[@Type='TM']/Tono/{eye}/Value1", "TM", eye, "mmHg", false, "First IOP value from Huvitz serial text."));
                measurements.Add(new($"{tonoPrefix}-value2", $"Tonometrie {eye} Wert 2", $"Measure[@Type='TM']/Tono/{eye}/Value2", "TM", eye, "mmHg", false, "Second IOP value from Huvitz serial text."));
                measurements.Add(new($"{tonoPrefix}-value3", $"Tonometrie {eye} Wert 3", $"Measure[@Type='TM']/Tono/{eye}/Value3", "TM", eye, "mmHg", false, "Third IOP value from Huvitz serial text."));
                measurements.Add(new($"{tonoPrefix}-average", $"Tonometrie {eye} Mittelwert", $"Measure[@Type='TM']/Tono/{eye}/Average", "TM", eye, "mmHg", false, "Average IOP value from Huvitz serial text."));

                var pachyPrefix = $"{prefix}-pachy-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{pachyPrefix}-value1", $"Pachymetrie {eye} Wert 1", $"Measure[@Type='CCT']/Pachy/{eye}/Value1", "CCT", eye, "um", false, "First CCT value from Huvitz serial text."));
                measurements.Add(new($"{pachyPrefix}-value2", $"Pachymetrie {eye} Wert 2", $"Measure[@Type='CCT']/Pachy/{eye}/Value2", "CCT", eye, "um", false, "Second CCT value from Huvitz serial text."));
                measurements.Add(new($"{pachyPrefix}-value3", $"Pachymetrie {eye} Wert 3", $"Measure[@Type='CCT']/Pachy/{eye}/Value3", "CCT", eye, "um", false, "Third CCT value from Huvitz serial text."));
                measurements.Add(new($"{pachyPrefix}-average", $"Pachymetrie {eye} Mittelwert", $"Measure[@Type='CCT']/Pachy/{eye}/Average", "CCT", eye, "um", false, "Average CCT value from Huvitz serial text."));
            }

            measurements.Add(new($"{prefix}-tono-medistar-line", "Tonometrie MEDISTAR-Zeile", "Measure[@Type='TM']/Tono/TonoListLine", "TM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6205 tonometry line."));
            measurements.Add(new($"{prefix}-pachy-medistar-line", "Pachymetrie MEDISTAR-Zeile", "Measure[@Type='CCT']/Pachy/MedistarLine", "CCT", string.Empty, string.Empty, false, "Prepared MEDISTAR 6220 pachymetry line."));
        }

        return measurements;
    }

    public static DeviceProfileDefinition CreateTomeyCf2000Default()
    {
        return CreateTomeyDefault(
            id: "device-tomey-cf2000-default",
            name: "TOMEY CF-2000",
            product: "CF-2000",
            model: "CF-2000",
            deviceType: "Lensmeter",
            description: "Built-in serial text profile for TOMEY CF-2000 lensmeter data derived from neutral reference parser rules. Lensmeter lines map to 6228; practical raw-data validation remains open.",
            connectionKind: DeviceConnectionKind.SerialRs232,
            serialSettings: new SerialCommunicationSettings(
                BaudRate: 9600,
                DataBits: 8,
                StopBits: SerialStopBitsSetting.One,
                Parity: SerialParitySetting.Odd,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: false,
                RtsEnable: false,
                IsBidirectional: false,
                LineTerminator: SerialLineTerminatorSetting.CR,
                ReadTimeoutMilliseconds: 5000,
                WriteTimeoutMilliseconds: 1000),
            includeLens: true,
            includeRef: false,
            includeKm: false,
            includeTonoPachy: false,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static DeviceProfileDefinition CreateTomeyTl2000CDefault()
    {
        return CreateTomeyLensFileDefault("device-tomey-tl2000c-default", "TOMEY TL-2000C", "TL-2000C");
    }

    public static DeviceProfileDefinition CreateTomeyTl6000Default()
    {
        return CreateTomeyLensFileDefault("device-tomey-tl6000-default", "TOMEY TL-6000", "TL-6000");
    }

    public static DeviceProfileDefinition CreateTomeyTl7000Default()
    {
        return CreateTomeyLensFileDefault("device-tomey-tl7000-default", "TOMEY TL-7000", "TL-7000");
    }

    public static DeviceProfileDefinition CreateTomeyMr6000Default()
    {
        return CreateTomeyDefault(
            id: "device-tomey-mr6000-default",
            name: "TOMEY MR-6000",
            product: "MR-6000",
            model: "MR-6000",
            deviceType: "Autorefraktor/Keratometer/Tonometer/Pachymeter",
            description: "Built-in XML profile for TOMEY MR-6000 combined REF/KM/IOP/CCT data derived from neutral reference parser rules. REF maps to 6228, KM to 6221, IOP to 6205 and CCT to 6220; practical raw-data validation remains open.",
            connectionKind: DeviceConnectionKind.FileImport,
            serialSettings: null,
            includeLens: false,
            includeRef: true,
            includeKm: true,
            includeTonoPachy: true,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static DeviceProfileDefinition CreateTomeyTop1000Default()
    {
        return CreateTomeyDefault(
            id: "device-tomey-top1000-default",
            name: "TOMEY TOP-1000",
            product: "TOP-1000",
            model: "TOP-1000",
            deviceType: "Tonometer/Pachymeter",
            description: "Built-in XML profile for TOMEY TOP-1000 tonometry and pachymetry data derived from neutral reference parser rules. IOP maps to 6205 and CCT to 6220; practical raw-data validation remains open.",
            connectionKind: DeviceConnectionKind.FileImport,
            serialSettings: null,
            includeLens: false,
            includeRef: false,
            includeKm: false,
            includeTonoPachy: true,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static DeviceProfileDefinition CreateTomeyEm3000Default()
    {
        return CreateTomeyEmDefault(
            id: "device-tomey-em3000-default",
            name: "TOMEY EM-3000",
            product: "EM-3000",
            model: "EM-3000",
            description: "Built-in file profile for TOMEY EM-3000 endothelial CSV data derived from neutral reference parser rules. Measurements map to 6228, comments to 6227 and image references to 6302; practical raw-data validation remains open.");
    }

    public static DeviceProfileDefinition CreateTomeyEm4000Default()
    {
        return CreateTomeyEmDefault(
            id: "device-tomey-em4000-default",
            name: "TOMEY EM-4000",
            product: "EM-4000",
            model: "EM-4000",
            description: "Built-in file profile for TOMEY EM-4000 endothelial CSV data derived from neutral reference parser rules. Measurements map to 6228, comments to 6227 and image references to 6302; practical raw-data validation remains open.");
    }

    private static DeviceProfileDefinition CreateTomeyLensFileDefault(string id, string name, string model)
    {
        return CreateTomeyDefault(
            id: id,
            name: name,
            product: model,
            model: model,
            deviceType: "Lensmeter",
            description: $"Built-in file profile for TOMEY {model} lensmeter CSV data derived from neutral reference parser rules. Lensmeter lines map to 6228; practical raw-data validation remains open.",
            connectionKind: DeviceConnectionKind.FileImport,
            serialSettings: null,
            includeLens: true,
            includeRef: false,
            includeKm: false,
            includeTonoPachy: false,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    private static DeviceProfileDefinition CreateTomeyEmDefault(
        string id,
        string name,
        string product,
        string model,
        string description)
    {
        var timestamp = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);
        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.DeviceProfile,
                Description: description,
                Vendor: "TOMEY",
                Product: product,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "TOMEY",
            Model: model,
            DeviceType: "Endothelmikroskop/Zellmessgerät",
            ParserMode: TomeyEmDeviceParser.ParserMode,
            Measurements: CreateTomeyEmMeasurements(product),
            SupportedExaminationTypes: new[] { "EM", "Endothel", "Zellmessung", "CCT" },
            CanContainMultipleExaminationTypes: true,
            IsBidirectional: false,
            DeviceImagePath: InterfaceProfileUiPolicy.GetBuiltInDeviceImagePathForDeviceProfileId(id),
            ConnectionKind: DeviceConnectionKind.FileImport);
    }

    private static DeviceProfileDefinition CreateTomeyDefault(
        string id,
        string name,
        string product,
        string model,
        string deviceType,
        string description,
        DeviceConnectionKind connectionKind,
        SerialCommunicationSettings? serialSettings,
        bool includeLens,
        bool includeRef,
        bool includeKm,
        bool includeTonoPachy,
        DateTimeOffset timestamp)
    {
        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.DeviceProfile,
                Description: description,
                Vendor: "TOMEY",
                Product: product,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "TOMEY",
            Model: model,
            DeviceType: deviceType,
            ParserMode: TomeyDeviceParser.ParserMode,
            Measurements: CreateTomeyMeasurements(product, includeLens, includeRef, includeKm, includeTonoPachy),
            SupportedExaminationTypes: CreateTomeySupportedExaminationTypes(includeLens, includeRef, includeKm, includeTonoPachy),
            CanContainMultipleExaminationTypes: (includeRef && includeKm) || includeTonoPachy,
            IsBidirectional: false,
            DeviceImagePath: InterfaceProfileUiPolicy.GetBuiltInDeviceImagePathForDeviceProfileId(id),
            ConnectionKind: connectionKind,
            SerialSettings: serialSettings);
    }

    private static IReadOnlyList<string> CreateTomeySupportedExaminationTypes(
        bool includeLens,
        bool includeRef,
        bool includeKm,
        bool includeTonoPachy)
    {
        var values = new List<string>();
        if (includeLens)
        {
            values.AddRange(new[] { "LM", "Lensmeter" });
        }

        if (includeRef)
        {
            values.AddRange(new[] { "REF", "Autorefraktor" });
        }

        if (includeKm)
        {
            values.AddRange(new[] { "KM", "Keratometer" });
        }

        if (includeTonoPachy)
        {
            values.AddRange(new[] { "TM", "Tonometrie", "CCT", "Pachymetrie" });
        }

        return values;
    }

    private static IReadOnlyList<DeviceMeasurementDefinition> CreateTomeyMeasurements(
        string product,
        bool includeLens,
        bool includeRef,
        bool includeKm,
        bool includeTonoPachy)
    {
        var prefix = $"tomey-{product.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase).ToLowerInvariant()}";
        var measurements = new List<DeviceMeasurementDefinition>
        {
            new($"{prefix}-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "TOMEY common company field."),
            new($"{prefix}-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "TOMEY model name.")
        };

        if (includeLens)
        {
            foreach (var eye in new[] { "R", "L" })
            {
                var eyePrefix = $"{prefix}-lm-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{eyePrefix}-sphere", $"LM {eye} Sphere", $"Measure[@Type='LM']/LM/{eye}/Sphere", "LM", eye, "dpt", false, "Lensmeter sphere from TOMEY data."));
                measurements.Add(new($"{eyePrefix}-cylinder", $"LM {eye} Cylinder", $"Measure[@Type='LM']/LM/{eye}/Cylinder", "LM", eye, "dpt", false, "Lensmeter cylinder from TOMEY data."));
                measurements.Add(new($"{eyePrefix}-axis", $"LM {eye} Axis", $"Measure[@Type='LM']/LM/{eye}/Axis", "LM", eye, "deg", false, "Lensmeter axis from TOMEY data."));
                measurements.Add(new($"{eyePrefix}-add", $"LM {eye} ADD", $"Measure[@Type='LM']/LM/{eye}/ADD", "LM", eye, "dpt", false, "Lensmeter addition from TOMEY data."));
                measurements.Add(new($"{eyePrefix}-add2", $"LM {eye} ADD2", $"Measure[@Type='LM']/LM/{eye}/ADD2", "LM", eye, "dpt", false, "Second lensmeter addition from TOMEY data when present."));
                measurements.Add(new($"{eyePrefix}-pd", $"LM {eye} PD", $"Measure[@Type='LM']/LM/{eye}/PD", "LM", eye, "mm", false, "Lensmeter PD from TOMEY data when present."));
                measurements.Add(new($"{eyePrefix}-medistar-line", $"LM {eye} MEDISTAR-Zeile", $"Measure[@Type='LM']/LM/{eye}/MedistarLine", "LM", eye, string.Empty, false, "Prepared MEDISTAR 6228 lensmeter line."));
            }
        }

        if (includeRef)
        {
            foreach (var eye in new[] { "R", "L" })
            {
                var eyePrefix = $"{prefix}-ref-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{eyePrefix}-sphere", $"REF {eye} Sphere", $"Measure[@Type='REF']/REF/{eye}/Sphere", "REF", eye, "dpt", false, "REF sphere from TOMEY data."));
                measurements.Add(new($"{eyePrefix}-cylinder", $"REF {eye} Cylinder", $"Measure[@Type='REF']/REF/{eye}/Cylinder", "REF", eye, "dpt", false, "REF cylinder from TOMEY data."));
                measurements.Add(new($"{eyePrefix}-axis", $"REF {eye} Axis", $"Measure[@Type='REF']/REF/{eye}/Axis", "REF", eye, "deg", false, "REF axis from TOMEY data."));
                measurements.Add(new($"{eyePrefix}-medistar-line", $"REF {eye} MEDISTAR-Zeile", $"Measure[@Type='REF']/REF/{eye}/MedistarLine", "REF", eye, string.Empty, false, "Prepared MEDISTAR 6228 REF line."));
            }
        }

        if (includeKm)
        {
            foreach (var eye in new[] { "R", "L" })
            {
                var eyePrefix = $"{prefix}-km-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{eyePrefix}-r1-radius", $"KM {eye} R1 Radius", $"Measure[@Type='KM']/KM/{eye}/R1/Radius", "KM", eye, "mm", false, "KM R1 radius from TOMEY data."));
                measurements.Add(new($"{eyePrefix}-r1-power", $"KM {eye} R1 Power", $"Measure[@Type='KM']/KM/{eye}/R1/Power", "KM", eye, "dpt", false, "KM R1 power from TOMEY data."));
                measurements.Add(new($"{eyePrefix}-r2-radius", $"KM {eye} R2 Radius", $"Measure[@Type='KM']/KM/{eye}/R2/Radius", "KM", eye, "mm", false, "KM R2 radius from TOMEY data."));
                measurements.Add(new($"{eyePrefix}-r2-power", $"KM {eye} R2 Power", $"Measure[@Type='KM']/KM/{eye}/R2/Power", "KM", eye, "dpt", false, "KM R2 power from TOMEY data."));
            }

            measurements.Add(new($"{prefix}-km-radii-line", "KM MEDISTAR R1/R2-Zeile", "Measure[@Type='KM']/KM/MedistarLine1", "KM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6221 KM R1/R2 line."));
            measurements.Add(new($"{prefix}-km-average-line", "KM MEDISTAR AV/CYL-Zeile", "Measure[@Type='KM']/KM/MedistarLine2", "KM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6221 KM average/cylinder line."));
        }

        if (includeTonoPachy)
        {
            foreach (var eye in new[] { "R", "L" })
            {
                var tonoPrefix = $"{prefix}-tono-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{tonoPrefix}-average", $"Tonometrie {eye} Mittelwert", $"Measure[@Type='TM']/Tono/{eye}/Average", "TM", eye, "mmHg", false, "Average IOP value from TOMEY data."));
                var pachyPrefix = $"{prefix}-pachy-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{pachyPrefix}-average", $"Pachymetrie {eye} Mittelwert", $"Measure[@Type='CCT']/Pachy/{eye}/Average", "CCT", eye, "um", false, "Average CCT value from TOMEY data."));
            }

            measurements.Add(new($"{prefix}-tono-line", "Tonometrie MEDISTAR-Zeile", "Measure[@Type='TM']/Tono/TonoListLine", "TM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6205 tonometry line."));
            measurements.Add(new($"{prefix}-tono-corrected-line", "Tonometrie Korrektur MEDISTAR-Zeile", "Measure[@Type='TM']/Tono/CorrectedLine", "TM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6205 corrected tonometry line."));
            measurements.Add(new($"{prefix}-pachy-line", "Pachymetrie MEDISTAR-Zeile", "Measure[@Type='CCT']/Pachy/MedistarLine", "CCT", string.Empty, string.Empty, false, "Prepared MEDISTAR 6220 pachymetry line."));
        }

        return measurements;
    }

    private static IReadOnlyList<DeviceMeasurementDefinition> CreateTomeyEmMeasurements(string product)
    {
        var prefix = $"tomey-{product.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase).ToLowerInvariant()}";
        var measurements = new List<DeviceMeasurementDefinition>
        {
            new($"{prefix}-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "TOMEY common company field."),
            new($"{prefix}-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "TOMEY model name."),
            new($"{prefix}-patient-id", "Geräte-Patient-ID", "Patient/Id", "Patient", string.Empty, string.Empty, false, "Patient id from TOMEY EM CSV when present."),
            new($"{prefix}-comment-line", "Endothel Kommentar MEDISTAR-Zeile", "Measure[@Type='EM']/Comment/MedistarLine", "EM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6227 endothelial comment line.")
        };

        foreach (var eye in new[] { "R", "L" })
        {
            var eyePrefix = $"{prefix}-em-{eye.ToLowerInvariant()}";
            measurements.Add(new($"{eyePrefix}-number", $"Endothel {eye} Anzahl", $"Measure[@Type='EM']/Endothelium/{eye}/Number", "EM", eye, string.Empty, false, "EM-3000 endothelial cell count."));
            measurements.Add(new($"{eyePrefix}-density", $"Endothel {eye} Dichte", $"Measure[@Type='EM']/Endothelium/{eye}/Density", "EM", eye, "mm2", false, "EM-3000 endothelial density."));
            measurements.Add(new($"{eyePrefix}-thickness", $"Endothel {eye} Hornhautdicke", $"Measure[@Type='EM']/Endothelium/{eye}/Thickness", "EM", eye, "um", false, "EM-3000 corneal thickness."));
            measurements.Add(new($"{eyePrefix}-em3000-line", $"Endothel {eye} MEDISTAR-Zeile", $"Measure[@Type='EM']/Endothelium/{eye}/MedistarLine", "EM", eye, string.Empty, false, "Prepared MEDISTAR 6228 EM-3000 endothelial result line."));
            measurements.Add(new($"{eyePrefix}-cd", $"Endothel {eye} CD", $"Measure[@Type='EM']/Endothelium/{eye}/CellDensity", "EM", eye, string.Empty, false, "EM-4000 endothelial CD value."));
            measurements.Add(new($"{eyePrefix}-cct", $"Endothel {eye} CCT", $"Measure[@Type='EM']/Endothelium/{eye}/CCT", "EM", eye, string.Empty, false, "EM-4000 endothelial CCT value."));
            measurements.Add(new($"{eyePrefix}-cd-line", $"Endothel {eye} CD MEDISTAR-Zeile", $"Measure[@Type='EM']/Endothelium/{eye}/CellDensity/MedistarLine", "EM", eye, string.Empty, false, "Prepared MEDISTAR 6228 EM-4000 CD line."));
            measurements.Add(new($"{eyePrefix}-cct-line", $"Endothel {eye} CCT MEDISTAR-Zeile", $"Measure[@Type='EM']/Endothelium/{eye}/CCT/MedistarLine", "EM", eye, string.Empty, false, "Prepared MEDISTAR 6228 EM-4000 CCT line."));
        }

        for (var index = 1; index <= 4; index++)
        {
            var imagePrefix = $"{prefix}-attachment-image{index}";
            measurements.Add(new($"{imagePrefix}-path", $"Endothel Bild {index} Pfad", $"Measure[@Type='EM']/Attachment/Image{index}/Path", "EM", string.Empty, string.Empty, false, "Endothelial image path."));
            measurements.Add(new($"{imagePrefix}-line", $"Endothel Bild {index} Verweis", $"Measure[@Type='EM']/Attachment/Image{index}/MedistarLine", "EM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6302 image reference line."));
        }

        return measurements;
    }

    public static DeviceProfileDefinition CreateCanonRkF2Default()
    {
        return CreateCanonZeissVisionixDefault(
            id: "device-canon-rkf2-default",
            name: "Canon RK-F2",
            manufacturer: "Canon",
            product: "RK-F2",
            model: "RK-F2",
            deviceType: "Autorefraktor",
            description: "Built-in Canon RK-F2 XML REF profile derived from reference XPath rules. REF maps to 6228; KM is not enabled without a confirmed target rule.",
            connectionKind: DeviceConnectionKind.FileImport,
            serialSettings: null,
            includeRef: true,
            includeKm: false,
            includeLens: false,
            includeTono: false,
            includePachy: false);
    }

    public static DeviceProfileDefinition CreateCanonTx20PDefault()
    {
        return CreateCanonZeissVisionixDefault(
            id: "device-canon-tx20p-default",
            name: "Canon TX-20P",
            manufacturer: "Canon",
            product: "TX-20P",
            model: "TX-20P",
            deviceType: "Tonometer/Pachymeter",
            description: "Built-in Canon TX-20P XML profile derived from reference XML scheme rules. IOP maps to 6205 and CCT/Pachy maps to 6220.",
            connectionKind: DeviceConnectionKind.FileImport,
            serialSettings: null,
            includeRef: false,
            includeKm: false,
            includeLens: false,
            includeTono: true,
            includePachy: true);
    }

    public static DeviceProfileDefinition CreateZeissVisulens550Default()
    {
        return CreateCanonZeissVisionixDefault(
            id: "device-zeiss-visulens550-default",
            name: "ZEISS VISULENS 550",
            manufacturer: "ZEISS",
            product: "VISULENS 550",
            model: "VISULENS 550",
            deviceType: "Lensmeter",
            description: "Built-in ZEISS VISULENS 550 XML lensmeter profile derived from reference XPath rules. LM maps to 6228.",
            connectionKind: DeviceConnectionKind.FileImport,
            serialSettings: null,
            includeRef: false,
            includeKm: false,
            includeLens: true,
            includeTono: false,
            includePachy: false);
    }

    public static DeviceProfileDefinition CreateZeissVisuplan500Default()
    {
        return CreateCanonZeissVisionixDefault(
            id: "device-zeiss-visuplan500-default",
            name: "ZEISS VISUPLAN 500",
            manufacturer: "ZEISS",
            product: "VISUPLAN 500",
            model: "VISUPLAN 500",
            deviceType: "Tonometer",
            description: "Built-in ZEISS VISUPLAN 500 serial text tonometry profile derived from reference parser rules. IOP maps to 6205; practical raw-data validation remains open.",
            connectionKind: DeviceConnectionKind.SerialRs232,
            serialSettings: new SerialCommunicationSettings(
                BaudRate: 19200,
                DataBits: 8,
                StopBits: SerialStopBitsSetting.One,
                Parity: SerialParitySetting.None,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: false,
                RtsEnable: false,
                IsBidirectional: false,
                LineTerminator: SerialLineTerminatorSetting.CRLF,
                ReadTimeoutMilliseconds: 5000,
                WriteTimeoutMilliseconds: 1000),
            includeRef: false,
            includeKm: false,
            includeLens: false,
            includeTono: true,
            includePachy: false);
    }

    public static DeviceProfileDefinition CreateZeissVisuref100Default()
    {
        return CreateCanonZeissVisionixDefault(
            id: "device-zeiss-visuref100-default",
            name: "ZEISS VISUREF 100",
            manufacturer: "ZEISS",
            product: "VISUREF 100",
            model: "VISUREF 100",
            deviceType: "Autorefraktor/Keratometer",
            description: "Built-in ZEISS VISUREF 100 serial text REF/KM profile derived from reference parser rules. REF maps to 6228 and KM maps to 6221; practical raw-data validation remains open.",
            connectionKind: DeviceConnectionKind.SerialRs232,
            serialSettings: new SerialCommunicationSettings(
                BaudRate: 9600,
                DataBits: 8,
                StopBits: SerialStopBitsSetting.One,
                Parity: SerialParitySetting.None,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: false,
                RtsEnable: false,
                IsBidirectional: false,
                LineTerminator: SerialLineTerminatorSetting.CR,
                ReadTimeoutMilliseconds: 5000,
                WriteTimeoutMilliseconds: 1000),
            includeRef: true,
            includeKm: true,
            includeLens: false,
            includeTono: false,
            includePachy: false);
    }

    public static DeviceProfileDefinition CreateZeissIolMaster700Default()
    {
        var timestamp = new DateTimeOffset(2026, 6, 6, 12, 0, 0, TimeSpan.Zero);
        var id = "device-zeiss-iolmaster700-default";
        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: "ZEISS IOLMaster 700",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Built-in ZEISS IOLMaster 700 XML profile derived from reference XPath rules. Biometry maps to 6227 and keratometry maps to 6228.",
                Vendor: "ZEISS",
                Product: "IOLMaster 700",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "ZEISS",
            Model: "IOLMaster 700",
            DeviceType: "Biometrie/IOL/Keratometer",
            ParserMode: ZeissIolMaster700DeviceParser.ParserMode,
            Measurements: CreateZeissIolMaster700Measurements(id),
            SupportedExaminationTypes: new[] { "IOL", "BIOM", "Biometrie", "Achslaenge", "KM", "Keratometer" },
            CanContainMultipleExaminationTypes: true,
            IsBidirectional: false,
            DeviceImagePath: InterfaceProfileUiPolicy.GetBuiltInDeviceImagePathForDeviceProfileId(id),
            ConnectionKind: DeviceConnectionKind.FileImport,
            SerialSettings: null);
    }

    public static DeviceProfileDefinition CreateVisionixRetinomax5Default()
    {
        return CreateCanonZeissVisionixDefault(
            id: "device-visionix-retinomax5-default",
            name: "Visionix Retinomax 5",
            manufacturer: "Visionix",
            product: "Retinomax 5",
            model: "Retinomax 5",
            deviceType: "Autorefraktor/Keratometer",
            description: "Built-in Visionix Retinomax 5 serial text REF/KM profile derived from reference parser rules. REF maps to 6228 and KM maps to 6221; practical raw-data validation remains open.",
            connectionKind: DeviceConnectionKind.SerialRs232,
            serialSettings: new SerialCommunicationSettings(
                BaudRate: 115200,
                DataBits: 8,
                StopBits: SerialStopBitsSetting.One,
                Parity: SerialParitySetting.None,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: false,
                RtsEnable: false,
                IsBidirectional: false,
                LineTerminator: SerialLineTerminatorSetting.CRLF,
                ReadTimeoutMilliseconds: 5000,
                WriteTimeoutMilliseconds: 1000),
            includeRef: true,
            includeKm: true,
            includeLens: false,
            includeTono: false,
            includePachy: false);
    }

    public static DeviceProfileDefinition CreateVisionixVx120Default()
    {
        return CreateCanonZeissVisionixDefault(
            id: "device-visionix-vx120-default",
            name: "Visionix VX 120",
            manufacturer: "Visionix",
            product: "VX 120",
            model: "VX 120",
            deviceType: "Autorefraktor",
            description: "Built-in Visionix VX 120 XML REF profile derived from reference XPath rules. Only clearly mapped REF values are exported with 6228.",
            connectionKind: DeviceConnectionKind.FileImport,
            serialSettings: null,
            includeRef: true,
            includeKm: false,
            includeLens: false,
            includeTono: false,
            includePachy: false);
    }

    public static DeviceProfileDefinition CreateVisionixVx650Default()
    {
        return CreateCanonZeissVisionixDefault(
            id: "device-visionix-vx650-default",
            name: "Visionix VX 650",
            manufacturer: "Visionix",
            product: "VX 650",
            model: "VX 650",
            deviceType: "Autorefraktor/Tonometer",
            description: "Built-in Visionix VX 650 XML REF/TM profile derived from reference XPath rules. REF maps to 6228 and tonometry maps to 6205.",
            connectionKind: DeviceConnectionKind.FileImport,
            serialSettings: null,
            includeRef: true,
            includeKm: false,
            includeLens: false,
            includeTono: true,
            includePachy: false);
    }

    private static DeviceProfileDefinition CreateCanonZeissVisionixDefault(
        string id,
        string name,
        string manufacturer,
        string product,
        string model,
        string deviceType,
        string description,
        DeviceConnectionKind connectionKind,
        SerialCommunicationSettings? serialSettings,
        bool includeRef,
        bool includeKm,
        bool includeLens,
        bool includeTono,
        bool includePachy)
    {
        var timestamp = new DateTimeOffset(2026, 6, 6, 12, 0, 0, TimeSpan.Zero);
        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.DeviceProfile,
                Description: description,
                Vendor: manufacturer,
                Product: product,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: manufacturer,
            Model: model,
            DeviceType: deviceType,
            ParserMode: CanonZeissVisionixDeviceParser.ParserMode,
            Measurements: CreateCanonZeissVisionixMeasurements(id, manufacturer, includeRef, includeKm, includeLens, includeTono, includePachy),
            SupportedExaminationTypes: CreateCanonZeissVisionixSupportedExaminationTypes(includeRef, includeKm, includeLens, includeTono, includePachy),
            CanContainMultipleExaminationTypes: new[] { includeRef, includeKm, includeLens, includeTono, includePachy }.Count(value => value) > 1,
            IsBidirectional: false,
            DeviceImagePath: InterfaceProfileUiPolicy.GetBuiltInDeviceImagePathForDeviceProfileId(id),
            ConnectionKind: connectionKind,
            SerialSettings: serialSettings);
    }

    private static IReadOnlyList<string> CreateCanonZeissVisionixSupportedExaminationTypes(
        bool includeRef,
        bool includeKm,
        bool includeLens,
        bool includeTono,
        bool includePachy)
    {
        var values = new List<string>();
        if (includeRef)
        {
            values.AddRange(new[] { "REF", "AUTO", "Refraktion" });
        }

        if (includeKm)
        {
            values.AddRange(new[] { "KM", "Keratometer" });
        }

        if (includeLens)
        {
            values.AddRange(new[] { "LM", "Lensmeter" });
        }

        if (includeTono)
        {
            values.AddRange(new[] { "TM", "Tonometrie" });
        }

        if (includePachy)
        {
            values.AddRange(new[] { "CCT", "Pachymetrie" });
        }

        return values;
    }

    private static IReadOnlyList<DeviceMeasurementDefinition> CreateCanonZeissVisionixMeasurements(
        string id,
        string manufacturer,
        bool includeRef,
        bool includeKm,
        bool includeLens,
        bool includeTono,
        bool includePachy)
    {
        var prefix = id.Replace("device-", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("-default", string.Empty, StringComparison.OrdinalIgnoreCase);
        var measurements = new List<DeviceMeasurementDefinition>
        {
            new($"{prefix}-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, $"{manufacturer} common company field."),
            new($"{prefix}-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, $"{manufacturer} model name.")
        };

        if (includeRef)
        {
            foreach (var eye in new[] { "R", "L" })
            {
                var eyePrefix = $"{prefix}-ref-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{eyePrefix}-sphere", $"REF {eye} Sphere", $"Measure[@Type='REF']/REF/{eye}/Sphere", "REF", eye, "dpt", false, "REF sphere."));
                measurements.Add(new($"{eyePrefix}-cylinder", $"REF {eye} Cylinder", $"Measure[@Type='REF']/REF/{eye}/Cylinder", "REF", eye, "dpt", false, "REF cylinder."));
                measurements.Add(new($"{eyePrefix}-axis", $"REF {eye} Axis", $"Measure[@Type='REF']/REF/{eye}/Axis", "REF", eye, "deg", false, "REF axis."));
                measurements.Add(new($"{eyePrefix}-add", $"REF {eye} ADD", $"Measure[@Type='REF']/REF/{eye}/ADD", "REF", eye, "dpt", false, "REF addition, when present."));
                measurements.Add(new($"{eyePrefix}-line", $"REF {eye} MEDISTAR-Zeile", $"Measure[@Type='REF']/REF/{eye}/MedistarLine", "REF", eye, string.Empty, false, "Prepared MEDISTAR 6228 REF line."));
            }

            measurements.Add(new($"{prefix}-ref-pd", "REF PD", "Measure[@Type='REF']/REF/PD", "REF", string.Empty, "mm", false, "REF PD."));
            measurements.Add(new($"{prefix}-ref-vd", "REF VD", "Measure[@Type='REF']/REF/VD", "REF", string.Empty, "mm", false, "REF VD."));
        }

        if (includeLens)
        {
            foreach (var eye in new[] { "R", "L" })
            {
                var eyePrefix = $"{prefix}-lm-{eye.ToLowerInvariant()}";
                measurements.Add(new($"{eyePrefix}-sphere", $"LM {eye} Sphere", $"Measure[@Type='LM']/LM/{eye}/Sphere", "LM", eye, "dpt", false, "LM sphere."));
                measurements.Add(new($"{eyePrefix}-cylinder", $"LM {eye} Cylinder", $"Measure[@Type='LM']/LM/{eye}/Cylinder", "LM", eye, "dpt", false, "LM cylinder."));
                measurements.Add(new($"{eyePrefix}-axis", $"LM {eye} Axis", $"Measure[@Type='LM']/LM/{eye}/Axis", "LM", eye, "deg", false, "LM axis."));
                measurements.Add(new($"{eyePrefix}-add", $"LM {eye} ADD", $"Measure[@Type='LM']/LM/{eye}/ADD", "LM", eye, "dpt", false, "LM addition."));
                measurements.Add(new($"{eyePrefix}-add2", $"LM {eye} ADD2", $"Measure[@Type='LM']/LM/{eye}/ADD2", "LM", eye, "dpt", false, "LM second addition."));
                measurements.Add(new($"{eyePrefix}-line", $"LM {eye} MEDISTAR-Zeile", $"Measure[@Type='LM']/LM/{eye}/MedistarLine", "LM", eye, string.Empty, false, "Prepared MEDISTAR 6228 LM line."));
            }
        }

        if (includeKm)
        {
            measurements.Add(new($"{prefix}-km-line1", "KM MEDISTAR R1/R2-Zeile", "Measure[@Type='KM']/KM/MedistarLine1", "KM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6221 R1/R2 line."));
            measurements.Add(new($"{prefix}-km-line2", "KM MEDISTAR AV/CYL-Zeile", "Measure[@Type='KM']/KM/MedistarLine2", "KM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6221 AV/CYL line."));
        }

        if (includeTono)
        {
            measurements.Add(new($"{prefix}-tm-line", "Tonometrie MEDISTAR-Zeile", "Measure[@Type='TM']/Tono/TonoListLine", "TM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6205 tonometry line."));
        }

        if (includePachy)
        {
            measurements.Add(new($"{prefix}-cct-line", "Pachymetrie MEDISTAR-Zeile", "Measure[@Type='CCT']/Pachy/MedistarLine", "CCT", string.Empty, string.Empty, false, "Prepared MEDISTAR 6220 pachymetry line."));
        }

        return measurements;
    }

    private static IReadOnlyList<DeviceMeasurementDefinition> CreateZeissIolMaster700Measurements(string id)
    {
        var prefix = id.Replace("device-", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("-default", string.Empty, StringComparison.OrdinalIgnoreCase);
        var measurements = new List<DeviceMeasurementDefinition>
        {
            new($"{prefix}-company", "Company", "Common/Company", "Common", string.Empty, string.Empty, true, "ZEISS common company field."),
            new($"{prefix}-model-name", "ModelName", "Common/ModelName", "Common", string.Empty, string.Empty, true, "ZEISS model name."),
            new($"{prefix}-patient-id", "Patient ID", "Common/PatientId", "Common", string.Empty, string.Empty, false, "Patient ID reported by the device, when present."),
            new($"{prefix}-iol-line", "IOL Biometrie MEDISTAR-Zeile", "Measure[@Type='IOL']/IOL/MedistarLine", "IOL", string.Empty, string.Empty, false, "Prepared MEDISTAR 6227 VKT/AL line."),
            new($"{prefix}-km-line", "IOLMaster Keratometrie MEDISTAR-Zeile", "Measure[@Type='KM']/KM/MedistarLine", "KM", string.Empty, string.Empty, false, "Prepared MEDISTAR 6228 R1/R2 line.")
        };

        foreach (var eye in new[] { "R", "L" })
        {
            var eyePrefix = $"{prefix}-{eye.ToLowerInvariant()}";
            measurements.Add(new($"{eyePrefix}-vkt", $"IOL {eye} VKT", $"Measure[@Type='IOL']/IOL/{eye}/VKT", "IOL", eye, "mm", false, "Anterior chamber depth/VKT."));
            measurements.Add(new($"{eyePrefix}-al", $"IOL {eye} AL", $"Measure[@Type='IOL']/IOL/{eye}/AL", "IOL", eye, "mm", false, "Axial length."));
            measurements.Add(new($"{eyePrefix}-r1", $"KM {eye} R1", $"Measure[@Type='KM']/KM/{eye}/R1", "KM", eye, "mm", false, "Keratometry R1 radius."));
            measurements.Add(new($"{eyePrefix}-r1-axis", $"KM {eye} R1 Achse", $"Measure[@Type='KM']/KM/{eye}/R1Axis", "KM", eye, "deg", false, "Keratometry R1 axis."));
            measurements.Add(new($"{eyePrefix}-r2", $"KM {eye} R2", $"Measure[@Type='KM']/KM/{eye}/R2", "KM", eye, "mm", false, "Keratometry R2 radius."));
            measurements.Add(new($"{eyePrefix}-r2-axis", $"KM {eye} R2 Achse", $"Measure[@Type='KM']/KM/{eye}/R2Axis", "KM", eye, "deg", false, "Keratometry R2 axis."));
        }

        return measurements;
    }

    public static DeviceProfileDefinition CreateDocumentAttachmentDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 20, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-document-attachment-default",
                Name: "Generisches Dokumentgerät",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile for devices that deliver documents or files as XDT attachments without measurement parsing.",
                Vendor: "XdtDeviceBridge",
                Product: "Dokumentanhang",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "Generisch",
            Model: "Dokumentanhang",
            DeviceType: "Dokument/Anhang",
            ParserMode: "AttachmentOnly",
            Measurements: Array.Empty<DeviceMeasurementDefinition>(),
            SupportedExaminationTypes: new[] { "Dokument", "Anhang" },
            CanContainMultipleExaminationTypes: false,
            DeviceImagePath: InterfaceProfileUiPolicy.DocumentAttachmentDeviceImagePath);
    }

    public static DeviceProfileDefinition CreateManualDocumentSelectionDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-manual-document-selection-default",
                Name: "Manuelle Dokumentauswahl",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile for manual document handoff where users select files or drop them into the transfer window.",
                Vendor: "XdtDeviceBridge",
                Product: "Manuelle Dokumentübergabe",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "Manuell",
            Model: "Dokumentauswahl",
            DeviceType: "Dokument/Manuell",
            ParserMode: "AttachmentOnlyManual",
            Measurements: Array.Empty<DeviceMeasurementDefinition>(),
            SupportedExaminationTypes: new[] { "Dokument", "Anhang", "Manuell" },
            CanContainMultipleExaminationTypes: false,
            DeviceImagePath: InterfaceProfileUiPolicy.ManualDocumentSelectionDeviceImagePath);
    }
}
