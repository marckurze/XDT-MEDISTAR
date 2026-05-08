using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class AisPatientDataReader : IAisPatientDataReader
{
    private readonly GdtParser _gdtParser = new();
    private readonly PatientDataMapper _patientDataMapper = new();

    public AisPatientDataReadResult Read(string aisFilePath)
    {
        if (string.IsNullOrWhiteSpace(aisFilePath))
        {
            return new AisPatientDataReadResult(false, null, "AIS-Dateipfad fehlt.");
        }

        try
        {
            var parseResult = _gdtParser.ParseFile(aisFilePath);
            if (parseResult.HasErrors)
            {
                return new AisPatientDataReadResult(false, null, "AIS-Datei konnte nicht fehlerfrei gelesen werden.");
            }

            return new AisPatientDataReadResult(true, _patientDataMapper.Map(parseResult.Records), null);
        }
        catch (Exception ex)
        {
            return new AisPatientDataReadResult(false, null, ex.Message);
        }
    }
}
