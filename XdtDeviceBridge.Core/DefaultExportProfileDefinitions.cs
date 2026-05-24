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
                    "6205",
                    "TonometryHeader",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='NT530P']/Tono/HeaderLine",
                    "{value}",
                    7,
                    true,
                    "MEDISTAR tonometry heading for NIDEK NT-530P."),
                new ExportRuleDefinition(
                    "8",
                    "6205",
                    "TonometryPachyRight",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='NT530P']/Tono/PachyRightLine",
                    "{value}",
                    8,
                    true,
                    "MEDISTAR tonometry pachymetry values for the right eye."),
                new ExportRuleDefinition(
                    "9",
                    "6205",
                    "TonometryPachyLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='NT530P']/Tono/PachyLeftLine",
                    "{value}",
                    9,
                    true,
                    "MEDISTAR tonometry pachymetry values for the left eye."),
                new ExportRuleDefinition(
                    "10",
                    "6205",
                    "TonometryMeasuredRight",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='NT530P']/Tono/MeasuredRightLine",
                    "{value}",
                    10,
                    true,
                    "MEDISTAR tonometry measured IOP for the right eye."),
                new ExportRuleDefinition(
                    "11",
                    "6205",
                    "TonometryCorrectedRight",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='NT530P']/Tono/CorrectedRightLine",
                    "{value}",
                    11,
                    true,
                    "MEDISTAR tonometry corrected IOP and parameters for the right eye."),
                new ExportRuleDefinition(
                    "12",
                    "6205",
                    "TonometryRightCctLeftMeasured",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='NT530P']/Tono/RightCctLeftMeasuredLine",
                    "{value}",
                    12,
                    true,
                    "MEDISTAR tonometry right CCT and left measured/corrected IOP."),
                new ExportRuleDefinition(
                    "13",
                    "6205",
                    "TonometryParameterLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='NT530P']/Tono/ParameterLeftLine",
                    "{value}",
                    13,
                    true,
                    "MEDISTAR tonometry parameters for the left eye."),
                new ExportRuleDefinition(
                    "14",
                    "6205",
                    "TonometryList",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='NT530P']/Tono/TonoListLine",
                    "{value}",
                    14,
                    true,
                    "MEDISTAR tonometry single values, averages and measurement time."),
                new ExportRuleDefinition(
                    "15",
                    "6220",
                    "PachymetryHeader",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='NT530P']/Pachy/HeaderLine",
                    "{value}",
                    15,
                    true,
                    "MEDISTAR pachymetry heading for NIDEK NT-530P."),
                new ExportRuleDefinition(
                    "16",
                    "6220",
                    "PachymetryResult",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='NT530P']/Pachy/MedistarLine",
                    "{value}",
                    16,
                    true,
                    "MEDISTAR pachymetry line for NIDEK NT-530P; micrometers are converted to millimeters.")
            });
    }

    public static ExportProfileDefinition CreateMedistarDocumentAttachmentDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 20, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-document-attachment-default",
                Name: "MEDISTAR + Dokumentanhang Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile for document-only devices. Optional documentation text is exported as 6227; attachments are appended as 6302-6305 link fields.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/Dokumentanhang",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            TargetAisProfileId: "ais-medistar-default",
            SourceDeviceProfileId: "device-document-attachment-default",
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
                    "6227",
                    "DocumentationText",
                    ExportRuleType.Template,
                    "Device.AttachmentOnly/DocumentationText",
                    "{value}",
                    7,
                    true,
                    "Optional documentation text entered for a document-only device workflow.")
            });
    }

    public static ExportProfileDefinition CreateMedistarManualDocumentTransferDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-manual-document-transfer-default",
                Name: "MEDISTAR + Manuelle Dokumentübergabe Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile for manual document handoff. Attachments are appended as 6302-6305 link fields; no measurement values are emitted.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/Manuelle Dokumentübergabe",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            TargetAisProfileId: "ais-medistar-default",
            SourceDeviceProfileId: "device-manual-document-selection-default",
            OutputEncoding: "Windows-1252",
            Rules: new[]
            {
                new ExportRuleDefinition("1", "8000", "MessageType", ExportRuleType.StaticValue, null, "6310", 1, true, "MEDISTAR XDT import control."),
                new ExportRuleDefinition("2", "3000", "PatientNumber", ExportRuleType.AisField, "AIS.PatientNumber", "{value}", 2, true, "Patient number from AIS."),
                new ExportRuleDefinition("3", "3101", "LastName", ExportRuleType.AisField, "AIS.LastName", "{value}", 3, true, "Last name from AIS."),
                new ExportRuleDefinition("4", "3102", "FirstName", ExportRuleType.AisField, "AIS.FirstName", "{value}", 4, true, "First name from AIS."),
                new ExportRuleDefinition("5", "3103", "BirthDate", ExportRuleType.AisField, "AIS.BirthDate", "{value}", 5, true, "Birth date from AIS."),
                new ExportRuleDefinition("6", "8402", "ExaminationType", ExportRuleType.AisField, "AIS.ExaminationType", "{value}", 6, true, "Examination type from AIS.")
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
                Description: "Default export profile definition for MEDISTAR and TOPCON CL-300 lensmeter XML data.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON CL-300",
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
                    "Device.Measure[@Type='LM']/LM/R/MedistarLine",
                    "{value}",
                    7,
                    true,
                    "MEDISTAR lensmeter card text for TOPCON CL-300 right eye; optional additions and PD are included only when present."),
                new ExportRuleDefinition(
                    "8",
                    "6228",
                    "LensmeterResultLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='LM']/LM/L/MedistarLine",
                    "{value}",
                    8,
                    true,
                    "MEDISTAR lensmeter card text for TOPCON CL-300 left eye; optional additions are included only when present.")
            });
    }

    public static ExportProfileDefinition CreateMedistarTopconSolosDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 24, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-topcon-solos-default",
                Name: "MEDISTAR + TOPCON Solos Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile definition for MEDISTAR and TOPCON SOLOS lensmeter XML data. Lensmeter values are emitted via MEDISTAR 6228; transmission values are not exported yet.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON SOLOS",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            TargetAisProfileId: "ais-medistar-default",
            SourceDeviceProfileId: "device-topcon-solos-default",
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
                    "MEDISTAR lensmeter card text for TOPCON SOLOS right eye; optional additions, prism and PD are included only when present."),
                new ExportRuleDefinition(
                    "8",
                    "6228",
                    "LensmeterResultLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='LM']/LM/L/MedistarLine",
                    "{value}",
                    8,
                    true,
                    "MEDISTAR lensmeter card text for TOPCON SOLOS left eye; optional additions and prism are included only when present.")
            });
    }

    public static ExportProfileDefinition CreateMedistarTopconKr800Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-topcon-kr800-default",
                Name: "MEDISTAR + TOPCON KR800S Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile definition for MEDISTAR and TOPCON KR-800S REF, KM and SBJ XML data.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON KR-800S",
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
                    "Device.Measure[@Type='REF']/REF/R/MedistarLine",
                    "{value}",
                    7,
                    true,
                    "MEDISTAR autorefractor card text for TOPCON KR-800S REF right eye."),
                new ExportRuleDefinition(
                    "8",
                    "6228",
                    "RefResultLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='REF']/REF/L/MedistarLine",
                    "{value}",
                    8,
                    true,
                    "MEDISTAR autorefractor card text for TOPCON KR-800S REF left eye."),
                new ExportRuleDefinition(
                    "9",
                    "6221",
                    "KeratometryRadii",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='KM']/KM/MedistarLine1",
                    "{value}",
                    9,
                    true,
                    "MEDISTAR keratometry R1/R2 card text for TOPCON KR-800S."),
                new ExportRuleDefinition(
                    "10",
                    "6221",
                    "KeratometryAverageCylinder",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='KM']/KM/MedistarLine2",
                    "{value}",
                    10,
                    true,
                    "MEDISTAR keratometry AV/CYL card text for TOPCON KR-800S."),
                new ExportRuleDefinition(
                    "11",
                    "6227",
                    "SubjectiveRefractionLine1",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='SBJ']/MedistarLine1",
                    "{value}",
                    11,
                    true,
                    "Conservative subjective refraction line 1 for TOPCON KR-800S."),
                new ExportRuleDefinition(
                    "12",
                    "6227",
                    "SubjectiveRefractionLine2",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='SBJ']/MedistarLine2",
                    "{value}",
                    12,
                    true,
                    "Conservative subjective refraction line 2 for TOPCON KR-800S."),
                new ExportRuleDefinition(
                    "13",
                    "6227",
                    "SubjectiveRefractionLine3",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='SBJ']/MedistarLine3",
                    "{value}",
                    13,
                    true,
                    "Conservative subjective refraction line 3 for TOPCON KR-800S when present."),
                new ExportRuleDefinition(
                    "14",
                    "6227",
                    "SubjectiveRefractionLine4",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='SBJ']/MedistarLine4",
                    "{value}",
                    14,
                    true,
                    "Conservative subjective refraction line 4 for TOPCON KR-800S when present.")
            });
    }

    public static ExportProfileDefinition CreateMedistarTopconKr1Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 24, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-topcon-kr1-default",
                Name: "MEDISTAR + TOPCON KR-1 Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile definition for MEDISTAR and TOPCON KR-1 REF XML data. KM/KRT export is intentionally not enabled without a real KM/KRT fixture.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON KR-1",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            TargetAisProfileId: "ais-medistar-default",
            SourceDeviceProfileId: "device-topcon-kr1-default",
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
                    "Device.Measure[@Type='REF']/REF/R/MedistarLine",
                    "{value}",
                    7,
                    true,
                    "MEDISTAR autorefractor card text for TOPCON KR-1 REF right eye."),
                new ExportRuleDefinition(
                    "8",
                    "6228",
                    "RefResultLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='REF']/REF/L/MedistarLine",
                    "{value}",
                    8,
                    true,
                    "MEDISTAR autorefractor card text for TOPCON KR-1 REF left eye.")
            });
    }

    public static ExportProfileDefinition CreateMedistarTopconTrk2PDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-topcon-trk2p-default",
                Name: "MEDISTAR + TOPCON TRK2P Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile definition for MEDISTAR and TOPCON TRK-2P REF, KM, TM, CCT and optional SBJ data.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON TRK-2P",
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
                    "RefResultRight",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='REF']/REF/R/MedistarLine",
                    "{value}",
                    7,
                    true,
                    "MEDISTAR autorefractor line for TOPCON TRK-2P right eye."),
                new ExportRuleDefinition(
                    "8",
                    "6228",
                    "RefResultLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='REF']/REF/L/MedistarLine",
                    "{value}",
                    8,
                    true,
                    "MEDISTAR autorefractor line for TOPCON TRK-2P left eye."),
                new ExportRuleDefinition(
                    "9",
                    "6221",
                    "KeratometryRadii",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='KM']/KM/MedistarLine1",
                    "{value}",
                    9,
                    true,
                    "MEDISTAR keratometry R1/R2 line for TOPCON TRK-2P."),
                new ExportRuleDefinition(
                    "10",
                    "6221",
                    "KeratometryAverage",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='KM']/KM/MedistarLine2",
                    "{value}",
                    10,
                    true,
                    "MEDISTAR keratometry AV/CYL line for TOPCON TRK-2P."),
                new ExportRuleDefinition(
                    "11",
                    "6220",
                    "PachymetryHeader",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='CCT']/Pachy/HeaderLine",
                    "{value}",
                    11,
                    true,
                    "MEDISTAR pachymetry heading for TOPCON TRK-2P when CCT is available."),
                new ExportRuleDefinition(
                    "12",
                    "6220",
                    "Pachymetry",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='CCT']/Pachy/MedistarLine",
                    "{value}",
                    12,
                    true,
                    "MEDISTAR pachymetry line for TOPCON TRK-2P when CCT is available."),
                new ExportRuleDefinition(
                    "13",
                    "6205",
                    "TonometryHeader",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/HeaderLine",
                    "{value}",
                    13,
                    true,
                    "MEDISTAR tonometry heading for TOPCON TRK-2P when TM values are available."),
                new ExportRuleDefinition(
                    "14",
                    "6205",
                    "TonometryPachyRight",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/PachyRightLine",
                    "{value}",
                    14,
                    true,
                    "MEDISTAR tonometry pachymetry line for TOPCON TRK-2P right eye when CCT is available."),
                new ExportRuleDefinition(
                    "15",
                    "6205",
                    "TonometryPachyLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/PachyLeftLine",
                    "{value}",
                    15,
                    true,
                    "MEDISTAR tonometry pachymetry line for TOPCON TRK-2P left eye when CCT is available."),
                new ExportRuleDefinition(
                    "16",
                    "6205",
                    "TonometryMeasuredRight",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/MeasuredRightLine",
                    "{value}",
                    16,
                    true,
                    "MEDISTAR tonometry measured/corrected line for TOPCON TRK-2P right eye when available."),
                new ExportRuleDefinition(
                    "17",
                    "6205",
                    "TonometryParameterRight",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/ParameterRightLine",
                    "{value}",
                    17,
                    true,
                    "MEDISTAR tonometry parameter line for TOPCON TRK-2P right eye when available."),
                new ExportRuleDefinition(
                    "18",
                    "6205",
                    "TonometryMeasuredLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/MeasuredLeftLine",
                    "{value}",
                    18,
                    true,
                    "MEDISTAR tonometry measured/corrected line for TOPCON TRK-2P left eye when available."),
                new ExportRuleDefinition(
                    "19",
                    "6205",
                    "TonometryParameterLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/ParameterLeftLine",
                    "{value}",
                    19,
                    true,
                    "MEDISTAR tonometry parameter line for TOPCON TRK-2P left eye when available."),
                new ExportRuleDefinition(
                    "20",
                    "6205",
                    "TonometryList",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/TonoListLine",
                    "{value}",
                    20,
                    true,
                    "MEDISTAR tonometry list line for TOPCON TRK-2P when TM values are available."),
                new ExportRuleDefinition(
                    "21",
                    "6227",
                    "SubjectiveRefractionLine1",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='SBJ']/MedistarLine1",
                    "{value}",
                    21,
                    true,
                    "Conservative subjective refraction line 1 for TOPCON TRK-2P when SBJ values are present."),
                new ExportRuleDefinition(
                    "22",
                    "6227",
                    "SubjectiveRefractionLine2",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='SBJ']/MedistarLine2",
                    "{value}",
                    22,
                    true,
                    "Conservative subjective refraction line 2 for TOPCON TRK-2P when SBJ values are present."),
                new ExportRuleDefinition(
                    "23",
                    "6227",
                    "SubjectiveRefractionLine3",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='SBJ']/MedistarLine3",
                    "{value}",
                    23,
                    true,
                    "Conservative subjective refraction line 3 for TOPCON TRK-2P when SBJ values are present."),
                new ExportRuleDefinition(
                    "24",
                    "6227",
                    "SubjectiveRefractionLine4",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='SBJ']/MedistarLine4",
                    "{value}",
                    24,
                    true,
                    "Conservative subjective refraction line 4 for TOPCON TRK-2P when SBJ values are present.")
            });
    }

    public static ExportProfileDefinition CreateMedistarTopconCt1PDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 22, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-topcon-ct1p-default",
                Name: "MEDISTAR + TOPCON CT1P Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile definition for MEDISTAR and TOPCON CT-1P TM and CorrectedIOP/CCT XML data.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON CT-1P",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            TargetAisProfileId: "ais-medistar-default",
            SourceDeviceProfileId: "device-topcon-ct1p-default",
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
                    "PachymetryHeader",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='CCT']/Pachy/HeaderLine",
                    "{value}",
                    7,
                    true,
                    "MEDISTAR pachymetry heading for TOPCON CT-1P when CCT is available."),
                new ExportRuleDefinition(
                    "8",
                    "6220",
                    "Pachymetry",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='CCT']/Pachy/MedistarLine",
                    "{value}",
                    8,
                    true,
                    "MEDISTAR pachymetry line for TOPCON CT-1P when CCT is available."),
                new ExportRuleDefinition(
                    "9",
                    "6205",
                    "TonometryHeader",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/HeaderLine",
                    "{value}",
                    9,
                    true,
                    "MEDISTAR tonometry heading for TOPCON CT-1P when TM values are available."),
                new ExportRuleDefinition(
                    "10",
                    "6205",
                    "TonometryPachyRight",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/PachyRightLine",
                    "{value}",
                    10,
                    true,
                    "MEDISTAR tonometry pachymetry line for TOPCON CT-1P right eye when CCT is available."),
                new ExportRuleDefinition(
                    "11",
                    "6205",
                    "TonometryPachyLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/PachyLeftLine",
                    "{value}",
                    11,
                    true,
                    "MEDISTAR tonometry pachymetry line for TOPCON CT-1P left eye when CCT is available."),
                new ExportRuleDefinition(
                    "12",
                    "6205",
                    "TonometryMeasuredRight",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/MeasuredRightLine",
                    "{value}",
                    12,
                    true,
                    "MEDISTAR tonometry measured/corrected line for TOPCON CT-1P right eye when available."),
                new ExportRuleDefinition(
                    "13",
                    "6205",
                    "TonometryParameterRight",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/ParameterRightLine",
                    "{value}",
                    13,
                    true,
                    "MEDISTAR tonometry parameter line for TOPCON CT-1P right eye when available."),
                new ExportRuleDefinition(
                    "14",
                    "6205",
                    "TonometryMeasuredLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/MeasuredLeftLine",
                    "{value}",
                    14,
                    true,
                    "MEDISTAR tonometry measured/corrected line for TOPCON CT-1P left eye when available."),
                new ExportRuleDefinition(
                    "15",
                    "6205",
                    "TonometryParameterLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/ParameterLeftLine",
                    "{value}",
                    15,
                    true,
                    "MEDISTAR tonometry parameter line for TOPCON CT-1P left eye when available."),
                new ExportRuleDefinition(
                    "16",
                    "6205",
                    "TonometryList",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='TM']/Tono/TonoListLine",
                    "{value}",
                    16,
                    true,
                    "MEDISTAR tonometry list line for TOPCON CT-1P when TM values are available.")
            });
    }

    public static ExportProfileDefinition CreateMedistarTopconCt800ADefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 24, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-topcon-ct800a-default",
                Name: "MEDISTAR + TOPCON CT-800A Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile definition for MEDISTAR and TOPCON CT-800A TM XML data. Tonometry is emitted via 6205; CorrectedIOP/CCT details are included only when complete.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON CT-800A",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            TargetAisProfileId: "ais-medistar-default",
            SourceDeviceProfileId: "device-topcon-ct800a-default",
            OutputEncoding: "Windows-1252",
            Rules: new[]
            {
                new ExportRuleDefinition("1", "8000", "MessageType", ExportRuleType.StaticValue, null, "6310", 1, true, "MEDISTAR XDT import control."),
                new ExportRuleDefinition("2", "3000", "PatientNumber", ExportRuleType.AisField, "AIS.PatientNumber", "{value}", 2, true, "Patient number from AIS."),
                new ExportRuleDefinition("3", "3101", "LastName", ExportRuleType.AisField, "AIS.LastName", "{value}", 3, true, "Last name from AIS."),
                new ExportRuleDefinition("4", "3102", "FirstName", ExportRuleType.AisField, "AIS.FirstName", "{value}", 4, true, "First name from AIS."),
                new ExportRuleDefinition("5", "3103", "BirthDate", ExportRuleType.AisField, "AIS.BirthDate", "{value}", 5, true, "Birth date from AIS."),
                new ExportRuleDefinition("6", "8402", "ExaminationType", ExportRuleType.AisField, "AIS.ExaminationType", "{value}", 6, true, "Examination type from AIS."),
                new ExportRuleDefinition("7", "6205", "TonometryHeader", ExportRuleType.Template, "Device.Measure[@Type='TM']/Tono/HeaderLine", "{value}", 7, true, "MEDISTAR tonometry heading for TOPCON CT-800A when TM values are available."),
                new ExportRuleDefinition("8", "6205", "TonometryPachyRight", ExportRuleType.Template, "Device.Measure[@Type='TM']/Tono/PachyRightLine", "{value}", 8, true, "MEDISTAR tonometry CCT line for TOPCON CT-800A right eye when CorrectedIOP is complete."),
                new ExportRuleDefinition("9", "6205", "TonometryPachyLeft", ExportRuleType.Template, "Device.Measure[@Type='TM']/Tono/PachyLeftLine", "{value}", 9, true, "MEDISTAR tonometry CCT line for TOPCON CT-800A left eye when CorrectedIOP is complete."),
                new ExportRuleDefinition("10", "6205", "TonometryMeasuredRight", ExportRuleType.Template, "Device.Measure[@Type='TM']/Tono/MeasuredRightLine", "{value}", 10, true, "MEDISTAR measured/corrected IOP line for TOPCON CT-800A right eye when complete."),
                new ExportRuleDefinition("11", "6205", "TonometryParameterRight", ExportRuleType.Template, "Device.Measure[@Type='TM']/Tono/ParameterRightLine", "{value}", 11, true, "MEDISTAR CorrectedIOP parameter line for TOPCON CT-800A right eye when complete."),
                new ExportRuleDefinition("12", "6205", "TonometryMeasuredLeft", ExportRuleType.Template, "Device.Measure[@Type='TM']/Tono/MeasuredLeftLine", "{value}", 12, true, "MEDISTAR measured/corrected IOP line for TOPCON CT-800A left eye when complete."),
                new ExportRuleDefinition("13", "6205", "TonometryParameterLeft", ExportRuleType.Template, "Device.Measure[@Type='TM']/Tono/ParameterLeftLine", "{value}", 13, true, "MEDISTAR CorrectedIOP parameter line for TOPCON CT-800A left eye when complete."),
                new ExportRuleDefinition("14", "6205", "TonometryList", ExportRuleType.Template, "Device.Measure[@Type='TM']/Tono/TonoListLine", "{value}", 14, true, "MEDISTAR tonometry list line for TOPCON CT-800A when TM values are available.")
            });
    }

    public static ExportProfileDefinition CreateMedistarTopconCv5000Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 23, 12, 0, 0, TimeSpan.Zero);

        return new ExportProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "export-medistar-topcon-cv5000-default",
                Name: "MEDISTAR + TOPCON CV-5000 Export",
                ProfileKind: ProfileKind.ExportProfile,
                Description: "Default export profile definition for MEDISTAR and TOPCON CV-5000 / CV-5000S phoropter SBJ XML return files. Prescription is emitted entirely with 6228; Full Correction is emitted entirely with 6227.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON CV-5000",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            TargetAisProfileId: "ais-medistar-default",
            SourceDeviceProfileId: "device-topcon-cv5000-default",
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
                    "PrescriptionHeader",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='SBJ']/Prescription/HeaderLine",
                    "{value}",
                    7,
                    true,
                    "MEDISTAR 6228 header for TOPCON CV-5000 Prescription / final prescription value."),
                new ExportRuleDefinition(
                    "8",
                    "6228",
                    "PrescriptionRight",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='SBJ']/Prescription/R/MedistarLine",
                    "{value}",
                    8,
                    true,
                    "MEDISTAR 6228 right-eye Prescription value from TOPCON CV-5000."),
                new ExportRuleDefinition(
                    "9",
                    "6228",
                    "PrescriptionLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='SBJ']/Prescription/L/MedistarLine",
                    "{value}",
                    9,
                    true,
                    "MEDISTAR 6228 left-eye Prescription value from TOPCON CV-5000."),
                new ExportRuleDefinition(
                    "10",
                    "6227",
                    "FullCorrectionHeader",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='SBJ']/FullCorrection/HeaderLine",
                    "{value}",
                    10,
                    true,
                    "MEDISTAR header for TOPCON CV-5000 Full Correction / maximum correction value."),
                new ExportRuleDefinition(
                    "11",
                    "6227",
                    "FullCorrectionRight",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='SBJ']/FullCorrection/R/MedistarLine",
                    "{value}",
                    11,
                    true,
                    "MEDISTAR 6227 right-eye Full Correction value from TOPCON CV-5000."),
                new ExportRuleDefinition(
                    "12",
                    "6227",
                    "FullCorrectionLeft",
                    ExportRuleType.Template,
                    "Device.Measure[@Type='SBJ']/FullCorrection/L/MedistarLine",
                    "{value}",
                    12,
                    true,
                    "MEDISTAR 6227 left-eye Full Correction value from TOPCON CV-5000.")
            });
    }
}
