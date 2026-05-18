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

    public static ExportProfileDefinition CreateMedistarNidekAr360Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-nidek-ar360-default",
                Name: "MEDISTAR + NIDEK AR360 Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile definition for MEDISTAR and NIDEK AR360 / AR-360A autorefractor LAN XML files.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/NIDEK AR360",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            TargetAisProfileId: "ais-medistar-default",
            SourceDeviceProfileId: "device-nidek-ar360-default",
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
                    "Device.R/AR/ARMedian/Sphere",
                    "R.:S={Device.R/AR/ARMedian/Sphere:Diopter} Z={Device.R/AR/ARMedian/Cylinder:Diopter}*{Device.R/AR/ARMedian/Axis} PD= {Device.PD/PDList[@No='1']/FarPD:Pd} VD= {Device.VD:Raw} mm",
                    7,
                    true,
                    "MEDISTAR autorefractor card text for right eye; ARMedian is used."),
                new ExportRuleDefinition(
                    "8",
                    "6228",
                    "ResultLeft",
                    ExportRuleType.Template,
                    "Device.L/AR/ARMedian/Sphere",
                    "L.:S={Device.L/AR/ARMedian/Sphere:Diopter} Z={Device.L/AR/ARMedian/Cylinder:Diopter}*{Device.L/AR/ARMedian/Axis}",
                    8,
                    true,
                    "MEDISTAR autorefractor card text for left eye; ARMedian is used.")
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
                    "Device.Measure[@Type='LM']/LM/R/MedistarLine",
                    "{value}",
                    7,
                    true,
                    "MEDISTAR lensmeter card text for right eye; optional additions, prism and PD are included only when present."),
                new ExportRuleDefinition(
                    "8",
                    "6228",
                    "LensmeterResultLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='LM']/LM/L/MedistarLine",
                    "{value}",
                    8,
                    true,
                    "MEDISTAR lensmeter card text for left eye; optional additions and prism are included only when present.")
            });
    }

    public static ExportProfileDefinition CreateMedistarNidekNt530PDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 18, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-nidek-nt530p-default",
                Name: "MEDISTAR + NIDEK NT530P Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile definition for MEDISTAR and NIDEK NT-530P tonometry and pachymetry XML files.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/NIDEK NT-530P",
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
                    "6220",
                    "PachymetryResult",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='NT530P']/Pachy/MedistarLine",
                    "{value}",
                    7,
                    true,
                    "MEDISTAR pachymetry line for NIDEK NT-530P; micrometers are converted to millimeters."),
                new ExportRuleDefinition(
                    "8",
                    "6205",
                    "TonometryResult",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='NT530P']/Tono/MedistarLine",
                    "{value}",
                    8,
                    true,
                    "MEDISTAR tonometry line for NIDEK NT-530P; optional fragments are included only when values are present.")
            });
    }

    public static ExportProfileDefinition CreateMedistarTopconCl300Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-topcon-cl300-default",
                Name: "MEDISTAR + TOPCON CL300 Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile definition for MEDISTAR and TOPCON CL300 lensmeter data. Prism/Additionswerte und JOIA-Namespace-Normalisierung werden später präzisiert.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON CL300",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            TargetAisProfileId: "ais-medistar-default",
            SourceDeviceProfileId: "device-topcon-cl300-default",
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
                    "R.:S={Device.Ophthalmology/Measure[@type='LM']/LM/R/Sphere:Diopter} Z={Device.Ophthalmology/Measure[@type='LM']/LM/R/Cylinder:Diopter}*{Device.Ophthalmology/Measure[@type='LM']/LM/R/Axis:Axis}                              PD={Device.Ophthalmology/Measure[@type='LM']/PD/B/Distance:Pd}",
                    7,
                    true,
                    "MEDISTAR lensmeter card text for TOPCON CL300 right eye. Prism/Additionswerte werden später ergänzt."),
                new ExportRuleDefinition(
                    "8",
                    "6228",
                    "LensmeterResultLeft",
                    ExportRuleType.Template,
                    null,
                    "L.:S={Device.Ophthalmology/Measure[@type='LM']/LM/L/Sphere:Diopter} Z={Device.Ophthalmology/Measure[@type='LM']/LM/L/Cylinder:Diopter}*{Device.Ophthalmology/Measure[@type='LM']/LM/L/Axis:Axis}",
                    8,
                    true,
                    "MEDISTAR lensmeter card text for TOPCON CL300 left eye. Prism/Additionswerte werden später ergänzt.")
            });
    }

    public static ExportProfileDefinition CreateMedistarTopconKr800Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-topcon-kr800-default",
                Name: "MEDISTAR + TOPCON KR800 Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile definition for MEDISTAR and TOPCON KR800 REF/KM/SBJ data. KM-Ausgabe, SBJ-Auswahl und JOIA-Namespace-Normalisierung werden später präzisiert.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON KR800",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            TargetAisProfileId: "ais-medistar-default",
            SourceDeviceProfileId: "device-topcon-kr800-default",
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
                    "RefResultRight",
                    ExportRuleType.Template,
                    null,
                    "R.:S={Device.Ophthalmology/Measure[@type='REF']/REF/R/Median/Sphere:Diopter} Z={Device.Ophthalmology/Measure[@type='REF']/REF/R/Median/Cylinder:Diopter}*{Device.Ophthalmology/Measure[@type='REF']/REF/R/Median/Axis:Axis}                              PD={Device.Ophthalmology/Measure[@type='REF']/PD/Distance:Pd}",
                    7,
                    true,
                    "MEDISTAR autorefractor card text for TOPCON KR800 REF right eye."),
                new ExportRuleDefinition(
                    "8",
                    "6228",
                    "RefResultLeft",
                    ExportRuleType.Template,
                    null,
                    "L.:S={Device.Ophthalmology/Measure[@type='REF']/REF/L/Median/Sphere:Diopter} Z={Device.Ophthalmology/Measure[@type='REF']/REF/L/Median/Cylinder:Diopter}*{Device.Ophthalmology/Measure[@type='REF']/REF/L/Median/Axis:Axis}                              PD={Device.Ophthalmology/Measure[@type='REF']/PD/Distance:Pd}",
                    8,
                    true,
                    "MEDISTAR autorefractor card text for TOPCON KR800 REF left eye."),
                new ExportRuleDefinition(
                    "9",
                    "6228",
                    "KeratometryResultRight",
                    ExportRuleType.Template,
                    null,
                    "KR: K1={Device.Ophthalmology/Measure[@type='KM']/KM/R/Median/R1/Power:Keratometry}*{Device.Ophthalmology/Measure[@type='KM']/KM/R/Median/R1/Axis:Axis} K2={Device.Ophthalmology/Measure[@type='KM']/KM/R/Median/R2/Power:Keratometry}*{Device.Ophthalmology/Measure[@type='KM']/KM/R/Median/R2/Axis:Axis}",
                    9,
                    true,
                    "KM-Ausgabe noch zu validieren."),
                new ExportRuleDefinition(
                    "10",
                    "6228",
                    "KeratometryResultLeft",
                    ExportRuleType.Template,
                    null,
                    "KL: K1={Device.Ophthalmology/Measure[@type='KM']/KM/L/Median/R1/Power:Keratometry}*{Device.Ophthalmology/Measure[@type='KM']/KM/L/Median/R1/Axis:Axis} K2={Device.Ophthalmology/Measure[@type='KM']/KM/L/Median/R2/Power:Keratometry}*{Device.Ophthalmology/Measure[@type='KM']/KM/L/Median/R2/Axis:Axis}",
                    10,
                    true,
                    "KM-Ausgabe noch zu validieren.")
            });
    }

    public static ExportProfileDefinition CreateMedistarTopconTrk2PDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-topcon-trk2p-default",
                Name: "MEDISTAR + TOPCON TRK2P Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile definition for MEDISTAR and TOPCON TRK2P TM/CCT data. CCT-Umrechnung nach µm und JOIA-Namespace-Normalisierung werden später präzisiert.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON TRK2P",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            TargetAisProfileId: "ais-medistar-default",
            SourceDeviceProfileId: "device-topcon-trk2p-default",
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
                    "TonometryBothEyes",
                    ExportRuleType.Template,
                    null,
                    "R = {Device.Ophthalmology/Measure[@type='TM']/TM/R/Average/IOP_mmHg:Iop} // L = {Device.Ophthalmology/Measure[@type='TM']/TM/L/Average/IOP_mmHg:Iop} mmHg",
                    7,
                    true,
                    "MEDISTAR tonometry card text for TOPCON TRK2P right/left IOP averages."),
                new ExportRuleDefinition(
                    "8",
                    "6228",
                    "PachymetryRight",
                    ExportRuleType.Template,
                    null,
                    "PR: {Device.Ophthalmology/Measure[@type='TM']/CCT/R/List[@No='3']/CCT_mm:Pachy} µm",
                    8,
                    true,
                    "MEDISTAR pachymetry card text for TOPCON TRK2P right eye; CCT source is mm and µm conversion is noch zu validieren."),
                new ExportRuleDefinition(
                    "9",
                    "6228",
                    "PachymetryLeft",
                    ExportRuleType.Template,
                    null,
                    "PL: {Device.Ophthalmology/Measure[@type='TM']/CCT/L/List[@No='1']/CCT_mm:Pachy} µm",
                    9,
                    true,
                    "MEDISTAR pachymetry card text for TOPCON TRK2P left eye; CCT source is mm and µm conversion is noch zu validieren.")
            });
    }
}
