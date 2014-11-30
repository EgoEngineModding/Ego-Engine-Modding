namespace EgoEngineLibrary.Graphics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class PssgAttributeCollection : IEnumerable<PssgAttribute>
    {
        private List<PssgAttribute> attributes;

        public PssgAttributeCollection()
        {
            attributes = new List<PssgAttribute>();
        }
        public PssgAttributeCollection(int capacity)
        {
            attributes = new List<PssgAttribute>(capacity);
        }

        public PssgAttribute this[string attributeName]
        {
            get
            {
                for (int i = 0; i < attributes.Count; i++)
                {
                    if (string.Equals(attributes[i].Name, attributeName, StringComparison.Ordinal))
                    {
                        return attributes[i];
                    }
                }
                return null;
            }
        }

        internal void Add(PssgAttribute attribute)
        {
            this.attributes.Add(attribute);
        }
        internal void Remove(PssgAttribute attribute)
        {
            this.attributes.Remove(attribute);
        }

        public int Count
        {
            get { return attributes.Count; }
        }

        public IEnumerator<PssgAttribute> GetEnumerator()
        {
            return attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
