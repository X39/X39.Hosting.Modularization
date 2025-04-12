using Microsoft.Extensions.DependencyInjection;

namespace X39.Hosting.Modularization;

internal class HierarchicalServiceScope : IServiceScope, IAsyncDisposable
{
    private readonly List<IServiceScope> _scopes;

    public HierarchicalServiceScope(HierarchicalServiceProvider hierarchicalServiceProvider)
    {
        _scopes = [];
        foreach (var serviceProvider in hierarchicalServiceProvider.GetServiceProviders())
        {
            var scope = serviceProvider.CreateScope();
            _scopes.Add(scope);
        }

        ServiceProvider = new HierarchicalServiceProvider(_scopes.Select(e => e.ServiceProvider));
    }

    public void Dispose()
    {
        foreach (var serviceScope in _scopes)
        {
            serviceScope.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var serviceScope in _scopes)
        {
            if (serviceScope is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else
                serviceScope.Dispose();
        }
    }

    public IServiceProvider ServiceProvider { get; }
}
