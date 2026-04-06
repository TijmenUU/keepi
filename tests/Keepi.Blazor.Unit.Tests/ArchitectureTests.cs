using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;

namespace Keepi.Blazor.Unit.Tests;

public class ArchitectureTests
{
    [Theory]
    [MemberData(nameof(PageTypes))]
    public void All_blazor_pages_must_use_authorization(Type pageType)
    {
        var attributes = Attribute.GetCustomAttributes(element: pageType);
        if (attributes.Any(a => a.GetType() == typeof(CompilerGeneratedAttribute)))
        {
            return;
        }

        attributes.ShouldContain(
            a => a.GetType() == typeof(AuthorizeAttribute),
            customMessage: $"The page {pageType.FullName} does not have an authorize attribute"
        );
    }

    public static TheoryData<Type> PageTypes() =>
        [
            .. typeof(Components.App)
                .Assembly.GetTypes()
                .Where(t =>
                    (t.Namespace?.StartsWith("Keepi.Blazor.Components.Pages") ?? false)
                    && t.IsClass
                    && !t.IsAbstract
                )
                // Generated classes have a suffix lead by a +
                .Where(t => t.FullName?.Contains('+') != true),
        ];
}
