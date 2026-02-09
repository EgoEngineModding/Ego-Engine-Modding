using CommunityToolkit.Mvvm.Input;
using EgoEngineLibrary.Frontend.Configuration;
using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.ViewModels;
using EgoErpArchiver.Configuration;

namespace EgoErpArchiver.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IWriteableOptions<SettingsConfig> _settingsOptions;
    
    public string F1Directory
    {
        get => _settingsOptions.Value.F1Directory;
        set
        {
            _settingsOptions.Value.F1Directory = value;
            OnPropertyChanged();
            SafeSave();
        }
    }

    public int StartingTab
    {
        get => _settingsOptions.Value.StartingTab;
        set
        {
            _settingsOptions.Value.StartingTab = value;
            OnPropertyChanged();
            SafeSave();
        }
    }

    public SettingsViewModel(IWriteableOptions<SettingsConfig> settingsOptions)
    {
        _settingsOptions = settingsOptions;
    }

    [RelayCommand]
    private async Task SetF1Directory()
    {
        var options = new FolderOpenOptions { Title = "Select the location of your game:", AllowMultiple = false, };

        var res = await FileDialog.ShowOpenFolderDialog(options);
        if (res.Count <= 0)
        {
            return;
        }

        F1Directory = res[0];
    }

    private void SafeSave()
    {
        try
        {
            _settingsOptions.Save();
        }
        catch
        {
        }
    }
}
