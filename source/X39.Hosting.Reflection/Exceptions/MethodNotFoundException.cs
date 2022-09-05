using System.Reflection;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using X39.Util;

namespace X39.Hosting.Reflection.Exceptions;

/// <summary>
/// Thrown when a method (<see cref="MissingMethod"/>) could not be found on the <see cref="ReflectedType"/>.
/// </summary>
[PublicAPI]
public class MethodNotFoundException : HostingReflectionException
{
    /// <summary>
    /// The <see cref="MethodInfo"/> not present in <see cref="ReflectedType"/>.
    /// </summary>
    public string MissingMethod { get; }

    /// <summary>
    /// The <see cref="Type"/> which is missing <see cref="MissingMethod"/>.
    /// </summary>
    public Type ReflectedType { get; }

    public MethodNotFoundException(Type type, string methodName)
        : base($"The method {methodName} could not be found on {type.FullName()}")
    {
        ReflectedType = type;
        MissingMethod = methodName;
    }

    protected MethodNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        MissingMethod = info.GetString(nameof(MissingMethod)) ?? "SERIALIZATION_ERROR";
        ReflectedType = Type.GetType(info.GetString(nameof(ReflectedType)) ?? typeof(void).AssemblyQualifiedName!)
                        ?? typeof(void);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(MissingMethod), MissingMethod);
        info.AddValue(nameof(ReflectedType), ReflectedType.AssemblyQualifiedName);
        base.GetObjectData(info, context);
    }
}