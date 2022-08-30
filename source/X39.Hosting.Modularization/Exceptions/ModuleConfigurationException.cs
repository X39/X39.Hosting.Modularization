using System.Runtime.Serialization;
using X39.Hosting.Modularization.Configuration;

namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Specialization for <see cref="ModuleConfiguration"/> related exceptions
/// </summary>
[PublicAPI]
public abstract class ModuleConfigurationException : ModularizationException
{
    /// <inheritdoc />
    internal ModuleConfigurationException()
    {
    }

    /// <inheritdoc />
    internal ModuleConfigurationException(string? message) : base(message)
    {
    }

    /// <inheritdoc />
    internal ModuleConfigurationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <inheritdoc />
    internal ModuleConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}