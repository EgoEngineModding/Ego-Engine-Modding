using EgoEngineLibrary.Frontend.ViewModels;

namespace EgoErpArchiver.ViewModels
{
    public abstract class TreeNodeViewModel : ViewModelBase
    {
        public virtual string ResourceType
        {
            get { return null; }
        }

        public virtual ulong? Size
        {
            get { return null; }
        }

        public virtual ulong? PackedSize
        {
            get { return null; }
        }

        public virtual string FullPath
        {
            get { return null; }
        }

        public virtual void UpdateSize()
        {
            OnPropertyChanged("Size");
            OnPropertyChanged("PackedSize");
        }
    }
}
