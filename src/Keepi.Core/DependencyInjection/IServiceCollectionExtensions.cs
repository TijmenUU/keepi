using Keepi.Core.Entries;
using Keepi.Core.Projects;
using Keepi.Core.UserInvoiceItemCustomizations;
using Keepi.Core.UserProjects;
using Keepi.Core.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Keepi.Core.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<ICreateProjectUseCase, CreateProjectUseCase>();
        services.AddScoped<IExportUserEntriesUseCase, ExportUserEntriesUseCase>();
        services.AddScoped<IGetUserProjectsUseCase, GetUserProjectsUseCase>();
        services.AddScoped<IGetUserEntriesForWeekUseCase, GetUserEntriesForWeekUseCase>();
        services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
        services.AddScoped<IUpdateProjectUseCase, UpdateProjectUseCase>();
        services.AddScoped<
            IUpdateUserInvoiceCustomizationsUseCase,
            UpdateUserInvoiceCustomizationsUseCase
        >();
        services.AddScoped<IUpdateWeekUserEntriesUseCase, UpdateWeekUserEntriesUseCase>();

        return services;
    }
}
