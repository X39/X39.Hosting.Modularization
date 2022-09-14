using System.Collections.Immutable;
using System.Reflection;
using X39.Hosting.Modularization.Exceptions;
using X39.Util.Collections;

namespace X39.Hosting.Modularization;

/// <summary>
/// Contains extension methods for the module system which are technically part of the library,
/// but not of the actual object.
/// </summary>
[PublicAPI]
public static class Extensions
{
    /// <summary>
    /// Creates a new type related to the <see cref="ModuleContext"/> by utilizing the given
    /// <see cref="IServiceProvider"/> that is expected to be derived from a service scope, which was created from
    /// <paramref name="self"/>.<see cref="ModuleContext.ServiceProvider"/>
    /// </summary>
    /// <param name="self">The <see cref="ModuleContext"/> relating to <paramref name="provider"/>.</param>
    /// <param name="provider">A valid <see cref="IServiceProvider"/> that is, if possible, scoped.</param>
    /// <param name="arguments">Additional arguments to be passed into the construction of the type</param>
    /// <typeparam name="T">The type to construct.</typeparam>
    /// <returns>The constructed instance</returns>
    public static T CreateType<T>(
        this ModuleContext self,
        IServiceProvider provider,
        IEnumerable<object> arguments)
    {
        return (T) CreateType(self, provider, typeof(T), arguments);
    }

    /// <summary>
    /// Creates a new type related to the <see cref="ModuleContext"/> by utilizing the given
    /// <see cref="IServiceProvider"/> that is expected to be derived from a service scope, which was created from
    /// <paramref name="self"/>.<see cref="ModuleContext.ServiceProvider"/>
    /// </summary>
    /// <param name="self">The <see cref="ModuleContext"/> relating to <paramref name="provider"/>.</param>
    /// <param name="provider">A valid <see cref="IServiceProvider"/> that is, if possible, scoped.</param>
    /// <param name="type">The type to construct.</param>
    /// <param name="arguments">Additional arguments to be passed into the construction of the type</param>
    /// <returns>The constructed instance</returns>
    public static object CreateType(
        this ModuleContext self,
        IServiceProvider provider,
        Type type,
        IEnumerable<object> arguments)
    {
        var constructor = GetMainConstructorOrNull(self, type);
        var instance = ResolveType(self, constructor, type, provider, arguments);
        return instance;
    }

    private static ConstructorInfo? GetMainConstructorOrNull(ModuleContext moduleContext, Type mainType)
    {
        var candidates = mainType.GetConstructors(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        switch (candidates.Length)
        {
            case > 1:
                throw new MultipleTypeConstructorsException(moduleContext, mainType.FullName());
        }

        var constructor = candidates.SingleOrDefault();
        return constructor;
    }

    private static object ResolveType(
        ModuleContext moduleContext,
        ConstructorInfo? constructor,
        Type mainType,
        IServiceProvider provider,
        IEnumerable<object> additionalArguments)
    {
        // ReSharper disable once ReturnTypeCanBeNotNullable
        object? ResolveInstance(
            Type type,
            IEnumerable<(object value, Type type)> additionalArgumentsTypes)
        {
            if (type.IsEquivalentTo(typeof(ModuleContext)))
                return moduleContext;
            if (type.IsEquivalentTo(typeof(Guid)))
                return moduleContext.Guid;
            if (additionalArgumentsTypes.FirstOrDefault((q) => q.type.IsEquivalentTo(type)) is object obj)
                return obj;
            return provider.GetService(type);
        }

        object instance;
        if (constructor is null)
        {
            instance = mainType.CreateInstance(Type.EmptyTypes, Array.Empty<object>());
        }
        else
        {
            var additionalArgumentsTypes =
                additionalArguments.Select((q) => (value: q, type: q.GetType())).ToArray();
            var parameters = constructor.GetParameters();
            var services = parameters.Select(
                    (parameterInfo) => (parameterInfo,
                        value: ResolveInstance(parameterInfo.ParameterType, additionalArgumentsTypes)))
                .ToImmutableArray();
            var nullViolatingServices = services
                .Indexed()
                .Where((tuple) => tuple.value.value is null)
                .Where((tuple) => tuple.value.parameterInfo.IsNullable())
                .ToImmutableArray();
            if (nullViolatingServices.Any())
            {
                throw new CannotResolveDependenciesException(
                    moduleContext,
                    nullViolatingServices
                        .Select((tuple) => (tuple.index, tuple.value.parameterInfo.ParameterType))
                        .ToImmutableArray());
            }

            instance = mainType.CreateInstanceWithUncached(
                services.Select((tuple) => tuple.parameterInfo.ParameterType).ToArray(),
                services.Select((tuple) => tuple.value).ToArray());
        }

        return instance;
    }
}