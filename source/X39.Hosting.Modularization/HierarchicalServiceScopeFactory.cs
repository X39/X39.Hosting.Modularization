using Microsoft.Extensions.DependencyInjection;

namespace X39.Hosting.Modularization;

internal sealed class HierarchicalServiceScopeFactory : IServiceScopeFactory
{
    private readonly HierarchicalServiceProvider _hierarchicalServiceProvider;

    public HierarchicalServiceScopeFactory(HierarchicalServiceProvider hierarchicalServiceProvider)
    {
        _hierarchicalServiceProvider = hierarchicalServiceProvider;
    }

    public IServiceScope CreateScope()
    {
        return new HierarchicalServiceScope(_hierarchicalServiceProvider);
    }
}