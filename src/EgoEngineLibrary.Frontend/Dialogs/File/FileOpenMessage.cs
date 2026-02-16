using CommunityToolkit.Mvvm.Messaging.Messages;

namespace EgoEngineLibrary.Frontend.Dialogs.File;

public class FileOpenMessage : AsyncRequestMessage<IReadOnlyList<string>>
{
    public FileOpenOptions Options { get; }

    public FileOpenMessage(FileOpenOptions options)
    {
        Options = options;
    }
}
