using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace X39.Hosting.Modularization;

internal class ModuleLoadContext : AssemblyLoadContext
{
    private readonly ModuleContext              _moduleContext;
    private readonly ILogger<ModuleLoadContext> _logger;
    private readonly AssemblyDependencyResolver _resolver;

    public ModuleLoadContext(ILogger<ModuleLoadContext> logger, ModuleContext moduleContext, string name) : base(name, true)
    {
        _moduleContext = moduleContext;
        _logger        = logger;
        _resolver = new AssemblyDependencyResolver(moduleContext.ModuleDirectory);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Load all dependencies into this assembly context
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath is not null ? LoadFromAssemblyPath(assemblyPath) : null;
    }
}