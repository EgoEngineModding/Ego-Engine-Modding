namespace EgoEngineLibrary.Avalonia.MessageBox;

/// <summary>Specifies the buttons that are displayed on a message box.</summary>
public enum MessageBoxButton
{
    /// <summary>The message box displays an OK button.</summary>
    OK,
    /// <summary>The message box displays OK and Cancel buttons.</summary>
    OKCancel,
    /// <summary>The message box displays Abort, Retry, and Ignore buttons.</summary>
    AbortRetryIgnore,
    /// <summary>The message box displays Yes, No, and Cancel buttons.</summary>
    YesNoCancel,
    /// <summary>The message box displays Yes and No buttons.</summary>
    YesNo,
    /// <summary>The message box displays Retry and Cancel** buttons.</summary>
    RetryCancel,
    /// <summary>The message box displays Cancel, Retry, and Continue buttons.</summary>
    CancelTryContinue,
}
