using X39.Hosting.Modularization.Configuration;
using X39.Hosting.Modularization.DependencyInjection;

namespace X39.Hosting.Modularization;

/// <summary>
/// Represents a module.
/// </summary>
[PublicAPI]
public sealed class ModuleContextManaged : ModuleContextBase
{
    /// <summary>
    /// The type the <see cref="ModuleContextManaged"/> is registered with.
    /// </summary>
    public Type MainType { get; }

    internal ModuleContextManaged(
        ModuleLoader moduleLoader,
        IServiceProvider serviceProvider,
        ModuleConfiguration configuration,
        Type mainType
    )
        : base(moduleLoader, serviceProvider, configuration)
    {
        MainType = mainType;
    }

    /// <inheritdoc />
    protected internal override async Task DoLoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            var constructor = GetMainConstructorOrNull(MainType);
            Instance          = default;
            ServiceCollection = new ModularizationServiceCollection();
            var hierarchicalServiceProvider = CreateHierarchicalServiceProvider();
            Instance = ResolveType(constructor, MainType, hierarchicalServiceProvider);
            await Instance.ConfigureServicesAsync(ServiceCollection, cancellationToken);
            var provider = new ServiceCollectionProviderPair(ServiceCollection);
            hierarchicalServiceProvider.Set(provider);
            ServiceProvider = hierarchicalServiceProvider;
            await Instance.ConfigureAsync(ServiceProvider, cancellationToken)
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
