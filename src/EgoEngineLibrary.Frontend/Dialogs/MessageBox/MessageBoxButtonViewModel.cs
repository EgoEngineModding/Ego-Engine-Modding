namespace EgoEngineLibrary.Frontend.Dialogs.MessageBox;

public class MessageBoxButtonViewModel
{
    public string Text { get; init; } = "";
    
    public MessageBoxResult Result { get; init; }
    
    public bool IsDefault { get; init; }
    
    public bool IsCancel { get; init; }
}