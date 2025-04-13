using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization.Abstraction;
using X39.Hosting.Modularization.Samples.CommonLibrary.Common;

namespace X39.Hosting.Modularization.Samples.CommonLibrary.ModuleA;

public class ModuleMain : IModuleMain, ICommonInterface
{
    private readonly ISingletonClass _singletonClass;

    public ModuleMain(ISingletonClass singletonClass)
    {
        _singletonClass = singletonClass;
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
        Console.WriteLine(_singletonClass is not null);
        Console.WriteLine("MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA");
        Console.WriteLine("MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA");
        Console.WriteLine("MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA");
        Console.WriteLine("MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA");
        Console.WriteLine("MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA-MODULEA");
    }
}
