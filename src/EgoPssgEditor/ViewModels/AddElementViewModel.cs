using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.Input;
using EgoEngineLibrary.Frontend.Dialogs.Custom;
using EgoEngineLibrary.Graphics.Pssg;

namespace EgoPssgEditor.ViewModels;

public partial class AddElementViewModel : DialogViewModel<bool>
{
    public override string Title => "Add Element";
    
    public ObservableCollection<string> Elements { get; }

    [Required]
    [MinLength(1)]
    public string ElementName
    {
        get;
        set
        {
            SetProperty(ref field, value, true);
            OkCommand.NotifyCanExecuteChanged();
        }
    }

    public AddElementViewModel()
    {
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