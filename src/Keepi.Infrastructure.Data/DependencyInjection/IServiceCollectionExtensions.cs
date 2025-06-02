using Keepi.Core.UserEntryCategories;
using Keepi.Core.Users;
using Keepi.Infrastructure.Data.UserEntryCategories;
using Keepi.Infrastructure.Data.Users;
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

    services.AddScoped<UserEntryCategoryRepository>();
    services.AddScoped<IDeleteUserEntryCategory>(sp => sp.GetRequiredService<UserEntryCategoryRepository>());
    services.AddScoped<IGetUserUserEntryCategories>(sp => sp.GetRequiredService<UserEntryCategoryRepository>());
    services.AddScoped<IStoreUserEntryCategory>(sp => sp.GetRequiredService<UserEntryCategoryRepository>());
    services.AddScoped<IUpdateUserEntryCategory>(sp => sp.GetRequiredService<UserEntryCategoryRepository>());

    services.AddScoped<UserRepository>();
    services.AddScoped<IGetUser>(sp => sp.GetRequiredService<UserRepository>());
    services.AddScoped<IGetUserExists>(sp => sp.GetRequiredService<UserRepository>());
    services.AddScoped<IStoreNewUser>(sp => sp.GetRequiredService<UserRepository>());

    return services;
  }
}