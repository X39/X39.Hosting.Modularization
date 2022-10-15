using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;

namespace X39.Hosting.Modularization;

/// <summary>
/// Provides a <see cref="IServiceProvider"/> that allows resolving services from multiple providers.
/// </summary>
public sealed class HierarchicalServiceProvider : IServiceProvider
{
    private ImmutableArray<IServiceProvider> _serviceProviders;

    /// <summary>
    /// Adds a service provider to the list of the providers,
    /// making it the last one being called for creation if all others failed.
    /// </summary>
    /// <param name="serviceProvider"></param>
    public void Add(IServiceProvider serviceProvider)
    {
        var arr = _serviceProviders;
        _serviceProviders = arr.Append(serviceProvider).ToImmutableArray();
    }

    /// <summary>
    /// Creates a new <see cref="HierarchicalServiceProvider"/>.
    /// </summary>
    /// <param name="serviceProviders">Initial <see cref="IServiceProvider"/>s</param>
    public HierarchicalServiceProvider(IEnumerable<IServiceProvider> serviceProviders)
    {
        _serviceProviders = serviceProviders.ToImmutableArray();
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType)
    {
        if (serviceType.IsEquivalentTo(typeof(IServiceScopeFactory)))
            return new HierarchicalServiceScopeFactory(this);

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

    internal IEnumerable<IServiceProvider> GetServiceProviders()
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