namespace XdtDeviceBridge.Infrastructure;

public sealed record AttachmentOnlyConfirmationResult(
    bool ShouldProcess,
    string? DocumentationText,
    IReadOnlyDictionary<string, string> AttachmentDescriptions,
    IReadOnlyList<AttachmentImportFileCandidate> SelectedCandidates)
{
    public static AttachmentOnlyConfirmationResult Proceed(string? documentationText)
    {
        return Proceed(documentationText, null);
    }

    public static AttachmentOnlyConfirmationResult Proceed(IReadOnlyDictionary<string, string>? attachmentDescriptions)
    {
        return Proceed(null, attachmentDescriptions);
    }

    public static AttachmentOnlyConfirmationResult Proceed(
        string? documentationText,
        IReadOnlyDictionary<string, string>? attachmentDescriptions)
    {
        return Proceed(documentationText, attachmentDescriptions, null);
    }

    public static AttachmentOnlyConfirmationResult Proceed(
        string? documentationText,
        IReadOnlyDictionary<string, string>? attachmentDescriptions,
        IReadOnlyList<AttachmentImportFileCandidate>? selectedCandidates)
    {
        return new AttachmentOnlyConfirmationResult(
            true,
            documentationText,
            CopyDescriptions(attachmentDescriptions),
            CopyCandidates(selectedCandidates));
    }

    public static AttachmentOnlyConfirmationResult Cancel()
    {
        return new AttachmentOnlyConfirmationResult(
            false,
            null,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            Array.Empty<AttachmentImportFileCandidate>());
    }

    private static IReadOnlyDictionary<string, string> CopyDescriptions(IReadOnlyDictionary<string, string>? attachmentDescriptions)
    {
        return attachmentDescriptions is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(attachmentDescriptions, StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<AttachmentImportFileCandidate> CopyCandidates(IReadOnlyList<AttachmentImportFileCandidate>? selectedCandidates)
    {
        return selectedCandidates is null
            ? Array.Empty<AttachmentImportFileCandidate>()
            : selectedCandidates.ToList();
    }
}
