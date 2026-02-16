namespace EgoEngineLibrary.Frontend.Configuration;

public class WriteableOptions<T> : IWriteableOptions<T>
    where T : class
{
    public T Value => Config.Load<T>();

    public void Save()
    {
        Config.Save<T>();
    }
}
