using CommunityToolkit.Mvvm.Messaging.Messages;

namespace EgoEngineLibrary.Frontend.Dialogs.Custom;

public class DialogShowMessage(DialogViewModel viewModel) : AsyncRequestMessage<bool>
{
    public DialogViewModel ViewModel { get; } = viewModel;
}