using Microsoft.Extensions.DependencyInjection;

namespace X39.Hosting.Modularization.DependencyInjection;

internal sealed class ServiceCollectionProviderPair : IServiceProvider
{
    internal IServiceCollection ServiceCollection { get; }
    private readonly ServiceProvider _serviceProvider;

    public ServiceCollectionProviderPair(IServiceCollection serviceCollection)
    {
        ServiceCollection = serviceCollection;
        _serviceProvider  = ServiceCollection.BuildServiceProvider();
    }

    public object? GetService(Type serviceType) => _serviceProvider.GetService(serviceType);

    public ServiceCollectionScopePair CreateScopePair()
    {
        return new(ServiceCollection, _serviceProvider);
    }
}
