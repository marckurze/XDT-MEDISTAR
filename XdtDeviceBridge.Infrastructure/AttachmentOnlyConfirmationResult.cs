namespace XdtDeviceBridge.Infrastructure;

public sealed record AttachmentOnlyConfirmationResult(bool ShouldProcess, string? DocumentationText)
{
    public static AttachmentOnlyConfirmationResult Proceed(string? documentationText)
    {
        return new AttachmentOnlyConfirmationResult(true, documentationText);
    }

    public static AttachmentOnlyConfirmationResult Cancel()
    {
        return new AttachmentOnlyConfirmationResult(false, null);
    }
}
