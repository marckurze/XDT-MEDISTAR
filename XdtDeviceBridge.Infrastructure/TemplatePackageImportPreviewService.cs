namespace XdtDeviceBridge.Infrastructure;

public sealed class TemplatePackageImportPreviewService
{
    private readonly TemplatePackageImporter _importer;
    private readonly TemplatePackageImportValidator _validator;
    private readonly TemplatePackageImportConflictAnalyzer _analyzer;
    private readonly TemplatePackageImportPlanBuilder _planBuilder;
    private readonly TemplatePackageImportDryRunService _dryRunService;
    private readonly TemplatePackageImportPreviewDisplayService _displayService;

    public TemplatePackageImportPreviewService()
        : this(
            new TemplatePackageImporter(),
            new TemplatePackageImportValidator(),
            new TemplatePackageImportConflictAnalyzer(),
            new TemplatePackageImportPlanBuilder(),
            new TemplatePackageImportDryRunService(),
            new TemplatePackageImportPreviewDisplayService())
    {
    }

    public TemplatePackageImportPreviewService(
        TemplatePackageImporter importer,
        TemplatePackageImportValidator validator,
        TemplatePackageImportConflictAnalyzer analyzer,
        TemplatePackageImportPlanBuilder planBuilder,
        TemplatePackageImportDryRunService dryRunService,
        TemplatePackageImportPreviewDisplayService displayService)
    {
        _importer = importer ?? throw new ArgumentNullException(nameof(importer));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
        _planBuilder = planBuilder ?? throw new ArgumentNullException(nameof(planBuilder));
        _dryRunService = dryRunService ?? throw new ArgumentNullException(nameof(dryRunService));
        _displayService = displayService ?? throw new ArgumentNullException(nameof(displayService));
    }

    public TemplatePackageImportPreviewResult Create(
        string zipFilePath,
        ProfileCatalog existingCatalog)
    {
        if (string.IsNullOrWhiteSpace(zipFilePath))
        {
            throw new ArgumentException("Template package ZIP file path must not be empty.", nameof(zipFilePath));
        }

        ArgumentNullException.ThrowIfNull(existingCatalog);

        var importResult = _importer.Import(zipFilePath);
        var validationResult = _validator.Validate(importResult);
        var analysisResult = _analyzer.Analyze(importResult, existingCatalog);
        var importPlan = _planBuilder.Build(analysisResult);
        var dryRunResult = _dryRunService.Preview(importResult, importPlan, existingCatalog);
        var display = _displayService.Create(validationResult, analysisResult, importPlan, dryRunResult);

        return new TemplatePackageImportPreviewResult(
            ImportResult: importResult,
            ValidationResult: validationResult,
            AnalysisResult: analysisResult,
            BasePlan: importPlan,
            Plan: importPlan,
            DryRunResult: dryRunResult,
            Display: display);
    }
}

public sealed record TemplatePackageImportPreviewResult(
    TemplatePackageImportResult ImportResult,
    TemplatePackageImportValidationResult ValidationResult,
    TemplatePackageImportAnalysisResult AnalysisResult,
    TemplatePackageImportPlan BasePlan,
    TemplatePackageImportPlan Plan,
    TemplatePackageImportDryRunResult DryRunResult,
    TemplatePackageImportPreviewDisplay Display);
