using System.Runtime.Serialization;

namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Represents errors that occur inside the modularization framework.
/// </summary>
[PublicAPI]
public abstract class ModularizationException : Exception
{
    /// <inheritdoc />
    internal ModularizationException()
    {
    }

    /// <inheritdoc />
    internal ModularizationException(string? message) : base(message)
    {
    }

    /// <inheritdoc />
    internal ModularizationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <inheritdoc />
    internal ModularizationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}