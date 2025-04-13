using Microsoft.Extensions.Hosting;

namespace X39.Hosting.Modularization;

/// <summary>
/// Contains extension methods used in conjunction with the <see cref="IHostApplicationBuilder"/>.
/// </summary>
/// <seealso cref="HostBuilderExtensions"/>
[PublicAPI]
public static class HostApplicationBuilderExtensions
{
    /// <summary>
    /// Initializes the modularization system with the given <paramref name="moduleDirectories"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="ModuleLoader"/> is added to the <see cref="IServiceProvider"/> as singleton.
    /// </remarks>
    /// <param name="hostBuilder">The <see cref="IHostApplicationBuilder"/> to add a <see cref="ModuleLoader"/> to.</param>
    /// <param name="moduleDirectory">A directory where to look in for modules.</param>
    /// <param name="moduleDirectories">Additional directories for <paramref name="moduleDirectory"/>.</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static IHostApplicationBuilder UseModularization(
        this IHostApplicationBuilder hostBuilder,
        string moduleDirectory,
        params string[] moduleDirectories
    )
        => UseModularization(hostBuilder, (_) => Task.CompletedTask, moduleDirectory, moduleDirectories);

    /// <summary>
    /// Initializes the modularization system with the given <paramref name="moduleDirectories"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="ModuleLoader"/> is added to the <see cref="IServiceProvider"/> as singleton.
    /// </remarks>
    /// <param name="hostBuilder">The <see cref="IHostApplicationBuilder"/> to add a <see cref="ModuleLoader"/> to.</param>
    /// <param name="configure">Allows to configure the <see cref="ModuleLoader"/>.</param>
    /// <param name="moduleDirectory">A directory where to look in for modules.</param>
    /// <param name="moduleDirectories">Additional directories for <paramref name="moduleDirectory"/>.</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static IHostApplicationBuilder UseModularization(
        this IHostApplicationBuilder hostBuilder,
        Action<ModuleLoader> configure,
        string moduleDirectory,
        params string[] moduleDirectories
    )
        => UseModularization(
            hostBuilder,
            async (moduleLoader) =>
            {
                configure(moduleLoader);
                await moduleLoader.ScanForModulesAsync(
                        default,
                        moduleDirectories.Prepend(moduleDirectory)
                            .ToArray()
                    )
                    .ConfigureAwait(false);
            }
        );

    /// <summary>
    /// Initializes the modularization system.
    /// </summary>
    /// <remarks>
    /// The <see cref="ModuleLoader"/> is added to the <see cref="IServiceProvider"/> as singleton.
    /// </remarks>
    /// <param name="hostBuilder">The <see cref="IHostApplicationBuilder"/> to add a <see cref="ModuleLoader"/> to.</param>
    /// <param name="configure">Allows to configure the <see cref="ModuleLoader"/>.</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static IHostApplicationBuilder UseModularization(
        this IHostApplicationBuilder hostBuilder,
        Action<ModuleLoader> configure
    )
        => UseModularization(
            hostBuilder,
            (moduleLoader) =>
            {
                configure(moduleLoader);
                return Task.CompletedTask;
            }
        );

    /// <summary>
    /// Initializes the modularization system with the given <paramref name="moduleDirectories"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="ModuleLoader"/> is added to the <see cref="IServiceProvider"/> as singleton.
    /// </remarks>
    /// <param name="hostBuilder">The <see cref="IHostApplicationBuilder"/> to add a <see cref="ModuleLoader"/> to.</param>
    /// <param name="configure">Allows to configure the <see cref="ModuleLoader"/>.</param>
    /// <param name="moduleDirectory">A directory where to look in for modules.</param>
    /// <param name="moduleDirectories">Additional directories for <paramref name="moduleDirectory"/>.</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static IHostApplicationBuilder UseModularization(
        this IHostApplicationBuilder hostBuilder,
        Func<ModuleLoader, Task> configure,
        string moduleDirectory,
        params string[] moduleDirectories
    )
        => UseModularization(
            hostBuilder,
            async (moduleLoader) =>
            {
                await configure(moduleLoader)
                    .ConfigureAwait(false);
                await moduleLoader.ScanForModulesAsync(
                        default,
                        moduleDirectories.Prepend(moduleDirectory)
                            .ToArray()
                    )
                    .ConfigureAwait(false);
            }
        );

    /// <summary>
    /// Initializes the modularization system with the given.
    /// </summary>
    /// <remarks>
    /// The <see cref="ModuleLoader"/> is added to the <see cref="IServiceProvider"/> as singleton.
    /// </remarks>
    /// <param name="hostBuilder">The <see cref="IHostApplicationBuilder"/> to add a <see cref="ModuleLoader"/> to.</param>
    /// <param name="configure">Allows to configure the <see cref="ModuleLoader"/>.</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static IHostApplicationBuilder UseModularization(
        this IHostApplicationBuilder hostBuilder,
        Func<ModuleLoader, Task> configure
    )
    {
        hostBuilder.Services.AddModularizationSupport(configure);
        return hostBuilder;
    }
}