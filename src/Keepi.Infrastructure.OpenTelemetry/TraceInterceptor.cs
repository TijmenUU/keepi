using System.Diagnostics;
using Castle.DynamicProxy;

namespace Keepi.Infrastructure.OpenTelemetry;

#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
// The type parameter is allowed to have null values according to the documentation, see
// https://github.com/JSkimming/Castle.Core.AsyncInterceptor?tab=readme-ov-file#option-3-extend-processingasyncinterceptortstate-class-to-intercept-invocations
public abstract class TraceInterceptor : ProcessingAsyncInterceptor<Activity?>, IDisposable
#pragma warning restore CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
{
    private readonly ActivitySource activitySource;

    public TraceInterceptor(string activitySourceName)
    {
        activitySource = new ActivitySource(activitySourceName);
    }

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
