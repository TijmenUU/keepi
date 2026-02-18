using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Keepi.App.Authorization;
using Keepi.App.Cancellation;
using Keepi.App.Services;
using Keepi.App.ViewModels;
using Keepi.App.Views;
using Keepi.Core.DependencyInjection;
using Keepi.Core.Users;
using Keepi.Infrastructure.Data;
using Keepi.Infrastructure.Data.DependencyInjection;
using Keepi.Infrastructure.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;

namespace Keepi.App;

public partial class App : Application
{
    const string serviceName = "keepi.app";

    private readonly CancellationTokenSource shutdownCancellationTokenSource = new();
    private ServiceProvider? serviceProvider;
    private IServiceScope? serviceScope;

    private IServiceScope CreateServiceScope()
    {
        if (serviceProvider == null)
        {
            Debug.Assert(serviceScope == null);
            serviceProvider = BuildServiceProvider(
                shutdownCancellationTokenSource: shutdownCancellationTokenSource
            );
        }
        serviceScope ??= serviceProvider.CreateScope();

        return serviceScope;
    }

    private static ServiceProvider BuildServiceProvider(
        CancellationTokenSource shutdownCancellationTokenSource
    )
    {
        var collection = new ServiceCollection();

        RegisterKeepiServices(
            collection: collection,
            shutdownCancellationTokenSource: shutdownCancellationTokenSource
        );
        RegisterAvaloniaViews(collection: collection);

        // Note that this step should be after all services have been registered
        RegisterTelemetry(collection: collection);

        return collection.BuildServiceProvider();
    }

    private static void RegisterKeepiServices(
        IServiceCollection collection,
        CancellationTokenSource shutdownCancellationTokenSource
    )
    {
        collection.AddSingleton<ICancellationTokenFactory, CancellationTokenFactory>(
            _ => new CancellationTokenFactory(
                shutdownCancellationTokenSource: shutdownCancellationTokenSource
            )
        );
        collection.AddScoped<IGetFirstAdminUserEmailAddress, GetFirstAdminUserEmailAddress>();
        collection.AddScoped<IGetUserName, GetUserName>();
        collection.AddScoped<IResolveUser, ResolveUser>();

        collection.AddScoped<IUserEntryWeekService, UserEntryWeekService>();

        collection.AddUseCases();

        collection.AddRepositories(sqliteConnectionString: "Data source=keepi.db"); // TODO consider using appsettings
        collection.AddEnsureDatabaseCreatedHelper();
    }

    private static void RegisterAvaloniaViews(IServiceCollection collection)
    {
        // TRANSIENT LIFETIMES ARE NOT DISPOSED
        // You probably want to use scoped. Only use Transient when you are sure
        // that the view does not (ever) require disposal and really needs a new
        // instance every time it is resolved from the service provider.
        collection.AddScoped<MainWindowViewModel>();
    }

    private static void RegisterTelemetry(IServiceCollection collection)
    {
        collection.AddSingleton<OtlpExporterOptions>(_ =>
            new() // TODO consider using appsettings
            {
                Endpoint = new Uri("http://localhost:4317"),
                Protocol = OtlpExportProtocol.Grpc,
            }
        );
        collection
            .AddKeepiOpenTelemetry(
                serviceName: serviceName,
                tracingConfigurator: (tracing) =>
                {
                    tracing.AddSource(AppServiceTraceInterceptor.ActivitySourceName);
                },
                metricsConfigurator: (_) => { }
            )
            .UseOtlpExporter();

        var servicesAssembly = typeof(IUserEntryWeekService).Assembly;
        collection.WrapServicesWithTracer<AppServiceTraceInterceptor>(predicate: s =>
            s.ServiceType.Assembly == servicesAssembly
            && s.ServiceType.IsInterface
            && s.ServiceType.Name.EndsWith("Service")
        );

        collection.RegisterTraceInterceptors(); // Note that this step should be after all Keepi services have been registered
    }

    private sealed class AppServiceTraceInterceptor : TraceInterceptor
    {
        public const string ActivitySourceName = "AppServiceActivitySource";

        public AppServiceTraceInterceptor()
            : base(activitySourceName: ActivitySourceName) { }
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var scopedServiceProvider = CreateServiceScope().ServiceProvider;

        EnsureDatabaseIsCreated(serviceProvider: scopedServiceProvider);

        var mainWindowLogger = scopedServiceProvider.GetRequiredService<ILogger<MainWindow>>();
        var mainWindowViewModel = scopedServiceProvider.GetRequiredService<MainWindowViewModel>();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow(logger: mainWindowLogger)
            {
                DataContext = mainWindowViewModel,
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            DisableAvaloniaDataAnnotationValidation();
            singleViewPlatform.MainView = new MainWindow(logger: mainWindowLogger)
            {
                DataContext = mainWindowViewModel,
            };
        }

        RegisterCleanUpOnExit();
        RegisterExceptionLoggerWithUiThread(serviceProvider: scopedServiceProvider);

        base.OnFrameworkInitializationCompleted();
    }

    private static void EnsureDatabaseIsCreated(IServiceProvider serviceProvider)
    {
        var ensureDatabaseCreated = serviceProvider.GetRequiredService<IEnsureDatabaseCreated>();
        ensureDatabaseCreated.Execute();
    }

    // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
    // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
    private static void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove = BindingPlugins
            .DataValidators.OfType<DataAnnotationsValidationPlugin>()
            .ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private static void RegisterExceptionLoggerWithUiThread(IServiceProvider serviceProvider)
    {
        var exceptionLogger = serviceProvider.GetRequiredService<ILogger<App>>();
        Dispatcher.UIThread.UnhandledException += (sender, args) =>
        {
            if (args.Handled)
            {
                exceptionLogger.LogInformation(
                    args.Exception,
                    "Handled exception with sender {SenderType}",
                    sender.GetType().FullName
                );
            }
            else
            {
                exceptionLogger.LogError(
                    args.Exception,
                    "Unhandled exception with sender {SenderType}",
                    sender.GetType().FullName
                );
            }
        };
    }

    private void RegisterCleanUpOnExit()
    {
        if (ApplicationLifetime is IControlledApplicationLifetime controlledApplication)
        {
            controlledApplication.Exit += (_, _) =>
            {
                shutdownCancellationTokenSource.Cancel();
                shutdownCancellationTokenSource.Dispose();
                serviceScope?.Dispose();
                serviceProvider?.Dispose();
            };
        }
        else
        {
            // Without an application lifetime there is no way to do clean up,
            // which makes the app behave unexpectedly on close.
            throw new NotSupportedException("Unsupported ApplicationLifetime");
        }
    }
}
