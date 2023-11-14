using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuantumCore.Core.Utils;

public static class ReflectionUtils
{
    public static MethodInfo[] GetExtensionMethods(this Type t)
    {
        var types = new List<Type>();

        foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
        {
            types.AddRange(item.GetTypes());
        }

        var query = from type in types
            where type.IsSealed && !type.IsGenericType && !type.IsNested
            from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            where method.IsDefined(typeof(ExtensionAttribute), false)
            where method.GetParameters()[0].ParameterType == t
            select method;
        return query.ToArray();
    }
}