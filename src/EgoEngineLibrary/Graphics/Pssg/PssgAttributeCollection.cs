using System.Collections;

namespace EgoEngineLibrary.Graphics.Pssg
{
    public class PssgAttributeCollection : IEnumerable<PssgAttribute>
    {
        private readonly List<PssgAttribute> _attributes;

        public PssgAttributeCollection()
        {
            _attributes = new List<PssgAttribute>();
        }
        public PssgAttributeCollection(int capacity)
        {
            _attributes = new List<PssgAttribute>(capacity);
        }

        public PssgAttribute this[string attributeName]
        {
            get
            {
                for (int i = 0; i < _attributes.Count; i++)
                {
                    if (string.Equals(_attributes[i].Name, attributeName, StringComparison.Ordinal))
                    {
                        return _attributes[i];
                    }
                }

                throw new ArgumentOutOfRangeException(nameof(attributeName));
            }
        }

        internal bool Contains(string attributeName)
        {
            for (int i = 0; i < _attributes.Count; i++)
            {
                if (string.Equals(_attributes[i].Name, attributeName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        internal void Add(PssgAttribute attribute)
        {
            this._attributes.Add(attribute);
        }
        internal PssgAttribute? Get(string name)
        {
            for (int i = 0; i < _attributes.Count; i++)
            {
                if (string.Equals(_attributes[i].Name, name, StringComparison.Ordinal))
                {
                    return _attributes[i];
                }
            }

            return null;
        }
        internal void Remove(PssgAttribute attribute)
        {
            this._attributes.Remove(attribute);
        }

        public int Count
        {
            get { return _attributes.Count; }
        }

        public IEnumerator<PssgAttribute> GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
