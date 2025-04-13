using Microsoft.Extensions.DependencyInjection;
using X39.Util.Collections;

namespace X39.Hosting.Modularization.DependencyInjection;

internal class ModularizationServiceScope : IServiceScope, IAsyncDisposable, IServiceProvider
{
    private readonly List<IServiceScope> _parentScopes;
    private readonly ServiceCollectionScopePair?      _actualScope;

    public ModularizationServiceScope(ModularizationServiceProvider modularizationServiceProvider)
    {
        _parentScopes = [];
        foreach (var serviceProvider in modularizationServiceProvider.ParentServiceProviders)
        {
            var scope = serviceProvider.CreateScope();
            _parentScopes.Add(scope);
        }

        if (modularizationServiceProvider.ActualServiceProvider is not null)
            _actualScope = modularizationServiceProvider.ActualServiceProvider.CreateScopePair();

        ServiceProvider = this;
    }

    public void Dispose()
    {
        foreach (var serviceScope in _parentScopes.Append(_actualScope)
                     .NotNull())
        {
            serviceScope.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var serviceScope in _parentScopes.Append(_actualScope)
                     .NotNull())
        {
            if (serviceScope is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else
                serviceScope.Dispose();
        }
    }

    public IServiceProvider ServiceProvider { get; }

    public object? GetService(Type serviceType)
    {
        // Always try parent first
        foreach (var serviceProvider in _parentScopes)
        {
            var service = serviceProvider.ServiceProvider.GetService(serviceType);
            if (service is not null)
                return service;
        }

        if (_actualScope is null)
            return null;
        return ModularizationServiceProvider.ResolveService(this, _actualScope.ServiceProvider, _actualScope.ServiceCollection, serviceType);
    }
}
