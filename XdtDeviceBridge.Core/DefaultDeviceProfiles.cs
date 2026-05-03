namespace XdtDeviceBridge.Core;

public static class DefaultDeviceProfiles
{
    public static DeviceProfile CreateNidekArk1sDefault()
    {
        return new DeviceProfile(
            Id: "nidek-ark1s-default",
            Name: "NIDEK ARK1S",
            AisImportFolder: string.Empty,
            DeviceImportFolder: string.Empty,
            ExportFolder: string.Empty,
            ArchiveFolder: string.Empty,
            ErrorFolder: string.Empty,
            ExportFileNamePattern: "NIDEK_ARK1S_{PatientNumber}_{yyyyMMdd_HHmmss}.XDT",
            DeviceParserMode: DeviceParserMode.Xml,
            OutputEncoding: "Windows-1252",
            AutoExport: false,
            AssignmentWindowMinutes: 10,
            MappingRules: new List<MappingRule>
            {
                // MEDISTAR-XDT-Server: Datei soll eingelesen werden.
                new("1", "8000", "MessageType", "AIS.PatientNumber", "6310", 1, true),

                // Patientendaten aus der AIS-GDT-Datei.
                new("2", "3000", "PatientNumber", "AIS.PatientNumber", "{value}", 2, true),
                new("3", "3101", "LastName", "AIS.LastName", "{value}", 3, true),
                new("4", "3102", "FirstName", "AIS.FirstName", "{value}", 4, true),
                new("5", "3103", "BirthDate", "AIS.BirthDate", "{value}", 5, true),

                // Untersuchungsart kommt aus MEDISTAR, z. B. 8402 = ARK1S.
                new("6", "8402", "ExaminationType", "AIS.ExaminationType", "{value}", 6, true),

                // Ergebniszeilen für MEDISTAR-Karteikarte.
                // MEDISTAR erzeugt daraus z. B.:
                // V1 R.:S=- 0.25 Z=- 0.25* 49                              PD=61
                new(
                    "7",
                    "6228",
                    "ResultRight",
                    "Device.R/AR/ARMedian/Sphere",
                    "R.:S={Device.R/AR/ARMedian/Sphere} Z={Device.R/AR/ARMedian/Cylinder}*{Device.R/AR/ARMedian/Axis}                              PD={Device.PD/PDList[@No='1']/FarPD}",
                    7,
                    true),

                new(
                    "8",
                    "6228",
                    "ResultLeft",
                    "Device.L/AR/ARMedian/Sphere",
                    "L.:S={Device.L/AR/ARMedian/Sphere} Z={Device.L/AR/ARMedian/Cylinder}*{Device.L/AR/ARMedian/Axis}                              PD={Device.PD/PDList[@No='1']/FarPD}",
                    8,
                    true)
            });
    }
}