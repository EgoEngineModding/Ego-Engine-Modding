using CommunityToolkit.Mvvm.Messaging;

namespace EgoPssgEditor.Dialogs.Pssg;

public class PssgDialog
{
    public static IMessenger Messenger { get; set; } = EgoEngineLibrary.Frontend.Messaging.Messenger.Default;
}
