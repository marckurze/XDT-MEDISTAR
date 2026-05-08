namespace XdtDeviceBridge.Core;

public sealed record AttachmentAutoProcessingEligibilityResult(
    bool IsAllowed,
    IReadOnlyList<string> Reasons);
