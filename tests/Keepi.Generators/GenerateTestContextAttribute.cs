using System;
using Microsoft.CodeAnalysis;

namespace Keepi.Generators;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class GenerateTestContextAttribute : Attribute
{
    public const string FullName = "Keepi.Generators.GenerateTestContextAttribute";

    public Type TargetType { get; set; }
}

// This class is defined here on purpose since it must be kept in sync with the attribute definition above.
internal sealed class GenerateTestContextAttributeData
{
    public GenerateTestContextAttributeData(INamedTypeSymbol targetType)
    {
        TargetType = targetType ?? throw new ArgumentNullException(paramName: nameof(targetType));
    }

    public INamedTypeSymbol TargetType { get; }
}
