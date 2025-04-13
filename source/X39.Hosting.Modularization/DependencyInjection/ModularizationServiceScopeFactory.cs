using Microsoft.Extensions.DependencyInjection;

namespace X39.Hosting.Modularization.DependencyInjection;

internal sealed class ModularizationServiceScopeFactory(ModularizationServiceProvider modularizationServiceProvider)
    : IServiceScopeFactory
{
    public IServiceScope CreateScope()
    {
        return new ModularizationServiceScope(modularizationServiceProvider);
    }
}
