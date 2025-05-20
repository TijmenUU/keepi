using Keepi.Core.Repositories;
using Keepi.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Keepi.Infrastructure.Data.DependencyInjection;

public static class IServiceCollectionExtensions
{
  public static IServiceCollection AddRepositories(
    this IServiceCollection services,
    string sqliteConnectionString)
  {
    services.AddDbContext<DatabaseContext>(options =>
    {
      options.UseSqlite(connectionString: sqliteConnectionString);
    });

    services.AddScoped<EntryCategoryRepository>();
    services.AddScoped<IDeleteEntryCategory>(sp => sp.GetRequiredService<EntryCategoryRepository>());
    services.AddScoped<IStoreEntryCategory>(sp => sp.GetRequiredService<EntryCategoryRepository>());
    services.AddScoped<IUpdateEntryCategory>(sp => sp.GetRequiredService<EntryCategoryRepository>());

    services.AddScoped<UserRepository>();
    services.AddScoped<IGetUser>(sp => sp.GetRequiredService<UserRepository>());
    services.AddScoped<IGetUserEntryCategories>(sp => sp.GetRequiredService<UserRepository>());
    services.AddScoped<IGetUserExists>(sp => sp.GetRequiredService<UserRepository>());
    services.AddScoped<IStoreNewUser>(sp => sp.GetRequiredService<UserRepository>());

    return services;
  }
}