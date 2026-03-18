using Vogen;

namespace Keepi.Core.Users;

[ValueObject<string>()]
public readonly partial struct UserName
{
    public const int MaxLength = 128;

    public static Validation Validate(string? value)
    {
        if (value == null)
        {
            return Validation.Invalid("Name cannot be null");
        }

        if (value.Length == 0)
        {
            return Validation.Invalid("Name cannot be empty");
        }

        if (value.Length > MaxLength)
        {
            return Validation.Invalid("Name length exceeds maximum");
        }

        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
    {
        return input.Trim();
    }
}
