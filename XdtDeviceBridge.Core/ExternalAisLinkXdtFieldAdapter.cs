namespace XdtDeviceBridge.Core;

public sealed class ExternalAisLinkXdtFieldAdapter
{
    public ExternalAisLinkXdtFieldAdapterResult Adapt(ExternalAisLinkFieldSet? fieldSet)
    {
        if (fieldSet is null)
        {
            return Fail("External AIS link field set must not be null.");
        }

        if (string.IsNullOrWhiteSpace(fieldSet.FullPath))
        {
            return Fail("External AIS link full path must not be empty.");
        }

        var fields = new List<ExportFieldRecord>();

        AddIfPresent(fields, "6302", fieldSet.DocumentName, 1);
        AddIfPresent(fields, "6303", fieldSet.FileFormat, 2);
        AddIfPresent(fields, "6304", fieldSet.Description, 3);
        fields.Add(new ExportFieldRecord("6305", fieldSet.FullPath.Trim(), 4));

        return new ExternalAisLinkXdtFieldAdapterResult(
            Success: true,
            Fields: fields,
            ErrorMessage: null);
    }

    private static void AddIfPresent(
        List<ExportFieldRecord> fields,
        string fieldCode,
        string? value,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        fields.Add(new ExportFieldRecord(fieldCode, value.Trim(), sortOrder));
    }

    private static ExternalAisLinkXdtFieldAdapterResult Fail(string message)
    {
        return new ExternalAisLinkXdtFieldAdapterResult(
            Success: false,
            Fields: Array.Empty<ExportFieldRecord>(),
            ErrorMessage: message);
    }
}
