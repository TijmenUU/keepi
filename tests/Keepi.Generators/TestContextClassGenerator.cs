using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Keepi.Generators
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class GenerateTestContextAttribute : Attribute
    {
        public const string FullName = "Keepi.Generators.GenerateTestContextAttribute";

        public Type TargetType { get; set; }
    }

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
            context.RegisterSourceOutput(compilationAndClasses, GenerateTestContextFor);
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

        private static void GenerateTestContextFor(
            SourceProductionContext context,
            (Compilation, ImmutableArray<ClassDeclarationSyntax>) values
        )
        {
            var classes = values.Item2;
            if (classes.IsDefaultOrEmpty)
            {
                return;
            }

            foreach (var testContextClassToGenerate in classes.Distinct())
            {
                var testContextClassSemanticModel = values.Item1.GetSemanticModel(
                    testContextClassToGenerate.SyntaxTree
                );
                var testContextClassSymbol = testContextClassSemanticModel.GetDeclaredSymbol(
                    declaration: testContextClassToGenerate
                );
                var testContextClassAttributes = testContextClassSymbol.GetAttributes();
                foreach (var testContextClassAttribute in testContextClassAttributes)
                {
                    if (
                        testContextClassAttribute.AttributeClass.ToDisplayString()
                        != GenerateTestContextAttribute.FullName
                    )
                    {
                        continue;
                    }

                    INamedTypeSymbol targetType = null;
                    foreach (var argument in testContextClassAttribute.NamedArguments)
                    {
                        if (argument.Key == nameof(GenerateTestContextAttribute.TargetType))
                        {
                            targetType = argument.Value.Value as INamedTypeSymbol;
                        }
                    }
                    if (targetType == null)
                    {
                        break;
                    }

                    var testContextClassNamespace =
                        testContextClassSymbol.ContainingNamespace.ToDisplayString();
                    var testContextClassShortName = testContextClassSymbol.ToDisplayString(
                        new SymbolDisplayFormat(
                            genericsOptions: SymbolDisplayGenericsOptions.None,
                            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly
                        )
                    );
                    var targetClassFullName = targetType.ToDisplayString(
                        new SymbolDisplayFormat(
                            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
                        )
                    );

                    var toMock = GetMockedDependencies(targetType: targetType);
                    var sourceBuilder = new StringBuilder();
                    sourceBuilder.AppendLine("using Moq;");
                    if (!string.IsNullOrWhiteSpace(testContextClassNamespace))
                    {
                        sourceBuilder.AppendLine($"namespace {testContextClassNamespace};");
                    }
                    sourceBuilder.AppendLine($"internal partial class {testContextClassShortName}");
                    sourceBuilder.AppendLine("{");

                    foreach (var type in toMock)
                    {
                        var behavior = type.IsLooseMock ? "Loose" : "Strict";
                        sourceBuilder.AppendLine(
                            $"    public Mock<{type.FullName}> {type.MockName} {{ get; }} = new(MockBehavior.{behavior});"
                        );
                    }
                    sourceBuilder.AppendLine($"    public {targetClassFullName} BuildTarget()");
                    sourceBuilder.AppendLine("    {");
                    if (IsFastEndpoint(targetType: targetType))
                    {
                        sourceBuilder.Append(
                            $"        return FastEndpoints.Factory.Create<{targetClassFullName}>("
                        );
                        for (int i = 0; i < toMock.Length; ++i)
                        {
                            if (i > 0)
                            {
                                sourceBuilder.Append(", ");
                            }
                            sourceBuilder.Append($"{toMock[i].MockName}.Object");
                        }
                        sourceBuilder.AppendLine(");");
                    }
                    else
                    {
                        sourceBuilder.Append($"        return new {targetClassFullName}(");
                        for (int i = 0; i < toMock.Length; ++i)
                        {
                            if (i > 0)
                            {
                                sourceBuilder.Append(", ");
                            }
                            sourceBuilder.Append($"{toMock[i].MockName}.Object");
                        }
                        sourceBuilder.AppendLine(");");
                    }
                    sourceBuilder.AppendLine("    }");

                    sourceBuilder.AppendLine("    public void VerifyNoOtherCalls()");
                    sourceBuilder.AppendLine("    {");
                    foreach (var type in toMock)
                    {
                        if (!type.IsVerified)
                        {
                            continue;
                        }

                        sourceBuilder.AppendLine($"        {type.MockName}.VerifyNoOtherCalls();");
                    }
                    sourceBuilder.AppendLine("    }");

                    sourceBuilder.AppendLine("}");

                    context.AddSource(
                        hintName: $"{testContextClassShortName}.g.cs",
                        source: sourceBuilder.ToString()
                    );
                    break;
                }
            }
        }

        private static TypeToMock[] GetMockedDependencies(INamedTypeSymbol targetType)
        {
            if (targetType.Constructors.Length != 1)
            {
                // Unsupported
                return [];
            }

            var results = new List<TypeToMock>();
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

                var isLoose = fullName.StartsWith("Microsoft.Extensions.Logging.ILogger<");

                results.Add(
                    new(
                        fullName: fullName,
                        mockName: $"{shortName}Mock",
                        isLooseMock: isLoose,
                        isVerified: !isLoose
                    )
                );
            }

            return results.ToArray();
        }

        private class TypeToMock
        {
            public TypeToMock(string fullName, string mockName, bool isLooseMock, bool isVerified)
            {
                FullName = fullName;
                MockName = mockName;
                IsLooseMock = isLooseMock;
                IsVerified = isVerified;
            }

            public string FullName { get; }
            public string MockName { get; }
            public bool IsLooseMock { get; }
            public bool IsVerified { get; }
        }

        private static bool IsFastEndpoint(INamedTypeSymbol targetType)
        {
            return targetType.AllInterfaces.Any(i =>
                i.ContainingNamespace.ToDisplayString() == "FastEndpoints" && i.Name == "IEndpoint"
            );
        }
    }
}
