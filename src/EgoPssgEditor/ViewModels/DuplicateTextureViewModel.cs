using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.Input;
using EgoEngineLibrary.Frontend.Dialogs.Custom;

namespace EgoPssgEditor.ViewModels;

public partial class DuplicateTextureViewModel : DialogViewModel<bool>
{
    public override string Title => "Duplicate Texture";

    [Required]
    [MinLength(1)]
    public string TextureName
    {
        get;
        set
        {
            SetProperty(ref field, value, true);
            OkCommand.NotifyCanExecuteChanged();
        }
    }

    public DuplicateTextureViewModel()
    {
        TextureName = string.Empty;
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