namespace EgoEngineLibrary.Frontend.Dialogs.MessageBox;

public class MessageBoxButtonViewModel
{
    public static readonly MessageBoxButtonViewModel Ok = new MessageBoxButtonViewModel()
    {
        Text = "Ok",
        Result = MessageBoxResult.OK,
        IsDefault = true,
        IsCancel = false,
    };

    public string Text { get; init; } = "";
    
    public MessageBoxResult Result { get; init; }
    
    public bool IsDefault { get; init; }
    
    public bool IsCancel { get; init; }
}