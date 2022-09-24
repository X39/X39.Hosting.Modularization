using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace X39.Hosting.Modularization;

/// <summary>
/// Contains extension methods used in conjunction with the <see cref="IHostBuilder"/>.
/// </summary>
[PublicAPI]
public static class HostExtensions
{
    /// <summary>
    /// Initializes the modularization system with the given <paramref name="moduleDirectories"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="ModuleLoader"/> is added to the <see cref="IServiceProvider"/> as singleton.
    /// </remarks>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to add a <see cref="ModuleLoader"/> to.</param>
    /// <param name="moduleDirectory">A directory where to look in for modules.</param>
    /// <param name="moduleDirectories">Additional directories for <paramref name="moduleDirectory"/>.</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static IHostBuilder UseModularization(
        this IHostBuilder hostBuilder,
        string moduleDirectory,
        params string[] moduleDirectories)
        => UseModularization(hostBuilder, (_) => Task.CompletedTask, moduleDirectory, moduleDirectories);

    /// <summary>
    /// Initializes the modularization system with the given <paramref name="moduleDirectories"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="ModuleLoader"/> is added to the <see cref="IServiceProvider"/> as singleton.
    /// </remarks>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to add a <see cref="ModuleLoader"/> to.</param>
    /// <param name="configure">Allows to configure the <see cref="ModuleLoader"/>.</param>
    /// <param name="moduleDirectory">A directory where to look in for modules.</param>
    /// <param name="moduleDirectories">Additional directories for <paramref name="moduleDirectory"/>.</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static IHostBuilder UseModularization(
        this IHostBuilder hostBuilder,
        Action<ModuleLoader> configure,
        string moduleDirectory,
        params string[] moduleDirectories)
        => UseModularization(
            hostBuilder,
            async (moduleLoader) =>
            {
                configure(moduleLoader);
                await moduleLoader.ScanForModulesAsync(
                        default,
                        moduleDirectories.Prepend(moduleDirectory).ToArray())
                    .ConfigureAwait(false);
            });

    /// <summary>
    /// Initializes the modularization system.
    /// </summary>
    /// <remarks>
    /// The <see cref="ModuleLoader"/> is added to the <see cref="IServiceProvider"/> as singleton.
    /// </remarks>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to add a <see cref="ModuleLoader"/> to.</param>
    /// <param name="configure">Allows to configure the <see cref="ModuleLoader"/>.</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static IHostBuilder UseModularization(
        this IHostBuilder hostBuilder,
        Action<ModuleLoader> configure)
        => UseModularization(
            hostBuilder,
            (moduleLoader) =>
            {
                configure(moduleLoader);
                return Task.CompletedTask;
            });

    /// <summary>
    /// Initializes the modularization system with the given <paramref name="moduleDirectories"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="ModuleLoader"/> is added to the <see cref="IServiceProvider"/> as singleton.
    /// </remarks>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to add a <see cref="ModuleLoader"/> to.</param>
    /// <param name="configure">Allows to configure the <see cref="ModuleLoader"/>.</param>
    /// <param name="moduleDirectory">A directory where to look in for modules.</param>
    /// <param name="moduleDirectories">Additional directories for <paramref name="moduleDirectory"/>.</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static IHostBuilder UseModularization(
        this IHostBuilder hostBuilder,
        Func<ModuleLoader, Task> configure,
        string moduleDirectory,
        params string[] moduleDirectories)
        => UseModularization(
            hostBuilder,
            async (moduleLoader) =>
            {
                await configure(moduleLoader)
                    .ConfigureAwait(false);
                await moduleLoader.ScanForModulesAsync(
                        default,
                        moduleDirectories.Prepend(moduleDirectory).ToArray())
                    .ConfigureAwait(false);
            });

    /// <summary>
    /// Initializes the modularization system with the given.
    /// </summary>
    /// <remarks>
    /// The <see cref="ModuleLoader"/> is added to the <see cref="IServiceProvider"/> as singleton.
    /// </remarks>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to add a <see cref="ModuleLoader"/> to.</param>
    /// <param name="configure">Allows to configure the <see cref="ModuleLoader"/>.</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static IHostBuilder UseModularization(
        this IHostBuilder hostBuilder,
        Func<ModuleLoader, Task> configure)
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
                        configure(moduleLoader)
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();
                        return moduleLoader;
                    });
            });
    }
}