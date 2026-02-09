using Microsoft.Extensions.Options;

namespace EgoEngineLibrary.Frontend.Configuration;

public interface IWriteableOptions<out T> : IOptions<T>
    where T : class
{
    void Save();
}
