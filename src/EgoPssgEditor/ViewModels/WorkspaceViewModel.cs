using EgoEngineLibrary.Frontend.ViewModels;

namespace EgoPssgEditor.ViewModels
{
    public abstract class WorkspaceViewModel : ViewModelBase
    {
        public MainViewModel MainView { get; set; }

        public abstract void LoadData();

        public abstract void ClearData();
    }
}
