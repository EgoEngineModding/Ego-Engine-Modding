namespace EgoEngineLibrary.Avalonia.MessageBox;

/// <summary>Specifies which message box button that a user clicks.</summary>
public enum MessageBoxResult
{
    /// <summary>The message box returns no result.</summary>
    None = 0,
    /// <summary>The result value of the message box is OK.</summary>
    OK = 1,
    /// <summary>The result value of the message box is Cancel.</summary>
    Cancel = 2,
    /// <summary>The result value of the message box is Abort (usually sent from a button labeled Abort).</summary>
    Abort = 3,
    /// <summary>The result value of the message box is Retry (usually sent from a button labeled Retry).</summary>
    Retry = 4,
    /// <summary>The result value of the message box is Ignore (usually sent from a button labeled Ignore).</summary>
    Ignore = 5,
    /// <summary>The result value of the message box is Yes.</summary>
    Yes = 6,
    /// <summary>The result value of the message box is No.</summary>
    No = 7,
    /// <summary>The result value of the message box is TryAgain (usually sent from a button labeled Try Again).</summary>
    TryAgain = 10,
    /// <summary>The result value of the message box is Continue (usually sent from a button labeled Continue).</summary>
    Continue = 11
}
