using System;

namespace Keepi.Generators;

internal sealed class TestContextTargetDependency
{
    public TestContextTargetDependency(
        string fullName,
        string shortName,
        TestContextTargetDependencyMethod[] methods,
        bool verifyLogging
    )
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException(
                message: "Fullname cannot be empty",
                paramName: nameof(fullName)
            );
        }

        if (string.IsNullOrWhiteSpace(shortName))
        {
            throw new ArgumentException(
                message: "Short name cannot be empty",
                paramName: nameof(shortName)
            );
        }

        var isLogger = fullName.StartsWith("Microsoft.Extensions.Logging.ILogger<");

        FullName = fullName;
        ShortName = shortName;
        MockName = $"{shortName}Mock";
        IsLooseMock = isLogger;
        IsVerified = !isLogger || verifyLogging;
        GenerateWithCallMethods = methods.Length > 0 && !isLogger;
        Methods = methods ?? throw new ArgumentNullException(paramName: nameof(methods));
    }

    public string FullName { get; }
    public string ShortName { get; }
    public string MockName { get; }
    public bool IsLooseMock { get; }
    public bool IsVerified { get; }
    public bool GenerateWithCallMethods { get; }
    public TestContextTargetDependencyMethod[] Methods { get; }
}

internal sealed class TestContextTargetDependencyMethod
{
    public TestContextTargetDependencyMethod(
        string name,
        string[] parameterTypeFullNames,
        string returnTypeFullName,
        bool useAsyncReturn
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(message: "Name cannot be empty", paramName: nameof(name));
        }

        if (parameterTypeFullNames == null)
        {
            throw new ArgumentNullException(paramName: nameof(parameterTypeFullNames));
        }
        foreach (var parameter in parameterTypeFullNames)
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                throw new ArgumentException(
                    message: "Parameter type full name cannot be empty",
                    paramName: nameof(parameterTypeFullNames)
                );
            }
        }

        if (string.IsNullOrWhiteSpace(returnTypeFullName))
        {
            throw new ArgumentException(
                message: "Return type full name cannot be empty",
                paramName: nameof(returnTypeFullName)
            );
        }

        Name = name;
        ParameterTypeFullNames = parameterTypeFullNames;
        ReturnTypeFullName = returnTypeFullName;
        UseAsyncReturn = useAsyncReturn;
    }

    public string Name { get; }
    public string[] ParameterTypeFullNames { get; }
    public string ReturnTypeFullName { get; }
    public bool UseAsyncReturn { get; }
}
