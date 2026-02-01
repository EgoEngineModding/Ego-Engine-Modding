using CommunityToolkit.Mvvm.Messaging.Messages;

namespace EgoPssgEditor.Dialogs.Pssg;

public class DuplicateTextureMessage(DuplicateTextureViewModel viewModel) : AsyncRequestMessage<bool>
{
    public DuplicateTextureViewModel ViewModel { get; } = viewModel;
}
