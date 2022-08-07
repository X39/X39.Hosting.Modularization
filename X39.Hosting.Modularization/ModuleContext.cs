using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization.Configuration;
using X39.Hosting.Modularization.Exceptions;

namespace X39.Hosting.Modularization;

/// <summary>
/// Represents a module.
/// </summary>
[PublicAPI]
public class ModuleContext : IAsyncDisposable
{
    /// <summary>
    /// The configuration of this module.
    /// </summary>
    public ModuleConfiguration Configuration { get; }

    /// <summary>
    /// Convenience property for getting the <see cref="Guid"/> from the <see cref="Configuration"/>.
    /// Returns the value of <see cref="ModuleConfiguration.Guid"/>.
    /// </summary>
    public Guid Guid => Configuration.Guid;

    /// <summary>
    /// Returns the path to the module's main assembly.
    /// </summary>
    public string AssemblyPath => Path.Combine(_moduleDirectory, Configuration.StartDll);
    private readonly AssemblyLoadContext _assemblyLoadContext;
    private          bool                _isLoaded;
    private readonly string              _moduleDirectory;
    internal ModuleContext(IServiceProvider serviceProvider, string moduleDirectory, ModuleConfiguration configuration)
    {
        _moduleDirectory = moduleDirectory;
        Configuration     = configuration;
        var assemblyLoadContextName = string.Concat(
            Path.GetFileNameWithoutExtension(configuration.StartDll),
            "-",
            configuration.Guid.ToString());

        var loggerFactory = serviceProvider.GetService<ILoggerFactory>()
                            ?? throw new NullReferenceException(
                                $"Failed to get logger factory ({typeof(ILoggerFactory).FullName()}) " +
                                $"from service provide ({typeof(IServiceProvider).FullName()}.");
        _assemblyLoadContext = new ModuleLoadContext(loggerFactory.CreateLogger<ModuleLoadContext>(), this);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _assemblyLoadContext.Unload();
        return ValueTask.CompletedTask;
    }

    internal async Task LoadModuleAsync(ILogger<ModuleLoader> logger, ModuleContext moduleContext)
    {
        lock (this)
        {
            if (_isLoaded)
                throw new ModuleAlreadyLoadedException(this);
            _isLoaded = true;
        }
        var assembly = _assemblyLoadContext.LoadFromAssemblyPath(AssemblyPath);
    }
}