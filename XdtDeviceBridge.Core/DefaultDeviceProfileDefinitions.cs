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
                new DeviceMeasurementDefinition("lm7-r-sphere", "R Sphere", "R/LM/Median/Sphere", "LM", "R", "dpt", true, "Right lens sphere, provisional source path."),
                new DeviceMeasurementDefinition("lm7-r-cylinder", "R Cylinder", "R/LM/Median/Cylinder", "LM", "R", "dpt", true, "Right lens cylinder, provisional source path."),
                new DeviceMeasurementDefinition("lm7-r-axis", "R Axis", "R/LM/Median/Axis", "LM", "R", "deg", true, "Right lens axis, provisional source path."),
                new DeviceMeasurementDefinition("lm7-r-prism-horizontal", "R PrismHorizontal", "R/LM/Median/PrismHorizontal", "LM", "R", "prism dpt", false, "Right horizontal prism, provisional source path."),
                new DeviceMeasurementDefinition("lm7-r-prism-horizontal-base", "R PrismHorizontalBase", "R/LM/Median/PrismHorizontalBase", "LM", "R", string.Empty, false, "Right horizontal prism base direction, provisional source path."),
                new DeviceMeasurementDefinition("lm7-r-prism-vertical", "R PrismVertical", "R/LM/Median/PrismVertical", "LM", "R", "prism dpt", false, "Right vertical prism, provisional source path."),
                new DeviceMeasurementDefinition("lm7-r-prism-vertical-base", "R PrismVerticalBase", "R/LM/Median/PrismVerticalBase", "LM", "R", string.Empty, false, "Right vertical prism base direction, provisional source path."),
                new DeviceMeasurementDefinition("lm7-l-sphere", "L Sphere", "L/LM/Median/Sphere", "LM", "L", "dpt", true, "Left lens sphere, provisional source path."),
                new DeviceMeasurementDefinition("lm7-l-cylinder", "L Cylinder", "L/LM/Median/Cylinder", "LM", "L", "dpt", true, "Left lens cylinder, provisional source path."),
                new DeviceMeasurementDefinition("lm7-l-axis", "L Axis", "L/LM/Median/Axis", "LM", "L", "deg", true, "Left lens axis, provisional source path."),
                new DeviceMeasurementDefinition("lm7-l-prism-horizontal", "L PrismHorizontal", "L/LM/Median/PrismHorizontal", "LM", "L", "prism dpt", false, "Left horizontal prism, provisional source path."),
                new DeviceMeasurementDefinition("lm7-l-prism-horizontal-base", "L PrismHorizontalBase", "L/LM/Median/PrismHorizontalBase", "LM", "L", string.Empty, false, "Left horizontal prism base direction, provisional source path."),
                new DeviceMeasurementDefinition("lm7-l-prism-vertical", "L PrismVertical", "L/LM/Median/PrismVertical", "LM", "L", "prism dpt", false, "Left vertical prism, provisional source path."),
                new DeviceMeasurementDefinition("lm7-l-prism-vertical-base", "L PrismVerticalBase", "L/LM/Median/PrismVerticalBase", "LM", "L", string.Empty, false, "Left vertical prism base direction, provisional source path."),
                new DeviceMeasurementDefinition("lm7-pd", "PD", "PD/Distance", "PD", string.Empty, "mm", false, "Lensmeter pupillary distance, provisional source path.")
            },
            SupportedExaminationTypes: new[] { "Lensmeter", "PD", "Prism" },
            CanContainMultipleExaminationTypes: false);
    }
}
