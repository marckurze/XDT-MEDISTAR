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
                new DeviceMeasurementDefinition("lm7-pd", "PD", "PD/Distance", "PD", string.Empty, "mm", false, "Lensmeter pupillary distance, noch zu validieren.")
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
}
