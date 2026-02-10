using System;
using System.Diagnostics;
using System.Threading;

namespace Keepi.App.Cancellation;

public interface ICancellationTokenFactory
{
    CancellationToken GetShutdownCancellationToken();
    ILinkedCancellationTokenSource CreateShutdownLinkedTokenSource(
        CancellationTokenSource sourceToLink
    );
}

public interface ILinkedCancellationTokenSource : IDisposable
{
    CancellationToken Token { get; }
}

internal sealed class CancellationTokenFactory(
    CancellationTokenSource shutdownCancellationTokenSource
) : ICancellationTokenFactory
{
    public CancellationToken GetShutdownCancellationToken() =>
        shutdownCancellationTokenSource.Token;

    public ILinkedCancellationTokenSource CreateShutdownLinkedTokenSource(
        CancellationTokenSource sourceToWrap
    ) =>
        new LinkedCancellationTokenSource(
            cancellationTokenSource: sourceToWrap,
            additionalTokensToLink: GetShutdownCancellationToken()
        );

    private sealed class LinkedCancellationTokenSource : ILinkedCancellationTokenSource
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CancellationTokenSource linkedTokenSource;

        public LinkedCancellationTokenSource(
            CancellationTokenSource cancellationTokenSource,
            params CancellationToken[] additionalTokensToLink
        )
        {
            Debug.Assert(additionalTokensToLink.Length > 0);

            this.cancellationTokenSource = cancellationTokenSource;
            CancellationToken[] tokens = [cancellationTokenSource.Token, .. additionalTokensToLink];
            linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokens: tokens);
        }

        public CancellationToken Token => linkedTokenSource.Token;

        public void Dispose()
        {
            linkedTokenSource.Cancel();
            linkedTokenSource.Dispose();

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}
