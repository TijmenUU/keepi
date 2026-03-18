using Vogen;

namespace Keepi.Core.Users;

[ValueObject<int>()]
public readonly partial struct UserId
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
