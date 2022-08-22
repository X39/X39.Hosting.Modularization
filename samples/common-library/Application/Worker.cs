using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization;
using X39.Hosting.Modularization.Samples.CommonLibrary.Library;

public class Worker : BackgroundService
{
    private readonly ModuleLoader    _moduleLoader;
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger, ModuleLoader moduleLoader)
    {
        _moduleLoader = moduleLoader;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var i = 0; i < 2; i++)
        {
            _logger.LogInformation("Loading all modules");
            await _moduleLoader.LoadAllAsync(stoppingToken);
            _logger.LogInformation("Executing method");
            Method();
            _logger.LogInformation("Unloading all modules");
            await _moduleLoader.UnloadAllAsync();
        }
    }

    private void Method()
    {
        var commonType = new CommonType();
        foreach (var commonInterface in _moduleLoader.AllModules.Select((q) => q.Instance).OfType<ICommonInterface>())
        {
            commonInterface.CommonFunction(commonType);
        }
    }
}