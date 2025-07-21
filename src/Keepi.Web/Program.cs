using System.Text.Json;
using System.Text.Json.Serialization;
using AspNet.Security.OAuth.GitHub;
using FastEndpoints;
using Keepi.Api.DependencyInjection;
using Keepi.Api.UserEntryCategories.Delete;
using Keepi.Core.DependencyInjection;
using Keepi.Infrastructure.Data.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Keepi.Web;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // https://fast-endpoints.com/docs/get-started#create-project-install-package
        builder.Services.AddFastEndpoints(options =>
        {
            options.Assemblies = [typeof(DeleteUserUserEntryCategoryEndpoint).Assembly];
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

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.MapGet(
            "/error",
            () =>
            {
                return Results.Content(
                    content: @"<!doctype html>
<html lang=""en"">

<head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Keepi</title>
</head>

<body>
    <p>Er is iets helemaal stukgegaan!</p>
</body>

</html>",
                    contentType: "text/html"
                );
            }
        );

        app.MapGet(
            "/signin",
            () =>
                Results.Challenge(
                    properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                    {
                        RedirectUri = "/",
                    },
                    authenticationSchemes: [GitHubAuthenticationDefaults.AuthenticationScheme]
                )
        );
        app.MapGet(
            "/signout",
            () =>
                Results.SignOut(
                    properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                    {
                        RedirectUri = "https://www.google.nl", // TODO Use an actual sign out page?
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
