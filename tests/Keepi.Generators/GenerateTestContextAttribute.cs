using System;
using Microsoft.CodeAnalysis;

namespace Keepi.Generators;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class GenerateTestContextAttribute : Attribute
{
    public const string FullName = "Keepi.Generators.GenerateTestContextAttribute";

    public GenerateTestContextAttribute(Type targetType)
    {
        TargetType = targetType;
    }

    /// <summary>
    /// The type the test context is generated for.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Whether the WithX methods should be generated automatically. It will
    /// generated a simple moq setup for each public or internal method of the
    /// target type with return value if applicable. The arguments of the method
    /// are not verified in the generated Setup code, verification is expected
    /// to be done manually, with usually a VerifyNoOtherLogging call at the end
    /// to assert all calls were expected and set up.
    ///
    /// For the Microsoft ILogger interface methods are never generated.
    ///
    /// By default no WithX methods are generated.
    /// </summary>
    public bool GenerateWithMethods { get; set; }

    /// <summary>
    /// Indicate whether the generated VerifyNoOtherCalls method should verify
    /// the mocked logger. By default logger mocks are not verified.
    /// </summary>
    public bool VerifyLogging { get; set; }
}

// This class is defined here on purpose since it must be kept in sync with the attribute definition above.
internal sealed class GenerateTestContextAttributeData
{
    public GenerateTestContextAttributeData(
        INamedTypeSymbol targetType,
        bool generateWithMethods,
        bool verifyLogging
    )
    {
        TargetType = targetType ?? throw new ArgumentNullException(paramName: nameof(targetType));
        GenerateWithMethods = generateWithMethods;
        VerifyLogging = verifyLogging;
    }

    public INamedTypeSymbol TargetType { get; }
    public bool GenerateWithMethods { get; }
    public bool VerifyLogging { get; }
}
