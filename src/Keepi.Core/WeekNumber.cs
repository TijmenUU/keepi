using Vogen;

namespace Keepi.Core;

[ValueObject<int>]
public readonly partial struct WeekNumber
{
    public static Validation Validate(int value)
    {
        if (value < 1)
        {
            return Validation.Invalid("Cannot less than 1");
        }
        if (value > 53)
        {
            return Validation.Invalid("Cannot be more than 53");
        }

        return Validation.Ok;
    }
}
