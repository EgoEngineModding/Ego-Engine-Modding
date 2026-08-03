namespace EgoEngineLibrary.Frontend.Dialogs.MessageBox;

public static class MessageBox
{
    public static Task<MessageBoxResult> Show(
        string messageBoxText,
        string caption,
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None)
    {
        return DialogService.Instance.ShowMessageBox(messageBoxText, caption, button, icon, defaultResult);
    }

    public static async Task<MessageBoxResult> ShowMessageBox(
        this IDialogService dialogService,
        string messageBoxText,
        string caption,
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None)
    {
        var vm = new MessageBoxViewModel(messageBoxText, caption, button, icon, defaultResult);
        await dialogService.ShowDialogAsync(vm);
        return vm.Result;
    }
}
