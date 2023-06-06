using System;
using System.Reflection;

namespace QuantumCore.Core.Networking;

public class FieldCache
{
    public PropertyInfo Property { get; }
    public FieldAttribute Attribute { get; }
    public int Position { get; set; }
    public Type FieldType { get; set; }

    public uint ArrayLength {
        get {
            return (uint)(Attribute?.ArrayLength ?? 0);
        }
    }

    public uint Length {
        get {
            return (uint)(Attribute?.Length ?? 0);
        }
    }

    public Type EnumType {
        get {
            return Attribute?.EnumType;
        }
    }

    public FieldCache()
    {
            
    }

    public FieldCache(PropertyInfo property)
    {
        Property = property;
        Attribute = property.GetCustomAttribute<FieldAttribute>();
        if (Attribute == null)
        {
            throw new ArgumentException("Must have the attribute Field", nameof(property));
        }

        Position = Attribute.Position;
        FieldType = Property.PropertyType;
    }

    public virtual object GetValue(object obj)
    {
        return Property.GetValue(obj);
    }

    public virtual void SetValue(object obj, object value)
    {
        Property.SetValue(obj, value);
    }
}