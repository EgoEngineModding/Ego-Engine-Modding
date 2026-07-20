namespace EgoEngineLibrary.Frontend.Dialogs;

public static class DialogService
{
    public static IDialogService Instance
    {
        get => field ?? throw new InvalidOperationException("No dialog service configured.");
        set;
    }
}