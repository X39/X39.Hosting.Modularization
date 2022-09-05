using Microsoft.Extensions.DependencyInjection;

namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when the <see cref="HostExtensions.UseModularization(Microsoft.Extensions.Hosting.IHostBuilder,string,string[])"/>
/// method was used but the DI container used is not of type <see cref="IServiceProviderFactory{TContainerBuilder}"/>
/// </summary>
[PublicAPI]
public class FailedToGetServiceProviderFactoryException : Exception
{
    /// <summary>
    /// The type that was found and is not implementing the expected interface.
    /// </summary>
    public Type TypeFound { get; }
    internal FailedToGetServiceProviderFactoryException(Type type)
    {
        TypeFound = type;
    }
}