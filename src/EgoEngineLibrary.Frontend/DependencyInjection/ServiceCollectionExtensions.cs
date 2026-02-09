using EgoEngineLibrary.Frontend.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace EgoEngineLibrary.Frontend.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddConfig<T>(IConfigProvider configProvider)
        {
            Config.Add<T>(configProvider);
            serviceCollection.AddOptions();
            return serviceCollection;
        }

        private void AddOptions()
        {
            // This registers 2 instances of WriteableOptions<> for the same generic type unfortunately
            serviceCollection.TryAddSingleton(typeof(IWriteableOptions<>), typeof(WriteableOptions<>));
            serviceCollection.TryAddSingleton(typeof(IOptions<>), typeof(WriteableOptions<>));
        }
    }
}
