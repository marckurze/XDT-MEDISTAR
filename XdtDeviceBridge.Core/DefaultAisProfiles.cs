namespace XdtDeviceBridge.Core;

public static class DefaultAisProfiles
{
    public static AisProfile CreateMedistarDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new AisProfile(
            Metadata: new ProfileMetadata(
                Id: "ais-medistar-default",
                Name: "MEDISTAR",
                ProfileKind: ProfileKind.AisProfile,
                Description: "Default AIS profile for MEDISTAR XDT import.",
                Vendor: "MEDISTAR Praxiscomputer GmbH",
                Product: "MEDISTAR",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            Name: "MEDISTAR",
            Vendor: "MEDISTAR Praxiscomputer GmbH",
            DefaultEncoding: "Windows-1252",
            RequiredStaticFields: new Dictionary<string, string>
            {
                ["8000"] = "6310"
            },
            RequiredPatientFieldCodes: new[] { "3000", "3101", "3102", "3103" },
            SupportedOutputFieldCodes: new[] { "6228", "8402" },
            SupportsResultTextField6228: true,
            SupportsCategoryValuePairs: true,
            RequiresExaminationType8402: true);
    }
}
