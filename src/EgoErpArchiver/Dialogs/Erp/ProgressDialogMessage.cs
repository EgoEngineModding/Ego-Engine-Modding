using CommunityToolkit.Mvvm.Messaging.Messages;

using EgoErpArchiver.ViewModels;

namespace EgoErpArchiver.Dialogs.Erp;

public class ProgressDialogMessage(ProgressDialogViewModel viewModel) : AsyncRequestMessage<bool>
{
    public ProgressDialogViewModel ViewModel { get; } = viewModel;
}
