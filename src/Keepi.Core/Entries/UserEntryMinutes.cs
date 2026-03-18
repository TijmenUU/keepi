using Vogen;

namespace Keepi.Core.Entries;

[ValueObject<int>()]
public readonly partial struct UserEntryMinutes
{
    public static Validation Validate(int value)
    {
        if (value <= 0)
        {
            return Validation.Invalid("Cannot be less than or equal to zero");
        }

        return Validation.Ok;
    }
}
