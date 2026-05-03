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

    public static ExportProfileDefinition CreateMedistarNidekLm7Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-nidek-lm7-default",
                Name: "MEDISTAR + NIDEK LM7 Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile definition for MEDISTAR and NIDEK LM7.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/NIDEK LM7",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            TargetAisProfileId: "ais-medistar-default",
            SourceDeviceProfileId: "device-nidek-lm7-default",
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
                    "LensmeterResultRight",
                    ExportRuleType.Template,
                    null,
                    "R.:S={Device.R/LM/Median/Sphere:Diopter} Z={Device.R/LM/Median/Cylinder:Diopter}*{Device.R/LM/Median/Axis:Axis} P={Device.R/LM/Median/PrismHorizontal:Prism} {Device.R/LM/Median/PrismHorizontalBase:Raw} {Device.R/LM/Median/PrismVertical:Prism} {Device.R/LM/Median/PrismVerticalBase:Raw}           PD={Device.PD/Distance:Pd}",
                    7,
                    true,
                    "MEDISTAR lensmeter card text for right eye."),
                new ExportRuleDefinition(
                    "8",
                    "6228",
                    "LensmeterResultLeft",
                    ExportRuleType.Template,
                    null,
                    "L.:S={Device.L/LM/Median/Sphere:Diopter} Z={Device.L/LM/Median/Cylinder:Diopter}*{Device.L/LM/Median/Axis:Axis} P={Device.L/LM/Median/PrismHorizontal:Prism} {Device.L/LM/Median/PrismHorizontalBase:Raw} {Device.L/LM/Median/PrismVertical:Prism} {Device.L/LM/Median/PrismVerticalBase:Raw}",
                    8,
                    true,
                    "MEDISTAR lensmeter card text for left eye.")
            });
    }

    public static ExportProfileDefinition CreateMedistarNidekNt530PDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-nidek-nt530p-default",
                Name: "MEDISTAR + NIDEK NT530P Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile definition for MEDISTAR and NIDEK NT530P. EV-/Attachment-Erweiterung wird später ergänzt.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/NIDEK NT530P",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            TargetAisProfileId: "ais-medistar-default",
            SourceDeviceProfileId: "device-nidek-nt530p-default",
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
                    "PachymetryRight",
                    ExportRuleType.Template,
                    null,
                    "PR: {Device.Data/R/PACHY/PACHYList[@No='1']/Thickness:Pachy} {Device.Data/R/PACHY/PACHYList[@No='2']/Thickness:Pachy} {Device.Data/R/PACHY/PACHYList[@No='3']/Thickness:Pachy} [{Device.Data/R/PACHY/PACHYAverage/Thickness:Pachy}] µm",
                    7,
                    true,
                    "MEDISTAR pachymetry card text for right eye; EV/attachment image export is not implemented yet."),
                new ExportRuleDefinition(
                    "8",
                    "6228",
                    "PachymetryLeft",
                    ExportRuleType.Template,
                    null,
                    "PL: {Device.Data/L/PACHY/PACHYList[@No='1']/Thickness:Pachy} {Device.Data/L/PACHY/PACHYList[@No='2']/Thickness:Pachy} {Device.Data/L/PACHY/PACHYList[@No='3']/Thickness:Pachy} [{Device.Data/L/PACHY/PACHYAverage/Thickness:Pachy}] µm",
                    8,
                    true,
                    "MEDISTAR pachymetry card text for left eye; EV/attachment image export is not implemented yet."),
                new ExportRuleDefinition(
                    "9",
                    "6228",
                    "TonometryBothEyes",
                    ExportRuleType.Template,
                    null,
                    "R = {Device.Data/R/NT/NTList[@No='1']/mmHg:Iop} {Device.Data/R/NT/NTList[@No='2']/mmHg:Iop} {Device.Data/R/NT/NTList[@No='3']/mmHg:Iop} [{Device.Data/R/NT/NTAverage/mmHg:Iop}] // L = {Device.Data/L/NT/NTList[@No='1']/mmHg:Iop} {Device.Data/L/NT/NTList[@No='2']/mmHg:Iop} {Device.Data/L/NT/NTList[@No='3']/mmHg:Iop} [{Device.Data/L/NT/NTAverage/mmHg:Iop}] mmHg {Device.Data/Time:Raw}",
                    9,
                    true,
                    "MEDISTAR tonometry card text; EV/attachment image export is not implemented yet.")
            });
    }
}
