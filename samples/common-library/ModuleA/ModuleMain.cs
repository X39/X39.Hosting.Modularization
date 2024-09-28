using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization.Abstraction;
using X39.Hosting.Modularization.Samples.CommonLibrary.Common;

namespace X39.Hosting.Modularization.Samples.CommonLibrary.ModuleA;

public class ModuleMain : IModuleMain, ICommonInterface
{
    private readonly IServiceClass _serviceClass;

    public ModuleMain(IServiceClass serviceClass)
    {
        _serviceClass = serviceClass;
    }
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ConfigureServicesAsync(IServiceCollection serviceCollection, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ConfigureAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public void CommonFunction(CommonType commonType)
    {
        Console.WriteLine("MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA");
        Console.WriteLine("MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA");
        Console.WriteLine("MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA");
        Console.WriteLine("MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA");
        Console.WriteLine("MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA");
        Console.WriteLine(commonType.ToString());
        Console.WriteLine(_serviceClass is not null);
        Console.WriteLine("MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA");
        Console.WriteLine("MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA");
        Console.WriteLine("MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA");
        Console.WriteLine("MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA");
        Console.WriteLine("MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA");
    }
}