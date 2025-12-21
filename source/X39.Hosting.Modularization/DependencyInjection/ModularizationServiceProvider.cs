using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace X39.Hosting.Modularization.DependencyInjection;

/// <summary>
/// Provides a <see cref="IServiceProvider"/> that allows resolving services from multiple providers.
/// </summary>
public sealed class ModularizationServiceProvider : IServiceProvider
{
    internal          ServiceCollectionProviderPair?        ActualServiceProvider;
    internal readonly IReadOnlyCollection<IServiceProvider> ParentServiceProviders;

    /// <summary>
    /// Adds a service provider to the list of the providers,
    /// making it the last one being called for creation if all others failed.
    /// </summary>
    /// <param name="serviceProvider">
    /// The way the service is resolved will change to a specialized solver instead
    /// of the default one used.
    /// Due to that, if we assume Application -> ModuleA -> ModuleB,
    /// ModuleB services, depending on ModuleA, depending on Application will go first through a manual
    /// constructor discovery to solve for the services individually, repeating for ModuleA constructor and, finally,
    /// hitting the default service provider in Application.
    /// </param>
    internal void Set(ServiceCollectionProviderPair serviceProvider)
    {
        ActualServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a new <see cref="ModularizationServiceProvider"/>.
    /// </summary>
    /// <param name="parentServiceProviders">Parented <see cref="IServiceProvider"/>s</param>
    public ModularizationServiceProvider(IEnumerable<IServiceProvider> parentServiceProviders)
    {
        ParentServiceProviders = [..parentServiceProviders];
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType)
    {
        if (serviceType.IsEquivalentTo(typeof(IServiceScopeFactory)))
            return new ModularizationServiceScopeFactory(this);

        if (serviceType.IsGenericType(typeof(IEnumerable<>)))
        {
            var objects = new List<object>();
            foreach (var serviceProvider in ParentServiceProviders)
            {
                var service = serviceProvider.GetService(serviceType);
                if (service is IEnumerable enumerable)
                    foreach (var obj in enumerable)
                        objects.Add(obj);
            }

            if (ActualServiceProvider is not null)
            {
                var service = ResolveService(
                    this,
                    ActualServiceProvider,
                    ActualServiceProvider.ServiceCollection,
                    serviceType
                );
                if (service is IEnumerable enumerable)
                    foreach (var obj in enumerable)
                        objects.Add(obj);
            }

            var array = Array.CreateInstance(serviceType.GenericTypeArguments[0], objects.Count);
            for (var i = 0; i < objects.Count; i++)
                array.SetValue(objects[i], i);
            return array;
        }
        else
        {
            // Always try parent first
            foreach (var serviceProvider in ParentServiceProviders)
            {
                var service = serviceProvider.GetService(serviceType);
                if (service is not null)
                    return service;
            }

            return ActualServiceProvider is null
                ? null
                : ResolveService(this, ActualServiceProvider, ActualServiceProvider.ServiceCollection, serviceType);
        }
    }

    internal static object? ResolveService(
        IServiceProvider self,
        IServiceProvider serviceProvider,
        IServiceCollection serviceCollection,
        Type serviceType
    )
    {
        try
        {
            return serviceProvider.GetService(serviceType);
        }
        catch (Exception ex)
        {
            if (serviceType.IsGenericType(typeof(IEnumerable<>)))
            {
                var concreteType = serviceType.GenericTypeArguments[0];
                var serviceDescriptors = serviceCollection.Where(e => e.ServiceType.IsEquivalentTo(concreteType));
                var output = new List<object>();
                try
                {

                    foreach (var serviceDescriptor in serviceDescriptors)
                    {
                        if (serviceDescriptor.ImplementationType is null)
                            throw;
                        var constructors = serviceDescriptor.ImplementationType.GetConstructors();
                        if (constructors.Length == 0)
                            throw;
                        var instance = ConstructType(self, serviceDescriptor.ImplementationType, constructors, ex);
                        output.Add(instance);
                    }
                }
                catch
                {
                    foreach (var o in output)
                    {
                        try
                        {
                            if (o is IDisposable disposable)
                                disposable.Dispose(); // Prefer IDisposable over IAsyncDisposable as we can await it
                            else if (o is IAsyncDisposable asyncDisposable)
                                asyncDisposable.DisposeAsync() // We just fire and forget here to follow the disposing guarantee
                                    .AsTask()
                                    .ConfigureAwait(false);
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                    throw;
                }
                var array = Array.CreateInstance(concreteType, output.Count);
                for (var i = 0; i < output.Count; i++)
                    array.SetValue(output[i], i);
                return array;
            }
            else
            {
                var serviceDescriptor = serviceCollection.LastOrDefault(e => e.ServiceType.IsEquivalentTo(serviceType));
                if (serviceDescriptor?.ImplementationType is null)
                    throw;
                serviceType = serviceDescriptor.ImplementationType;
                var constructors = serviceType.GetConstructors();
                if (constructors.Length == 0)
                    throw;
                return ConstructType(self, serviceType, constructors, ex);
            }
        }
    }

    private static object ConstructType(
        IServiceProvider self,
        Type serviceType,
        ConstructorInfo[] constructors,
        Exception ex
    )
    {
        var constructor = constructors.Length == 1
            ? constructors[0]
            : constructors.FirstOrDefault(e
                => e.GetCustomAttribute<ActivatorUtilitiesConstructorAttribute>() is not null
            );
        #pragma warning disable CA2201
        if (constructor is null)
            throw new Exception(
                $"Could not find a constructor for {serviceType.FullName}; Mark the dependency injection constructor with [ActivatorUtilitiesConstructor] to resolve it manually.",
                ex
            );
        #pragma warning restore CA2201
        var parameters = constructor.GetParameters();
        var instances = parameters.Select(e => self.GetService(e.ParameterType))
            .ToArray();
        return constructor.Invoke(instances);
    }
}
