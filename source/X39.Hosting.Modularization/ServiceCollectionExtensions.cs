using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace X39.Hosting.Modularization;

internal static class ServiceCollectionExtensions
{
    // !IMPORTANT!
    // Albeit, this being "internal" scoped, this may actually be relied upon by external users.
    // So try not to change anything that may break things in reflection.
    internal static void AddModularizationSupport(this IServiceCollection services, Func<ModuleLoader, Task> configure)
    {
        services.AddSingleton(
            (serviceProvider) =>
            {
                #pragma warning disable CA2201
                var logger = serviceProvider.GetService<ILoggerFactory>()
                                 ?.CreateLogger<ModuleLoader>()
                             ?? throw new NullReferenceException(
                                 "Failed to create logger for ModuleLoader. "
                                 + $"Make sure you have added a working {typeof(ILoggerFactory).FullName()} "
                                 + "to your services."
                             );
                #pragma warning restore CA2201
                var moduleLoader = new ModuleLoader(logger, serviceProvider);
                configure(moduleLoader)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
                return moduleLoader;
            }
        );
    }
}