namespace EgoEngineLibrary.Graphics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class PssgNodeCollection : IEnumerable<PssgNode>
    {
        private List<PssgNode> nodes;

        public PssgNodeCollection()
        {
            nodes = new List<PssgNode>();
        }
        public PssgNodeCollection(int capacity)
        {
            nodes = new List<PssgNode>(capacity);
        }

        public PssgNode this[int index]
        {
            get
            {
                return nodes[index];
            }
        }
        public PssgNode this[string nodeName]
        {
            get
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (string.Equals(nodes[i].Name, nodeName, StringComparison.Ordinal))
                    {
                        return nodes[i];
                    }
                }

                throw new ArgumentOutOfRangeException(nameof(nodeName));
            }
        }

        internal void Add(PssgNode node)
        {
            this.nodes.Add(node);
        }
        internal PssgNode? Set(PssgNode node, PssgNode newNode)
        {
            for (int i = 0; i < this.nodes.Count; i++)
            {
                if (object.ReferenceEquals(this.nodes[i], node))
                {
                    return this.nodes[i] = newNode;
                }
            }

            return null;
        }
        internal bool Remove(PssgNode node)
        {
            return this.nodes.Remove(node);
        }

        public int Count
        {
            get { return nodes.Count; }
        }

        public IEnumerator<PssgNode> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
