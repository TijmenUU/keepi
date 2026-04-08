using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

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

    // A service provider is injected instead of the usecases themselves as the
    // "normal" scoped services are tied to the circuit lifetime which may be
    // too long causing strange issues, e.g. the DatabaseContext should have a
    // as short as possible lifetime.
    [Theory]
    [MemberData(nameof(PageTypes))]
    public void Blazor_pages_must_not_inject_use_cases_directly(Type pageType)
    {
        var t = pageType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        var injected = pageType
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Select(f => (f.FieldType, Attribute.GetCustomAttributes(element: f)))
            .Concat(
                pageType
                    .GetProperties(
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                    )
                    .Select(p => (p.PropertyType, Attribute.GetCustomAttributes(element: p)))
            )
            .Where(p =>
            {
                return p.Item2.Any(a => a.GetType() == typeof(InjectAttribute))
                    && (p.Item1.FullName?.Contains("UseCase") ?? false);
            });

        injected.ShouldBeEmpty(
            customMessage: $"The page {pageType.FullName} must not inject use cases directly, use an injected IServiceProvider instead and create a new scope just for the component."
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
