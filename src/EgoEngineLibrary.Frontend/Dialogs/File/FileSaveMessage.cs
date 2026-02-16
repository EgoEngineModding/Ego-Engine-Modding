using CommunityToolkit.Mvvm.Messaging.Messages;

namespace EgoEngineLibrary.Frontend.Dialogs.File;

public class FileSaveMessage : AsyncRequestMessage<string?>
{
    public FileSaveOptions Options { get; }

    public FileSaveMessage(FileSaveOptions options)
    {
        Options = options;
    }
}
