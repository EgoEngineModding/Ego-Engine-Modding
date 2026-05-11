using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;

namespace EgoEngineLibrary.Frontend.Dialogs.Custom;

public static class DialogAvalonia
{
    public static void Register(Window recipient)
    {
        Dialog.Messenger.Register<Window, DialogShowMessage>(recipient, DialogShowHandler);
    }

    public static void Unregister(Window recipient)
    {
        Dialog.Messenger.Unregister<DialogShowMessage>(recipient);
    }

    private static void DialogShowHandler(Window recipient, DialogShowMessage message)
    {
        message.Reply(Show(recipient, message.ViewModel));
        return;

        static async Task<bool> Show(Window owner, DialogViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(owner);

            using var cts = new CancellationTokenSource();
            var dialog = new Window
            {
                Title = viewModel.Title,
                CanMinimize = viewModel.CanMinimize,
                CanResize = viewModel.CanResize,
                Content = viewModel,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Tag = cts.Token,
            };

            try
            {
                dialog.Loaded += DialogOnLoaded;
                _ = await dialog.ShowDialog<bool>(owner);
            }
            finally
            {
                dialog.Loaded -= DialogOnLoaded;
                await cts.CancelAsync();
            }

            return false;

            static async void DialogOnLoaded(object? sender, RoutedEventArgs e)
            {
                try
                {
                    var window = sender as Window;
                    if (window?.Content is not DialogViewModel viewModel ||
                        window.Tag is not CancellationToken token)
                    {
                        return;
                    }

                    await viewModel.WaitForDialogResult().WaitAsync(token);
                    window.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
    }
}