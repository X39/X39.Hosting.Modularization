using Microsoft.Extensions.Hosting;
using X39.Hosting.Modularization;
using X39.Hosting.Modularization.Samples.CommonLibrary.Library;

public class Worker : BackgroundService
{
    private readonly ModuleLoader _moduleLoader;

    public Worker(ModuleLoader moduleLoader)
    {
        _moduleLoader = moduleLoader;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (int i = 0; i < 2; i++)
        {
            await _moduleLoader.LoadAllAsync(stoppingToken);
            //Method();
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