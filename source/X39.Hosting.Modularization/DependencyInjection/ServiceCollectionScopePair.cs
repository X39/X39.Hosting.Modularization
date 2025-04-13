using Microsoft.Extensions.DependencyInjection;

namespace X39.Hosting.Modularization.DependencyInjection;

internal sealed class ServiceCollectionScopePair(IServiceCollection serviceCollection, ServiceProvider serviceProvider)
    : IServiceScope, IAsyncDisposable
{
    internal IServiceCollection ServiceCollection { get; } = serviceCollection;
    private readonly IServiceScope _serviceScope = serviceProvider.CreateScope();

    public void Dispose()
    {
        _serviceScope.Dispose();
    }

    public IServiceProvider ServiceProvider => _serviceScope.ServiceProvider;

    public async ValueTask DisposeAsync()
    {
        if (_serviceScope is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        else
            _serviceScope.Dispose();
    }
}
