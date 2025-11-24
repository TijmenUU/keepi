using System.Security.Claims;
using Keepi.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Keepi.Web.Integration.Tests;

public class KeepiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest");

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
            services.AddHostedService<InitializeDatabaseHostedService>();
        });
    }

    public async Task<KeepiClient> CreateClientWithRandomUser()
    {
        return await KeepiClient.CreateWithRandomUser(httpClient: CreateClient());
    }

    public async Task<KeepiClient> CreateClientWithUser(string fullName, string subjectClaim)
    {
        return await KeepiClient.CreateWithUser(
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

internal class InitializeDatabaseHostedService(DatabaseContext databaseContext) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await databaseContext.Database.EnsureDeletedAsync(cancellationToken);
        await databaseContext.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
