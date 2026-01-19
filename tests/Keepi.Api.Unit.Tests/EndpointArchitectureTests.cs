using Keepi.Api.Users.Get;
using Keepi.Core.Users;

namespace Keepi.Api.Unit.Tests;

public class EndpointArchitectureTests
{
    private static readonly Type[] EndpointTypes = typeof(GetUserEndpoint)
        .Assembly.GetTypes()
        .Where(t =>
            !t.IsAbstract && t.IsClass && t.IsSubclassOf(typeof(FastEndpoints.BaseEndpoint))
        )
        .ToArray();

    [Fact]
    public void No_endpoint_should_use_repository_interfaces()
    {
        foreach (var endpoint in EndpointTypes)
        {
            var constructors = endpoint.GetConstructors();
            constructors.ShouldHaveSingleItem();

            // Repository interfaces do not check permissions, use cases do,
            // hence repository interfaces should never be used by endpoints.
            constructors[0]
                .GetParameters()
                .Where(p =>
                    // Exclude any allowed dependencies here
                    !(
                        p.ParameterType.IsGenericType
                        && p.ParameterType.GetGenericTypeDefinition()
                            == typeof(Microsoft.Extensions.Logging.ILogger<>)
                    )
                    && !p.ParameterType.Name.EndsWith("UseCase")
                    && p.ParameterType != typeof(IResolveUser)
                )
                .ShouldBeEmpty(
                    customMessage: $"Disallowed parameter type in ctor of {endpoint.Name}"
                );
        }
    }
}
