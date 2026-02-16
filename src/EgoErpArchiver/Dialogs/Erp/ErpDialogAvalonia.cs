using Avalonia.Controls;

using CommunityToolkit.Mvvm.Messaging;

using EgoErpArchiver.ViewModels;
using EgoErpArchiver.Views;

namespace EgoErpArchiver.Dialogs.Erp;

public class ErpDialogAvalonia
{
    public static void Register(Window recipient)
    {
        ErpDialog.Messenger.Register<Window, ProgressDialogMessage>(recipient, ProgressDialogHandler);
    }

    public static void Unregister(Window recipient)
    {
        ErpDialog.Messenger.Unregister<ProgressDialogMessage>(recipient);
    }

    private static void ProgressDialogHandler(Window recipient, ProgressDialogMessage message)
    {
        message.Reply(Handle(recipient, message.ViewModel));
        return;

        static async Task<bool> Handle(Window recipient, ProgressDialogViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(recipient);

            ProgressDialog win = new() { DataContext = viewModel };
            var res = await win.ShowDialog<bool>(recipient);
            return res;
        }
    }
}
