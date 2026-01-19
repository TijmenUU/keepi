using Keepi.Api.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Keepi.Api.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddApiHelpers(this IServiceCollection services)
    {
        services.AddScoped<Core.Users.IResolveUser, ResolveUser>();

        return services;
    }
}
