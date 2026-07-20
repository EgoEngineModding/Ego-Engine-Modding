namespace EgoEngineLibrary.Frontend.Dialogs;

/// <inheritdoc />
public abstract class DialogServiceBase : IDialogService
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="DialogServiceBase"/>.
    /// </summary>
    protected DialogServiceBase(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public abstract Task<bool?> ShowDialogAsync(IDialogViewModel viewModel);

    /// <inheritdoc />
    public abstract void Show(IDialogViewModel viewModel);

    /// <inheritdoc />
    public T CreateViewModel<T>() where T : IDialogViewModel
    {
        var serviceType = typeof(T);
        var service = _serviceProvider.GetService(serviceType);
        if (service is not T viewModel)
        {
            throw new InvalidOperationException($"No service for type '{typeof(T)}' has been registered.");
        }

        return viewModel;
    }
}
