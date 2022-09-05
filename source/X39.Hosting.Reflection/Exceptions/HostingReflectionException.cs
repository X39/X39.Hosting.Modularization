using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace X39.Hosting.Reflection.Exceptions;

/// <summary>
/// Base class for any <see cref="Exception"/> thrown by this library.
/// </summary>
[PublicAPI]
public abstract class HostingReflectionException : Exception
{
    protected HostingReflectionException()
    {
    }

    protected HostingReflectionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected HostingReflectionException(string? message) : base(message)
    {
    }

    protected HostingReflectionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}