namespace XdtDeviceBridge.Core;

public static class DefaultExportProfileDefinitions
{
    public static ExportProfileDefinition CreateMedistarNidekArk1sDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-nidek-ark1s-default",
                Name: "MEDISTAR + NIDEK ARK1S Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile definition for MEDISTAR and NIDEK ARK1S.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/NIDEK ARK1S",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            TargetAisProfileId: "ais-medistar-default",
            SourceDeviceProfileId: "device-nidek-ark1s-default",
            OutputEncoding: "Windows-1252",
            Rules: new[]
            {
                new ExportRuleDefinition("1", "8000", "MessageType", ExportRuleType.StaticValue, null, "6310", 1, true, "MEDISTAR XDT import control."),
                new ExportRuleDefinition("2", "3000", "PatientNumber", ExportRuleType.AisField, "AIS.PatientNumber", "{value}", 2, true, "Patient number from AIS."),
                new ExportRuleDefinition("3", "3101", "LastName", ExportRuleType.AisField, "AIS.LastName", "{value}", 3, true, "Last name from AIS."),
                new ExportRuleDefinition("4", "3102", "FirstName", ExportRuleType.AisField, "AIS.FirstName", "{value}", 4, true, "First name from AIS."),
                new ExportRuleDefinition("5", "3103", "BirthDate", ExportRuleType.AisField, "AIS.BirthDate", "{value}", 5, true, "Birth date from AIS."),
                new ExportRuleDefinition("6", "8402", "ExaminationType", ExportRuleType.AisField, "AIS.ExaminationType", "{value}", 6, true, "Examination type from AIS."),
                new ExportRuleDefinition(
                    "7",
                    "6228",
                    "ResultRight",
                    ExportRuleType.Template,
                    null,
                    "R.:S={Device.R/AR/ARMedian/Sphere} Z={Device.R/AR/ARMedian/Cylinder}*{Device.R/AR/ARMedian/Axis}                              PD={Device.PD/PDList[@No='1']/FarPD}",
                    7,
                    true,
                    "MEDISTAR card text for right eye."),
                new ExportRuleDefinition(
                    "8",
                    "6228",
                    "ResultLeft",
                    ExportRuleType.Template,
                    null,
                    "L.:S={Device.L/AR/ARMedian/Sphere} Z={Device.L/AR/ARMedian/Cylinder}*{Device.L/AR/ARMedian/Axis}                              PD={Device.PD/PDList[@No='1']/FarPD}",
                    8,
                    true,
                    "MEDISTAR card text for left eye.")
            });
    }
}
