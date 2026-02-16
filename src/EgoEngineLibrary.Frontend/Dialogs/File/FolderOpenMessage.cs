using CommunityToolkit.Mvvm.Messaging.Messages;

namespace EgoEngineLibrary.Frontend.Dialogs.File;

public class FolderOpenMessage : AsyncRequestMessage<IReadOnlyList<string>>
{
    public FolderOpenOptions Options { get; }

    public FolderOpenMessage(FolderOpenOptions options)
    {
        Options = options;
    }
}
