namespace XdtDeviceBridge.Core;

public sealed class PatientDataMapper
{
    public PatientData Map(IEnumerable<FieldRecord> fields)
    {
        var records = fields.ToList();

        string? Get(string code) => records.LastOrDefault(r => r.FieldCode == code)?.Value;

        return new PatientData(
            PatientNumber: Get("3000"),
            LastName: Get("3101"),
            FirstName: Get("3102"),
            BirthDate: Get("3103"),
            PostalCodeCity: Get("3106"),
            Street: Get("3107"),
            GenderCode: Get("3110"),
            SourceSystem: Get("0102"),
            TargetSystem: Get("0103"),
            GdtVersion: Get("9218"),
            ExaminationType: Get("8402"));
    }
}