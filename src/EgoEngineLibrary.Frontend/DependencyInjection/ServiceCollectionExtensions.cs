using EgoEngineLibrary.Frontend.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EgoEngineLibrary.Frontend.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddConfigOptions()
        {
            // This registers 2 instances of WriteableOptions<> for the same generic type unfortunately
            serviceCollection.TryAddSingleton(typeof(IWriteableOptions<>), typeof(WriteableOptions<>));
            serviceCollection.TryAddSingleton(typeof(IOptions<>), typeof(WriteableOptions<>));
            return serviceCollection;
        }
    }
}
