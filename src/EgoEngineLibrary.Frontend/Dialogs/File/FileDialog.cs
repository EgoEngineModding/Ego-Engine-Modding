namespace EgoEngineLibrary.Frontend.Dialogs.File;

public static class FileDialog
{
    public static Task<IReadOnlyList<string>> ShowOpenFileDialog(FileOpenOptions openOptions)
    {
        return DialogService.Instance.ShowOpenFileDialog(openOptions);
    }

    public static async Task<IReadOnlyList<string>> ShowOpenFileDialog(this IDialogService dialogService, FileOpenOptions openOptions)
    {
        var vm = new FileOpenViewModel(openOptions);
        await dialogService.ShowDialogAsync(vm);
        return vm.Result;
    }

    public static Task<string?> ShowSaveFileDialog(FileSaveOptions saveOptions)
    {
        return DialogService.Instance.ShowSaveFileDialog(saveOptions);
    }

    public static async Task<string?> ShowSaveFileDialog(this IDialogService dialogService, FileSaveOptions saveOptions)
    {
        var vm = new FileSaveViewModel(saveOptions);
        await dialogService.ShowDialogAsync(vm);
        return vm.Result;
    }

    public static Task<IReadOnlyList<string>> ShowOpenFolderDialog(FolderOpenOptions openOptions)
    {
        return DialogService.Instance.ShowOpenFolderDialog(openOptions);
    }

    public static async Task<IReadOnlyList<string>> ShowOpenFolderDialog(this IDialogService dialogService, FolderOpenOptions openOptions)
    {
        var vm = new FolderOpenViewModel(openOptions);
        await dialogService.ShowDialogAsync(vm);
        return vm.Result;
    }
}
