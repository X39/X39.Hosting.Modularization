using X39.Hosting.Modularization.Abstraction.Attributes;

namespace X39.Hosting.Modularization.Samples.CommonLibrary.Library;

public class ModuleMain : IModuleMain
{
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask LoadModuleAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}