using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace X39.Hosting.Modularization;

/// <summary>
/// Contains extension methods used in conjunction with the <see cref="IHostBuilder"/>.
/// </summary>
public static class HostExtensions
{
    
    /// <summary>
    /// Initializes the modularization system with the given <paramref name="moduleDirectories"/>.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to add a <see cref="ModuleLoader"/> to.</param>
    /// <param name="moduleDirectory">A directory where to look in for modules.</param>
    /// <param name="moduleDirectories">Additional directories for <paramref name="moduleDirectory"/>.</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static IHostBuilder UseModularization(
        this IHostBuilder hostBuilder,
        string moduleDirectory,
        params string[] moduleDirectories)
    {
        return hostBuilder.ConfigureServices(
            (_, services) =>
            {
                services.AddSingleton(
                    (serviceProvider) =>
                    {
                        var logger = serviceProvider.GetService<ILoggerFactory>()
                                         ?.CreateLogger<ModuleLoader>()
#pragma warning disable CA2201
                                     ?? throw new NullReferenceException(
                                         "Failed to create logger for ModuleLoader. " +
                                         $"Make sure you have added a working {typeof(ILoggerFactory).FullName()} " +
                                         "to your services.");
#pragma warning restore CA2201
                        var moduleLoader = new ModuleLoader(logger, serviceProvider);
                        moduleLoader.ScanForModulesInAsync(
                            default,
                            moduleDirectories.Prepend(moduleDirectory).ToArray())
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();
                        return moduleLoader;
                    });
            });
    }
}