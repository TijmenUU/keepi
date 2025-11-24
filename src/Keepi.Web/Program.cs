using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using AspNet.Security.OAuth.GitHub;
using Castle.DynamicProxy;
using FastEndpoints;
using Keepi.Api.DependencyInjection;
using Keepi.Api.Users.Get;
using Keepi.Core.DependencyInjection;
using Keepi.Infrastructure.Data.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
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
            .Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .UseOtlpExporter()
            .WithLogging()
            .WithTracing(tracing =>
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddSource(UseCaseTraceInterceptor.ActivitySourceName)
            )
            .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation());

        builder.Services.AddSingleton(new ProxyGenerator()); // Used to generate interceptors in order to trace the use cases (open telemetry)

        // Note that the steps below should be last in the service registration process
        RegisterTraceInterceptorForUseCases(builder.Services);

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
                        RedirectUri = "/", // TODO redirect to the actual page the user came from?
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

    private static void RegisterTraceInterceptorForUseCases(IServiceCollection services)
    {
        services.AddSingleton<UseCaseTraceInterceptor>();

        var targetAssembly = typeof(Core.Projects.IUpdateProjectUseCase).Assembly;
        var useCaseRegistrations = services
            .Where(s =>
                s.ServiceType.Assembly == targetAssembly
                && s.ServiceType.IsInterface
                && s.ServiceType.Name.EndsWith("UseCase")
            )
            .ToArray();
        foreach (var registration in useCaseRegistrations)
        {
            Debug.Assert(registration.ImplementationType != null);

            var serviceType = registration.ServiceType;
            var implementationType = registration.ImplementationType;
            var lifetime = registration.Lifetime;

            services.Remove(registration);
            services.Add(
                new ServiceDescriptor(
                    serviceType,
                    provider =>
                    {
                        var implemtation = ActivatorUtilities.CreateInstance(
                            provider,
                            implementationType
                        );

                        var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
                        var interceptor = provider.GetRequiredService<UseCaseTraceInterceptor>();
                        return proxyGenerator.CreateInterfaceProxyWithTargetInterface(
                            serviceType,
                            implemtation,
                            interceptor
                        );
                    },
                    lifetime
                )
            );
        }
    }

#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
    // The type parameter is allowed to have null values according to the documentation, see
    // https://github.com/JSkimming/Castle.Core.AsyncInterceptor?tab=readme-ov-file#option-3-extend-processingasyncinterceptortstate-class-to-intercept-invocations
    class UseCaseTraceInterceptor : ProcessingAsyncInterceptor<Activity?>, IDisposable
#pragma warning restore CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
    {
        public const string ActivitySourceName = "UseCaseTraceActivitySource";
        private readonly ActivitySource activitySource = new(ActivitySourceName);

        public void Dispose()
        {
            activitySource.Dispose();
        }

        protected override Activity? StartingInvocation(IInvocation invocation)
        {
            // See https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing-instrumentation-walkthroughs
            // for more information how/why this works
            return activitySource.StartActivity(
                $"{invocation.TargetType.Name}.{invocation.Method.Name}",
                ActivityKind.Internal
            );
        }

        protected override void CompletedInvocation(IInvocation invocation, Activity? state)
        {
            state?.Dispose();
            base.CompletedInvocation(invocation, state);
        }
    }
}
