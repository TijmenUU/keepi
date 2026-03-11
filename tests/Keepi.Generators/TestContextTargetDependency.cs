using System;

namespace Keepi.Generators;

internal sealed class TestContextTargetDependency
{
    public TestContextTargetDependency(string fullName, string shortName)
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

        var isLoose = fullName.StartsWith("Microsoft.Extensions.Logging.ILogger<");

        FullName = fullName;
        MockName = $"{shortName}Mock";
        IsLooseMock = isLoose;
        IsVerified = !isLoose;
    }

    public string FullName { get; }
    public string MockName { get; }
    public bool IsLooseMock { get; }
    public bool IsVerified { get; }
}
