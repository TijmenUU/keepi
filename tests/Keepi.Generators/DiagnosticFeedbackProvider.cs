using Microsoft.CodeAnalysis;

namespace Keepi.Generators;

internal interface IDiagnosticFeedbackProvider
{
    void ReportMultipleConstructorsNotSupportedForTargetDiagnostic();
}

internal class DiagnosticFeedbackProvider : IDiagnosticFeedbackProvider
{
    private const string DiagnosticCategory = "KeepiGenerator";
    private const string MultipleConstructorsNotSupportedForTargetCode = "KG0001";

    private SourceProductionContext sourceProductionContext;
    private readonly string targetTypeName;
    private readonly Location testContextClassLocation;

    public DiagnosticFeedbackProvider(
        SourceProductionContext sourceProductionContext,
        string targetTypeName,
        Location testContextClassLocation
    )
    {
        this.sourceProductionContext = sourceProductionContext;
        this.targetTypeName = targetTypeName;
        this.testContextClassLocation = testContextClassLocation;
    }

    public void ReportMultipleConstructorsNotSupportedForTargetDiagnostic()
    {
        sourceProductionContext.ReportDiagnostic(
            Diagnostic.Create(
                descriptor: new DiagnosticDescriptor(
                    id: MultipleConstructorsNotSupportedForTargetCode,
                    title: "The generator does not (yet) support multiple constructors for a target",
                    messageFormat: "The generator does not (yet) support the multiple constructors defined by the target {0}",
                    category: DiagnosticCategory,
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true
                ),
                location: testContextClassLocation,
                messageArgs: [targetTypeName]
            )
        );
    }
}
