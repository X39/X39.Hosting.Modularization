using System.Reflection;
using X39.Hosting.Reflection.Exceptions;

namespace X39.Hosting.Reflection;

internal static class Reflection
{
    public static T? GetPrivateFieldValue<T>(this object source, string fieldName)
    {
        var type = source.GetType();
        var field = type.GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (field is null)
            throw new FieldNotFoundException(type, fieldName);
        var value = field.GetValue(source);
        return value switch
        {
            T t  => t,
            null => default,
            _    => throw new InvalidFieldTypeException(type, fieldName, typeof(T), value.GetType())
        };
    }

    public static T? CallPrivateMethod<T>(this object source, string methodName, params object[] args)
    {
        var type = source.GetType();
        var method = type.GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (method is null)
            throw new FieldNotFoundException(type, methodName);
        var value = method.Invoke(source, args);
        return value switch
        {
            T t  => t,
            null => default,
            _    => throw new InvalidMethodTypeException(type, methodName, typeof(T), value.GetType())
        };
    }
}