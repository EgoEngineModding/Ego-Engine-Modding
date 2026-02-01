using CommunityToolkit.Mvvm.Messaging.Messages;

namespace EgoEngineLibrary.Frontend.Dialogs.MessageBox;

public class MessageBoxShowMessage : AsyncRequestMessage<MessageBoxResult>
{
    public string MessageBoxText { get; }

    public string Caption { get; }
    
    public MessageBoxButton Button { get; }

    public MessageBoxImage Icon { get; }

    public MessageBoxResult DefaultResult { get; }

    public MessageBoxShowMessage(
        string messageBoxText,
        string caption,
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None)
    {
        MessageBoxText = messageBoxText;
        Caption = caption;
        Button = button;
        Icon = icon;
        DefaultResult = defaultResult;
    }
}
