using Microsoft.Extensions.DependencyInjection;
using X39.Hosting.Modularization.Abstraction;
using X39.Hosting.Modularization.Samples.CommonLibrary.Common;

namespace X39.Hosting.Modularization.Samples.CommonLibrary.ModuleB;

public class ModuleMain : IModuleMain, ICommonInterface
{
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
        Console.WriteLine("MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB");
        Console.WriteLine("MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB");
        Console.WriteLine("MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB");
        Console.WriteLine("MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB");
        Console.WriteLine("MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB");
        Console.WriteLine(commonType.ToString());
        Console.WriteLine("MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB");
        Console.WriteLine("MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB");
        Console.WriteLine("MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB");
        Console.WriteLine("MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB");
        Console.WriteLine("MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB-MODULEB");
        new SkiaSharpUser().Test();
    }
}
