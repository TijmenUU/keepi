using Vogen;

namespace Keepi.Core;

[ValueObject<string>()]
public readonly partial struct EmailAddress
{
    public const int MaxLength = 128;

    public static Validation Validate(string? value)
    {
        if (value == null)
        {
            return Validation.Invalid("Email address cannot be null");
        }

        if (value.Length == 0)
        {
            return Validation.Invalid("Email address cannot be empty");
        }

        if (value.Length > MaxLength)
        {
            return Validation.Invalid("Email address length exceeds maximum");
        }

        var parts = value.Split(
            separator: '@',
            options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        if (parts.Length != 2)
        {
            return Validation.Invalid(
                "Email address should contain one part before the @ and one part after the @"
            );
        }

        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
    {
        return input.Trim();
    }
}
