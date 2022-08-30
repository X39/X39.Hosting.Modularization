using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization.Abstraction;

namespace X39.Hosting.Modularization.Template;

public sealed class ModuleMain : IModuleMain
{
    private readonly ILogger<ModuleMain> logger;
    public ModuleMain(ILogger<ModuleMain> logger)
    {
        _logger = logger;
    }
    public ValueTask LoadModuleAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}