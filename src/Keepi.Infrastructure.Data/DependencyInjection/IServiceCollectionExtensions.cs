using Keepi.Infrastructure.Data.Entries;
using Keepi.Infrastructure.Data.Projects;
using Keepi.Infrastructure.Data.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Keepi.Infrastructure.Data.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(
        this IServiceCollection services,
        string sqliteConnectionString
    )
    {
        services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseSqlite(connectionString: sqliteConnectionString);
        });

        AddRepositoryWithInterfaces<ProjectRepository>(services);
        AddRepositoryWithInterfaces<UserEntryRepository>(services);
        AddRepositoryWithInterfaces<UserRepository>(services);

        return services;
    }

    private static void AddRepositoryWithInterfaces<TRepository>(IServiceCollection services)
        where TRepository : class
    {
        var repositoryType = typeof(TRepository);
        services.AddScoped(repositoryType);

        var interfaceTypes = repositoryType.GetInterfaces();
        foreach (var interfaceType in interfaceTypes)
        {
            services.AddScoped(interfaceType, sp => sp.GetRequiredService(repositoryType));
        }
    }

    public static IServiceCollection AddEnsureDatabaseCreatedHelper(
        this IServiceCollection services
    )
    {
        services.AddScoped<IEnsureDatabaseCreated, EnsureDatabaseCreated>();
        return services;
    }
}
