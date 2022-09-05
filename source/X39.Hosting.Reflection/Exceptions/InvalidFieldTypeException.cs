using System.Reflection;
using System.Runtime.Serialization;
using X39.Util;

namespace X39.Hosting.Reflection.Exceptions;

/// <summary>
/// Thrown when a field was found on the <see cref="ReflectedType"/>
/// but the <see cref="ActualValueType"/> is not matching the <see cref="ExpectedValueType"/>.
/// </summary>
public class InvalidFieldTypeException : HostingReflectionException
{
    /// <summary>
    /// The <see cref="FieldInfo"/> not present in <see cref="ReflectedType"/>.
    /// </summary>
    public string MissingField { get; }

    /// <summary>
    /// The <see cref="Type"/> which is missing <see cref="MissingField"/>.
    /// </summary>
    public Type ReflectedType { get; }

    /// <summary>
    /// The <see cref="Type"/> of the value that was expected.
    /// </summary>
    public Type ExpectedValueType { get; }

    /// <summary>
    /// The <see cref="Type"/> of the value.
    /// </summary>
    public Type ActualValueType { get; }

    public InvalidFieldTypeException(Type type, string fieldName, Type expectedValueType, Type actualValueType)
        : base($"The field {fieldName} was found on {type.FullName()} but the value returned is not of type " +
               $"{expectedValueType.FullName()} but instead was {actualValueType.FullName()}")
    {
        ReflectedType     = type;
        MissingField      = fieldName;
        ExpectedValueType = expectedValueType;
        ActualValueType   = actualValueType;
    }

    protected InvalidFieldTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        MissingField = info.GetString(nameof(MissingField)) ?? "SERIALIZATION_ERROR";
        ReflectedType = Type.GetType(info.GetString(nameof(ReflectedType)) ?? typeof(void).AssemblyQualifiedName!)
                        ?? typeof(void);
        ExpectedValueType = Type.GetType(info.GetString(nameof(ExpectedValueType)) ?? typeof(void).AssemblyQualifiedName!)
                            ?? typeof(void);
        ActualValueType = Type.GetType(info.GetString(nameof(ActualValueType)) ?? typeof(void).AssemblyQualifiedName!)
                          ?? typeof(void);
        
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(MissingField), MissingField);
        info.AddValue(nameof(ReflectedType), ReflectedType.AssemblyQualifiedName);
        info.AddValue(nameof(ExpectedValueType), ExpectedValueType.AssemblyQualifiedName);
        info.AddValue(nameof(ActualValueType), ActualValueType.AssemblyQualifiedName);
        base.GetObjectData(info, context);
    }
}