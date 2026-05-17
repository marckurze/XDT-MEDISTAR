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
            CanContainMultipleExaminationTypes: false);
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
            CanContainMultipleExaminationTypes: false);
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
                new DeviceMeasurementDefinition("lm7-r-sphere", "R Sphere", "R/Sphare", "LM", "R", "dpt", true, "Right lens sphere from validated LM7 sample XML."),
                new DeviceMeasurementDefinition("lm7-r-cylinder", "R Cylinder", "R/Cylinder", "LM", "R", "dpt", true, "Right lens cylinder from validated LM7 sample XML."),
                new DeviceMeasurementDefinition("lm7-r-axis", "R Axis", "R/Axis", "LM", "R", "deg", true, "Right lens axis from validated LM7 sample XML."),
                new DeviceMeasurementDefinition("lm7-r-prism-horizontal", "R PrismHorizontal", "R/PrismX", "LM", "R", "prism dpt", true, "Right horizontal prism from validated LM7 sample XML."),
                new DeviceMeasurementDefinition("lm7-r-prism-horizontal-base", "R PrismHorizontalBase", "R/PrismX/@base", "LM", "R", string.Empty, true, "Right horizontal prism base direction from validated LM7 sample XML."),
                new DeviceMeasurementDefinition("lm7-r-prism-vertical", "R PrismVertical", "R/PrismY", "LM", "R", "prism dpt", true, "Right vertical prism from validated LM7 sample XML."),
                new DeviceMeasurementDefinition("lm7-r-prism-vertical-base", "R PrismVerticalBase", "R/PrismY/@base", "LM", "R", string.Empty, true, "Right vertical prism base direction from validated LM7 sample XML."),
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
                new DeviceMeasurementDefinition("lm7-lan-pd-near-l", "PD NearL", "Measure[@Type='LM']/PD/NearL", "PD", "L", "mm", false, "LM7 LAN XML left near pupillary distance.")
            },
            SupportedExaminationTypes: new[] { "Lensmeter", "PD", "Prism" },
            CanContainMultipleExaminationTypes: false);
    }

    public static DeviceProfileDefinition CreateNidekNt530PDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-nidek-nt530p-default",
                Name: "NIDEK NT530P",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for NIDEK NT530P tonometry and pachymetry XML measurement files.",
                Vendor: "NIDEK",
                Product: "NT530P",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "NIDEK",
            Model: "NT530P",
            DeviceType: "Tonometer/Pachymeter",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("nt530p-r-iop-1", "R IOP 1", "Data/R/NT/NTList[@No='1']/mmHg", "NT", "R", "mmHg", true, "Right eye tonometry value 1 from validated NT530P sample XML."),
                new DeviceMeasurementDefinition("nt530p-r-iop-2", "R IOP 2", "Data/R/NT/NTList[@No='2']/mmHg", "NT", "R", "mmHg", true, "Right eye tonometry value 2 from validated NT530P sample XML."),
                new DeviceMeasurementDefinition("nt530p-r-iop-3", "R IOP 3", "Data/R/NT/NTList[@No='3']/mmHg", "NT", "R", "mmHg", true, "Right eye tonometry value 3 from validated NT530P sample XML."),
                new DeviceMeasurementDefinition("nt530p-r-iop-average", "R IOP Average", "Data/R/NT/NTAverage/mmHg", "NT", "R", "mmHg", true, "Right eye tonometry average from validated NT530P sample XML."),
                new DeviceMeasurementDefinition("nt530p-l-iop-1", "L IOP 1", "Data/L/NT/NTList[@No='1']/mmHg", "NT", "L", "mmHg", true, "Left eye tonometry value 1 from validated NT530P sample XML."),
                new DeviceMeasurementDefinition("nt530p-l-iop-2", "L IOP 2", "Data/L/NT/NTList[@No='2']/mmHg", "NT", "L", "mmHg", true, "Left eye tonometry value 2 from validated NT530P sample XML."),
                new DeviceMeasurementDefinition("nt530p-l-iop-3", "L IOP 3", "Data/L/NT/NTList[@No='3']/mmHg", "NT", "L", "mmHg", true, "Left eye tonometry value 3 from validated NT530P sample XML."),
                new DeviceMeasurementDefinition("nt530p-l-iop-average", "L IOP Average", "Data/L/NT/NTAverage/mmHg", "NT", "L", "mmHg", true, "Left eye tonometry average from validated NT530P sample XML."),
                new DeviceMeasurementDefinition("nt530p-r-corrected-iop-measured", "R CorrectedIOP Measured", "Data/R/NT/CorrectedIOP/Measured/mmHg", "CorrectedIOP", "R", "mmHg", false, "Right measured IOP for corrected IOP block."),
                new DeviceMeasurementDefinition("nt530p-r-corrected-iop-corrected", "R CorrectedIOP Corrected", "Data/R/NT/CorrectedIOP/Corrected/mmHg", "CorrectedIOP", "R", "mmHg", false, "Right corrected IOP value."),
                new DeviceMeasurementDefinition("nt530p-r-corrected-iop-cct", "R CorrectedIOP CCT", "Data/R/NT/CorrectedIOP/CCT", "CorrectedIOP", "R", "um", false, "Right CCT value used for corrected IOP."),
                new DeviceMeasurementDefinition("nt530p-l-corrected-iop-measured", "L CorrectedIOP Measured", "Data/L/NT/CorrectedIOP/Measured/mmHg", "CorrectedIOP", "L", "mmHg", false, "Left measured IOP for corrected IOP block."),
                new DeviceMeasurementDefinition("nt530p-l-corrected-iop-corrected", "L CorrectedIOP Corrected", "Data/L/NT/CorrectedIOP/Corrected/mmHg", "CorrectedIOP", "L", "mmHg", false, "Left corrected IOP value."),
                new DeviceMeasurementDefinition("nt530p-l-corrected-iop-cct", "L CorrectedIOP CCT", "Data/L/NT/CorrectedIOP/CCT", "CorrectedIOP", "L", "um", false, "Left CCT value used for corrected IOP."),
                new DeviceMeasurementDefinition("nt530p-r-pachy-1", "R Pachy 1", "Data/R/PACHY/PACHYList[@No='1']/Thickness", "PACHY", "R", "um", true, "Right pachymetry value 1 from validated NT530P sample XML."),
                new DeviceMeasurementDefinition("nt530p-r-pachy-2", "R Pachy 2", "Data/R/PACHY/PACHYList[@No='2']/Thickness", "PACHY", "R", "um", false, "Right pachymetry value 2; noch zu validieren for all sample variants."),
                new DeviceMeasurementDefinition("nt530p-r-pachy-3", "R Pachy 3", "Data/R/PACHY/PACHYList[@No='3']/Thickness", "PACHY", "R", "um", false, "Right pachymetry value 3; noch zu validieren for all sample variants."),
                new DeviceMeasurementDefinition("nt530p-r-pachy-average", "R Pachy Average", "Data/R/PACHY/PACHYAverage/Thickness", "PACHY", "R", "um", true, "Right pachymetry average from validated NT530P sample XML."),
                new DeviceMeasurementDefinition("nt530p-l-pachy-1", "L Pachy 1", "Data/L/PACHY/PACHYList[@No='1']/Thickness", "PACHY", "L", "um", true, "Left pachymetry value 1 from validated NT530P sample XML."),
                new DeviceMeasurementDefinition("nt530p-l-pachy-2", "L Pachy 2", "Data/L/PACHY/PACHYList[@No='2']/Thickness", "PACHY", "L", "um", false, "Left pachymetry value 2; noch zu validieren for all sample variants."),
                new DeviceMeasurementDefinition("nt530p-l-pachy-3", "L Pachy 3", "Data/L/PACHY/PACHYList[@No='3']/Thickness", "PACHY", "L", "um", false, "Left pachymetry value 3; noch zu validieren for all sample variants."),
                new DeviceMeasurementDefinition("nt530p-l-pachy-average", "L Pachy Average", "Data/L/PACHY/PACHYAverage/Thickness", "PACHY", "L", "um", true, "Left pachymetry average from validated NT530P sample XML."),
                new DeviceMeasurementDefinition("nt530p-r-pachy-image", "R PACHYImage", "Data/R/PACHY/PACHYImage", "Attachment", "R", string.Empty, false, "Right pachymetry image reference for future attachment handling; optional because JPG files can be missing."),
                new DeviceMeasurementDefinition("nt530p-l-pachy-image", "L PACHYImage", "Data/L/PACHY/PACHYImage", "Attachment", "L", string.Empty, false, "Left pachymetry image reference for future attachment handling; optional because JPG files can be missing."),
                new DeviceMeasurementDefinition("nt530p-measurement-date", "MeasurementDate", "Data/Date", "Metadata", string.Empty, string.Empty, true, "Measurement date from NT530P XML."),
                new DeviceMeasurementDefinition("nt530p-measurement-time", "MeasurementTime", "Data/Time", "Metadata", string.Empty, string.Empty, true, "Measurement time from NT530P XML.")
            },
            SupportedExaminationTypes: new[] { "Tonometrie", "Pachymetrie", "CorrectedIOP", "Attachment" },
            CanContainMultipleExaminationTypes: true);
    }

    public static DeviceProfileDefinition CreateTopconCl300Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-topcon-cl300-default",
                Name: "TOPCON CL300",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for TOPCON CL300 JOIA/Ophthalmology XML lensmeter files. Namespace-Normalisierung ist später im Parser erforderlich.",
                Vendor: "TOPCON",
                Product: "CL300",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "TOPCON",
            Model: "CL300",
            DeviceType: "Lensmeter",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("cl300-company", "Company", "Ophthalmology/Common/Company", "Common", string.Empty, string.Empty, true, "TOPCON JOIA common company field; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("cl300-model-name", "ModelName", "Ophthalmology/Common/ModelName", "Common", string.Empty, string.Empty, true, "TOPCON JOIA common model field; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("cl300-r-sphere", "R Sphere", "Ophthalmology/Measure[@type='LM']/LM/R/Sphere", "LM", "R", "dpt", true, "Right lens sphere from TOPCON CL300 JOIA XML; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("cl300-r-cylinder", "R Cylinder", "Ophthalmology/Measure[@type='LM']/LM/R/Cylinder", "LM", "R", "dpt", true, "Right lens cylinder from TOPCON CL300 JOIA XML; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("cl300-r-axis", "R Axis", "Ophthalmology/Measure[@type='LM']/LM/R/Axis", "LM", "R", "deg", true, "Right lens axis from TOPCON CL300 JOIA XML; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("cl300-r-addition", "R Addition", "Ophthalmology/Measure[@type='LM']/LM/R/Add1", "LM", "R", "dpt", false, "Right lens addition from TOPCON CL300 JOIA XML; optional and Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("cl300-r-prism-horizontal", "R PrismHorizontal", "Ophthalmology/Measure[@type='LM']/LM/R/H", "LM", "R", "prism dpt", false, "Right horizontal prism from TOPCON CL300 JOIA XML; MEDISTAR basis notation noch zu validieren."),
                new DeviceMeasurementDefinition("cl300-r-prism-vertical", "R PrismVertical", "Ophthalmology/Measure[@type='LM']/LM/R/V", "LM", "R", "prism dpt", false, "Right vertical prism from TOPCON CL300 JOIA XML; MEDISTAR basis notation noch zu validieren."),
                new DeviceMeasurementDefinition("cl300-l-sphere", "L Sphere", "Ophthalmology/Measure[@type='LM']/LM/L/Sphere", "LM", "L", "dpt", true, "Left lens sphere from TOPCON CL300 JOIA XML; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("cl300-l-cylinder", "L Cylinder", "Ophthalmology/Measure[@type='LM']/LM/L/Cylinder", "LM", "L", "dpt", true, "Left lens cylinder from TOPCON CL300 JOIA XML; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("cl300-l-axis", "L Axis", "Ophthalmology/Measure[@type='LM']/LM/L/Axis", "LM", "L", "deg", true, "Left lens axis from TOPCON CL300 JOIA XML; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("cl300-l-addition", "L Addition", "Ophthalmology/Measure[@type='LM']/LM/L/Add1", "LM", "L", "dpt", false, "Left lens addition from TOPCON CL300 JOIA XML; optional and Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("cl300-l-prism-horizontal", "L PrismHorizontal", "Ophthalmology/Measure[@type='LM']/LM/L/H", "LM", "L", "prism dpt", false, "Left horizontal prism from TOPCON CL300 JOIA XML; MEDISTAR basis notation noch zu validieren."),
                new DeviceMeasurementDefinition("cl300-l-prism-vertical", "L PrismVertical", "Ophthalmology/Measure[@type='LM']/LM/L/V", "LM", "L", "prism dpt", false, "Left vertical prism from TOPCON CL300 JOIA XML; MEDISTAR basis notation noch zu validieren."),
                new DeviceMeasurementDefinition("cl300-pd-distance", "PD Distance", "Ophthalmology/Measure[@type='LM']/PD/B/Distance", "PD", string.Empty, "mm", false, "Binocular PD distance from TOPCON CL300 JOIA XML; optional and Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("cl300-pd-r-distance", "R PD Distance", "Ophthalmology/Measure[@type='LM']/PD/R/Distance", "PD", "R", "mm", false, "Right PD distance from TOPCON CL300 JOIA XML; optional and Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("cl300-pd-l-distance", "L PD Distance", "Ophthalmology/Measure[@type='LM']/PD/L/Distance", "PD", "L", "mm", false, "Left PD distance from TOPCON CL300 JOIA XML; optional and Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("cl300-diopter-step", "DiopterStep", "Ophthalmology/Measure[@type='LM']/DiopterStep", "LM", string.Empty, "dpt", false, "Diopter step from TOPCON CL300 JOIA XML."),
                new DeviceMeasurementDefinition("cl300-axis-step", "AxisStep", "Ophthalmology/Measure[@type='LM']/AxisStep", "LM", string.Empty, "deg", false, "Axis step from TOPCON CL300 JOIA XML."),
                new DeviceMeasurementDefinition("cl300-prism-step", "PrismStep", "Ophthalmology/Measure[@type='LM']/PrismStep", "LM", string.Empty, "prism dpt", false, "Prism step from TOPCON CL300 JOIA XML.")
            },
            SupportedExaminationTypes: new[] { "Lensmeter", "PD", "Prism" },
            CanContainMultipleExaminationTypes: false);
    }

    public static DeviceProfileDefinition CreateTopconKr800Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-topcon-kr800-default",
                Name: "TOPCON KR800",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for TOPCON KR800 JOIA/Ophthalmology XML multi-examination files. Namespace-Normalisierung ist später im Parser erforderlich.",
                Vendor: "TOPCON",
                Product: "KR800",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "TOPCON",
            Model: "KR800",
            DeviceType: "Autorefractor/Keratometer",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("kr800-company", "Company", "Ophthalmology/Common/Company", "Common", string.Empty, string.Empty, true, "TOPCON JOIA common company field; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-model-name", "ModelName", "Ophthalmology/Common/ModelName", "Common", string.Empty, string.Empty, true, "TOPCON JOIA common model field; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-ref-r-sphere", "REF R Sphere", "Ophthalmology/Measure[@type='REF']/REF/R/Median/Sphere", "REF", "R", "dpt", true, "Right autorefractor sphere from TOPCON KR800 REF median values; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-ref-r-cylinder", "REF R Cylinder", "Ophthalmology/Measure[@type='REF']/REF/R/Median/Cylinder", "REF", "R", "dpt", true, "Right autorefractor cylinder from TOPCON KR800 REF median values; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-ref-r-axis", "REF R Axis", "Ophthalmology/Measure[@type='REF']/REF/R/Median/Axis", "REF", "R", "deg", true, "Right autorefractor axis from TOPCON KR800 REF median values; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-ref-r-se", "REF R SE", "Ophthalmology/Measure[@type='REF']/REF/R/Median/SE", "REF", "R", "dpt", false, "Right spherical equivalent from TOPCON KR800 REF median values; optional for output."),
                new DeviceMeasurementDefinition("kr800-ref-l-sphere", "REF L Sphere", "Ophthalmology/Measure[@type='REF']/REF/L/Median/Sphere", "REF", "L", "dpt", true, "Left autorefractor sphere from TOPCON KR800 REF median values; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-ref-l-cylinder", "REF L Cylinder", "Ophthalmology/Measure[@type='REF']/REF/L/Median/Cylinder", "REF", "L", "dpt", true, "Left autorefractor cylinder from TOPCON KR800 REF median values; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-ref-l-axis", "REF L Axis", "Ophthalmology/Measure[@type='REF']/REF/L/Median/Axis", "REF", "L", "deg", true, "Left autorefractor axis from TOPCON KR800 REF median values; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-ref-l-se", "REF L SE", "Ophthalmology/Measure[@type='REF']/REF/L/Median/SE", "REF", "L", "dpt", false, "Left spherical equivalent from TOPCON KR800 REF median values; optional for output."),
                new DeviceMeasurementDefinition("kr800-ref-pd-distance", "REF PD Distance", "Ophthalmology/Measure[@type='REF']/PD/Distance", "REF", string.Empty, "mm", false, "Binocular PD distance from TOPCON KR800 REF block; optional because output selection is noch zu validieren."),
                new DeviceMeasurementDefinition("kr800-ref-pd-near", "REF PD Near", "Ophthalmology/Measure[@type='REF']/PD/Near", "REF", string.Empty, "mm", false, "Near PD from TOPCON KR800 REF block; optional and noch zu validieren."),
                new DeviceMeasurementDefinition("kr800-km-r-k1-radius", "KM R K1 Radius", "Ophthalmology/Measure[@type='KM']/KM/R/Median/R1/Radius", "KM", "R", "mm", false, "Right K1 radius; KM-Ausgabe noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-km-r-k1-power", "KM R K1 Power", "Ophthalmology/Measure[@type='KM']/KM/R/Median/R1/Power", "KM", "R", "dpt", false, "Right K1 power; KM-Ausgabe noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-km-r-k1-axis", "KM R K1 Axis", "Ophthalmology/Measure[@type='KM']/KM/R/Median/R1/Axis", "KM", "R", "deg", false, "Right K1 axis; KM-Ausgabe noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-km-r-k2-radius", "KM R K2 Radius", "Ophthalmology/Measure[@type='KM']/KM/R/Median/R2/Radius", "KM", "R", "mm", false, "Right K2 radius; KM-Ausgabe noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-km-r-k2-power", "KM R K2 Power", "Ophthalmology/Measure[@type='KM']/KM/R/Median/R2/Power", "KM", "R", "dpt", false, "Right K2 power; KM-Ausgabe noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-km-r-k2-axis", "KM R K2 Axis", "Ophthalmology/Measure[@type='KM']/KM/R/Median/R2/Axis", "KM", "R", "deg", false, "Right K2 axis; KM-Ausgabe noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-km-l-k1-radius", "KM L K1 Radius", "Ophthalmology/Measure[@type='KM']/KM/L/Median/R1/Radius", "KM", "L", "mm", false, "Left K1 radius; KM-Ausgabe noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-km-l-k1-power", "KM L K1 Power", "Ophthalmology/Measure[@type='KM']/KM/L/Median/R1/Power", "KM", "L", "dpt", false, "Left K1 power; KM-Ausgabe noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-km-l-k1-axis", "KM L K1 Axis", "Ophthalmology/Measure[@type='KM']/KM/L/Median/R1/Axis", "KM", "L", "deg", false, "Left K1 axis; KM-Ausgabe noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-km-l-k2-radius", "KM L K2 Radius", "Ophthalmology/Measure[@type='KM']/KM/L/Median/R2/Radius", "KM", "L", "mm", false, "Left K2 radius; KM-Ausgabe noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-km-l-k2-power", "KM L K2 Power", "Ophthalmology/Measure[@type='KM']/KM/L/Median/R2/Power", "KM", "L", "dpt", false, "Left K2 power; KM-Ausgabe noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-km-l-k2-axis", "KM L K2 Axis", "Ophthalmology/Measure[@type='KM']/KM/L/Median/R2/Axis", "KM", "L", "deg", false, "Left K2 axis; KM-Ausgabe noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-sbj-r-sphere", "SBJ R Sphere", "Ophthalmology/Measure[@type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/R/Sph", "SBJ", "R", "dpt", false, "Subjective right sphere; noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-sbj-r-cylinder", "SBJ R Cylinder", "Ophthalmology/Measure[@type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/R/Cyl", "SBJ", "R", "dpt", false, "Subjective right cylinder; noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-sbj-r-axis", "SBJ R Axis", "Ophthalmology/Measure[@type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/R/Axis", "SBJ", "R", "deg", false, "Subjective right axis; noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-sbj-l-sphere", "SBJ L Sphere", "Ophthalmology/Measure[@type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/L/Sph", "SBJ", "L", "dpt", false, "Subjective left sphere; noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-sbj-l-cylinder", "SBJ L Cylinder", "Ophthalmology/Measure[@type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/L/Cyl", "SBJ", "L", "dpt", false, "Subjective left cylinder; noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-sbj-l-axis", "SBJ L Axis", "Ophthalmology/Measure[@type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/L/Axis", "SBJ", "L", "deg", false, "Subjective left axis; noch zu validieren, Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("kr800-sbj-va-r", "SBJ VA R", "Ophthalmology/Measure[@type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/VA/R", "SBJ", "R", string.Empty, false, "Subjective VA right; noch zu validieren."),
                new DeviceMeasurementDefinition("kr800-sbj-va-l", "SBJ VA L", "Ophthalmology/Measure[@type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/VA/L", "SBJ", "L", string.Empty, false, "Subjective VA left; noch zu validieren."),
                new DeviceMeasurementDefinition("kr800-sbj-pd-b", "SBJ PD B", "Ophthalmology/Measure[@type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/PD/B", "SBJ", string.Empty, "mm", false, "Subjective binocular PD; noch zu validieren.")
            },
            SupportedExaminationTypes: new[] { "REF", "KM", "SBJ", "PD" },
            CanContainMultipleExaminationTypes: true);
    }

    public static DeviceProfileDefinition CreateTopconTrk2PDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new DeviceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "device-topcon-trk2p-default",
                Name: "TOPCON TRK2P",
                ProfileKind: ProfileKind.DeviceProfile,
                Description: "Default device profile definition for TOPCON TRK2P JOIA/Ophthalmology XML tonometry and CCT files. Namespace-Normalisierung ist später im Parser erforderlich.",
                Vendor: "TOPCON",
                Product: "TRK2P",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Manufacturer: "TOPCON",
            Model: "TRK2P",
            DeviceType: "Tonometer/Pachymeter",
            ParserMode: "Xml",
            Measurements: new[]
            {
                new DeviceMeasurementDefinition("trk2p-company", "Company", "Ophthalmology/Common/Company", "Common", string.Empty, string.Empty, false, "TOPCON JOIA common company field; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("trk2p-model-name", "ModelName", "Ophthalmology/Common/ModelName", "Common", string.Empty, string.Empty, false, "TOPCON JOIA common model field; Namespace-Normalisierung erforderlich."),
                new DeviceMeasurementDefinition("trk2p-measurement-date", "MeasurementDate", "Ophthalmology/Common/Date", "Common", string.Empty, string.Empty, false, "Measurement date from TOPCON TRK2P common block; optional."),
                new DeviceMeasurementDefinition("trk2p-measurement-time", "MeasurementTime", "Ophthalmology/Common/Time", "Common", string.Empty, string.Empty, false, "Measurement time from TOPCON TRK2P common block; optional."),
                new DeviceMeasurementDefinition("trk2p-r-iop-1", "TM R IOP 1", "Ophthalmology/Measure[@type='TM']/TM/R/List[@No='1']/IOP_mmHg", "TM", "R", "mmHg", false, "Right IOP single value 1; optional because productive output selection is noch zu validieren."),
                new DeviceMeasurementDefinition("trk2p-r-iop-2", "TM R IOP 2", "Ophthalmology/Measure[@type='TM']/TM/R/List[@No='2']/IOP_mmHg", "TM", "R", "mmHg", false, "Right IOP single value 2; optional because productive output selection is noch zu validieren."),
                new DeviceMeasurementDefinition("trk2p-r-iop-3", "TM R IOP 3", "Ophthalmology/Measure[@type='TM']/TM/R/List[@No='3']/IOP_mmHg", "TM", "R", "mmHg", false, "Right IOP single value 3; optional because productive output selection is noch zu validieren."),
                new DeviceMeasurementDefinition("trk2p-r-iop-average", "TM R IOP Average", "Ophthalmology/Measure[@type='TM']/TM/R/Average/IOP_mmHg", "TM", "R", "mmHg", true, "Right IOP average from documented TOPCON TRK2P sample."),
                new DeviceMeasurementDefinition("trk2p-l-iop-1", "TM L IOP 1", "Ophthalmology/Measure[@type='TM']/TM/L/List[@No='1']/IOP_mmHg", "TM", "L", "mmHg", false, "Left IOP single value 1; optional because productive output selection is noch zu validieren."),
                new DeviceMeasurementDefinition("trk2p-l-iop-2", "TM L IOP 2", "Ophthalmology/Measure[@type='TM']/TM/L/List[@No='2']/IOP_mmHg", "TM", "L", "mmHg", false, "Left IOP single value 2; optional because productive output selection is noch zu validieren."),
                new DeviceMeasurementDefinition("trk2p-l-iop-3", "TM L IOP 3", "Ophthalmology/Measure[@type='TM']/TM/L/List[@No='3']/IOP_mmHg", "TM", "L", "mmHg", false, "Left IOP single value 3; optional because productive output selection is noch zu validieren."),
                new DeviceMeasurementDefinition("trk2p-l-iop-average", "TM L IOP Average", "Ophthalmology/Measure[@type='TM']/TM/L/Average/IOP_mmHg", "TM", "L", "mmHg", true, "Left IOP average from documented TOPCON TRK2P sample."),
                new DeviceMeasurementDefinition("trk2p-r-cct-3", "CCT R Pachy 3", "Ophthalmology/Measure[@type='TM']/CCT/R/List[@No='3']/CCT_mm", "CCT", "R", "mm", false, "Right CCT value in mm; MEDISTAR µm conversion and productive selection noch zu validieren."),
                new DeviceMeasurementDefinition("trk2p-r-cct-4", "CCT R Pachy 4", "Ophthalmology/Measure[@type='TM']/CCT/R/List[@No='4']/CCT_mm", "CCT", "R", "mm", false, "Right CCT value in mm; MEDISTAR µm conversion and productive selection noch zu validieren."),
                new DeviceMeasurementDefinition("trk2p-l-cct-1", "CCT L Pachy 1", "Ophthalmology/Measure[@type='TM']/CCT/L/List[@No='1']/CCT_mm", "CCT", "L", "mm", false, "Left CCT value in mm; MEDISTAR µm conversion and productive selection noch zu validieren."),
                new DeviceMeasurementDefinition("trk2p-l-cct-2", "CCT L Pachy 2", "Ophthalmology/Measure[@type='TM']/CCT/L/List[@No='2']/CCT_mm", "CCT", "L", "mm", false, "Left CCT value in mm; MEDISTAR µm conversion and productive selection noch zu validieren."),
                new DeviceMeasurementDefinition("trk2p-l-cct-3", "CCT L Pachy 3", "Ophthalmology/Measure[@type='TM']/CCT/L/List[@No='3']/CCT_mm", "CCT", "L", "mm", false, "Left CCT value in mm; MEDISTAR µm conversion and productive selection noch zu validieren.")
            },
            SupportedExaminationTypes: new[] { "TM", "CCT", "Tonometrie", "Pachymetrie" },
            CanContainMultipleExaminationTypes: true);
    }
}
