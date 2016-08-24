using EgoEngineLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoPssgEditor.ViewModel
{
    public class PssgAttributeViewModel : ViewModelBase
    {
        readonly PssgAttribute attribute;
        readonly PssgNodeViewModel parent;

        public PssgAttribute Attribute
        {
            get { return attribute; }
        }
        public PssgNodeViewModel Parent
        {
            get { return parent; }
        }

        public override string DisplayName
        {
            get { return attribute.Name; }
        }
        public string DisplayValue
        {
            get { return attribute.DisplayValue; }
            set { attribute.DisplayValue = value; }
        }

        public PssgAttributeViewModel(PssgAttribute attribute, PssgNodeViewModel parent)
        {
            this.attribute = attribute;
            this.parent = parent;
        }
    }
}
