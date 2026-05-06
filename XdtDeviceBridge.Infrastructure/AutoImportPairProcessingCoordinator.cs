using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class AutoImportPairProcessingCoordinator
{
    private readonly IInterfaceProfileManualProcessor _manualProcessor;
    private readonly HashSet<string> _processedPairKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _processingPairKeys = new(StringComparer.OrdinalIgnoreCase);

    public AutoImportPairProcessingCoordinator()
        : this(new InterfaceProfileManualProcessor())
    {
    }

    public AutoImportPairProcessingCoordinator(IInterfaceProfileManualProcessor manualProcessor)
    {
        _manualProcessor = manualProcessor ?? throw new ArgumentNullException(nameof(manualProcessor));
    }

    public AutoImportPairProcessingBatchResult ProcessReadyPairs(
        InterfaceProfileDefinition interfaceProfile,
        ExportProfileDefinition exportProfile,
        IEnumerable<PendingImportPair> readyPairs,
        bool automaticProcessingEnabled,
        DateTime timestamp)
    {
        ArgumentNullException.ThrowIfNull(interfaceProfile);
        ArgumentNullException.ThrowIfNull(exportProfile);
        ArgumentNullException.ThrowIfNull(readyPairs);

        if (!automaticProcessingEnabled)
        {
            return new AutoImportPairProcessingBatchResult(
                ProcessedCount: 0,
                SkippedAlreadyProcessedCount: 0,
                ErrorCount: 0,
                Results: Array.Empty<AutoImportPairProcessingResult>());
        }

        var results = new List<AutoImportPairProcessingResult>();
        foreach (var pair in readyPairs.Where(pair => pair.IsReady))
        {
            var pairKey = CreatePairKey(interfaceProfile.Metadata.Id, pair.AisFile.FilePath, pair.DeviceFile.FilePath);
            if (_processedPairKeys.Contains(pairKey) || _processingPairKeys.Contains(pairKey))
            {
                results.Add(new AutoImportPairProcessingResult(
                    PairKey: pairKey,
                    AisFilePath: pair.AisFile.FilePath,
                    DeviceFilePath: pair.DeviceFile.FilePath,
                    WasProcessed: false,
                    WasSkipped: true,
                    Success: false,
                    Status: "Bereits verarbeitet",
                    ExportFilePath: null,
                    ManualProcessingResult: null,
                    Messages: new[] { "Paar bereits verarbeitet." }));
                continue;
            }

            _processingPairKeys.Add(pairKey);
            try
            {
                var processingResult = _manualProcessor.Process(
                    interfaceProfile,
                    exportProfile,
                    pair.AisFile.FilePath,
                    pair.DeviceFile.FilePath,
                    timestamp);
                _processedPairKeys.Add(pairKey);
                results.Add(CreateProcessedResult(interfaceProfile, pairKey, pair, processingResult));
            }
            catch (Exception ex)
            {
                _processedPairKeys.Add(pairKey);
                results.Add(new AutoImportPairProcessingResult(
                    PairKey: pairKey,
                    AisFilePath: pair.AisFile.FilePath,
                    DeviceFilePath: pair.DeviceFile.FilePath,
                    WasProcessed: true,
                    WasSkipped: false,
                    Success: false,
                    Status: "Automatischer Fehler",
                    ExportFilePath: null,
                    ManualProcessingResult: null,
                    Messages: new[] { ex.Message }));
            }
            finally
            {
                _processingPairKeys.Remove(pairKey);
            }
        }

        return new AutoImportPairProcessingBatchResult(
            ProcessedCount: results.Count(result => result.WasProcessed && result.Success),
            SkippedAlreadyProcessedCount: results.Count(result => result.WasSkipped),
            ErrorCount: results.Count(result => result.WasProcessed && !result.Success),
            Results: results);
    }

    private static AutoImportPairProcessingResult CreateProcessedResult(
        InterfaceProfileDefinition interfaceProfile,
        string pairKey,
        PendingImportPair pair,
        InterfaceProfileManualProcessingResult processingResult)
    {
        return new AutoImportPairProcessingResult(
            PairKey: pairKey,
            AisFilePath: pair.AisFile.FilePath,
            DeviceFilePath: pair.DeviceFile.FilePath,
            WasProcessed: true,
            WasSkipped: false,
            Success: processingResult.Success,
            Status: CreateStatus(interfaceProfile, processingResult),
            ExportFilePath: processingResult.ExportFilePath,
            ManualProcessingResult: processingResult,
            Messages: processingResult.Messages);
    }

    private static string CreateStatus(
        InterfaceProfileDefinition interfaceProfile,
        InterfaceProfileManualProcessingResult processingResult)
    {
        if (processingResult.Success)
        {
            if (processingResult.ArchiveResult is null)
            {
                return "Automatisch verarbeitet";
            }

            if (processingResult.ArchiveResult.HasErrors)
            {
                return "Automatisch verarbeitet, Archivierung mit Fehlern";
            }

            return interfaceProfile.FolderOptions.ArchiveProcessedFileMode == ArchiveProcessedFileMode.Move
                ? "Automatisch verarbeitet und ins Archiv verschoben"
                : "Automatisch verarbeitet und archiviert";
        }

        if (processingResult.FailedFileCopyResult is null)
        {
            return "Automatischer Fehler";
        }

        return processingResult.FailedFileCopyResult.HasErrors
            ? "Automatischer Fehler, Fehlerablage fehlgeschlagen"
            : "Automatischer Fehler, Dateien kopiert";
    }

    public static string CreatePairKey(string interfaceProfileId, string aisFilePath, string deviceFilePath)
    {
        return $"{interfaceProfileId}|{aisFilePath}|{deviceFilePath}";
    }
}
