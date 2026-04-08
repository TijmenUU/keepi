using Microsoft.AspNetCore.Components;

public abstract class AsyncComponentBase : ComponentBase, IDisposable
{
    [Inject]
    private IHostApplicationLifetime? Lifetime { get; set; }
    private CancellationTokenSource? _cts;
    protected CancellationToken ComponentCancellationToken => _cts?.Token ?? CancellationToken.None;

    protected override void OnInitialized()
    {
        _cts =
            Lifetime != null
                ? CancellationTokenSource.CreateLinkedTokenSource(Lifetime.ApplicationStopping)
                : new();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
}
