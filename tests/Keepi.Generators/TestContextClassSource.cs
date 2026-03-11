using System;
using System.Text;

namespace Keepi.Generators;

internal sealed class TestContextClassSource
{
    public static TestContextClassSource Create(
        string @namespace,
        string className,
        string targetFullName,
        TestContextTargetDependency[] targetDependencies,
        bool targetIsFastEndpoint
    )
    {
        var sourceBuilder = new StringBuilder();
        sourceBuilder.AppendLine("using Moq;");
        if (!string.IsNullOrWhiteSpace(@namespace))
        {
            sourceBuilder.AppendLine($"namespace {@namespace};");
        }
        sourceBuilder.AppendLine($"internal partial class {className}");
        sourceBuilder.AppendLine("{");

        AddMockProperties(sourceBuilder: sourceBuilder, properties: targetDependencies);

        AddBuildTargetMethod(
            sourceBuilder: sourceBuilder,
            returnTypeFullName: targetFullName,
            returnTypeIsFastEndpoint: targetIsFastEndpoint,
            returnTypeConstructorArguments: targetDependencies
        );

        foreach (var dependency in targetDependencies)
        {
            if (dependency.IsVerified)
            {
                AddVerifyNoOtherCallsMethod(
                    sourceBuilder: sourceBuilder,
                    properties: targetDependencies
                );
                break;
            }
        }

        sourceBuilder.AppendLine("}");

        return new TestContextClassSource(
            name: $"{className}.g.cs",
            content: sourceBuilder.ToString()
        );
    }

    private static void AddMockProperties(
        StringBuilder sourceBuilder,
        TestContextTargetDependency[] properties
    )
    {
        foreach (var property in properties)
        {
            var behavior = property.IsLooseMock ? "Loose" : "Strict";
            sourceBuilder.AppendLine(
                $"    public Mock<{property.FullName}> {property.MockName} {{ get; }} = new(MockBehavior.{behavior});"
            );
        }
    }

    private static void AddBuildTargetMethod(
        StringBuilder sourceBuilder,
        string returnTypeFullName,
        bool returnTypeIsFastEndpoint,
        TestContextTargetDependency[] returnTypeConstructorArguments
    )
    {
        sourceBuilder.AppendLine($"    public {returnTypeFullName} BuildTarget()");
        sourceBuilder.AppendLine("    {");
        if (returnTypeIsFastEndpoint)
        {
            sourceBuilder.Append(
                $"        return FastEndpoints.Factory.Create<{returnTypeFullName}>("
            );
            for (int i = 0; i < returnTypeConstructorArguments.Length; ++i)
            {
                if (i > 0)
                {
                    sourceBuilder.Append(", ");
                }
                sourceBuilder.Append($"{returnTypeConstructorArguments[i].MockName}.Object");
            }
            sourceBuilder.AppendLine(");");
        }
        else
        {
            sourceBuilder.Append($"        return new {returnTypeFullName}(");
            for (int i = 0; i < returnTypeConstructorArguments.Length; ++i)
            {
                if (i > 0)
                {
                    sourceBuilder.Append(", ");
                }
                sourceBuilder.Append($"{returnTypeConstructorArguments[i].MockName}.Object");
            }
            sourceBuilder.AppendLine(");");
        }
        sourceBuilder.AppendLine("    }");
    }

    private static void AddVerifyNoOtherCallsMethod(
        StringBuilder sourceBuilder,
        TestContextTargetDependency[] properties
    )
    {
        sourceBuilder.AppendLine("    public void VerifyNoOtherCalls()");
        sourceBuilder.AppendLine("    {");
        foreach (var property in properties)
        {
            if (!property.IsVerified)
            {
                continue;
            }

            sourceBuilder.AppendLine($"        {property.MockName}.VerifyNoOtherCalls();");
        }
        sourceBuilder.AppendLine("    }");
    }

    private TestContextClassSource(string name, string content)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                message: "Source file name cannot be empty",
                paramName: nameof(name)
            );
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                message: "Source file cannot be empty",
                paramName: nameof(content)
            );
        }

        Name = name;
        Content = content;
    }

    public string Name { get; }
    public string Content { get; }
}
