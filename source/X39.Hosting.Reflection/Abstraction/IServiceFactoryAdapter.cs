using Microsoft.Extensions.DependencyInjection;

namespace X39.Hosting.Reflection.Abstraction;

public interface IReflectedServiceFactoryAdapter
{
    object CreateBuilder(IServiceCollection services);

    IServiceProvider CreateServiceProvider(object containerBuilder);
}