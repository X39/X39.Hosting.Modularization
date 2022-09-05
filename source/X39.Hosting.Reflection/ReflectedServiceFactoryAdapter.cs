using Microsoft.Extensions.DependencyInjection;
using X39.Hosting.Reflection.Abstraction;
using X39.Hosting.Reflection.Exceptions;

namespace X39.Hosting.Reflection;

internal class ReflectedServiceFactoryAdapter : IReflectedServiceFactoryAdapter
{
    private readonly object _serviceProviderFactory;

    public ReflectedServiceFactoryAdapter(object serviceProviderFactory)
    {
        _serviceProviderFactory = serviceProviderFactory;
    }

    public object CreateBuilder(IServiceCollection services)
    {
        return _serviceProviderFactory.CallPrivateMethod<object>(nameof(CreateBuilder), services)
               ?? throw new ReflectedNullException("Failed to receive a non-null value from " + nameof(CreateBuilder));
    }

    public IServiceProvider CreateServiceProvider(object containerBuilder)
    {
        return _serviceProviderFactory.CallPrivateMethod<IServiceProvider>(
                   nameof(CreateServiceProvider),
                   containerBuilder)
               ?? throw new ReflectedNullException(
                   "Failed to receive a non-null value from " + nameof(CreateServiceProvider));
    }
}