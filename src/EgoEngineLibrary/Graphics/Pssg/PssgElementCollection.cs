using System.Collections;

namespace EgoEngineLibrary.Graphics.Pssg
{
    public class PssgElementCollection : IEnumerable<PssgElement>
    {
        private readonly List<PssgElement> _elements;

        public PssgElementCollection()
        {
            _elements = new List<PssgElement>();
        }
        public PssgElementCollection(int capacity)
        {
            _elements = new List<PssgElement>(capacity);
        }

        public void EnsureCapacity(int capacity)
        {
            _elements.EnsureCapacity(capacity);
        }

        public PssgElement this[int index]
        {
            get
            {
                return _elements[index];
            }
        }
        public PssgElement this[string name]
        {
            get
            {
                for (int i = 0; i < _elements.Count; i++)
                {
                    if (string.Equals(_elements[i].Name, name, PssgStringHelper.StringComparison))
                    {
                        return _elements[i];
                    }
                }

                throw new ArgumentOutOfRangeException(nameof(name));
            }
        }

        internal void Add(PssgElement element)
        {
            this._elements.Add(element);
        }
        internal PssgElement? Set(PssgElement element, PssgElement newElement)
        {
            for (int i = 0; i < this._elements.Count; i++)
            {
                if (object.ReferenceEquals(this._elements[i], element))
                {
                    return this._elements[i] = newElement;
                }
            }

            return null;
        }
        internal bool Remove(PssgElement element)
        {
            return this._elements.Remove(element);
        }

        public int Count
        {
            get { return _elements.Count; }
        }

        public IEnumerator<PssgElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
