using Avalonia.Controls;

using CommunityToolkit.Mvvm.Messaging;

namespace EgoEngineLibrary.Frontend.Dialogs.MessageBox;

public static class MessageBoxAvalonia
{
    public static void Register(Window recipient)
    {
        MessageBox.Messenger.Register<Window, MessageBoxShowMessage>(recipient, MessageBoxShowHandler);
    }

    public static void Unregister(Window recipient)
    {
        MessageBox.Messenger.Unregister<MessageBoxShowMessage>(recipient);
    }

    private static void MessageBoxShowHandler(Window recipient, MessageBoxShowMessage message)
    {
        message.Reply(Show(recipient, message.MessageBoxText, message.Caption, message.Button, message.Icon,
            message.DefaultResult));
    }

    private static Task<MessageBoxResult> Show(
        Window owner,
        string messageBoxText,
        string caption,
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None)
    {
        ArgumentNullException.ThrowIfNull(owner);

        MessageBoxWindow box = new()
        {
            MinWidth = 114,
            MinHeight = 94,
            MaxWidth = owner.Width,
            MaxHeight = owner.Height,
            Title = caption,
            Message = messageBoxText,
            Buttons = button,
            ImageIcon = icon,
            DefaultResult = defaultResult
        };
        return box.ShowDialog<MessageBoxResult>(owner);
    }
}
