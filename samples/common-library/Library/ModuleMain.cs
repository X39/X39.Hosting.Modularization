using Microsoft.Extensions.DependencyInjection;
using X39.Hosting.Modularization.Abstraction;
using X39.Hosting.Modularization.Samples.CommonLibrary.Common;

namespace X39.Hosting.Modularization.Samples.CommonLibrary.Library;

public class ModuleMain : IModuleMain
{
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ConfigureServicesAsync(IServiceCollection serviceCollection, CancellationToken cancellationToken)
    {
        serviceCollection.AddSingleton<IServiceClass, ServiceClass>();
        return ValueTask.CompletedTask;
    }

    public ValueTask ConfigureAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}