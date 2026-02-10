using Keepi.App.Cancellation;

namespace Keepi.App.Unit.Tests.Cancellation;

public class CancellationTokenFactoryTests
{
    [Fact]
    public async Task Linked_token_is_cancelled_by_shutdown()
    {
        using var context = new TestContext();

        var factory = context.BuildFactory();
        var spinningTask = Task.Run(async () =>
        {
            using var linkedSource = factory.CreateShutdownLinkedTokenSource(
                sourceToWrap: new CancellationTokenSource()
            );
            await Task.Delay(
                millisecondsDelay: Timeout.Infinite,
                cancellationToken: linkedSource.Token
            );
        });

        context.ShutdownCancellationToken.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(spinningTask);
        context.ShutdownCancellationToken.IsCancellationRequested.ShouldBeTrue();
    }

    [Fact]
    public async Task Linked_token_is_cancelled_by_linked_token_source()
    {
        using var context = new TestContext();

        var factory = context.BuildFactory();
        var sourceToWrap = new CancellationTokenSource(); // The linked source takes ownership of this, hence no dispose/using
        var spinningTask = Task.Run(async () =>
        {
            using var linkedSource = factory.CreateShutdownLinkedTokenSource(
                sourceToWrap: sourceToWrap
            );
            await Task.Delay(
                millisecondsDelay: Timeout.Infinite,
                cancellationToken: linkedSource.Token
            );
        });

        sourceToWrap.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(spinningTask);
        context.ShutdownCancellationToken.IsCancellationRequested.ShouldBeFalse();
    }

    private class TestContext : IDisposable
    {
        public CancellationTokenSource ShutdownCancellationToken { get; } = new();

        public CancellationTokenFactory BuildFactory() =>
            new(shutdownCancellationTokenSource: ShutdownCancellationToken);

        public void Dispose()
        {
            ShutdownCancellationToken.Dispose();
        }
    }
}
