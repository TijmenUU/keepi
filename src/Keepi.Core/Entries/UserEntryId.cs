using Vogen;

namespace Keepi.Core.Entries;

[ValueObject<int>()]
public readonly partial struct UserEntryId
{
    public static Validation Validate(int value)
    {
        if (value <= 0)
        {
            return Validation.Invalid("Cannot be equal to or less than zero");
        }

        return Validation.Ok;
    }
}
