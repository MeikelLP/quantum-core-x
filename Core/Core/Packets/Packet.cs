using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;

namespace QuantumCore.Core.Packets
{
    [Flags]
    public enum EDirection
    {
        Incoming = 1,
        Outgoing = 2
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class PacketAttribute : Attribute
    {
        public PacketAttribute(byte header, EDirection direction)
        {
            Header = header;
            Direction = direction;
        }

        public byte Header { get; set; }
        public EDirection Direction { get; set; }
        public bool Sequence { get; set; }
    }

    public class PacketCache
    {
        private readonly List<PropertyInfo> _properties = new List<PropertyInfo>();
        private readonly Dictionary<int, PacketCache> _subTypes = new Dictionary<int, PacketCache>();
        private PropertyInfo _dynamicValueProperty;
        private PropertyInfo _dynamicSizeProperty;

        public PacketCache(byte header, Type type)
        {
            Header = header;
            Type = type;

            CalculateSize();
        }

        public byte Header { get; }
        public Type Type { get; }
        public uint Size { get; private set; }
        public bool IsDynamic { get; private set; }
        public bool HasSequence { get; private set; }

        private void WriteField(object value, Type type, BinaryWriter bw, FieldAttribute field)
        {
            if (type == typeof(uint))
                bw.Write((uint) value);
            else if (type == typeof(int))
                bw.Write((int) value);
            else if (type == typeof(ushort)) 
                bw.Write((ushort) value);
            else if (type == typeof(short))
                bw.Write((short) value);
            else if (type == typeof(byte))
                bw.Write((byte) value);
            else if (type == typeof(float))
                bw.Write((float) value);
            else if (type == typeof(string))
            {
                var str = (string) value;
                for (var i = 0; i < field.Length; i++)
                {
                    bw.Write(str != null && str.Length >= i + 1 ? str[i] : '\0');
                }
            }
            else if (type.IsClass)
            {
                var subType = _subTypes[field.Position];
                bw.Write(subType.Serialize(value));
            }
            else
                Debug.Assert(false);
        }
        
        public byte[] Serialize([CanBeNull] object obj)
        {
            Debug.Assert(Size > 0);

            uint DataSize = Size;
            if (IsDynamic)
                DataSize = GetDynamicSize(obj);

            var ret = new byte[DataSize];

            if (obj == null) return ret;          

            if (obj.GetType() != Type) throw new ArgumentException("Invalid packet given", nameof(obj));

            using var ms = new MemoryStream(ret);
            using var bw = new BinaryWriter(ms, Encoding.ASCII);
            
            // Write header
            if(Header > 0) bw.Write(Header);

            foreach (var field in _properties)
            {
                var attr = field.GetCustomAttribute<FieldAttribute>();
                if(attr == null) continue;
                        
                var type = field.PropertyType;

                if (type.IsArray)
                {
                    var array = (Array) field.GetValue(obj);
                    for (var i = 0; i < attr.ArrayLength; i++)
                    {
                        WriteField(array.GetValue(i), type.GetElementType(), bw, attr);
                    }   
                }
                else
                {
                    WriteField(field.GetValue(obj), type, bw, attr);
                }
            }

            if (IsDynamic)
            {
                if (_dynamicValueProperty.PropertyType == typeof(string))
                {
                    var msg = (string)_dynamicValueProperty.GetValue(obj);
                    var chars = Encoding.UTF8.GetBytes(msg);
                    bw.Write(chars);
                    bw.Write((byte)0);
                }
                else
                {
                    Debug.Assert(false);
                }
            }

            return ret;
        }

        public void Deserialize(object obj, byte[] data)
        {
            if (data.Length != Size - 1) throw new ArgumentException("Invalid data stream given", nameof(data));
            if (obj.GetType() != Type) throw new ArgumentException("Invalid packet given", nameof(obj));

            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms, Encoding.ASCII);
            
