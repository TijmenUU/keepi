using Keepi.Core.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Keepi.Core.DependencyInjection;

public static class IServiceCollectionExtensions
{
  public static IServiceCollection AddUseCases(this IServiceCollection services)
  {
    services.AddScoped<ICreateEntryCategoryUseCase, CreateEntryCategoryUseCase>();
    services.AddScoped<IDeleteEntryCategoryUseCase, DeleteEntryCategoryUseCase>();
    services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
    services.AddScoped<IUpdateEntryCategoryUseCase, UpdateEntryCategoryUseCase>();

    return services;
  }
}