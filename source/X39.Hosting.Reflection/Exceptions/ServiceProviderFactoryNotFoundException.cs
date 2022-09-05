namespace X39.Hosting.Reflection.Exceptions;

/// <summary>
/// Fatal <see cref="Exception"/> that gets raised when a service provider factory could not be found.
/// </summary>
public class ServiceProviderFactoryNotFoundException : HostingReflectionException
{
    public ServiceProviderFactoryNotFoundException()
        : base(
            "Failed to receive ServiceProviderFactory from HostBuilder. " +
            "This indicates that either you are using a non-default IHostBuilder instance " +
            "or that the ABI for the underlying type has changed. " +
            "Please check that you are indeed using the Microsoft.Extensions.Hosting.HostBuilder or, " +
            "given you are using that type, raise a bug against the library ASAP.")
    {
    }
}

/// <summary>
/// Fatal <see cref="Exception"/> thrown when a reflected value is null but was required to be not null.
/// </summary>
public class ReflectedNullException : HostingReflectionException
{
    public ReflectedNullException(string message)
        : base(
            string.Concat(
                message,
                " Failed to resolve a reflected value. Please raise a bug against the library ASAP."))
    {
    }
}