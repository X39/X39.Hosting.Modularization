using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace X39.Hosting.Modularization;

internal class ModuleLoadContext : AssemblyLoadContext
{
    private readonly ModuleContext              _moduleContext;
    private readonly ILogger<ModuleLoadContext> _logger;

    public ModuleLoadContext(ILogger<ModuleLoadContext> logger, ModuleContext moduleContext, string name) : base(name)
    {
        _moduleContext = moduleContext;
        _logger        = logger;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        return base.Load(assemblyName);
    }
}