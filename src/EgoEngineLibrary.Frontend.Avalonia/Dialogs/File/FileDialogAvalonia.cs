using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

using CommunityToolkit.Mvvm.Messaging;

namespace EgoEngineLibrary.Frontend.Dialogs.File;

public static class FileDialogAvalonia
{
    public static void Register(Visual recipient)
    {
        FileDialog.Messenger.Register<Visual, FileOpenMessage>(recipient, FileOpenHandler);
        FileDialog.Messenger.Register<Visual, FileSaveMessage>(recipient, FileSaveHandler);
    }

    public static void Unregister(Visual recipient)
    {
        FileDialog.Messenger.Unregister<FileOpenMessage>(recipient);
        FileDialog.Messenger.Unregister<FileSaveMessage>(recipient);
    }

    private static void FileOpenHandler(Visual recipient, FileOpenMessage message)
    {
        message.Reply(FileOpen(recipient, message));
    }

    private static async Task<IReadOnlyList<string>> FileOpen(Visual recipient, FileOpenMessage message)
    {
        // Get a reference to our TopLevel (in our case the parent Window)
        var topLevel = TopLevel.GetTopLevel(recipient);
        if (topLevel is null)
        {
            return [];
        }

        var openOptions = message.Options;
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
        return storageFiles.Select(x => x.Path.LocalPath).ToArray();
    }

    private static void FileSaveHandler(Visual recipient, FileSaveMessage message)
    {
        message.Reply(FileSave(recipient, message));
    }

    private static async Task<string?> FileSave(Visual recipient, FileSaveMessage message)
    {
        // Get a reference to our TopLevel (in our case the parent Window)
        var topLevel = TopLevel.GetTopLevel(recipient);
        if (topLevel is null)
        {
            return null;
        }

        var saveOptions = message.Options;
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
        return storageFiles?.Path.LocalPath;
    }
}
