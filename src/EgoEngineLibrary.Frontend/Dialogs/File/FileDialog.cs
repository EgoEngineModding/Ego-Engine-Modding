using CommunityToolkit.Mvvm.Messaging;

namespace EgoEngineLibrary.Frontend.Dialogs.File;

public class FileDialog
{
    public static IMessenger Messenger { get; set; } = Messaging.Messenger.Default;

    public static async Task<IReadOnlyList<string>> ShowOpenFileDialog(FileOpenOptions openOptions)
    {
        return await Messenger.Send(new FileOpenMessage(openOptions));
    }

    public static async Task<string?> ShowSaveFileDialog(FileSaveOptions saveOptions)
    {
        return await Messenger.Send(new FileSaveMessage(saveOptions));
    }

    public static async Task<IReadOnlyList<string>> ShowOpenFolderDialog(FolderOpenOptions openOptions)
    {
        return await Messenger.Send(new FolderOpenMessage(openOptions));
    }
}
