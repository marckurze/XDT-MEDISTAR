using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class AttachmentCompletionServiceTests
{
    private static readonly DateTime Timestamp = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Decide_WaitForQuietPeriodShouldStartWaitOnFirstStableFile()
    {
        var service = new AttachmentCompletionService();

        var decision = service.Decide(
            CreateProfile(quietPeriodSeconds: 10),
            "interface|pair",
            new[] { Candidate("a.pdf") },
            Timestamp);

        Assert.False(decision.CanComplete);
        Assert.True(decision.ShouldWait);
        Assert.Equal(AttachmentCompletionDecisionReason.QuietPeriodStarted, decision.Reason);
    }

    [Fact]
    public void Decide_WaitForQuietPeriodShouldRestartWhenFileCollectionChanges()
    {
        var service = new AttachmentCompletionService();
        var profile = CreateProfile(quietPeriodSeconds: 10);

        service.Decide(profile, "interface|pair", new[] { Candidate("a.pdf") }, Timestamp);
        var decision = service.Decide(
            profile,
            "interface|pair",
            new[] { Candidate("a.pdf"), Candidate("b.jpg") },
            Timestamp.AddSeconds(7));

        Assert.False(decision.CanComplete);
        Assert.Equal(AttachmentCompletionDecisionReason.QuietPeriodRestarted, decision.Reason);
    }

    [Fact]
    public void Decide_WaitForQuietPeriodShouldCompleteAfterQuietPeriod()
    {
        var service = new AttachmentCompletionService();
        var profile = CreateProfile(quietPeriodSeconds: 5);
        var candidates = new[] { Candidate("a.pdf"), Candidate("b.jpg") };

        service.Decide(profile, "interface|pair", candidates, Timestamp);
        var decision = service.Decide(profile, "interface|pair", candidates, Timestamp.AddSeconds(5));

        Assert.True(decision.CanComplete);
        Assert.False(decision.ShouldWait);
        Assert.Equal(AttachmentCompletionDecisionReason.QuietPeriodComplete, decision.Reason);
        Assert.Equal(2, decision.SelectedCandidates.Count);
    }

    [Fact]
    public void Decide_UnstableFileShouldWait()
    {
        var service = new AttachmentCompletionService();

        var decision = service.Decide(
            CreateProfile(),
            "interface|pair",
            new[] { Candidate("a.pdf", isStable: false) },
            Timestamp);

        Assert.False(decision.CanComplete);
        Assert.True(decision.ShouldWait);
        Assert.Equal(AttachmentCompletionDecisionReason.NoStableFiles, decision.Reason);
    }

    [Fact]
    public void Decide_ManualConfirmationShouldRequireUserConfirmation()
    {
        var service = new AttachmentCompletionService();

        var decision = service.Decide(
            CreateProfile(AttachmentCompletionMode.ManualConfirmation),
            "interface|pair",
            new[] { Candidate("a.pdf") },
            Timestamp);

        Assert.False(decision.CanComplete);
        Assert.True(decision.RequiresManualConfirmation);
        Assert.Equal(AttachmentCompletionDecisionReason.ManualConfirmationRequired, decision.Reason);
    }

    [Fact]
    public void ResetProfile_ShouldClearStoredQuietPeriodStateForSelectedProfile()
    {
        var service = new AttachmentCompletionService();
        var profile = CreateProfile(quietPeriodSeconds: 5);
        var candidates = new[] { Candidate("a.pdf") };

        service.Decide(profile, "interface-document|pair", candidates, Timestamp);
        service.ResetProfile("interface-document");
        var decision = service.Decide(profile, "interface-document|pair", candidates, Timestamp.AddSeconds(5));

        Assert.False(decision.CanComplete);
        Assert.Equal(AttachmentCompletionDecisionReason.QuietPeriodStarted, decision.Reason);
    }

    private static InterfaceProfileDefinition CreateProfile(
        AttachmentCompletionMode completionMode = AttachmentCompletionMode.WaitForQuietPeriod,
        int quietPeriodSeconds = 10)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarDocumentAttachmentDefault() with
        {
            FolderOptions = DefaultInterfaceProfileDefinitions.CreateMedistarDocumentAttachmentDefault().FolderOptions with
            {
                AttachmentCompletionMode = completionMode,
                AttachmentQuietPeriodSeconds = quietPeriodSeconds
            },
            IsActive = true
        };
    }

    private static AttachmentImportFileCandidate Candidate(string fileName, bool isStable = true)
    {
        var fullPath = Path.Combine(@"C:\Import\Documents", fileName);
        return new AttachmentImportFileCandidate(
            FileName: fileName,
            Extension: Path.GetExtension(fileName).ToLowerInvariant(),
            FullPath: fullPath,
            SizeBytes: 123,
            LastWriteTimeUtc: Timestamp,
            IsSupported: true,
            StableStatus: isStable ? "Stabil." : "Noch nicht stabil.",
            ErrorMessage: null,
            IsStable: isStable);
    }
}
