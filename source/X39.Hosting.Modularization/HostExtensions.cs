using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace X39.Hosting.Modularization;

public static class HostExtensions
{
    public static IHostBuilder UseModularization(
        this IHostBuilder hostBuilder,
        string moduleDirectory,
        params string[] moduleDirectories)
    {
        return hostBuilder.ConfigureServices(
            (_, services) =>
            {
                services.AddSingleton<ModuleLoader>(
                    (serviceProvider) =>
                    {
                        var logger = serviceProvider.GetService<ILoggerFactory>()
                                         ?.CreateLogger<ModuleLoader>()
                                     ?? throw new NullReferenceException(
                                         "Failed to create logger for ModuleLoader. " +
                                         "Make sure you have added a working {typeof(ILoggerFactory).FullName()} " +
                                         "to your services.");
                        var moduleLoader = new ModuleLoader(logger, serviceProvider);
                        moduleLoader.PrepareModulesInAsync(
                            default,
                            serviceProvider,
                            moduleDirectories.Prepend(moduleDirectory).ToArray())
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();
                        return moduleLoader;
                    });
            });
    }
}