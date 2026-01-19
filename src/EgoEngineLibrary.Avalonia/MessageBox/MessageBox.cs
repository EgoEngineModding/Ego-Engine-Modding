using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace EgoEngineLibrary.Avalonia.MessageBox;

public sealed class MessageBox
{
    public static Task<MessageBoxResult> Show(
        string messageBoxText,
        string caption,
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return Show(desktop.MainWindow!, messageBoxText, caption, button, icon, defaultResult);
        }
        else if (Application.Current?.ApplicationLifetime is null)
        {
            return Task.FromResult(MessageBoxResult.None);
        }

        throw new InvalidOperationException("Operation not supported on current application lifetime.");
    }

    public static Task<MessageBoxResult> Show(
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
        return box.ShowDialog<MessageBoxResult>();
    }
}
