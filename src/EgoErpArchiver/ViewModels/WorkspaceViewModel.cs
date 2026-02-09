using EgoEngineLibrary.Frontend.ViewModels;

namespace EgoErpArchiver.ViewModels;

public abstract class WorkspaceViewModel : ViewModelBase
{
    public abstract void OnFileOpened();

    public abstract void OnFileClosed();
}
