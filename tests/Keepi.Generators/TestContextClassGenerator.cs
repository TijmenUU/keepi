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
                .SyntaxProvider.CreateSyntaxProvider(
                    predicate: IsApplicableNode,
                    transform: TransformToClassDeclaration
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

        private static bool IsNotNull(ClassDeclarationSyntax c) => c != null;

        private static ClassDeclarationSyntax TransformToClassDeclaration(
            GeneratorSyntaxContext context,
            CancellationToken cancellationToken
        )
        {
            if (context.Node is not ClassDeclarationSyntax classNode)
            {
                return null;
            }

            foreach (var list in classNode.AttributeLists)
            {
                foreach (var attribute in list.Attributes)
                {
                    if (
                        context.SemanticModel.GetSymbolInfo(attribute).Symbol
                        is not IMethodSymbol attributeSymbol
                    )
                    {
                        continue;
                    }

                    var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                    if (
                        attributeContainingTypeSymbol.ToDisplayString()
                        == GenerateTestContextAttribute.FullName
                    )
                    {
                        return classNode;
                    }
                }
            }

            return null;
        }

        private static void GeneratedTestContexts(
            SourceProductionContext context,
            (Compilation, ImmutableArray<ClassDeclarationSyntax>) values
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
                    testContextClass: @class,
                    compilation: values.Item1,
                    context: context
                );
            }
        }

        private static void GeneratedTestContext(
            ClassDeclarationSyntax testContextClass,
            Compilation compilation,
            SourceProductionContext context
        )
        {
            var testContextClassSemanticModel = compilation.GetSemanticModel(
                testContextClass.SyntaxTree
            );
            var testContextClassSymbol = testContextClassSemanticModel.GetDeclaredSymbol(
                declaration: testContextClass
            );
            var generateTestContextAttribute = GetGenerateTestContextAttributeOrNull(
                attributes: testContextClassSymbol.GetAttributes()
            );
            if (generateTestContextAttribute == null)
            {
                return;
            }

            AddGeneratedSourceFor(
                context: context,
                testContextClassSymbol: testContextClassSymbol,
                attributeData: generateTestContextAttribute
            );
        }

        private static GenerateTestContextAttributeData GetGenerateTestContextAttributeOrNull(
            ImmutableArray<AttributeData> attributes
        )
        {
            foreach (var attribute in attributes)
            {
                if (
                    attribute.AttributeClass.ToDisplayString()
                    != GenerateTestContextAttribute.FullName
                )
                {
                    continue;
                }

                INamedTypeSymbol targetType = null;
                bool generateWithMethods = false;
                bool verifyLogging = false;
                foreach (var argument in attribute.NamedArguments)
                {
                    switch (argument.Key)
                    {
                        case nameof(GenerateTestContextAttribute.TargetType):
                            targetType = argument.Value.Value as INamedTypeSymbol;
                            break;

                        case nameof(GenerateTestContextAttribute.GenerateWithMethods):
                            generateWithMethods = ParseBool(argument.Value.Value);
                            break;

                        case nameof(GenerateTestContextAttribute.VerifyLogging):
                            verifyLogging = ParseBool(argument.Value.Value);
                            break;
                    }
                }
                if (targetType == null)
                {
                    return null;
                }

                return new GenerateTestContextAttributeData(
                    targetType: targetType,
                    generateWithMethods: generateWithMethods,
                    verifyLogging: verifyLogging
                );
            }

            return null;
        }

        private static bool ParseBool(object typedConstant) =>
            typedConstant is bool booleanValue && booleanValue;

        private static void AddGeneratedSourceFor(
            SourceProductionContext context,
            ISymbol testContextClassSymbol,
            GenerateTestContextAttributeData attributeData
        )
        {
            var sourceFile = GenerateSourceFile(
                testContextClassSymbol: testContextClassSymbol,
                attributeData: attributeData
            );

            context.AddSource(hintName: sourceFile.Name, source: sourceFile.Content);
        }

        private static TestContextClassSource GenerateSourceFile(
            ISymbol testContextClassSymbol,
            GenerateTestContextAttributeData attributeData
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
            var targetClassFullName = attributeData.TargetType.ToDisplayString(
                new SymbolDisplayFormat(
                    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
                )
            );

            var mocks = GetTestContextTargetDependencies(
                targetType: attributeData.TargetType,
                gatherMethods: attributeData.GenerateWithMethods,
                verifyLogging: attributeData.VerifyLogging
            );

            return TestContextClassSource.Create(
                @namespace: testContextClassNamespace,
                className: testContextClassShortName,
                targetFullName: targetClassFullName,
                targetDependencies: mocks,
                targetIsFastEndpoint: IsFastEndpoint(targetType: attributeData.TargetType)
            );
        }

        private static bool IsFastEndpoint(INamedTypeSymbol targetType)
        {
            return targetType.AllInterfaces.Any(i =>
                i.ContainingNamespace.ToDisplayString() == "FastEndpoints" && i.Name == "IEndpoint"
            );
        }

        private static TestContextTargetDependency[] GetTestContextTargetDependencies(
            INamedTypeSymbol targetType,
            bool gatherMethods,
            bool verifyLogging
        )
        {
            if (targetType.Constructors.Length != 1)
            {
                // Unsupported
                return [];
            }

            var results = new List<TestContextTargetDependency>();
            foreach (var parameter in targetType.Constructors[0].Parameters)
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
