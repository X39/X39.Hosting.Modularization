using Microsoft.Extensions.DependencyInjection;
using X39.Hosting.Modularization.Abstraction;

namespace X39.Hosting.Modularization.Samples.CommonLibrary.Library;

public class ModuleMain : IModuleMain
{
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ConfigureServicesAsync(IServiceCollection serviceCollection, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ConfigureAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}