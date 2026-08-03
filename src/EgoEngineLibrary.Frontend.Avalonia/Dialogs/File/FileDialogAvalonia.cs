using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace EgoEngineLibrary.Frontend.Dialogs.File;

internal static class FileDialogAvalonia
{
    public static async Task FileOpen(Visual recipient, FileOpenViewModel viewModel)
    {
        // Get a reference to our TopLevel (in our case the parent Window)
        var topLevel = TopLevel.GetTopLevel(recipient);
        if (topLevel is null)
        {
            return;
        }

        var openOptions = viewModel.Options;
        var options = new FilePickerOpenOptions
        {
            Title = openOptions.Title,
            FileTypeFilter = openOptions.FileTypeChoices?.Select(x => x.ToFilePickerType()).ToArray(),
            SuggestedFileType = openOptions.SuggestedFileType?.ToFilePickerType(),
            AllowMultiple = openOptions.AllowMultiple,
            SuggestedFileName = openOptions.FileName,
            SuggestedStartLocation = openOptions.InitialDirectory is null
                ? null
                : await topLevel.StorageProvider.TryGetFolderFromPathAsync(openOptions.InitialDirectory),
        };

        var storageFiles = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        viewModel.Result = storageFiles.Select(x => x.Path.LocalPath).ToArray();
    }

    public static async Task FileSave(Visual recipient, FileSaveViewModel viewModel)
    {
        // Get a reference to our TopLevel (in our case the parent Window)
        var topLevel = TopLevel.GetTopLevel(recipient);
        if (topLevel is null)
        {
            return;
        }

        var saveOptions = viewModel.Options;
        var options = new FilePickerSaveOptions
        {
            Title = saveOptions.Title,
            FileTypeChoices = saveOptions.FileTypeChoices?.Select(x => x.ToFilePickerType()).ToArray(),
            SuggestedFileType = saveOptions.SuggestedFileType?.ToFilePickerType(),
            DefaultExtension = saveOptions.DefaultExtension,
            ShowOverwritePrompt = saveOptions.ShowOverwritePrompt,
            SuggestedFileName = saveOptions.FileName,
            SuggestedStartLocation = saveOptions.InitialDirectory is null
                ? null
                : await topLevel.StorageProvider.TryGetFolderFromPathAsync(saveOptions.InitialDirectory),
        };

        var storageFiles = await topLevel.StorageProvider.SaveFilePickerAsync(options);
        viewModel.Result = storageFiles?.Path.LocalPath;
    }

    public static async Task FolderOpen(Visual recipient, FolderOpenViewModel viewModel)
    {
        // Get a reference to our TopLevel (in our case the parent Window)
        var topLevel = TopLevel.GetTopLevel(recipient);
        if (topLevel is null)
        {
            return;
        }

        var openOptions = viewModel.Options;
        var options = new FolderPickerOpenOptions
        {
            Title = openOptions.Title,
            AllowMultiple = openOptions.AllowMultiple,
            SuggestedFileName = openOptions.FileName,
            SuggestedStartLocation = openOptions.InitialDirectory is null
                ? null
                : await topLevel.StorageProvider.TryGetFolderFromPathAsync(openOptions.InitialDirectory),
        };

        var storageFiles = await topLevel.StorageProvider.OpenFolderPickerAsync(options);
        viewModel.Result = storageFiles.Select(x => x.Path.LocalPath).ToArray();
    }
}
