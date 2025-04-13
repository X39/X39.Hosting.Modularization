using Microsoft.Extensions.DependencyInjection;

namespace X39.Hosting.Modularization.DependencyInjection;

internal class ServiceCollectionScopePair : IServiceScope
{
    internal IServiceCollection ServiceCollection { get; }
    private readonly IServiceScope _serviceScope;

    public ServiceCollectionScopePair(IServiceCollection serviceCollection, ServiceProvider serviceProvider)
    {
        ServiceCollection = serviceCollection;
        _serviceScope     = serviceProvider.CreateScope();
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }

    public IServiceProvider ServiceProvider => _serviceScope.ServiceProvider;
}
