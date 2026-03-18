using Vogen;

namespace Keepi.Core.Users;

[ValueObject<string>()]
public readonly partial struct UserExternalId
{
    public const int MaxLength = 64;

    public static Validation Validate(string? value)
    {
        if (value == null)
        {
            return Validation.Invalid("External ID cannot be null");
        }

        if (value.Length == 0)
        {
            return Validation.Invalid("External ID cannot be empty");
        }

        if (value.Length > MaxLength)
        {
            return Validation.Invalid("External ID length exceeds maximum");
        }

        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
    {
        return input.Trim();
    }
}
