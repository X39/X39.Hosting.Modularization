using Microsoft.Extensions.DependencyInjection;

namespace X39.Hosting.Modularization;

internal class HierarchicalServiceScope : IServiceScope
{
    public HierarchicalServiceScope(HierarchicalServiceProvider hierarchicalServiceProvider)
    {
        ServiceProvider = hierarchicalServiceProvider;
    }

    public void Dispose()
    {
    }

    public IServiceProvider ServiceProvider { get; }
}