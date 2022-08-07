using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization.Abstraction.Attributes;
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
    private          bool                _isLoading;
    private readonly string              _moduleDirectory;
    private readonly IServiceProvider    _serviceProvider;


    public bool IsLoaded => _isLoaded;
    public bool IsLoading => _isLoading;

    internal ModuleContext(
        IServiceProvider serviceProvider,
        string moduleDirectory,
        ModuleConfiguration configuration)
    {
        _moduleDirectory = moduleDirectory;
        Configuration    = configuration;
        var assemblyLoadContextName = string.Concat(
            Path.GetFileNameWithoutExtension(configuration.StartDll),
            "-",
            configuration.Guid.ToString());

        _serviceProvider = serviceProvider;
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>()
                            ?? throw new NullReferenceException(
                                $"Failed to get logger factory ({typeof(ILoggerFactory).FullName()}) " +
                                $"from service provider ({serviceProvider.GetType().FullName()}.");
        _assemblyLoadContext = new ModuleLoadContext(
            loggerFactory.CreateLogger<ModuleLoadContext>(),
            this,
            assemblyLoadContextName);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (_isLoaded)
            throw new ModuleStillLoadedException(this);
        return ValueTask.CompletedTask;
    }

    internal async Task LoadModuleAsync(ModuleContext moduleContext, CancellationToken cancellationToken)
    {
        lock (this)
        {
            if (_isLoaded)
                throw new ModuleAlreadyLoadedException(this);
            if (_isLoading)
                throw new ModuleAlreadyLoadingException(this);
            _isLoading = true;
        }

        // ToDo: use Disposable to ensure isloaded is set to false
        var assembly = _assemblyLoadContext.LoadFromAssemblyPath(AssemblyPath);
        var mainType = GetMainType(assembly);
        var constructor = GetMainConstructorOrNull(mainType);

        object instance;
        if (constructor is null)
        {
            // ToDo: Catch exception if thrown and auto-unload
            instance = mainType.CreateInstance();
        }
        else
        {
            var parameters = constructor.GetParameters();
            var types = parameters.Select((parameterInfo) => parameterInfo.ParameterType);
            var services = types.Select(
                (type) => type.IsEquivalentTo(typeof(ModuleContext))
                    ? this
                    : _serviceProvider.GetService(type));
            // ToDo: Check nullability and throw an exception if not matched.
        }


        lock (this)
        {
            _isLoading = false;
            _isLoaded  = true;
        }
    }

    private ConstructorInfo? GetMainConstructorOrNull(Type mainType)
    {
        var candidates = mainType.GetConstructors(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        switch (candidates.Length)
        {
            case > 1:
                var typeName = mainType.FullName();
                _assemblyLoadContext.Unload();
                throw new MultipleMainTypeConstructorsException(this, typeName);
        }

        var constructor = candidates.SingleOrDefault();
        return constructor;
    }

    private Type GetMainType(Assembly assembly)
    {
        var query = from type in assembly.GetTypes()
            select (type, attribute: type.GetCustomAttribute<ModuleMainAttribute>())
            into tuple
            where tuple.attribute is not null
            select tuple.type;
        var candidates = query.ToImmutableArray();
        switch (candidates.Length)
        {
            case 0:
                _assemblyLoadContext.Unload();
                throw new NoModuleMainTypeException(this);
            case > 1:
                var typeNames = candidates.Select((q) => q.FullName());
                _assemblyLoadContext.Unload();
                throw new MultipleModuleMainTypesException(this, typeNames);
        }

        var mainType = candidates.Single();
        if (mainType.IsGenericType)
        {
            var typeName = mainType.FullName();
            _assemblyLoadContext.Unload();
            throw new ModuleMainTypeIsGenericException(this, typeName);
        }

        return mainType;
    }

    internal async Task UnloadModuleAsync()
    {
        lock (this)
        {
            if (!_isLoaded)
                throw new ModuleIsNotLoadedException(this);
            if (_isLoading)
                throw new ModuleLoadingNotFinishedException(this);
            _isLoading = true;
        }

        throw new NotImplementedException();
        _assemblyLoadContext.Unload();
    }
}

internal class ModuleMainTypeIsGenericException : Exception
{
    public ModuleMainTypeIsGenericException(ModuleContext moduleContext, string typeName)
    {
        throw new NotImplementedException();
    }
}

public class MultipleMainTypeConstructorsException : Exception
{
    internal MultipleMainTypeConstructorsException(ModuleContext moduleContext, string candidates)
    {
        throw new NotImplementedException();
    }
}

public class NoModuleMainTypeException : Exception
{
    internal NoModuleMainTypeException(ModuleContext moduleContext)
    {
        throw new NotImplementedException();
    }
}

public class MultipleModuleMainTypesException : Exception
{
    internal MultipleModuleMainTypesException(ModuleContext moduleContext, IEnumerable<string> candidates)
    {
        throw new NotImplementedException();
    }
}