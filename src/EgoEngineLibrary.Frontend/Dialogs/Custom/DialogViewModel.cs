using CommunityToolkit.Mvvm.ComponentModel;

namespace EgoEngineLibrary.Frontend.Dialogs.Custom;

public abstract class DialogViewModel : ObservableValidator
{
    public abstract string Title { get; }

    public virtual bool CanMinimize => false;

    public virtual bool CanResize => false;

    /// <summary>
    /// Waits until a dialog result is set.
    /// </summary>
    public abstract Task WaitForDialogResult();
}

public abstract class DialogViewModel<T> : DialogViewModel
{
    private readonly AsyncManualResetEvent _closeEvent = new();
    internal T? DialogResult { get; private set; }

    internal void ResetResult()
    {
        DialogResult = default;
        _closeEvent.Reset();
    }

    protected void SetDialogResult(T? value)
    {
        DialogResult = value;
        _closeEvent.Set();
    }

    /// <inheritdoc />
    public override Task WaitForDialogResult() => _closeEvent.WaitAsync();

    private sealed class AsyncManualResetEvent
    {
        private volatile TaskCompletionSource<bool> _tcs = new();
        
        public Task WaitAsync() => _tcs.Task;
        
        public void Set() => _tcs.TrySetResult(true);

        public void Reset()
        {
            while (true)
            {
                var tcs = _tcs;
                if (!tcs.Task.IsCompleted ||
                    Interlocked.CompareExchange(ref _tcs, new TaskCompletionSource<bool>(), tcs) == tcs)
                {
                    return;
                }
            }
        }
    }
}