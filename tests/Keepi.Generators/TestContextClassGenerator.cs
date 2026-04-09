using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Keepi.Generators
{
    [Generator(LanguageNames.CSharp)]
    internal sealed class TestContextClassGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
#if DEBUG
            // // Only works on Windows
            // if (!Debugger.IsAttached)
            // {
            //     Debugger.Launch();
            // }
            // // Spin so that Linux users can manually attach
            // while (!Debugger.IsAttached)
            // {
            //     Thread.Sleep(500);
            // }
#endif

            var classDeclarations = context
                .SyntaxProvider.ForAttributeWithMetadataName(
                    fullyQualifiedMetadataName: GenerateTestContextAttribute.FullName,
                    predicate: IsApplicableNode,
                    transform: TransformToTestContextClassDeclaration
                )
                .Where(IsNotNull);

            var compilationAndClasses = context.CompilationProvider.Combine(
                classDeclarations.Collect()
            );
            context.RegisterSourceOutput(compilationAndClasses, GeneratedTestContexts);
        }

        private static bool IsApplicableNode(
            SyntaxNode node,
            CancellationToken cancellationToken
        ) => node is ClassDeclarationSyntax c && c.AttributeLists.Count > 0;

        private static bool IsNotNull(TestContextClassDeclaration c) => c != null;

        private static TestContextClassDeclaration TransformToTestContextClassDeclaration(
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken
        )
        {
            if (context.TargetNode is not ClassDeclarationSyntax classNode)
            {
                return null;
            }

            return new TestContextClassDeclaration(
                classDeclarationSyntax: classNode,
                attributeData: context.Attributes.First(a =>
                    a.AttributeClass.Name == nameof(GenerateTestContextAttribute)
                )
            );
        }

        private sealed class TestContextClassDeclaration
        {
            public TestContextClassDeclaration(
                ClassDeclarationSyntax classDeclarationSyntax,
                AttributeData attributeData
            )
            {
                Debug.Assert(
                    attributeData.AttributeClass.Name
                        == nameof(Generators.GenerateTestContextAttribute)
                );

                ClassDeclarationSyntax = classDeclarationSyntax;
                GenerateTestContextAttribute = attributeData;
            }

            public ClassDeclarationSyntax ClassDeclarationSyntax { get; }
            public AttributeData GenerateTestContextAttribute { get; }
        }

        private static void GeneratedTestContexts(
            SourceProductionContext context,
            (Compilation, ImmutableArray<TestContextClassDeclaration>) values
        )
        {
            var classes = values.Item2;
            if (classes.IsDefaultOrEmpty)
            {
                return;
            }

            foreach (var @class in classes.Distinct())
            {
                GeneratedTestContext(
                    testContextClassDeclaration: @class,
                    compilation: values.Item1,
                    context: context
                );
            }
        }

        private static void GeneratedTestContext(
            TestContextClassDeclaration testContextClassDeclaration,
            Compilation compilation,
            SourceProductionContext context
        )
        {
            var testContextClassSemanticModel = compilation.GetSemanticModel(
                testContextClassDeclaration.ClassDeclarationSyntax.SyntaxTree
            );
            var testContextClassSymbol = testContextClassSemanticModel.GetDeclaredSymbol(
                declaration: testContextClassDeclaration.ClassDeclarationSyntax
            );
            var generateTestContextAttribute = GetGenerateTestContextAttributeOrNull(
                attribute: testContextClassDeclaration.GenerateTestContextAttribute
            );
            if (generateTestContextAttribute == null)
            {
                return;
            }

            AddGeneratedSourceFor(
                context: context,
                testContextClassDeclaration: testContextClassDeclaration.ClassDeclarationSyntax,
                testContextClassSymbol: testContextClassSymbol,
                attributeData: generateTestContextAttribute
            );
        }

        private static GenerateTestContextAttributeData GetGenerateTestContextAttributeOrNull(
            AttributeData attribute
        )
        {
            if (attribute.ConstructorArguments.Length != 1)
            {
                // Malformed constructor, should be reported by the C# compiler
                return null;
            }

            INamedTypeSymbol target = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;
            if (target == null)
            {
                // Malformed constructor argument, should be reported by the C# compiler
                return null;
            }

            bool generateWithMethods = false;
            bool verifyLogging = false;
            foreach (var argument in attribute.NamedArguments)
            {
                switch (argument.Key)
                {
                    case nameof(GenerateTestContextAttribute.GenerateWithMethods):
                        generateWithMethods = ParseBool(argument.Value.Value);
                        break;

                    case nameof(GenerateTestContextAttribute.VerifyLogging):
                        verifyLogging = ParseBool(argument.Value.Value);
                        break;
                }
            }

            return new GenerateTestContextAttributeData(
                target: target,
                generateWithMethods: generateWithMethods,
                verifyLogging: verifyLogging
            );
        }

        private static bool ParseBool(object typedConstant) =>
            typedConstant is bool booleanValue && booleanValue;

        private static void AddGeneratedSourceFor(
            SourceProductionContext context,
            ClassDeclarationSyntax testContextClassDeclaration,
            ISymbol testContextClassSymbol,
            GenerateTestContextAttributeData attributeData
        )
        {
            var diagnosticFeedbackProvider = new DiagnosticFeedbackProvider(
                sourceProductionContext: context,
                targetTypeName: attributeData.Target.ToDisplayString(
                    new SymbolDisplayFormat(
                        genericsOptions: SymbolDisplayGenericsOptions.None,
                        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly
                    )
                ),
                testContextClassName: testContextClassSymbol.ToDisplayString(
                    new SymbolDisplayFormat(
                        genericsOptions: SymbolDisplayGenericsOptions.None,
                        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly
                    )
                ),
                testContextClassLocation: testContextClassSymbol.Locations.First()
            );

            if (
                !ValidateClassDeclarationRequirements(
                    testContextClassDeclaration: testContextClassDeclaration,
                    testContextClassSymbol: testContextClassSymbol,
                    attributeData: attributeData,
                    diagnosticFeedbackProvider: diagnosticFeedbackProvider
                )
            )
            {
                return;
            }

            var sourceFile = GenerateSourceFile(
                testContextClassSymbol: testContextClassSymbol,
                attributeData: attributeData,
                diagnosticFeedbackProvider: diagnosticFeedbackProvider
            );

            context.AddSource(hintName: sourceFile.Name, source: sourceFile.Content);
        }

        private static bool ValidateClassDeclarationRequirements(
            ClassDeclarationSyntax testContextClassDeclaration,
            ISymbol testContextClassSymbol,
            GenerateTestContextAttributeData attributeData,
            DiagnosticFeedbackProvider diagnosticFeedbackProvider
        )
        {
            #region Warnings
            var expectedTestContextClassName = $"{attributeData.Target.Name}TestContext";
            if (testContextClassSymbol.Name != expectedTestContextClassName)
            {
                diagnosticFeedbackProvider.ReportUnexpectedTestContextClassNameDiagnostic();
            }
            #endregion

            bool isValid = true;
            #region Errors
            if (
                !testContextClassDeclaration.Modifiers.Any(m =>
                    m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)
                )
            )
            {
                diagnosticFeedbackProvider.ReportUnsupportedNonPartialClassDefinitionDiagnostic();
                isValid = false;
            }

            if (testContextClassSymbol.DeclaredAccessibility != Accessibility.Internal)
            {
                diagnosticFeedbackProvider.ReportUnsupportedNonInternalClassDefinitionDiagnostic();
                isValid = false;
            }

            if (
                testContextClassDeclaration.Parent?.IsKind(
                    Microsoft.CodeAnalysis.CSharp.SyntaxKind.ClassDeclaration
                ) == true
            )
            {
                diagnosticFeedbackProvider.ReportUnsupportedNonGlobalClassDefinitionDiagnostic();
                isValid = false;
            }

            if (attributeData.Target.IsAbstract)
            {
                diagnosticFeedbackProvider.ReportUnsupportedAbstractTargetTypeDiagnostic();
                isValid = false;
            }
            #endregion

            return isValid;
        }

        private static TestContextClassSource GenerateSourceFile(
            ISymbol testContextClassSymbol,
            GenerateTestContextAttributeData attributeData,
            IDiagnosticFeedbackProvider diagnosticFeedbackProvider
        )
        {
            var testContextClassNamespace =
                testContextClassSymbol.ContainingNamespace.ToDisplayString();
            var testContextClassShortName = testContextClassSymbol.ToDisplayString(
                new SymbolDisplayFormat(
                    genericsOptions: SymbolDisplayGenericsOptions.None,
                    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly
                )
            );
            var targetClassFullName = attributeData.Target.ToDisplayString(
                new SymbolDisplayFormat(
                    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
                )
            );

            var mocks = GetTestContextTargetDependencies(
                target: attributeData.Target,
                gatherMethods: attributeData.GenerateWithMethods,
                verifyLogging: attributeData.VerifyLogging,
                diagnosticFeedbackProvider: diagnosticFeedbackProvider
            );

            return TestContextClassSource.Create(
                @namespace: testContextClassNamespace,
                className: testContextClassShortName,
                targetFullName: targetClassFullName,
                targetDependencies: mocks,
                targetIsFastEndpoint: IsFastEndpoint(target: attributeData.Target)
            );
        }

        private static bool IsFastEndpoint(INamedTypeSymbol target)
        {
            return target.AllInterfaces.Any(i =>
                i.ContainingNamespace.ToDisplayString() == "FastEndpoints" && i.Name == "IEndpoint"
            );
        }

        private static TestContextTargetDependency[] GetTestContextTargetDependencies(
            INamedTypeSymbol target,
            bool gatherMethods,
            bool verifyLogging,
            IDiagnosticFeedbackProvider diagnosticFeedbackProvider
        )
        {
            if (target.Constructors.Length > 1)
            {
                diagnosticFeedbackProvider.ReportMultipleConstructorsNotSupportedForTargetDiagnostic();
                return [];
            }

            if (target.Constructors.Length == 0)
            {
                return [];
            }

            var results = new List<TestContextTargetDependency>();
            foreach (var parameter in target.Constructors[0].Parameters)
            {
                var shortName = parameter.Type.ToDisplayString(
                    new SymbolDisplayFormat(
                        genericsOptions: SymbolDisplayGenericsOptions.None,
                        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly
                    )
                );
                if (parameter.Type.TypeKind == TypeKind.Interface && shortName.StartsWith("I"))
                {
                    shortName = shortName.Substring(startIndex: 1);
                }

                var fullName = parameter.Type.ToDisplayString(
                    new SymbolDisplayFormat(
                        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
                    )
                );

                var methods = Array.Empty<ITestContextTargetDependencyMethod>();
                if (gatherMethods)
                {
                    methods = GetTestContextTargetDependencyMethods(type: parameter.Type);
                }

                results.Add(
                    new(
                        fullName: fullName,
                        shortName: shortName,
                        methods: methods,
                        verifyLogging: verifyLogging
                    )
                );
            }

            return results.ToArray();
        }

        private static ITestContextTargetDependencyMethod[] GetTestContextTargetDependencyMethods(
            ITypeSymbol type
        )
        {
            var hasMethods = type.TypeKind == TypeKind.Interface || type.TypeKind == TypeKind.Class;
            if (!hasMethods)
            {
                return Array.Empty<TestContextTargetDependencyMethod>();
            }

            Debug.Assert(!type.IsStatic);

            var methods = new List<ITestContextTargetDependencyMethod>();
            foreach (var member in type.GetMembers())
            {
                if (
                    member.Kind != SymbolKind.Method
                    || !IsPublicOrInternal(accessibility: member.DeclaredAccessibility)
                    || member is not IMethodSymbol memberSymbol
                    || !MethodIsSupportedToBeMocked(memberSymbol.MethodKind)
                )
                {
                    continue;
                }

                methods.Add(item: GetTestContextTargetDependencyMethod(member: memberSymbol));
            }

            return methods.ToArray();
        }

        private static bool IsPublicOrInternal(Accessibility accessibility) =>
            accessibility == Accessibility.Public || accessibility == Accessibility.Internal;

        private static bool MethodIsSupportedToBeMocked(MethodKind kind) =>
            kind switch
            {
                MethodKind.Ordinary or MethodKind.PropertyGet or MethodKind.PropertySet => true,
                _ => false,
            };

        private static ITestContextTargetDependencyMethod GetTestContextTargetDependencyMethod(
            IMethodSymbol member
        )
        {
            Debug.Assert(member != null);

            var returnTypeFullName = member.ReturnsVoid
                ? "void"
                : member.ReturnType.ToDisplayString(
                    new SymbolDisplayFormat(
                        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
                    )
                );

            if (member.MethodKind == MethodKind.PropertyGet)
            {
                return new TestContextTargetDependencyGetter(
                    name: member.AssociatedSymbol.Name,
                    returnTypeFullName: returnTypeFullName
                );
            }
            else if (member.MethodKind == MethodKind.PropertySet)
            {
                return new TestContextTargetDependencySetter(
                    name: member.AssociatedSymbol.Name,
                    returnTypeFullName: returnTypeFullName
                );
            }

            if (member.MethodKind != MethodKind.Ordinary)
            {
                throw new NotSupportedException(
                    $"Method of kind {member.MethodKind} are not supported"
                );
            }

            var parameterTypes = new List<string>();
            foreach (var parameter in member.Parameters)
            {
                parameterTypes.Add(
                    parameter.Type.ToDisplayString(
                        new SymbolDisplayFormat(
                            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
                        )
                    )
                );
            }

            var useAsyncReturn = false;
            ITypeSymbol unwrappedTaskType = member.ReturnType;
            if (
                member.ReturnType is INamedTypeSymbol returnNamedTypeSymbol
                && returnNamedTypeSymbol.IsGenericType
                && returnTypeFullName.StartsWith("System.Threading.Tasks.Task<")
            )
            {
                Debug.Assert(returnNamedTypeSymbol.TypeArguments.Length == 1);

                useAsyncReturn = true;
                unwrappedTaskType = returnNamedTypeSymbol.TypeArguments[0];

                returnTypeFullName = unwrappedTaskType.ToDisplayString(
                    new SymbolDisplayFormat(
                        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
                    )
                );
            }

            if (
                unwrappedTaskType is INamedTypeSymbol unwrappedTaskSymbol
                && unwrappedTaskSymbol.IsGenericType
                && unwrappedTaskSymbol.ContainingNamespace.ToDisplayString(
                    new SymbolDisplayFormat(
                        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
                    )
                ) == "Keepi.Core"
                && unwrappedTaskSymbol.Name.EndsWith("Result")
            )
            {
                if (unwrappedTaskSymbol.Name == "IMaybeErrorResult")
                {
                    Debug.Assert(unwrappedTaskSymbol.TypeArguments.Length == 1);

                    return new TestContextTargetDependencyResultMethod(
                        name: member.Name,
                        parameterTypeFullNames: parameterTypes.ToArray(),
                        returnTypeFullName: returnTypeFullName,
                        resultErrorTypeFullName: unwrappedTaskSymbol
                            .TypeArguments[0]
                            .ToDisplayString(
                                new SymbolDisplayFormat(
                                    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                                    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
                                )
                            ),
                        resultSuccessTypeFullName: null,
                        useAsyncReturn: useAsyncReturn
                    );
                }
                else if (unwrappedTaskType.Name == "IValueOrErrorResult")
                {
                    Debug.Assert(unwrappedTaskSymbol.TypeArguments.Length == 2);

                    return new TestContextTargetDependencyResultMethod(
                        name: member.Name,
                        parameterTypeFullNames: parameterTypes.ToArray(),
                        returnTypeFullName: returnTypeFullName,
                        resultErrorTypeFullName: unwrappedTaskSymbol
                            .TypeArguments[1]
                            .ToDisplayString(
                                new SymbolDisplayFormat(
                                    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                                    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
                                )
                            ),
                        resultSuccessTypeFullName: unwrappedTaskSymbol
                            .TypeArguments[0]
                            .ToDisplayString(
                                new SymbolDisplayFormat(
                                    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                                    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
                                )
                            ),
                        useAsyncReturn: useAsyncReturn
                    );
                }
            }

            return new TestContextTargetDependencyMethod(
                name: member.Name,
                parameterTypeFullNames: parameterTypes.ToArray(),
                returnTypeFullName: returnTypeFullName,
                useAsyncReturn: useAsyncReturn
            );
        }
    }
}
