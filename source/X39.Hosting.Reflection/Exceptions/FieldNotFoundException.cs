using System.Reflection;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using X39.Util;

namespace X39.Hosting.Reflection.Exceptions;

/// <summary>
/// Thrown when a field (<see cref="MissingField"/>) could not be found on the <see cref="ReflectedType"/>.
/// </summary>
[PublicAPI]
public class FieldNotFoundException : HostingReflectionException
{
    /// <summary>
    /// The <see cref="FieldInfo"/> not present in <see cref="ReflectedType"/>.
    /// </summary>
    public string MissingField { get; }

    /// <summary>
    /// The <see cref="Type"/> which is missing <see cref="MissingField"/>.
    /// </summary>
    public Type ReflectedType { get; }

    public FieldNotFoundException(Type type, string fieldName)
        : base($"The field {fieldName} could not be found on {type.FullName()}")
    {
        ReflectedType = type;
        MissingField  = fieldName;
    }

    protected FieldNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        MissingField = info.GetString(nameof(MissingField)) ?? "SERIALIZATION_ERROR";
        ReflectedType = Type.GetType(info.GetString(nameof(ReflectedType)) ?? typeof(void).AssemblyQualifiedName!)
                        ?? typeof(void);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(MissingField), MissingField);
        info.AddValue(nameof(ReflectedType), ReflectedType.AssemblyQualifiedName);
        base.GetObjectData(info, context);
    }
}