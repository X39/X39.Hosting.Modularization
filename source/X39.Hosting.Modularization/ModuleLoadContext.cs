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
            isCollectible: true)
    {
        _dependencyContextsFunc = dependencyContextsFunc;
        _moduleContext          = moduleContext;
        _logger                 = logger;
        _resolver               = new AssemblyDependencyResolver(moduleContext.ModuleDirectory);
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
            return assemblyPath is not null
                ? LoadFromAssemblyPath(assemblyPath)
                : null;
        
        var tmp = Path.Combine(_moduleContext.ModuleDirectory, assemblyName.Name);
        if (File.Exists(tmp))
            assemblyPath = tmp;

        return assemblyPath is not null
            ? LoadFromAssemblyPath(assemblyPath)
            : null;
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
}