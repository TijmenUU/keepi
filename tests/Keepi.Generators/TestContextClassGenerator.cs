using System.Collections.Generic;
using System.Collections.Immutable;
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
            // Only works on Windows
            // if (!Debugger.IsAttached)
            // {
            //     Debugger.Launch();
            // }
            // Spin so that Linux users can manually attach
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
                foreach (var argument in attribute.NamedArguments)
                {
                    if (argument.Key == nameof(GenerateTestContextAttribute.TargetType))
                    {
                        targetType = argument.Value.Value as INamedTypeSymbol;
                    }
                }
                if (targetType == null)
                {
                    return null;
                }

                return new GenerateTestContextAttributeData(targetType: targetType);
            }

            return null;
        }

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

            var mocks = GetTestContextTargetDependencies(targetType: attributeData.TargetType);

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
            INamedTypeSymbol targetType
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

                results.Add(new(fullName: fullName, shortName: shortName));
            }

            return results.ToArray();
        }
    }
}
