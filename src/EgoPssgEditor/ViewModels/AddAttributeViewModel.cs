using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using EgoEngineLibrary.Frontend.Dialogs;
using EgoEngineLibrary.Frontend.ViewModels;
using EgoEngineLibrary.Graphics.Pssg;
using FluentValidation;

namespace EgoPssgEditor.ViewModels;

public partial class AddAttributeViewModel : ValidatableViewModelBase<AddAttributeViewModel>, IDialogViewModel
{
    private readonly PssgSchemaElement _schemaElement;

    public string Title => "Add Attribute";
    
    public IDialogContext? DialogContext { get; set; }

    public ObservableCollection<PssgAttributeType> AttributeTypes { get; }
    
    public ObservableCollection<PssgSchemaAttribute> Attributes { get; }

    public PssgSchemaAttribute? SelectedSchemaAttribute
    {
        get;
        set
        {
            SetProperty(ref field, value);
            SelectedAttributeType = value?.DataType ?? PssgAttributeType.String;
            OnPropertyChanged(nameof(CanModifyType));
        }
    }

    public PssgAttributeType SelectedAttributeType
    {
        get;
        set 
        {
            SetProperty(ref field, value);
            ValidateProperty(x => x.Value);
            OkCommand.NotifyCanExecuteChanged();
        }
    }
    
    public bool CanModifyType => SelectedSchemaAttribute is null;

    public string Value
    {
        get;
        set
        {
            SetProperty(ref field, value);
            ValidateProperty(x => x.Value);
            OkCommand.NotifyCanExecuteChanged();
        }
    }

    public string AttributeName
    {
        get;
        set
        {
            SetProperty(ref field, value);
            ValidateProperty(x => x.AttributeName);
        }
    }

    protected override IValidator<AddAttributeViewModel> Validator { get; }

    public AddAttributeViewModel(PssgSchemaElement schemaElement)
    {
        Validator = new ClassValidator();
        _schemaElement = schemaElement;
        AttributeTypes = new ObservableCollection<PssgAttributeType>(Enum.GetValues<PssgAttributeType>());
        Attributes = new ObservableCollection<PssgSchemaAttribute>(GetAllAttributes());
        SelectedAttributeType = PssgAttributeType.String;
        SelectedSchemaAttribute = Attributes.Count > 0 ? Attributes[0] : null;
        Value = string.Empty;
        AttributeName = SelectedSchemaAttribute?.Name ?? string.Empty;
    }

    private IEnumerable<PssgSchemaAttribute> GetAllAttributes()
    {
        PssgSchemaElement? element = _schemaElement;
        while (element is not null)
        {
            foreach (PssgSchemaAttribute attribute in element.Attributes)
            {
                yield return attribute;
            }

            element = element.BaseElement;
        }
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

    private class ClassValidator : AbstractValidator<AddAttributeViewModel>
    {
        public ClassValidator()
        {
            RuleFor(x => x.Value).NotNull().Custom(ValidateValue);
            RuleFor(x => x.AttributeName).NotNull().MinimumLength(1);
        }
        
        private static void ValidateValue(string value, ValidationContext<AddAttributeViewModel> ctx)
        {
            AddAttributeViewModel instance = ctx.InstanceToValidate;
            try
            {
                _ = value.ToPssgValue(instance.SelectedAttributeType);
            }
            catch
            {
                ctx.AddFailure("The value could not be converted to the selected data type.");
            }
        }
    }
}