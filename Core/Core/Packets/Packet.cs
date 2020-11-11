using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public class Packet : Attribute
    {
        public Packet(byte header, EDirection direction)
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

        public PacketCache(byte header, Type type)
        {
            Header = header;
            Type = type;

            CalculateSize();
        }

        public byte Header { get; }
        public Type Type { get; }
        public uint Size { get; private set; }

        private void WriteField(object value, Type type, BinaryWriter bw, Field field)
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
            var ret = new byte[Size];
            if (obj == null) return ret;
            
            if (obj.GetType() != Type) throw new ArgumentException("Invalid packet given", nameof(obj));
            
            using (var ms = new MemoryStream(ret))
            {
                using (var bw = new BinaryWriter(ms))
                {
                    // Write header
                    if(Header > 0) bw.Write(Header);

                    foreach (var field in _properties)
                    {
                        var attr = field.GetCustomAttribute<Field>();
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
                }
            }

            return ret;
        }

        public void Deserialize(object obj, byte[] data)
        {
            if (data.Length != Size - 1) throw new ArgumentException("Invalid data stream given", nameof(data));
            if (obj.GetType() != Type) throw new ArgumentException("Invalid packet given", nameof(obj));

            using (var ms = new MemoryStream(data))
            {
                using (var br = new BinaryReader(ms))
                {
                    foreach (var field in _properties)
                    {
                        var type = field.PropertyType;
                        var attribute = field.GetCustomAttribute<Field>();
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
            }
        }

        private void CalculateSize()
        {
            var fields = Type.GetProperties().Where(field => field.GetCustomAttribute<Field>() != null)
                .OrderBy(field => field.GetCustomAttribute<Field>().Position);
            Size = Header > 0 ? 1u : 0u;
            var packetAttribute = Type.GetCustomAttribute<Packet>();
            if (packetAttribute != null)
            {
                if (packetAttribute.Sequence) Size++;
            }

            foreach (var field in fields)
            {
                var type = field.PropertyType;
                var attribute = field.GetCustomAttribute<Field>();
                if(attribute == null) continue;
                
                uint multiplier = 1;

                if (type.IsArray)
                {
                    type = type.GetElementType();
                    multiplier = (uint) attribute.ArrayLength;
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
    public class Field : Attribute
    {
        public Field(int position)
        {
            Position = position;
        }

        public int Position { get; set; }
        public int Length { get; set; } = -1;
        public int ArrayLength { get; set; } = -1;
    }
}