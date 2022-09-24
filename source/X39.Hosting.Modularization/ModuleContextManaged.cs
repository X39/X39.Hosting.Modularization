using Microsoft.Extensions.DependencyInjection;
using X39.Hosting.Modularization.Configuration;

namespace X39.Hosting.Modularization;

/// <summary>
/// Represents a module.
/// </summary>
[PublicAPI]
public sealed class ModuleContextManaged : ModuleContextBase
{
    private readonly Type _mainType;

    internal ModuleContextManaged(
        ModuleLoader moduleLoader,
        IServiceProvider serviceProvider,
        ModuleConfiguration configuration,
        Type mainType) : base(moduleLoader, serviceProvider, configuration)
    {
        _mainType = mainType;
    }

    /// <inheritdoc />
    protected internal override async Task DoLoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            var constructor = GetMainConstructorOrNull(_mainType);
            Instance           = default;
            ServiceCollection = new ServiceCollection();
            var hierarchicalServiceProvider = CreateHierarchicalServiceProvider();
            Instance = ResolveType(constructor, _mainType, hierarchicalServiceProvider);
            await Instance.ConfigureServicesAsync(ServiceCollection, cancellationToken);
            var provider = ServiceCollection.BuildServiceProvider();
            hierarchicalServiceProvider.Add(provider);
            ServiceProvider = hierarchicalServiceProvider;
            await Instance.ConfigureAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            await DisposeOfInstance();
            await DisposeOfServiceCollection();
            throw;
        }
    }

    /// <inheritdoc />
    protected internal override async Task DoUnloadAsync()
    {
        // We do not try-catch around this as exceptions here actually hinder unloading
        await DisposeOfInstance();
        await DisposeOfServiceCollection();
    }
}