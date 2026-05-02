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
                new("1", "3000", "PatientNumber", "AIS.PatientNumber", "{value}", 1, true),
                new("2", "3101", "LastName", "AIS.LastName", "{value}", 2, true),
                new("3", "3102", "FirstName", "AIS.FirstName", "{value}", 3, true),
                new("4", "3103", "BirthDate", "AIS.BirthDate", "{value}", 4, true),
                new("5", "9001", "SphereR", "Device.R/AR/ARMedian/Sphere", "{value}", 5, true),
                new("6", "9002", "CylinderR", "Device.R/AR/ARMedian/Cylinder", "{value}", 6, true),
                new("7", "9003", "AxisR", "Device.R/AR/ARMedian/Axis", "{value}", 7, true),
                new("8", "9004", "SER", "Device.R/AR/ARMedian/SE", "{value}", 8, true),
                new("9", "9011", "SphereL", "Device.L/AR/ARMedian/Sphere", "{value}", 9, true),
                new("10", "9012", "CylinderL", "Device.L/AR/ARMedian/Cylinder", "{value}", 10, true),
                new("11", "9013", "AxisL", "Device.L/AR/ARMedian/Axis", "{value}", 11, true),
                new("12", "9014", "SEL", "Device.L/AR/ARMedian/SE", "{value}", 12, true),
                new("13", "9021", "FarPD", "Device.PD/PDList/FarPD", "{value}", 13, true),
                new("14", "9022", "NearPD", "Device.PD/PDList/NearPD", "{value}", 14, true)
            });
    }
}
