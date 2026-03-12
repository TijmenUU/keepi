using System;

namespace Keepi.Generators;

internal sealed class TestContextTargetDependency
{
    public TestContextTargetDependency(
        string fullName,
        string shortName,
        ITestContextTargetDependencyMethod[] methods,
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
    public ITestContextTargetDependencyMethod[] Methods { get; }
}

internal interface ITestContextTargetDependencyMethod
{
    public string Name { get; }
    public TestContextTargetDependencyMethodKind Kind { get; }
}

internal sealed class TestContextTargetDependencyMethod : ITestContextTargetDependencyMethod
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
        Kind = useAsyncReturn
            ? TestContextTargetDependencyMethodKind.AsyncMethod
            : TestContextTargetDependencyMethodKind.Method;
    }

    public string Name { get; }
    public string[] ParameterTypeFullNames { get; }
    public string ReturnTypeFullName { get; }
    public TestContextTargetDependencyMethodKind Kind { get; }
}

internal sealed class TestContextTargetDependencyGetter : ITestContextTargetDependencyMethod
{
    public TestContextTargetDependencyGetter(string name, string returnTypeFullName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(message: "Name cannot be empty", paramName: nameof(name));
        }

        if (string.IsNullOrWhiteSpace(returnTypeFullName))
        {
            throw new ArgumentException(
                message: "Return type full name cannot be empty",
                paramName: nameof(returnTypeFullName)
            );
        }

        Name = name;
        ReturnTypeFullName = returnTypeFullName;
        Kind = TestContextTargetDependencyMethodKind.Getter;
    }

    public string Name { get; }
    public string ReturnTypeFullName { get; }
    public TestContextTargetDependencyMethodKind Kind { get; }
}

internal sealed class TestContextTargetDependencySetter : ITestContextTargetDependencyMethod
{
    public TestContextTargetDependencySetter(string name, string returnTypeFullName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(message: "Name cannot be empty", paramName: nameof(name));
        }

        if (string.IsNullOrWhiteSpace(returnTypeFullName))
        {
            throw new ArgumentException(
                message: "Return type full name cannot be empty",
                paramName: nameof(returnTypeFullName)
            );
        }

        Name = name;
        ReturnTypeFullName = returnTypeFullName;
        Kind = TestContextTargetDependencyMethodKind.Setter;
    }

    public string Name { get; }
    public string ReturnTypeFullName { get; }
    public TestContextTargetDependencyMethodKind Kind { get; }
}

internal sealed class TestContextTargetDependencyResultMethod : ITestContextTargetDependencyMethod
{
    public TestContextTargetDependencyResultMethod(
        string name,
        string[] parameterTypeFullNames,
        string returnTypeFullName,
        string resultErrorTypeFullName,
        string resultSuccessTypeFullName,
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

        if (string.IsNullOrWhiteSpace(resultErrorTypeFullName))
        {
            throw new ArgumentException(
                message: "Error type full name cannot be empty",
                paramName: nameof(resultErrorTypeFullName)
            );
        }

        Name = name;
        ParameterTypeFullNames = parameterTypeFullNames;
        ReturnTypeFullName = returnTypeFullName;
        ResultErrorTypeFullName = resultErrorTypeFullName;
        ResultSuccessTypeFullName = resultSuccessTypeFullName;
        Kind = useAsyncReturn
            ? TestContextTargetDependencyMethodKind.AsyncResultMethod
            : TestContextTargetDependencyMethodKind.ResultMethod;
    }

    public string Name { get; }
    public string[] ParameterTypeFullNames { get; }
    public string ReturnTypeFullName { get; }
    public string ResultErrorTypeFullName { get; }

    /// <summary>
    /// The success result type. Null if there is no result (void).
    /// </summary>
    public string ResultSuccessTypeFullName { get; }
    public TestContextTargetDependencyMethodKind Kind { get; }
}

internal enum TestContextTargetDependencyMethodKind
{
    Getter,
    Setter,
    Method,
    AsyncMethod,
    ResultMethod,
    AsyncResultMethod,
}
