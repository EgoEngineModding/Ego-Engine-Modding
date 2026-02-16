using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace EgoEngineLibrary.Frontend;

public static class WindowExtensions
{
    public static Task ShowDialog(this Window window)
    {
        return ShowDialog<object>(window);
    }
    
    public static Task<TResult> ShowDialog<TResult>(this Window window)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return window.ShowDialog<TResult>(desktop.MainWindow!);
        }

        throw new InvalidOperationException("Operation not supported on current application lifetime.");
    }
}
