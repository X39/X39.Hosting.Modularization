using System.Reflection;
using System.Runtime.Serialization;
using X39.Util;

namespace X39.Hosting.Reflection.Exceptions;

/// <summary>
/// Thrown when a method was found on the <see cref="ReflectedType"/>
/// but the <see cref="ActualValueType"/> is not matching the <see cref="ExpectedValueType"/>.
/// </summary>
public class InvalidMethodTypeException : HostingReflectionException
{
    /// <summary>
    /// The <see cref="MethodInfo"/> not present in <see cref="ReflectedType"/>.
    /// </summary>
    public string MissingMethod { get; }

    /// <summary>
    /// The <see cref="Type"/> which is missing <see cref="MissingMethod"/>.
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

    public InvalidMethodTypeException(Type type, string methodName, Type expectedValueType, Type actualValueType)
        : base($"The method {methodName} was found on {type.FullName()} but the value returned is not of type " +
               $"{expectedValueType.FullName()} but instead was {actualValueType.FullName()}")
    {
        ReflectedType     = type;
        MissingMethod     = methodName;
        ExpectedValueType = expectedValueType;
        ActualValueType   = actualValueType;
    }

    protected InvalidMethodTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        MissingMethod = info.GetString(nameof(MissingMethod)) ?? "SERIALIZATION_ERROR";
        ReflectedType = Type.GetType(info.GetString(nameof(ReflectedType)) ?? typeof(void).AssemblyQualifiedName!)
                        ?? typeof(void);
        ExpectedValueType = Type.GetType(info.GetString(nameof(ExpectedValueType)) ?? typeof(void).AssemblyQualifiedName!)
                            ?? typeof(void);
        ActualValueType = Type.GetType(info.GetString(nameof(ActualValueType)) ?? typeof(void).AssemblyQualifiedName!)
                          ?? typeof(void);
        
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(MissingMethod), MissingMethod);
        info.AddValue(nameof(ReflectedType), ReflectedType.AssemblyQualifiedName);
        info.AddValue(nameof(ExpectedValueType), ExpectedValueType.AssemblyQualifiedName);
        info.AddValue(nameof(ActualValueType), ActualValueType.AssemblyQualifiedName);
        base.GetObjectData(info, context);
    }
}