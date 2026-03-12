using System;
using System.Diagnostics;
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
        sourceBuilder.AppendLine("using System.Threading.Tasks;");

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

        foreach (var dependency in targetDependencies)
        {
            GenerateWithMethods(
                sourceBuilder: sourceBuilder,
                testContextClassName: className,
                dependency: dependency
            );
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

    private static void GenerateWithMethods(
        StringBuilder sourceBuilder,
        string testContextClassName,
        TestContextTargetDependency dependency
    )
    {
        if (!dependency.GenerateWithMethods || dependency.Methods.Length < 1)
        {
            return;
        }

        foreach (var method in dependency.Methods)
        {
            GenerateWithMethodsMockSetup(
                method: method,
                dependency: dependency,
                testContextClassName: testContextClassName,
                sourceBuilder: sourceBuilder
            );
        }
    }

    private static void GenerateWithMethodsMockSetup(
        ITestContextTargetDependencyMethod method,
        TestContextTargetDependency dependency,
        string testContextClassName,
        StringBuilder sourceBuilder
    )
    {
        switch (method.Kind)
        {
            case TestContextTargetDependencyMethodKind.Getter:
                GenerateWithGetterMockSetup(
                    method: method as TestContextTargetDependencyGetter,
                    dependency: dependency,
                    testContextClassName: testContextClassName,
                    sourceBuilder: sourceBuilder
                );
                break;

            case TestContextTargetDependencyMethodKind.Setter:
                // TODO ? What can be generated for this?
                break;

            case TestContextTargetDependencyMethodKind.Method:
            case TestContextTargetDependencyMethodKind.AsyncMethod:
                GenerateWithMethodMockSetup(
                    method: method as TestContextTargetDependencyMethod,
                    dependency: dependency,
                    testContextClassName: testContextClassName,
                    sourceBuilder: sourceBuilder
                );
                break;

            case TestContextTargetDependencyMethodKind.ResultMethod:
            case TestContextTargetDependencyMethodKind.AsyncResultMethod:
                GenerateWithResultMethodMockSetup(
                    method: method as TestContextTargetDependencyResultMethod,
                    dependency: dependency,
                    testContextClassName: testContextClassName,
                    sourceBuilder: sourceBuilder
                );
                break;

            default:
                throw new NotImplementedException(
                    $"Missing implementation for method mock kind {method.Kind}"
                );
        }
    }

    private static void GenerateWithGetterMockSetup(
        TestContextTargetDependencyGetter method,
        TestContextTargetDependency dependency,
        string testContextClassName,
        StringBuilder sourceBuilder
    )
    {
        sourceBuilder.AppendLine(
            $"    public {testContextClassName} With{dependency.ShortName}{method.Name}Get({method.ReturnTypeFullName} result)"
        );

        sourceBuilder.AppendLine("    {");
        sourceBuilder.AppendLine(
            $"        {dependency.MockName}.Setup(x => x.{method.Name}).Returns(result);"
        );
        sourceBuilder.AppendLine("        return this;");
        sourceBuilder.AppendLine("    }");
    }

    private static void GenerateWithMethodMockSetup(
        TestContextTargetDependencyMethod method,
        TestContextTargetDependency dependency,
        string testContextClassName,
        StringBuilder sourceBuilder
    )
    {
        string withMethodName =
            dependency.Methods.Length == 1
                ? $"With{dependency.ShortName}Call"
                : $"With{dependency.ShortName}{method.Name}Call";

        bool returnsValue =
            method.ReturnTypeFullName != "void"
            && method.ReturnTypeFullName != "System.Threading.Tasks.Task";
        if (returnsValue)
        {
            sourceBuilder.AppendLine(
                $"    public {testContextClassName} {withMethodName}({method.ReturnTypeFullName} returnValue)"
            );
        }
        else
        {
            sourceBuilder.AppendLine($"    public {testContextClassName} {withMethodName}()");
        }

        sourceBuilder.AppendLine("    {");

        sourceBuilder.Append($"        {dependency.MockName}.Setup(x => x.{method.Name}(");
        for (var i = 0; i < method.ParameterTypeFullNames.Length; ++i)
        {
            if (i > 0)
            {
                sourceBuilder.Append(", ");
            }
            sourceBuilder.Append($"It.IsAny<{method.ParameterTypeFullNames[i]}>()");
        }
        sourceBuilder.Append("))");

        if (returnsValue)
        {
            if (method.Kind == TestContextTargetDependencyMethodKind.AsyncMethod)
            {
                sourceBuilder.Append(".ReturnsAsync(returnValue)");
            }
            else
            {
                sourceBuilder.Append(".Returns(returnValue)");
            }
        }
        else if (method.Kind == TestContextTargetDependencyMethodKind.AsyncMethod)
        {
            sourceBuilder.Append(".Returns(Task.CompletedTask)");
        }
        sourceBuilder.AppendLine(";");

        sourceBuilder.AppendLine("        return this;");
        sourceBuilder.AppendLine("    }");
    }

    private static void GenerateWithResultMethodMockSetup(
        TestContextTargetDependencyResultMethod method,
        TestContextTargetDependency dependency,
        string testContextClassName,
        StringBuilder sourceBuilder
    )
    {
        if (method.ResultSuccessTypeFullName == null)
        {
            GenerateWithResultMethodMockSetup(
                method: method,
                suffix: "Success",
                returnValueParameterName: null,
                returnValueTypeFullName: null,
                resultGenericDefinition: $"Keepi.Core.Result.Success<{method.ResultErrorTypeFullName}>",
                dependency: dependency,
                testContextClassName: testContextClassName,
                sourceBuilder: sourceBuilder
            );

            GenerateWithResultMethodMockSetup(
                method: method,
                suffix: "Error",
                returnValueParameterName: "error",
                returnValueTypeFullName: method.ResultErrorTypeFullName,
                resultGenericDefinition: $"Keepi.Core.Result.Failure<{method.ResultErrorTypeFullName}>",
                dependency: dependency,
                testContextClassName: testContextClassName,
                sourceBuilder: sourceBuilder
            );
        }
        else
        {
            GenerateWithResultMethodMockSetup(
                method: method,
                suffix: "Success",
                returnValueParameterName: "result",
                returnValueTypeFullName: method.ResultSuccessTypeFullName,
                resultGenericDefinition: $"Keepi.Core.Result.Success<{method.ResultSuccessTypeFullName},{method.ResultErrorTypeFullName}>",
                dependency: dependency,
                testContextClassName: testContextClassName,
                sourceBuilder: sourceBuilder
            );

            GenerateWithResultMethodMockSetup(
                method: method,
                suffix: "Error",
                returnValueParameterName: "error",
                returnValueTypeFullName: method.ResultErrorTypeFullName,
                resultGenericDefinition: $"Keepi.Core.Result.Failure<{method.ResultSuccessTypeFullName},{method.ResultErrorTypeFullName}>",
                dependency: dependency,
                testContextClassName: testContextClassName,
                sourceBuilder: sourceBuilder
            );
        }
    }

    private static void GenerateWithResultMethodMockSetup(
        TestContextTargetDependencyResultMethod method,
        string suffix,
        string returnValueParameterName,
        string returnValueTypeFullName,
        string resultGenericDefinition,
        TestContextTargetDependency dependency,
        string testContextClassName,
        StringBuilder sourceBuilder
    )
    {
        string withMethodName =
            dependency.Methods.Length == 1
                ? $"With{dependency.ShortName}{suffix}"
                : $"With{dependency.ShortName}{method.Name}{suffix}";

        Debug.Assert(returnValueTypeFullName != "void");
        Debug.Assert(returnValueTypeFullName != "System.Threading.Tasks.Task");

        sourceBuilder.AppendLine(
            $"    public {testContextClassName} {withMethodName}({returnValueTypeFullName} {returnValueParameterName})"
        );
        sourceBuilder.AppendLine("    {");

        sourceBuilder.Append($"        {dependency.MockName}.Setup(x => x.{method.Name}(");
        for (var i = 0; i < method.ParameterTypeFullNames.Length; ++i)
        {
            if (i > 0)
            {
                sourceBuilder.Append(", ");
            }
            sourceBuilder.Append($"It.IsAny<{method.ParameterTypeFullNames[i]}>()");
        }
        sourceBuilder.Append("))");

        if (method.Kind == TestContextTargetDependencyMethodKind.AsyncResultMethod)
        {
            sourceBuilder.AppendLine(
                $".ReturnsAsync({resultGenericDefinition}({returnValueParameterName}));"
            );
        }
        else
        {
            sourceBuilder.AppendLine(
                $".Returns({resultGenericDefinition}({returnValueParameterName}));"
            );
        }

        sourceBuilder.AppendLine("        return this;");
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
