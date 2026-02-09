namespace EgoEngineLibrary.Frontend.Configuration;

public interface IConfigProvider
{
    public object Load();
    
    public void Save(object config);
}
