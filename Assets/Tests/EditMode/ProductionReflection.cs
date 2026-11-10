using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public static class ProductionReflection
{
    public static Type TypeOf(string typeName)
    {
        Type type = Type.GetType(typeName + ", Assembly-CSharp");

        Assert.IsNotNull(type, $"{typeName} must exist in Assembly-CSharp.");

        return type;
    }

    public static object New(string typeName, params object[] args)
    {
        return Activator.CreateInstance(TypeOf(typeName), args);
    }

    public static object EnumValue(string enumTypeName, string member)
    {
        return Enum.Parse(TypeOf(enumTypeName), member);
    }

    public static object Call(object instance, string methodName, params object[] args)
    {
        return Invoke(instance.GetType(), instance, methodName, args);
    }

    public static object CallStatic(string typeName, string methodName, params object[] args)
    {
        return Invoke(TypeOf(typeName), null, methodName, args);
    }

    public static object Property(object instance, string propertyName)
    {
        return instance
            .GetType()
            .GetProperty(propertyName)
            .GetValue(instance);
    }

    public static object StaticProperty(string typeName, string propertyName)
    {
        return TypeOf(typeName)
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static)
            .GetValue(null);
    }

    public static object Field(object instance, string fieldName)
    {
        return instance
            .GetType()
            .GetField(fieldName)
            .GetValue(instance);
    }

    public static void SetField(object instance, string fieldName, object value)
    {
        instance
            .GetType()
            .GetField(fieldName)
            .SetValue(instance, value);
    }

    private static object Invoke(
        Type type,
        object instance,
        string methodName,
        object[] args)
    {
        MethodInfo method = type
            .GetMethods()
            .FirstOrDefault(candidate =>
            {
                if (candidate.Name != methodName)
                    return false;

                ParameterInfo[] parameters = candidate.GetParameters();

                if (parameters.Length != args.Length)
                    return false;

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (args[i] != null &&
                        !parameters[i].ParameterType.IsInstanceOfType(args[i]))
                    {
                        return false;
                    }
                }

                return true;
            });

        Assert.IsNotNull(method, $"{type.Name}.{methodName} must exist.");

        return method.Invoke(instance, args);
    }
}
