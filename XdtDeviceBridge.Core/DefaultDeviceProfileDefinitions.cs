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
            CanContainMultipleExaminationTypes: false);
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
            CanContainMultipleExaminationTypes: true);
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
            CanContainMultipleExaminationTypes: false);
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
            CanContainMultipleExaminationTypes: false);
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
            CanContainMultipleExaminationTypes: true);
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
            CanContainMultipleExaminationTypes: true);
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
            CanContainMultipleExaminationTypes: true);
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
            CanContainMultipleExaminationTypes: true);
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
            CanContainMultipleExaminationTypes: false);
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
            DeviceImagePath: string.Empty,
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
            CanContainMultipleExaminationTypes: false);
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
            CanContainMultipleExaminationTypes: false);
    }
}
