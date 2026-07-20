using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using EgoEngineLibrary.Frontend.ViewModels;

namespace EgoEngineLibrary.Frontend.Dialogs.MessageBox;

public partial class MessageBoxViewModel : ViewModelBase, IDialogViewModel
{
    public string MessageBoxText { get; }

    public string Title { get; }

    public MessageBoxImage Icon { get; }

    public MessageBoxResult Result { get; private set; }
    
    public MessageBoxButtonViewModel[] Buttons { get; }

    public IDialogContext? DialogContext { get; set; }

    public MessageBoxViewModel(
        string messageBoxText,
        string caption,
        MessageBoxButton button,
        MessageBoxImage icon,
        MessageBoxResult defaultResult)
    {
        MessageBoxText = messageBoxText;
        Title = caption;
        Icon = icon;

        Buttons = button switch
        {
            MessageBoxButton.OK =>
            [
                new MessageBoxButtonViewModel
                {
                    Text = "Ok",
                    Result = MessageBoxResult.OK,
                    IsDefault = true,
                    IsCancel = true,
                },
            ],
            MessageBoxButton.OKCancel =>
            [
                new MessageBoxButtonViewModel
                {
                    Text = "Ok",
                    Result = MessageBoxResult.OK,
                    IsDefault = defaultResult is not MessageBoxResult.Cancel,
                    IsCancel = false,
                },
                new MessageBoxButtonViewModel
                {
                    Text = "Cancel",
                    Result = MessageBoxResult.Cancel,
                    IsDefault = defaultResult is MessageBoxResult.Cancel,
                    IsCancel = true,
                },
            ],
            MessageBoxButton.AbortRetryIgnore =>
            [
                new MessageBoxButtonViewModel
                {
                    Text = "Abort",
                    Result = MessageBoxResult.Abort,
                    IsDefault = defaultResult is not MessageBoxResult.Retry and not MessageBoxResult.Ignore,
                    IsCancel = true,
                },
                new MessageBoxButtonViewModel
                {
                    Text = "Retry",
                    Result = MessageBoxResult.Retry,
                    IsDefault = defaultResult is MessageBoxResult.Retry,
                    IsCancel = false,
                },
                new MessageBoxButtonViewModel
                {
                    Text = "Ignore",
                    Result = MessageBoxResult.Ignore,
                    IsDefault = defaultResult is MessageBoxResult.Ignore,
                    IsCancel = false,
                },
            ],
            MessageBoxButton.YesNoCancel =>
            [
                new MessageBoxButtonViewModel
                {
                    Text = "Yes",
                    Result = MessageBoxResult.Yes,
                    IsDefault = defaultResult is not MessageBoxResult.No and not MessageBoxResult.Cancel,
                    IsCancel = false,
                },
                new MessageBoxButtonViewModel
                {
                    Text = "No",
                    Result = MessageBoxResult.No,
                    IsDefault = defaultResult is MessageBoxResult.No,
                    IsCancel = false,
                },
                new MessageBoxButtonViewModel
                {
                    Text = "Cancel",
                    Result = MessageBoxResult.Cancel,
                    IsDefault = defaultResult is MessageBoxResult.Cancel,
                    IsCancel = true,
                },
            ],
            MessageBoxButton.YesNo =>
            [
                new MessageBoxButtonViewModel
                {
                    Text = "Yes",
                    Result = MessageBoxResult.Yes,
                    IsDefault = defaultResult is not MessageBoxResult.No,
                    IsCancel = false,
                },
                new MessageBoxButtonViewModel
                {
                    Text = "No",
                    Result = MessageBoxResult.No,
                    IsDefault = defaultResult is MessageBoxResult.No,
                    IsCancel = true,
                },
            ],
            MessageBoxButton.RetryCancel =>
            [
                new MessageBoxButtonViewModel
                {
                    Text = "Retry",
                    Result = MessageBoxResult.Retry,
                    IsDefault = defaultResult is not MessageBoxResult.Cancel,
                    IsCancel = false,
                },
                new MessageBoxButtonViewModel
                {
                    Text = "Cancel",
                    Result = MessageBoxResult.Cancel,
                    IsDefault = defaultResult is MessageBoxResult.Cancel,
                    IsCancel = true,
                },
            ],
            MessageBoxButton.CancelTryContinue =>
            [
                new MessageBoxButtonViewModel
                {
                    Text = "Cancel",
                    Result = MessageBoxResult.Cancel,
                    IsDefault = defaultResult is not MessageBoxResult.TryAgain and not MessageBoxResult.Continue,
                    IsCancel = true,
                },
                new MessageBoxButtonViewModel
                {
                    Text = "Try Again",
                    Result = MessageBoxResult.TryAgain,
                    IsDefault = defaultResult is MessageBoxResult.TryAgain,
                    IsCancel = false,
                },
                new MessageBoxButtonViewModel
                {
                    Text = "Continue",
                    Result = MessageBoxResult.Continue,
                    IsDefault = defaultResult is MessageBoxResult.Continue,
                    IsCancel = false,
                },
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
        };

        Debug.Assert(Buttons.Count(x => x.IsDefault) == 1);
        Debug.Assert(Buttons.Count(x => x.IsCancel) == 1);
        Result = Buttons.First(x => x.IsCancel).Result;
    }

    [RelayCommand]
    private void ButtonClick(MessageBoxButtonViewModel buttonViewModel)
    {
        Result = buttonViewModel.Result;
        // context result unused
        DialogContext?.Close();
    }
}