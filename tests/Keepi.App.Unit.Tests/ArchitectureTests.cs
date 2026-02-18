namespace Keepi.App.Unit.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Error_enum_types_should_have_unknown_as_their_default_value()
    {
        var appEnumTypes = typeof(Keepi.App.Services.GetWeekInputsError)
            .Assembly.GetTypes()
            .Where(t => t.IsEnum && t.Name.EndsWith("Error"))
            .ToArray();

        var deviatingResults = appEnumTypes
            .Select(t => new { Type = t, DefaultValue = Activator.CreateInstance(t) })
            .Where(r => r.DefaultValue == null || Enum.GetName(r.Type, r.DefaultValue) != "Unknown")
            .ToArray();

        deviatingResults.ShouldBeEmpty(
            customMessage: "Enum types should have unknown as their default (0) value. Any serialization issues or accidental (default)s should yield unknown errors instead of a random known error."
        );
    }
}
