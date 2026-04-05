using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.Input;
using EgoEngineLibrary.Frontend.Dialogs.Custom;
using EgoEngineLibrary.Graphics.Pssg;

namespace EgoPssgEditor.ViewModels;

public partial class AddAttributeViewModel : DialogViewModel<bool>
{
    private readonly PssgSchemaElement _schemaElement;

    public override string Title => "Add Attribute";

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
            ValidateProperty(Value, nameof(Value));
            OkCommand.NotifyCanExecuteChanged();
        }
    }
    
    public bool CanModifyType => SelectedSchemaAttribute is null;

    [Required]
    [CustomValidation(typeof(AddAttributeViewModel), nameof(ValidateValue))]
    public string Value
    {
        get;
        set
        {
            SetProperty(ref field, value, true);
            OkCommand.NotifyCanExecuteChanged();
        }
    }

    [Required]
    [MinLength(1)]
    public string AttributeName
    {
        get;
        set
        {
            SetProperty(ref field, value, true);
        }
    }

    public AddAttributeViewModel(PssgSchemaElement schemaElement)
    {
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

    public static ValidationResult? ValidateValue(string value, ValidationContext context)
    {
        AddAttributeViewModel instance = (AddAttributeViewModel)context.ObjectInstance;
        try
        {
            _ = value.ToPssgValue(instance.SelectedAttributeType);
            return ValidationResult.Success;
        }
        catch
        {
            return new ValidationResult("The value could not be converted to the selected data type.");
        }
    }

    [RelayCommand(CanExecute = nameof(OkCanExecute))]
    private void Ok()
    {
        SetDialogResult(true);
    }
    private bool OkCanExecute()
    {
        return !HasErrors;
    }

    [RelayCommand]
    private void Cancel()
    {
        SetDialogResult(false);
    }
}