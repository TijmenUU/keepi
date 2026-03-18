using Vogen;

namespace Keepi.Core.UserInvoiceItemCustomizations;

[ValueObject<int>()]
public readonly partial struct UserInvoiceITemCustomizationOrdinal
{
    public static Validation Validate(int value)
    {
        if (value < 0)
        {
            return Validation.Invalid("Cannot be less than zero");
        }

        return Validation.Ok;
    }
}
