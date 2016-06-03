using EgoEngineLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoPssgEditor.ViewModel
{
    public abstract class WorkspaceViewModel : ViewModelBase
    {
        protected readonly MainViewModel mainView;

        public WorkspaceViewModel(MainViewModel mainView)
        {
            this.mainView = mainView;
        }

        public abstract void LoadData(PssgFile file);

        public abstract void ClearData();
    }
}
