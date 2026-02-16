using CommunityToolkit.Mvvm.Messaging;

namespace EgoEngineLibrary.Frontend.Dialogs.MessageBox;

public static class MessageBox
{
    public static IMessenger Messenger { get; set; } = Messaging.Messenger.Default;
    
    public static async Task<MessageBoxResult> Show(
        string messageBoxText,
        string caption,
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None)
    {
        return await Messenger.Send(new MessageBoxShowMessage(messageBoxText, caption, button, icon, defaultResult));
    }
}
