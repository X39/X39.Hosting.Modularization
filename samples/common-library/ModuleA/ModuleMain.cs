using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization.Abstraction;
using X39.Hosting.Modularization.Samples.CommonLibrary.Library;

namespace X39.Hosting.Modularization.Samples.CommonLibrary.ModuleA;

public class ModuleMain : IModuleMain, ICommonInterface
{
    
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask LoadModuleAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public void CommonFunction(CommonType commonType)
    {
        Console.WriteLine(commonType.ToString());
    }
}