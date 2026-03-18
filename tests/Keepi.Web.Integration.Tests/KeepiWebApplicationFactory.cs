using System.Security.Claims;
using Keepi.Core;
using Keepi.Core.Users;
using Keepi.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Keepi.Web.Integration.Tests;

public class KeepiWebApplicationFactory : WebApplicationFactory<Program>
{
    private static string? adminUserName;
    private static string? adminUserSubjectClaim;

    public static void SetAdmin(string name, string subjectClaim)
    {
        if (adminUserName != null || adminUserSubjectClaim != null)
        {
            throw new InvalidOperationException("The admin user has already been set");
        }

        adminUserName = name;
        adminUserSubjectClaim = subjectClaim;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest");

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(
                initialData:
                [
                    new(
                        key: "Authentication:FirstAdminUserEmailAddress",
                        value: "admin@example.com"
                    ),
                ]
            );
        });

        builder.ConfigureServices(services =>
        {
            services
                .AddAuthentication(defaultScheme: "integrationtest")
                .AddScheme<AuthenticationSchemeOptions, IntegrationTestAuthenticationHandler>(
                    authenticationScheme: "integrationtest",
                    configureOptions: null
                );
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<Core.Users.IGetFirstAdminUserEmailAddress>(
                new IntegrationTestGetFirstAdminUserEmailAddress(emailAddress: "admin@example.com")
            );

            services.AddHostedService<InitializeDatabaseHostedService>();
            services.AddHostedService<SetupInitialAdminUserHostedService>();
        });
    }

    public async Task<KeepiClient> CreateClientForRandomZeroPermissionsUser()
    {
        var adminKeepiClient = CreateClientForAdminUser();

        var zeroPermissionKeepiClient = CreateClientForRandomNormalUser();
        var zeroPermissionUser = await zeroPermissionKeepiClient.GetUser();

        await adminKeepiClient.UpdateUserPermissions(
            userId: zeroPermissionUser.Id,
            request: new()
            {
                EntriesPermission = Api.Users
                    .UpdatePermissions
                    .UpdateUserPermissionsRequestPermission
                    .None,
                ExportsPermission = Api.Users
                    .UpdatePermissions
                    .UpdateUserPermissionsRequestPermission
                    .None,
                ProjectsPermission = Api.Users
                    .UpdatePermissions
                    .UpdateUserPermissionsRequestPermission
                    .None,
                UsersPermission = Api.Users
                    .UpdatePermissions
                    .UpdateUserPermissionsRequestPermission
                    .None,
            }
        );

        return zeroPermissionKeepiClient;
    }

    public KeepiClient CreateClientForAdminUser()
    {
        var httpClient = CreateClient(); // Force initialization of the app factory

        if (adminUserName == null || adminUserSubjectClaim == null)
        {
            throw new InvalidOperationException("The admin user has not (yet) been set");
        }

        return KeepiClient.CreateWithUser(
            httpClient: httpClient,
            fullName: adminUserName,
            subjectClaim: adminUserSubjectClaim
        );
    }

    public KeepiClient CreateClientForRandomNormalUser()
    {
        return KeepiClient.CreateWithRandomUser(httpClient: CreateClient());
    }

    public KeepiClient CreateClientForNormalUser(string fullName, string subjectClaim)
    {
        return KeepiClient.CreateWithUser(
            httpClient: CreateClient(),
            fullName: fullName,
            subjectClaim: subjectClaim
        );
    }
}

internal class IntegrationTestAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public IntegrationTestAuthenticationHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder
    )
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!TryGetHeaderValue("X-User-Subject-Claim", out var subjectClaim))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        if (!TryGetHeaderValue("X-User-Name", out var userName))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var ticket = new AuthenticationTicket(
            principal: new ClaimsPrincipal(
                new ClaimsIdentity(
                    claims:
                    [
                        new Claim(ClaimTypes.NameIdentifier, subjectClaim),
                        new Claim(ClaimTypes.Name, userName),
                        new Claim(ClaimTypes.Email, $"{subjectClaim}@example.com"),
                        new Claim(ClaimTypes.Authentication, "GitHub"),
                    ],
                    authenticationType: "GitHub"
                )
            ),
            authenticationScheme: Scheme.Name
        );

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private bool TryGetHeaderValue(string name, out string value)
    {
        if (
            !Context.Request.Headers.TryGetValue(name, out var headerValue)
            || string.IsNullOrWhiteSpace(headerValue)
        )
        {
            value = string.Empty;
            return false;
        }

        value = headerValue!;
        return true;
    }
}

internal sealed class IntegrationTestGetFirstAdminUserEmailAddress(string emailAddress)
    : Core.Users.IGetFirstAdminUserEmailAddress
{
    public IValueOrErrorResult<
        EmailAddress,
        Core.Users.GetFirstAdminUserEmailAddressError
    > Execute() =>
        Result.Success<EmailAddress, Core.Users.GetFirstAdminUserEmailAddressError>(
            EmailAddress.From(emailAddress)
        );
}

internal sealed class InitializeDatabaseHostedService(DatabaseContext databaseContext)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await databaseContext.Database.EnsureDeletedAsync(cancellationToken);
        await databaseContext.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

internal sealed class SetupInitialAdminUserHostedService(IServiceProvider serviceProvider)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        const string adminName = "Bob";
        const string adminSubjectClaim = "f4be8a16-6958-4023-8987-2cee88d45b42";

        using var scope = serviceProvider.CreateScope();
        var getOrRegisterNewUserUseCase =
            scope.ServiceProvider.GetRequiredService<Core.Users.IGetOrRegisterNewUserUseCase>();
        var result = await getOrRegisterNewUserUseCase.Execute(
            externalId: UserExternalId.From(adminSubjectClaim),
            emailAddress: EmailAddress.From("admin@example.com"),
            name: UserName.From(adminName),
            identityProvider: Core.Users.UserIdentityProvider.GitHub,
            cancellationToken: cancellationToken
        );

        if (!result.TrySuccess(out var successResult, out var errorResult))
        {
            throw new InvalidOperationException(
                $"Failed to register the admin user with error {errorResult}"
            );
        }

        if (!successResult.NewlyRegistered)
        {
            throw new InvalidOperationException(
                "Failed to register the admin user because it already exists"
            );
        }

        KeepiWebApplicationFactory.SetAdmin(name: adminName, subjectClaim: adminSubjectClaim);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
