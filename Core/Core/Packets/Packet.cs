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

    class FieldCache
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

    class SubHeaderField : FieldCache
    {
        public byte SubHeader { get; set; }
        
        public SubHeaderField(byte subHeader, int position)
        {
            SubHeader = subHeader;
            Position = position;
            FieldType = typeof(byte);
        }

        public override object GetValue(object obj)
        {
            return SubHeader;
        }

        public override void SetValue(object obj, object value)
        {
            Debug.Assert(false);
        }
    }

    public class PacketCache
    {
        private readonly List<FieldCache> _properties = new List<FieldCache>();
        private readonly Dictionary<int, PacketCache> _subTypes = new Dictionary<int, PacketCache>();
        private PropertyInfo _dynamicValueProperty;
        private PropertyInfo _dynamicSizeProperty;
        private SubPacketAttribute _subPacketAttribute;

        private PacketCache()
        {
            
        }
        
        public PacketCache(byte header, Type type, int untilPosition = -1)
        {
            Header = header;
            Type = type;

            CalculateSize(untilPosition);
        }

        public byte Header { get; private set; }
        public byte SubHeader { get; set; }
        public Type Type { get; private set; }
        public uint Size { get; private set; }
        public bool IsDynamic { get; private set; }
        public bool IsSubHeader { get; private set; }
        public bool HasSequence { get; private set; }

        private void WriteField(object value, Type type, BinaryWriter bw, FieldCache field)
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

            var size = Size;
            if (IsDynamic)
            {
                size = CalculateTotalSize(obj);
            }

            if (_dynamicSizeProperty != null)
            {
                _dynamicSizeProperty.SetValue(obj, (ushort) size);
            }

            var ret = new byte[size];

            if (obj == null) return ret;          

            if (obj.GetType() != Type) throw new ArgumentException("Invalid packet given", nameof(obj));

            using var ms = new MemoryStream(ret);
            using var bw = new BinaryWriter(ms, Encoding.ASCII);
            
            // Write header
            if(Header > 0) bw.Write(Header);

            foreach (var field in _properties)
            {
                var type = field.FieldType;

                if (type.IsArray)
                {
                    var array = (Array) field.GetValue(obj);
                    for (var i = 0; i < field.ArrayLength; i++)
                    {
                        // todo: implement array of an enum
                        WriteField(array.GetValue(i), type.GetElementType(), bw, field);
                    }   
                }
                else
                {
                    // Use the field type or if the field type is an enum use the enum type defined at the field attribute
                    WriteField(field.GetValue(obj), type.IsEnum ? field.EnumType : type, bw, field);
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
                else if (_dynamicValueProperty.PropertyType == typeof(byte[]))
                {
                    var buffer = (byte[]) _dynamicValueProperty.GetValue(obj);
                    bw.Write(buffer);
                }
                else
                {
                    Debug.Assert(false);
                }
            }

            return ret;
        }

        public byte Deserialize(object obj, byte[] data)
        {
            var expectedSize = Size;
            if (Header != 0)
            {
                expectedSize--;
            }
            
            if (data.Length != expectedSize) throw new ArgumentException("Invalid data stream given", nameof(data));
            if (obj.GetType() != Type) throw new ArgumentException("Invalid packet given", nameof(obj));

            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms, Encoding.ASCII);

            byte subHeader = 0;
            
            foreach (var field in _properties)
            {
                var type = field.FieldType;
                var multiplier = 1;
                Array array = null;

                if (type.IsArray)
                {
                    type = type.GetElementType();
                    multiplier = (int) field.ArrayLength;
                    array = Array.CreateInstance(type, multiplier);
                }

                if (type.IsEnum)
                {
                    type = field.EnumType;
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
                        var chars = br.ReadChars((int) field.Length);
                        var idx = Array.IndexOf(chars, '\0');
                        value = new string(chars, 0, idx < 0 ? chars.Length : idx);
                    }
                    else if (type == typeof(float))
                    {
                        value = br.ReadSingle();
                    }
                    else if (type.IsClass)
                    {
                        var subType = _subTypes[field.Position];
                        var instance = Activator.CreateInstance(type);
                        subType.Deserialize(instance, br.ReadBytes((int) subType.Size));
                        value = instance;
                    }
                    else
                    {
                        Debug.Assert(false);
                    }

                    if (array != null)
                    {
                        array.SetValue(value, i);
                    }
                    else
                    {
                        if (IsSubHeader && field is SubHeaderField)
                        {
                            subHeader = (byte) value;
                        }
                        else
                        {
                            field.SetValue(obj, value);
                        }
                    }
                }

                if (array != null) field.SetValue(obj, array);
            }

            return subHeader;
        }
        
        public void UpdateDynamicSize(object packet, uint packetSize)
        {
            if (_dynamicValueProperty.PropertyType == typeof(string))
            {
                var msg = (string)_dynamicValueProperty.GetValue(packet);
                _dynamicSizeProperty.SetValue(packet, (ushort)((packetSize + msg.Length + 1) & 0xFFFF));
            }
            else if (_dynamicValueProperty.PropertyType == typeof(byte[]))
            {
                var buffer = (byte[]) _dynamicValueProperty.GetValue(packet);
                _dynamicSizeProperty.SetValue(packet, (ushort) (packetSize + buffer.Length));
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
            return (ushort) dynSize;
        }

        public uint CalculateTotalSize(object packet)
        {
            Debug.Assert(IsDynamic);
            Debug.Assert(_dynamicValueProperty != null);

            var dynSize = _dynamicValueProperty.GetValue(packet);
            if (dynSize == null)
            {
                return Size;
            }
            
            if(dynSize is string dynString)
            {
                return Size + (ushort)(dynString.Length + 1);
            }

            if (dynSize is byte[] buffer)
            {
                return Size + (ushort) buffer.Length;
            }

            Debug.Assert(false);
            return Size;
        }

        private void CalculateSize(int untilPosition)
        {
            var fields = Type.GetProperties().Where(field => field.GetCustomAttribute<FieldAttribute>() != null)
                .OrderBy(field => field.GetCustomAttribute<FieldAttribute>().Position)
                .Select(field => new FieldCache(field))
                .ToList();
            
            Size = Header > 0 ? 1u : 0u;
            var packetAttribute = Type.GetCustomAttribute<PacketAttribute>();
            if (packetAttribute != null)
            {
                HasSequence = packetAttribute.Sequence;
            }

            var subPacketAttribute = Type.GetCustomAttribute<SubPacketAttribute>();
            if (subPacketAttribute != null)
            {
                _subPacketAttribute = subPacketAttribute;
                
                // Insert sub header field
                var position = subPacketAttribute.Position;
                fields.Insert(position, new SubHeaderField(subPacketAttribute.SubHeader, position));
                IsSubHeader = true;
                SubHeader = subPacketAttribute.SubHeader;
            }
            
            // Check if we have a size field
            var sizeField = Type.GetProperties()
                .FirstOrDefault(field => field.GetCustomAttribute<SizeAttribute>() != null);
            if (sizeField != null)
            {
                _dynamicSizeProperty = sizeField;
            }

            // Check if we have a dynamic field
            var dynamicField =
                Type.GetProperties().FirstOrDefault(field => field.GetCustomAttribute<DynamicAttribute>() != null);
            if (dynamicField != null)
            {
                IsDynamic = true;
                _dynamicValueProperty = dynamicField;
                
                Debug.Assert(_dynamicSizeProperty != null);
            }
            
            foreach (var field in fields)
            {
                var type = field.FieldType;
                uint multiplier = 1;

                if (untilPosition > -1 && field.Position > untilPosition)
                {
                    continue;
                }

                if (type.IsArray)
                {
                    type = type.GetElementType();
                    multiplier = field.ArrayLength;
                    Debug.Assert(multiplier > 0);
                }

                if (type.IsEnum)
                {
                    type = field.EnumType;
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
                    Debug.Assert(field.Length > 0);
                    Size += field.Length * multiplier;
                }
                else if (type == typeof(float))
                {
                    Size += 4 * multiplier;
                }
                else if (type != null && type.IsClass)
                {
                    var subType = new PacketCache(0, type);
                    _subTypes[field.Position] = subType;
                    Size += subType.Size * multiplier;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Packet {Type.Name} contains invalid type {type.Name} for property {field.Property.Name}!");
                }

                _properties.Add(field);
            }
        }

        public PacketCache CreateGeneralCache()
        {
            Debug.Assert(_subPacketAttribute != null);
            return new PacketCache(Header, Type, _subPacketAttribute.Position) {
                HasSequence = false
            };
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
        public Type EnumType { get; set; } = null;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SizeAttribute : Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DynamicAttribute : Attribute
    {
        
    }
    
    /// <summary>
    /// Marks the packet to be a sub packet, if the position is not 0 all fields before the position
    /// HAVE to match with all other packets of the same header!
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SubPacketAttribute : Attribute
    {
        public byte SubHeader { get; set; }
        public int Position { get; set; }
        
        public SubPacketAttribute(byte subHeader, int position)
        {
            SubHeader = subHeader;
            Position = position;
        }
    }
}