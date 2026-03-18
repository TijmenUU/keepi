using Vogen;

namespace Keepi.Core.Projects;

[ValueObject<int>()]
public readonly partial struct ProjectId
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
