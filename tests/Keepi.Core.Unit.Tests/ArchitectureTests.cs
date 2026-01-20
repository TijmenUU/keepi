using Keepi.Core.Users;

namespace Keepi.Core.Unit.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Entity_types_should_be_static()
    {
        var coreEntityTypes = typeof(Core.Entries.UserEntryEntity)
            .Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.Name.EndsWith("Entity"))
            .ToArray();

        coreEntityTypes.ShouldBeEmpty(
            customMessage: "Avoid entity data structures as they promote coupling between application parts that probably should not be there."
        );
    }

    [Fact]
    public void Error_enum_types_should_have_unknown_as_their_default_value()
    {
        var coreEnumTypes = typeof(Core.Entries.UserEntryEntity)
            .Assembly.GetTypes()
            .Where(t => t.IsEnum && t.Name.EndsWith("Error"))
            .ToArray();

        var deviatingResults = coreEnumTypes
            .Select(t => new { Type = t, DefaultValue = Activator.CreateInstance(t) })
            .Where(r => r.DefaultValue == null || Enum.GetName(r.Type, r.DefaultValue) != "Unknown")
            .ToArray();

        deviatingResults.ShouldBeEmpty(
            customMessage: "Enum types should have unknown as their default (0) value. Any serialization issues or accidental (default)s should yield unknown errors instead of a random known error."
        );
    }

    [Fact]
    public void Use_cases_must_use_resolve_user_for_authorization()
    {
        var toIgnore = new Type[]
        {
            // This use case is allowed to be called by all authenticated users,
            // hence no authorization is necessary.
            typeof(GetOrRegisterNewUserUseCase),
        };

        var useCaseTypes = typeof(GetAllUsersUseCase)
            .Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("UseCase"))
            .Except(toIgnore)
            .ToArray();

        foreach (var useCase in useCaseTypes)
        {
            var constructors = useCase.GetConstructors();
            constructors.ShouldHaveSingleItem();

            constructors[0]
                .GetParameters()
                .ShouldContain(
                    p => p.ParameterType == typeof(IResolveUser),
                    customMessage: $"The use case {useCase.Name} is lacking authorization"
                );
        }
    }
}
