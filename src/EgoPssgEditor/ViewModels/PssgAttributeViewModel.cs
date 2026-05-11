using EgoEngineLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EgoEngineLibrary.Graphics.Pssg;
using EgoEngineLibrary.Graphics.Pssg.Elements;

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

        public PssgObject? TryGetLinkedObject()
        {
            if (Attribute.SchemaAttribute.DataType is not PssgAttributeType.String)
            {
                return null;
            }

            var str = (string)Attribute.Value;
            var linkIndex = str.IndexOf('#');
            if (linkIndex != 0)
            {
                return null;
            }

            var objectId = str.AsMemory(1);
            var obj = Attribute.ParentElement.File.TryGetObject<PssgObject>(objectId);
            return obj;
        }
    }
}
