using System.Collections.Immutable;

namespace X39.Hosting.Modularization;

internal sealed class HierarchicalServiceProvider : IServiceProvider
{
    private ImmutableArray<IServiceProvider> _serviceProviders;

    public HierarchicalServiceProvider(
        IServiceProvider serviceProvider,
        params IServiceProvider[] otherServiceProviders)
        : this(otherServiceProviders.Prepend(serviceProvider))
    {
    }

    public void Add(IServiceProvider serviceProvider)
    {
        var arr = _serviceProviders;
        _serviceProviders = arr.Append(serviceProvider).ToImmutableArray();
    }
    public HierarchicalServiceProvider(IEnumerable<IServiceProvider> serviceProviders)
    {
        _serviceProviders = serviceProviders.ToImmutableArray();
    }

    public object? GetService(Type serviceType)
    {
        var arr = _serviceProviders;
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var serviceProvider in _serviceProviders)
        {
            var service = serviceProvider.GetService(serviceType);
            if (service is not null)
                return service;
        }

        return null;
    }

    public IEnumerable<IServiceProvider> GetServiceProviders()
    {
        foreach (var serviceProvider in _serviceProviders)
        {
            if (serviceProvider is not HierarchicalServiceProvider sub)
                yield return serviceProvider;
            else
            {
                foreach (var subProvider in sub.GetServiceProviders())
                {
                    yield return subProvider;
                }
            }
        }
    }
}