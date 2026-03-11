namespace Keepi.Generators;

internal sealed class TestContextTargetDependency
{
    public TestContextTargetDependency(string fullName, string shortName)
    {
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
