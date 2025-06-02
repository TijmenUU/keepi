using Keepi.Core.UserEntryCategories;
using Keepi.Core.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Keepi.Core.DependencyInjection;

public static class IServiceCollectionExtensions
{
  public static IServiceCollection AddUseCases(this IServiceCollection services)
  {
    services.AddScoped<ICreateUserEntryCategoryUseCase, CreateUserEntryCategoryUseCase>();
    services.AddScoped<IDeleteUserEntryCategoryUseCase, DeleteUserEntryCategoryUseCase>();
    services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
    services.AddScoped<IUpdateUserEntryCategoryUseCase, UpdateUserEntryCategoryUseCase>();

    return services;
  }
}