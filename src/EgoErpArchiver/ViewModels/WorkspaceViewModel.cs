namespace EgoErpArchiver.ViewModels
{
    public abstract class WorkspaceViewModel : ViewModelBase
    {
        protected readonly MainViewModel mainView;

        public WorkspaceViewModel(MainViewModel mainView)
        {
            this.mainView = mainView;
        }

        public abstract void LoadData(object data);

        public abstract void ClearData();
    }
}
