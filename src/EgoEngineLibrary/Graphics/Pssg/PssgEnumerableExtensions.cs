namespace EgoEngineLibrary.Graphics.Pssg
{
    public static class PssgEnumerableExtensions
    {
        public static IEnumerable<PssgElement> FindElements(this IEnumerable<PssgElement> elements, string elementName)
        {
            foreach (var element in elements)
            {
                if (element.Name == elementName)
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<PssgElement> FindElements<T>(this IEnumerable<PssgElement> elements, string elementName, string attributeName, T attributeValue)
            where T : notnull
        {
            foreach (var element in elements)
            {
                if (element.Name == elementName &&
                    element.HasAttribute(attributeName) &&
                    element.Attributes[attributeName].GetValue<T>().Equals(attributeValue))
                {
                    yield return element;
                }
            }
        }
    }
}
