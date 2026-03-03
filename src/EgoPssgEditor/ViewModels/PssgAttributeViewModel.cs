using EgoEngineLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EgoEngineLibrary.Graphics.Pssg;

namespace EgoPssgEditor.ViewModels
{
    public class PssgAttributeViewModel : ViewModelBase
    {
        readonly PssgAttribute attribute;
        readonly PssgElementViewModel parent;

        public PssgAttribute Attribute
        {
            get { return attribute; }
        }
        public PssgElementViewModel Parent
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

        public PssgAttributeViewModel(PssgAttribute attribute, PssgElementViewModel parent)
        {
            this.attribute = attribute;
            this.parent = parent;
        }
    }
}
