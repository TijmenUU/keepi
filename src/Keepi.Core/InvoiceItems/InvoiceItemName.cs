using Vogen;

namespace Keepi.Core.InvoiceItems;

[ValueObject<string>()]
public readonly partial struct InvoiceItemName
{
    public const int MaxLength = 64;

    public static Validation Validate(string? value)
    {
        if (value == null)
        {
            return Validation.Invalid("Invoice item name cannot be null");
        }

        if (value.Length > MaxLength)
        {
            return Validation.Invalid("Invoice item name length exceeds maximum");
        }

        if (value.Length == 0)
        {
            return Validation.Invalid("Invoice item name cannot be empty");
        }

        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
    {
        return input.Trim();
    }
}
