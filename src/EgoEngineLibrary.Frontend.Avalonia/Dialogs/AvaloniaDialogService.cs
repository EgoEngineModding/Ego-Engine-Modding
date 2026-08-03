using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using EgoEngineLibrary.Frontend.Dialogs.File;

namespace EgoEngineLibrary.Frontend.Dialogs;

/// <summary>
/// A <see cref="IDialogService"/> for Avalonia.
/// </summary>
public sealed class AvaloniaDialogService : DialogServiceBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="AvaloniaDialogService"/>.
    /// </summary>
    public AvaloniaDialogService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <inheritdoc />
    public override async Task<bool?> ShowDialogAsync(IDialogViewModel viewModel)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            return await Dispatcher.UIThread.InvokeAsync(() => ShowDialogAsync(viewModel));
        }

        var frameworkHandled = await HandleFrameworkDialogsAsync(viewModel);
        if (frameworkHandled)
        {
            // result unused
            return false;
        }

        var window = CreateWindow(viewModel);
        var ctx = new DialogContext(window, viewModel);
        try
        {
            viewModel.DialogContext = ctx;
            _ = await window.ShowDialog<bool>(GetOwner());
            return await ctx.ResultTask;
        }
        finally
        {
            ctx.Close(false);
        }
    }

    private static async Task<bool> HandleFrameworkDialogsAsync(IDialogViewModel viewModel)
    {
        Task? task = viewModel switch
        {
            FileOpenViewModel fileOpenViewModel => FileDialogAvalonia.FileOpen(GetOwner(), fileOpenViewModel),
            FileSaveViewModel fileSaveViewModel => FileDialogAvalonia.FileSave(GetOwner(), fileSaveViewModel),
            FolderOpenViewModel folderOpenViewModel => FileDialogAvalonia.FolderOpen(GetOwner(), folderOpenViewModel),
            _ => null
        };

        if (task is null)
        {
            return false;
        }
        
        await task;
        return true;
    }

    public override void Show(IDialogViewModel viewModel)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Invoke(() => Show(viewModel));
            return;
        }
        
        var window = CreateWindow(viewModel);
        var ctx = new DialogContext(window, viewModel);
        try
        {
            viewModel.DialogContext = ctx;
        }
        catch
        {
            ctx.Close(false);
            throw;
        }

        window.Show(GetOwner());
    }

    private static Window CreateWindow(IDialogViewModel viewModel)
    {
        return new Window
        {
            Title = viewModel.Title,
            CanMinimize = false,
            CanResize = false,
            Content = viewModel,
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };
    }

    private static Window GetOwner()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow ?? throw new InvalidOperationException("Main window is null.");
        }

        throw new InvalidOperationException("ApplicationLifetime not supported");
    }

    private class DialogContext : IDialogContext
    {
        private readonly TaskCompletionSource<bool> _resultTcs;
        private Window? _dialogWindow;
        private IDialogViewModel? _viewModel;

        public Task<bool> ResultTask => _resultTcs.Task;

        public DialogContext(Window dialogWindow, IDialogViewModel viewModel)
        {
            _resultTcs = new TaskCompletionSource<bool>();
            _dialogWindow = dialogWindow;
            _viewModel = viewModel;
            _dialogWindow.Closed += DialogWindowOnClosed;
        }

        private void DialogWindowOnClosed(object? sender, EventArgs e)
        {
            _dialogWindow?.Closed -= DialogWindowOnClosed;
            Close(false);
        }

        public void Close(bool result = true)
        {
            if (!_resultTcs.TrySetResult(result))
            {
                // Already closed
                return;
            }
            
            _viewModel?.DialogContext = null;
            _viewModel = null;

            var dialogWindow = _dialogWindow;
            _dialogWindow = null;
            if (dialogWindow is null)
            {
                return;
            }
            
            dialogWindow.Closed -= DialogWindowOnClosed;
            if (dialogWindow.Dispatcher.CheckAccess())
            {
                dialogWindow.Close();
            }
            else
            {
                dialogWindow.Dispatcher.Invoke(dialogWindow.Close);
            }
        }
    }
}
