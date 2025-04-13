using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace X39.Hosting.Modularization;

internal class ModuleLoadContext : AssemblyLoadContext
{
    private readonly ModuleContext                          _moduleContext;
    private readonly ILogger<ModuleLoadContext>             _logger;
    private readonly AssemblyDependencyResolver             _resolver;
    private readonly Func<IEnumerable<AssemblyLoadContext>> _dependencyContextsFunc;

    public ModuleLoadContext(
        ILogger<ModuleLoadContext> logger,
        ModuleContext moduleContext,
        string name,
        Func<IEnumerable<AssemblyLoadContext>> dependencyContextsFunc)
        : base(
            name,
            isCollectible: !moduleContext.Configuration.DisableUnload)
    {
        _dependencyContextsFunc = dependencyContextsFunc;
        _moduleContext          = moduleContext;
        _logger                 = logger;
        _resolver               = new AssemblyDependencyResolver(moduleContext.AssemblyPath);
    }

    public new void Unload()
    {
        _logger.LogTrace("Unloading {ModuleGuid}", _moduleContext.Configuration.Guid);
        base.Unload();
    }

    public new Assembly LoadFromAssemblyPath(string assemblyPath)
    {
        _logger.LogTrace("Loading {ModuleGuid}", _moduleContext.Configuration.Guid);
        return base.LoadFromAssemblyPath(assemblyPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
#if DEBUG
        var assemblyLoadContexts = _dependencyContextsFunc().ToArray();
#else
        var assemblyLoadContexts = _dependencyContextsFunc();
#endif
        foreach (var dependencyContext in assemblyLoadContexts)
        {
            if (TryGetAssembly(assemblyName, dependencyContext, out var assembly))
                return assembly;
        }

        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath is not null || assemblyName.Name is null)
            if (assemblyPath is not null)
            {
                _logger.LogTrace(
                    "Resolved assembly {AssemblyName} at {AssemblyPath}",
                    assemblyName.Name,
                    assemblyPath);
                return LoadFromAssemblyPath(assemblyPath);
            }
            else
            {
                _logger.LogTrace(
                    "Failed to resolve assembly {AssemblyName} for module {ModuleGuid}",
                    assemblyName.Name,
                    _moduleContext.Guid);
                return null;
            }

        if (assemblyPath is null)
        {
            var tmp = Path.Combine(_moduleContext.ModuleDirectory, assemblyName.Name + ".dll");
            if (File.Exists(tmp))
                assemblyPath = tmp;
        }

        if (assemblyPath is not null)
        {
            _logger.LogTrace(
                "Resolved assembly {AssemblyName} at {AssemblyPath}",
                assemblyName.Name,
                assemblyPath);
            return LoadFromAssemblyPath(assemblyPath);
        }
        else
        {
            _logger.LogTrace(
                "Failed to resolve assembly {AssemblyName} for module {ModuleGuid}",
                assemblyName.Name,
                _moduleContext.Guid);
            return null;
        }
    }

    private bool TryGetAssembly(
        AssemblyName assemblyName,
        AssemblyLoadContext dependencyContext,
        out Assembly? outAssembly)
    {
        foreach (var assembly in dependencyContext.Assemblies)
        {
            if (assembly.GetName().Name != assemblyName.Name)
                continue;
            _logger.LogTrace(
                "Resolved assembly {AssemblyName} with {AssemblyDomain}",
                assemblyName.Name,
                dependencyContext.Name);
            outAssembly = assembly;
            return true;
        }

        outAssembly = null;
        return false;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            _logger.LogTrace(
                "Resolved native library {DllName} at {DllPath}",
                unmanagedDllName,
                libraryPath);
            return LoadUnmanagedDllFromPath(libraryPath);
        }
        {
            _logger.LogTrace(
                "Failed to resolve native library {AssemblyName} for module {ModuleGuid}",
                unmanagedDllName,
                _moduleContext.Guid);
            return IntPtr.Zero;
        }
    }
}