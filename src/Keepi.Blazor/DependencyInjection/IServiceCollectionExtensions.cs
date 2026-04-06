using Keepi.Blazor.Authorization;

namespace Keepi.Blazor.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizationHelpers(this IServiceCollection services)
    {
        services.AddScoped<
            Core.Users.IGetFirstAdminUserEmailAddress,
            GetFirstAdminUserEmailAddress
        >();
        services.AddScoped<Core.Users.IResolveUser, ResolveUser>();

        return services;
    }
}
