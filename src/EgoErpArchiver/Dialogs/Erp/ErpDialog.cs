using CommunityToolkit.Mvvm.Messaging;

using EgoErpArchiver.ViewModels;

namespace EgoErpArchiver.Dialogs.Erp;

public class ErpDialog
{
    public static IMessenger Messenger { get; set; } = EgoEngineLibrary.Frontend.Messaging.Messenger.Default;

    public static async Task ShowProgressDialog(ProgressDialogViewModel viewModel)
    {
        await Messenger.Send(new ProgressDialogMessage(viewModel));
    }
}