            foreach (var field in _properties)
            {
                var type = field.PropertyType;
                var attribute = field.GetCustomAttribute<FieldAttribute>();
                var multiplier = 1;
                Array array = null;

                if (type.IsArray)
                {
                    type = type.GetElementType();
                    multiplier = attribute.ArrayLength;
                    array = Array.CreateInstance(type, multiplier);
                }

                for (var i = 0; i < multiplier; i++)
                {
                    object value = null;
                    if (type == typeof(uint))
                    {
                        value = br.ReadUInt32();
                    }
                    else if (type == typeof(int))
                    {
                        value = br.ReadInt32();
                    }
                    else if (type == typeof(ushort))
                    {
                        value = br.ReadUInt16();
                    }
                    else if (type == typeof(short))
                    {
                        value = br.ReadInt16();
                    }
                    else if (type == typeof(float))
                    {
                        value = br.ReadSingle();
                    }
                    else if (type == typeof(byte))
                    {
                        value = br.ReadByte();
                    }
                    else if (type == typeof(string))
                    {
                        var chars = br.ReadChars(attribute.Length);
                        var idx = Array.IndexOf(chars, '\0');
                        value = new string(chars, 0, idx < 0 ? chars.Length : idx);
                    }
                    else if (type == typeof(float))
                    {
                        value = br.ReadSingle();
                    }
                    else
                    {
                        Debug.Assert(false);
                    }

                    if (array != null)
                        array.SetValue(value, i);
                    else
                        field.SetValue(obj, value);
                }

                if (array != null) field.SetValue(obj, array);
            }
        }
        public void UpdateDynamicSize(object packet, uint packet_size)
        {
            if (_dynamicValueProperty.PropertyType == typeof(string))
            {
                var msg = (string)_dynamicValueProperty.GetValue(packet);
                _dynamicSizeProperty.SetValue(packet, (ushort)((packet_size + msg.Length + 1) & 0xFFFF));
                
            }
            else
            {
                Debug.Assert(false);
            }

            var sizeField = Type.GetProperties().FirstOrDefault(field => field.GetCustomAttribute<SizeAttribute>() != null);
            Debug.Assert(sizeField != null);
            sizeField.SetValue(packet, _dynamicSizeProperty.GetValue(packet));
        }

        public void DeserializeDynamic(object packet, byte[] data)
        {
            Debug.Assert(IsDynamic);
            Debug.Assert(_dynamicValueProperty != null);
            
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms, Encoding.ASCII);

            if (_dynamicValueProperty.PropertyType == typeof(string))
            {
                var chars = br.ReadChars(data.Length);
                var idx = Array.IndexOf(chars, '\0');
                var value = new string(chars, 0, idx < 0 ? chars.Length : idx);
                _dynamicValueProperty.SetValue(packet, value);
            }
            else
            {
                Debug.Assert(false);
            }
        }

        public ushort GetDynamicSize(object packet)
        {
            Debug.Assert(IsDynamic);
            Debug.Assert(_dynamicSizeProperty != null);

            var dynSize = _dynamicSizeProperty.GetValue(packet);
            if (dynSize == null) return 0;

            return (ushort) dynSize;
        }

        private void CalculateSize()
        {
            var fields = Type.GetProperties().Where(field => field.GetCustomAttribute<FieldAttribute>() != null)
                .OrderBy(field => field.GetCustomAttribute<FieldAttribute>().Position);
            Size = Header > 0 ? 1u : 0u;
            var packetAttribute = Type.GetCustomAttribute<PacketAttribute>();
            if (packetAttribute != null)
            {
                HasSequence = packetAttribute.Sequence;
            }

            // Check if we have a dynamic field
            var dynamicField =
                Type.GetProperties().FirstOrDefault(field => field.GetCustomAttribute<DynamicAttribute>() != null);
            if (dynamicField != null)
            {
                IsDynamic = true;
                _dynamicValueProperty = dynamicField;

                var sizeField = Type.GetProperties()
                    .FirstOrDefault(field => field.GetCustomAttribute<SizeAttribute>() != null);
                Debug.Assert(sizeField != null);
                _dynamicSizeProperty = sizeField;
            }
            
            foreach (var field in fields)
            {
                var type = field.PropertyType;
                var attribute = field.GetCustomAttribute<FieldAttribute>();
                if(attribute == null) continue;
                
                uint multiplier = 1;

                if (type.IsArray)
                {
                    type = type.GetElementType();
                    multiplier = (uint) attribute.ArrayLength;
                    Debug.Assert(multiplier > 0);
                }

                if (type == typeof(uint) || type == typeof(int))
                {
                    Size += 4 * multiplier;
                }
                else if (type == typeof(ushort) || type == typeof(short))
                {
                    Size += 2 * multiplier;
                }
                else if (type == typeof(byte))
                {
                    Size += 1 * multiplier;
                }
                else if (type == typeof(string))
                {
                    Debug.Assert(attribute.Length > 0);
                    Size += (uint) attribute.Length * multiplier;
                }
                else if (type == typeof(float))
                {
                    Size += 4 * multiplier;
                }
                else if (type != null && type.IsClass)
                {
                    var subType = new PacketCache(0, type);
                    _subTypes[attribute.Position] = subType;
                    Size += subType.Size * multiplier;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Packet {Type.Name} contains invalid type {type.Name} for property {field.Name}!");
                }

                _properties.Add(field);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {
        public FieldAttribute(int position)
        {
            Position = position;
        }

        public int Position { get; set; }
        public int Length { get; set; } = -1;
        public int ArrayLength { get; set; } = -1;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SizeAttribute : Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DynamicAttribute : Attribute
    {
        
    }
}