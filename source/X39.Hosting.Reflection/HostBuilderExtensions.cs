using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using X39.Hosting.Reflection.Abstraction;
using X39.Hosting.Reflection.Exceptions;

namespace X39.Hosting.Reflection;

/// <summary>
/// Offers extensions to the <see cref="IHostBuilder"/>.
/// The extensions expect the default HostBuilder to be used.
/// </summary>
[PublicAPI]
public static class HostBuilderExtensions
{
    /// <summary>
    /// Creates a <see cref="IReflectedServiceFactoryAdapter"/> from the given <paramref name="hostBuilder"/>.
    /// </summary>
    /// <remarks>
    /// This method is breaking encapsulation by utilizing reflection!
    /// Please only use it if you know what you are doing.
    /// </remarks>
    /// <param name="hostBuilder">A microsoft HostBuilder instance.</param>
    /// <returns>A reflected version of the IServiceFactoryAdapter.</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static IReflectedServiceFactoryAdapter GetServiceFactoryAdapter(this IHostBuilder hostBuilder)
    {
        var serviceProviderFactory = hostBuilder.GetPrivateFieldValue<object>(
            Constants.HostBuilder.Fields.ServiceProviderFactory);
        if (serviceProviderFactory is null)
            throw new ServiceProviderFactoryNotFoundException();
        return new ReflectedServiceFactoryAdapter(serviceProviderFactory);
    }
}