using System.Text.Json;
using System.Text.Json.Serialization;
using AspNet.Security.OAuth.GitHub;
using FastEndpoints;
using Keepi.Api.DependencyInjection;
using Keepi.Api.Users.Get;
using Keepi.Core.DependencyInjection;
using Keepi.Infrastructure.Data.DependencyInjection;
using Keepi.Infrastructure.OpenTelemetry;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Keepi.Web;

public partial class Program
{
    const string serviceName = "keepi.web";

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // https://fast-endpoints.com/docs/get-started#create-project-install-package
        builder.Services.AddFastEndpoints(options =>
        {
            options.Assemblies = [typeof(GetUserEndpoint).Assembly];
        });

        if (builder.Environment.IsDevelopment())
        {
            // https://github.com/berhir/AspNetCore.SpaYarp
            builder.Services.AddSpaYarp();
        }

        builder.Services.AddApiHelpers();

        const string connectionStringName = "Keepi";
        builder.Services.AddRepositories(
            sqliteConnectionString: builder.Configuration.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException(
                    $"Missing required connection string {connectionStringName}"
                )
        );

        builder.Services.AddUseCases();

        builder.Services.AddAuthorization(configure =>
        {
            configure.DefaultPolicy =
                new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
        });
        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/signin";
                options.LogoutPath = "/signout";

                options.Events = new CookieAuthenticationEvents()
                {
                    OnRedirectToLogin = (ctx) =>
                    {
                        if (
                            ctx.Request.Path.StartsWithSegments("/api")
                            && ctx.Response.StatusCode == 200
                        )
                        {
                            ctx.Response.StatusCode = 401;
                        }
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = (ctx) =>
                    {
                        if (
                            ctx.Request.Path.StartsWithSegments("/api")
                            && ctx.Response.StatusCode == 200
                        )
                        {
                            ctx.Response.StatusCode = 403;
                        }
                        return Task.CompletedTask;
                    },
                };
            })
            // https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers
            .AddGitHub(options =>
            {
                options.ClientId =
                    builder.Configuration["Authentication:GitHub:ClientId"]
                    ?? throw new InvalidOperationException(
                        "The GitHub client ID is not configured"
                    );
                options.ClientSecret =
                    builder.Configuration["Authentication:GitHub:ClientSecret"]
                    ?? throw new InvalidOperationException(
                        "The GitHub client secret is not configured"
                    );
                options.CallbackPath = "/signin-oidc-github";
                options.Scope.Add("user:email");
            });

        builder.Services.Configure<OtlpExporterOptions>(
            builder.Configuration.GetSection("OpenTelemetry:Otlp")
        );
        builder
            .Services.AddKeepiOpenTelemetry(
                serviceName: serviceName,
                tracingConfigurator: cfg =>
                    cfg.AddAspNetCoreInstrumentation().AddEntityFrameworkCoreInstrumentation(),
                metricsConfigurator: cfg => cfg.AddAspNetCoreInstrumentation()
            )
            .UseOtlpExporter();

        builder.Services.RegisterTraceInterceptors(); // Note that this step should be after all Keepi services have been registered

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.MapGet(
            "/signin",
            (HttpRequest request) =>
            {
                var returnUrl = request.Query["returnUrl"].ToString();
                if (
                    !Uri.TryCreate(
                        uriString: returnUrl,
                        uriKind: UriKind.Absolute,
                        out var parsedResultUrl
                    )
                    || parsedResultUrl.AbsolutePath == "/signin"
                )
                {
                    returnUrl = "/";
                }

                return Results.Challenge(
                    properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                    {
                        RedirectUri = returnUrl,
                    },
                    authenticationSchemes: [GitHubAuthenticationDefaults.AuthenticationScheme]
                );
            }
        );
        app.MapGet(
            "/signout",
            () =>
                Results.SignOut(
                    properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                    {
                        RedirectUri = "/signedout",
                    },
                    authenticationSchemes: [CookieAuthenticationDefaults.AuthenticationScheme]
                )
        );

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseFastEndpoints(config =>
        {
            config.Endpoints.RoutePrefix = "api";
            config.Serializer.Options.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            );
        });

        if (builder.Environment.IsDevelopment())
        {
            app.UseSpaYarp();
        }
        else
        {
            app.UseStaticFiles();
            app.MapFallbackToFile("index.html");
        }

        app.Run();
    }
}
