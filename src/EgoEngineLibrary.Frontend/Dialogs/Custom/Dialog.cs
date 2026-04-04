using CommunityToolkit.Mvvm.Messaging;

namespace EgoEngineLibrary.Frontend.Dialogs.Custom;

public static class Dialog
{
    public static IMessenger Messenger { get; set; } = Messaging.Messenger.Default;

    public static async Task<T?> ShowDialog<T>(DialogViewModel<T> viewModel)
    {
        viewModel.ResetResult();
        await Messenger.Send(new DialogShowMessage(viewModel));
        return viewModel.DialogResult;
    }
}