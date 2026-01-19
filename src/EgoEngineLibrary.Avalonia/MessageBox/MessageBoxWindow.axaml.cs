using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace EgoEngineLibrary.Avalonia.MessageBox;

internal partial class MessageBoxWindow : Window
{
    private bool _closeByButton;
    
    public string? Message
    {
        get => MessageTextBlock.Text;
        set => MessageTextBlock.Text = value;
    }

    public MessageBoxResult DefaultResult
    {
        get;
        set
        {
            field = value;
            SetDefaultButton();
        }
    }

    public MessageBoxButton Buttons
    {
        get;
        set
        {
            field = value;
            Button cancelButton;
            switch (value)
            {
                case MessageBoxButton.OK:
                    FirstButton.Content = "Ok";
                    SecondButton.IsVisible = false;
                    ThirdButton.IsVisible = false;
                    cancelButton = FirstButton;
                    break;
                case MessageBoxButton.OKCancel:
                    FirstButton.Content = "Ok";
                    SecondButton.Content = "Cancel";
                    ThirdButton.IsVisible = false;
                    cancelButton = SecondButton;
                    break;
                case MessageBoxButton.AbortRetryIgnore:
                    FirstButton.Content = "Abort";
                    SecondButton.Content = "Retry";
                    ThirdButton.Content = "Ignore";
                    cancelButton = FirstButton;
                    break;
                case MessageBoxButton.YesNoCancel:
                    FirstButton.Content = "Yes";
                    SecondButton.Content = "No";
                    ThirdButton.Content = "Cancel";
                    cancelButton = ThirdButton;
                    break;
                case MessageBoxButton.YesNo:
                    FirstButton.Content = "Yes";
                    SecondButton.Content = "No";
                    ThirdButton.IsVisible = false;
                    cancelButton = SecondButton;
                    break;
                case MessageBoxButton.RetryCancel:
                    FirstButton.Content = "Retry";
                    SecondButton.Content = "Cancel";
                    ThirdButton.IsVisible = false;
                    cancelButton = SecondButton;
                    break;
                case MessageBoxButton.CancelTryContinue:
                    FirstButton.Content = "Cancel";
                    SecondButton.Content = "Try Again";
                    ThirdButton.Content = "Continue";
                    cancelButton = FirstButton;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Buttons), value, null);
            }

            FirstButton.IsCancel = false;
            SecondButton.IsCancel = false;
            ThirdButton.IsCancel = false;
            cancelButton.IsCancel = true;
            
