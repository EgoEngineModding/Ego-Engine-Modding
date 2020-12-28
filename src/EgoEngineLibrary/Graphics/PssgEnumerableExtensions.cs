using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Graphics
{
    public static class PssgEnumerableExtensions
    {
        public static IEnumerable<PssgNode> FindNodes(this IEnumerable<PssgNode> nodes, string nodeName)
        {
            foreach (var node in nodes)
            {
                if (node.Name == nodeName)
                {
                    yield return node;
                }
            }
        }

        public static IEnumerable<PssgNode> FindNodes(this IEnumerable<PssgNode> nodes, string nodeName, string attributeName)
        {
            foreach (var node in nodes)
            {
                if (node.Name == nodeName &&
                    node.HasAttribute(attributeName))
                {
                    yield return node;
                }
            }
        }

        public static IEnumerable<PssgNode> FindNodes<T>(this IEnumerable<PssgNode> nodes, string nodeName, string attributeName, T attributeValue)
            where T : notnull
        {
            foreach (var node in nodes)
            {
                if (node.Name == nodeName &&
                    node.HasAttribute(attributeName) &&
                    node.Attributes[attributeName].GetValue<T>().Equals(attributeValue))
                {
                    yield return node;
                }
            }
        }
    }
}
