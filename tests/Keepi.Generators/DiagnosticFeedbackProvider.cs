using Microsoft.CodeAnalysis;

namespace Keepi.Generators;

internal interface IDiagnosticFeedbackProvider
{
    void ReportMultipleConstructorsNotSupportedForTargetDiagnostic();
    void ReportUnexpectedTestContextClassNameDiagnostic();
    void ReportUnsupportedNonPartialClassDefinitionDiagnostic();
    void ReportUnsupportedNonInternalClassDefinitionDiagnostic();
    void ReportUnsupportedNonGlobalClassDefinitionDiagnostic();
    void ReportUnsupportedAbstractTargetTypeDiagnostic();
}

internal class DiagnosticFeedbackProvider : IDiagnosticFeedbackProvider
{
    private const string DiagnosticCategory = "KeepiGenerator";
    private const string MultipleConstructorsNotSupportedForTargetCode = "KG0001";
    private const string UnexpectedTestContextClassNameCode = "KG0002";
    private const string UnsupportedNonPartialClassDefinitionCode = "KG0003";
    private const string UnsupportedNonInternalClassDefinitionCode = "KG0004";
    private const string UnsupportedNonGlobalClassDefinitionCode = "KG0005";
    private const string UnsupportedAbstractTargetTypeCode = "KG0006";

    private readonly SourceProductionContext sourceProductionContext;
    private readonly string targetTypeName;
    private readonly string testContextClassName;
    private readonly Location testContextClassLocation;

    public DiagnosticFeedbackProvider(
        SourceProductionContext sourceProductionContext,
        string targetTypeName,
        string testContextClassName,
        Location testContextClassLocation
    )
    {
        this.sourceProductionContext = sourceProductionContext;
        this.testContextClassName = testContextClassName;
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

    public void ReportUnexpectedTestContextClassNameDiagnostic()
    {
        sourceProductionContext.ReportDiagnostic(
            Diagnostic.Create(
                descriptor: new DiagnosticDescriptor(
                    id: UnexpectedTestContextClassNameCode,
                    title: "The test context class name is unexpected",
                    messageFormat: "The test context class name {0} was expected to be {1}TestContext",
                    category: DiagnosticCategory,
                    defaultSeverity: DiagnosticSeverity.Warning,
                    isEnabledByDefault: true
                ),
                location: testContextClassLocation,
                messageArgs: [testContextClassName, targetTypeName]
            )
        );
    }

    public void ReportUnsupportedNonPartialClassDefinitionDiagnostic()
    {
        sourceProductionContext.ReportDiagnostic(
            Diagnostic.Create(
                descriptor: new DiagnosticDescriptor(
                    id: UnsupportedNonPartialClassDefinitionCode,
                    title: "Test context classes must be partial",
                    messageFormat: "Test context classes must be declared partial, {0} is not",
                    category: DiagnosticCategory,
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true
                ),
                location: testContextClassLocation,
                messageArgs: [testContextClassName]
            )
        );
    }

    public void ReportUnsupportedNonInternalClassDefinitionDiagnostic()
    {
        sourceProductionContext.ReportDiagnostic(
            Diagnostic.Create(
                descriptor: new DiagnosticDescriptor(
                    id: UnsupportedNonInternalClassDefinitionCode,
                    title: "Test context classes must be internal",
                    messageFormat: "Test context classes must be declared internal, {0} is not",
                    category: DiagnosticCategory,
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true
                ),
                location: testContextClassLocation,
                messageArgs: [testContextClassName]
            )
        );
    }

    public void ReportUnsupportedNonGlobalClassDefinitionDiagnostic()
    {
        sourceProductionContext.ReportDiagnostic(
            Diagnostic.Create(
                descriptor: new DiagnosticDescriptor(
                    id: UnsupportedNonGlobalClassDefinitionCode,
                    title: "Test context classes must be declared outside of other classes",
                    messageFormat: "Test context classes must be declared in a namespace, {0} is declared within a class",
                    category: DiagnosticCategory,
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true
                ),
                location: testContextClassLocation,
                messageArgs: [testContextClassName]
            )
        );
    }

    public void ReportUnsupportedAbstractTargetTypeDiagnostic()
    {
        sourceProductionContext.ReportDiagnostic(
            Diagnostic.Create(
                descriptor: new DiagnosticDescriptor(
                    id: UnsupportedAbstractTargetTypeCode,
                    title: "The generator does not (yet) support abstract types as a target",
                    messageFormat: "The generator does not (yet) support the abstract type {0} as a target",
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
