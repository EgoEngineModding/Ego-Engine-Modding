namespace EgoEngineLibrary;

public static class ObjectExtensions
{
    extension(object o)
    {
        public bool IsExactType<T>()
        {
            return o.GetType() == typeof(T);
        }
    }
}