            SetDefaultButton();
        }
    }

    public MessageBoxImage ImageIcon
    {
        get;
        set
        {
            field = value;
            Image.IsVisible = true;
            switch (value)
            {
                case MessageBoxImage.None:
                    Image.Data = null;
                    Image.IsVisible = false;
                    break;
                case MessageBoxImage.Error:
                    Image.Data = this.FindResource(this.ActualThemeVariant, "ErrorCircleRegular") as Geometry;
                    Image.Foreground = Brushes.Firebrick;
                    break;
                case MessageBoxImage.Question:
                    Image.Data = this.FindResource(this.ActualThemeVariant, "QuestionCircleRegular") as Geometry;
                    Image.Foreground = Brushes.SteelBlue;
                    break;
                case MessageBoxImage.Exclamation:
                    Image.Data = this.FindResource(this.ActualThemeVariant, "WarningRegular") as Geometry;
                    Image.Foreground = Brushes.Goldenrod;
                    break;
                case MessageBoxImage.Asterisk:
                    Image.Data = this.FindResource(this.ActualThemeVariant, "InfoRegular") as Geometry;
                    Image.Foreground = Brushes.SteelBlue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ImageIcon), value, null);
            }
        }
    }

    public MessageBoxWindow()
    {
        InitializeComponent();
        Buttons = MessageBoxButton.OK;
        ImageIcon = MessageBoxImage.None;
        DefaultResult = MessageBoxResult.None;
    }

    private void SetDefaultButton()
    {
        FirstButton.IsDefault = false;
        SecondButton.IsDefault = false;
        ThirdButton.IsDefault = false;
        
        if (DefaultResult == MessageBoxResult.None)
        {
            FirstButton.IsDefault = true;
            return;
        }

        var defaultButton = Buttons switch
        {
            MessageBoxButton.OK => FirstButton,
            MessageBoxButton.OKCancel => DefaultResult is MessageBoxResult.Cancel ? SecondButton : FirstButton,
            MessageBoxButton.AbortRetryIgnore => DefaultResult switch
            {
                MessageBoxResult.Retry => SecondButton,
                MessageBoxResult.Ignore => ThirdButton,
                _ => FirstButton
            },
            MessageBoxButton.YesNoCancel => DefaultResult switch
            {
                MessageBoxResult.No => SecondButton,
                MessageBoxResult.Cancel => ThirdButton,
                _ => FirstButton
            },
            MessageBoxButton.YesNo => DefaultResult is MessageBoxResult.No ? SecondButton : FirstButton,
            MessageBoxButton.RetryCancel => DefaultResult is MessageBoxResult.Cancel ? SecondButton : FirstButton,
            MessageBoxButton.CancelTryContinue => DefaultResult switch
            {
                MessageBoxResult.TryAgain => SecondButton,
                MessageBoxResult.Continue => ThirdButton,
                _ => FirstButton
            },
            _ => FirstButton
        };

        defaultButton.IsDefault = true;
    }

    private void FirstButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _closeByButton = true;
        switch (Buttons)
        {
            case MessageBoxButton.OK:
                Close(MessageBoxResult.OK);
                break;
            case MessageBoxButton.OKCancel:
                Close(MessageBoxResult.OK);
                break;
            case MessageBoxButton.AbortRetryIgnore:
                Close(MessageBoxResult.Abort);
                break;
            case MessageBoxButton.YesNoCancel:
                Close(MessageBoxResult.Yes);
                break;
            case MessageBoxButton.YesNo:
                Close(MessageBoxResult.Yes);
                break;
            case MessageBoxButton.RetryCancel:
                Close(MessageBoxResult.Retry);
                break;
            case MessageBoxButton.CancelTryContinue:
                Close(MessageBoxResult.Cancel);
                break;
            default:
                throw new InvalidOperationException("First button is not supported.");
        }
    }

    private void SecondButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _closeByButton = true;
        switch (Buttons)
        {
            case MessageBoxButton.OKCancel:
                Close(MessageBoxResult.Cancel);
                break;
            case MessageBoxButton.AbortRetryIgnore:
                Close(MessageBoxResult.Retry);
                break;
            case MessageBoxButton.YesNoCancel:
                Close(MessageBoxResult.No);
                break;
            case MessageBoxButton.YesNo:
                Close(MessageBoxResult.No);
                break;
            case MessageBoxButton.RetryCancel:
                Close(MessageBoxResult.Cancel);
                break;
            case MessageBoxButton.CancelTryContinue:
                Close(MessageBoxResult.TryAgain);
                break;
            case MessageBoxButton.OK:
            default:
                throw new InvalidOperationException("Second button is not supported.");
        }
    }

    private void ThirdButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _closeByButton = true;
        switch (Buttons)
        {
            case MessageBoxButton.AbortRetryIgnore:
                Close(MessageBoxResult.Ignore);
                break;
            case MessageBoxButton.YesNoCancel:
                Close(MessageBoxResult.Cancel);
                break;
            case MessageBoxButton.CancelTryContinue:
                Close(MessageBoxResult.Continue);
                break;
            case MessageBoxButton.OK:
            case MessageBoxButton.OKCancel:
            case MessageBoxButton.YesNo:
            case MessageBoxButton.RetryCancel:
            default:
                throw new InvalidOperationException("Third button is not supported.");
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (_closeByButton)
        {
            base.OnClosing(e);
            return;
        }

        e.Cancel = true;
        var cancelButton = SecondButton.IsCancel ? SecondButton : (ThirdButton.IsCancel ? ThirdButton : FirstButton);
        cancelButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    }
}
