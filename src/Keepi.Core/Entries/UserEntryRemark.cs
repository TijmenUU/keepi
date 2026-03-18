using Vogen;

namespace Keepi.Core.Entries;

[ValueObject<string>()]
public readonly partial struct UserEntryRemark
{
    public const int MaxLength = 256;

    public static Validation Validate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Remark cannot be empty");
        }

        if (value.Length > MaxLength)
        {
            return Validation.Invalid("Remark length exceeds maximum");
        }

        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
    {
        return input.Trim();
    }
}
