using Vogen;

namespace Keepi.Core;

[ValueObject<int>]
public readonly partial struct Year
{
    public static Validation Validate(int value)
    {
        if (value < 1900)
        {
            return Validation.Invalid("Cannot less than 1900");
        }
        if (value >= 10_000)
        {
            return Validation.Invalid("Cannot be more than 10,000");
        }

        return Validation.Ok;
    }
}
