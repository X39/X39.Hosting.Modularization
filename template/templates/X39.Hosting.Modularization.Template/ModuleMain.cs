using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization.Abstraction;

namespace X39.Hosting.Modularization.Template;

public sealed class ModuleMain : IModuleMain
{
    private readonly ILogger<ModuleMain> _logger;

    public ModuleMain(ILogger<ModuleMain> logger)
    {
        _logger = logger;
    }

    public ValueTask ConfigureServicesAsync(IServiceCollection serviceCollection, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ConfigureAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}