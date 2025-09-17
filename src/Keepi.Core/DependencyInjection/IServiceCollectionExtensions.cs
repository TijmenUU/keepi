using Keepi.Core.Entries;
using Keepi.Core.UserEntryCategories;
using Keepi.Core.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Keepi.Core.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IExportUserEntriesUseCase, ExportUserEntriesUseCase>();
        services.AddScoped<IGetUserEntriesForWeekUseCase, GetUserEntriesForWeekUseCase>();
        services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
        services.AddScoped<IUpdateUserEntryCategoriesUseCase, UpdateUserEntryCategoriesUseCase>();
        services.AddScoped<IUpdateWeekUserEntriesUseCase, UpdateWeekUserEntriesUseCase>();

        return services;
    }
}
