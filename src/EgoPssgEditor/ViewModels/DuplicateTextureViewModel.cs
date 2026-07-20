using CommunityToolkit.Mvvm.Input;
using EgoEngineLibrary.Frontend.Dialogs;
using EgoEngineLibrary.Frontend.ViewModels;
using FluentValidation;

namespace EgoPssgEditor.ViewModels;

public partial class DuplicateTextureViewModel : ValidatableViewModelBase<DuplicateTextureViewModel>, IDialogViewModel
{
    public string Title => "Duplicate Texture";

    public IDialogContext? DialogContext { get; set; }

    public string TextureName
    {
        get;
        set
        {
            SetProperty(ref field, value);
            ValidateProperty(x => x.TextureName);
            OkCommand.NotifyCanExecuteChanged();
        }
    }

    protected override IValidator<DuplicateTextureViewModel> Validator { get; }

    public DuplicateTextureViewModel()
    {
        Validator = new ClassValidator();
        TextureName = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(OkCanExecute))]
    private void Ok()
    {
        DialogContext?.Close();
    }

    private bool OkCanExecute()
    {
        return !HasErrors;
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogContext?.Close(false);
    }

    private class ClassValidator : AbstractValidator<DuplicateTextureViewModel>
    {
        public ClassValidator()
        {
            RuleFor(x => x.TextureName).NotNull().MinimumLength(1);
        }
    }
}