namespace EgoEngineLibrary.Frontend.Configuration;

public class NullOptions<T>(T config) : IWriteableOptions<T>
    where T : class
{
    public T Value { get; } = config;
    
    public void Save()
    {
    }
}
