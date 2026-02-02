using System.Diagnostics;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Keepi.Infrastructure.OpenTelemetry;

public static class DependencyInjection
{
    public static IOpenTelemetryBuilder AddKeepiOpenTelemetry(
        this IServiceCollection services,
        string serviceName,
        Action<TracerProviderBuilder> tracingConfigurator,
        Action<MeterProviderBuilder> metricsConfigurator
    )
    {
        return services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithLogging()
            .WithTracing(tracing =>
            {
                tracingConfigurator(tracing);
                tracing.AddSource(UseCaseTraceInterceptor.ActivitySourceName);
            })
            .WithMetrics(metricsConfigurator);
    }

    public static void RegisterTraceInterceptors(this IServiceCollection services)
    {
        services.AddSingleton(new ProxyGenerator());

        var targetAssembly = typeof(Core.Projects.IUpdateProjectUseCase).Assembly;
        services.WrapServicesWithTracer<UseCaseTraceInterceptor>(predicate: s =>
            s.ServiceType.Assembly == targetAssembly
            && s.ServiceType.IsInterface
            && s.ServiceType.Name.EndsWith("UseCase")
        );
    }

    public static void WrapServicesWithTracer<TInterceptor>(
        this IServiceCollection services,
        Func<ServiceDescriptor, bool> predicate
    )
        where TInterceptor : class, IAsyncInterceptor
    {
        services.AddSingleton(typeof(TInterceptor));

        var toWrap = services.Where(predicate).ToArray();
        foreach (var registration in toWrap)
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
                        var implementation = ActivatorUtilities.CreateInstance(
                            provider,
                            implementationType
                        );

                        var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
                        var interceptor = provider.GetRequiredService<TInterceptor>();
                        return proxyGenerator.CreateInterfaceProxyWithTargetInterface(
                            serviceType,
                            implementation,
                            interceptor
                        );
                    },
                    lifetime
                )
            );
        }
    }

    private sealed class UseCaseTraceInterceptor : TraceInterceptor
    {
        public const string ActivitySourceName = "UseCaseTraceActivitySource";

        public UseCaseTraceInterceptor()
            : base(activitySourceName: ActivitySourceName) { }
    }
}
