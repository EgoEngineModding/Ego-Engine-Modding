using CommunityToolkit.Mvvm.Messaging;

namespace EgoPssgEditor.Dialogs.Pssg;

public class PssgDialog
{
    public static IMessenger Messenger { get; set; } = EgoEngineLibrary.Frontend.Messaging.Messenger.Default;
    
    public static async Task<string?> ShowAddElementDialog()
    {
        return await Messenger.Send(new AddElementMessage());
    }
    
    public static async Task<AddAttributeResponse?> ShowAddAttributeDialog()
    {
        return await Messenger.Send(new AddAttributeMessage());
    }

    public static async Task<bool> ShowDuplicateTextureDialog(DuplicateTextureViewModel viewModel)
    {
        return await Messenger.Send(new DuplicateTextureMessage(viewModel));
    }
}
