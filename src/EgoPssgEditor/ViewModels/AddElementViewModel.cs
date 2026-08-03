using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using EgoEngineLibrary.Frontend.Dialogs;
using EgoEngineLibrary.Frontend.ViewModels;
using EgoEngineLibrary.Graphics.Pssg;
using FluentValidation;

namespace EgoPssgEditor.ViewModels;

public partial class AddElementViewModel : ValidatableViewModelBase<AddElementViewModel>, IDialogViewModel
{
    public string Title => "Add Element";
    
    public IDialogContext? DialogContext { get; set; }
    
    public ObservableCollection<string> Elements { get; }

    public string ElementName
    {
        get;
        set
        {
            SetProperty(ref field, value);
            ValidateProperty(x => x.ElementName);
            OkCommand.NotifyCanExecuteChanged();
        }
    }

    protected override IValidator<AddElementViewModel> Validator { get; }

    public AddElementViewModel()
    {
        Validator = new ClassValidator();
        Elements = new ObservableCollection<string>(GetAllElements());
        ElementName = string.Empty;
    }

    private IEnumerable<string> GetAllElements()
    {
        return PssgSchema.GetElementNames();
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

    private class ClassValidator : AbstractValidator<AddElementViewModel>
    {
        public ClassValidator()
        {
            RuleFor(x => x.ElementName).NotNull().MinimumLength(1);
        }
    }
}