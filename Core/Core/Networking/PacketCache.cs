using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using QuantumCore.Extensions;

namespace QuantumCore.Core.Networking;

public class PacketCache : IPacketCache
{
    public bool HasHeader => true;
    public FieldCache[] Fields { get; }
    private readonly Dictionary<int, PacketCache> _subTypes = new Dictionary<int, PacketCache>();
    private PropertyInfo _dynamicValueProperty;
    private PropertyInfo _dynamicSizeProperty;
    private SubPacketAttribute _subPacketAttribute;
        
    public PacketCache(byte header, Type type, int untilPosition = -1)
    {
        Header = header;
        Type = type;

        var packetAttribute = type.GetCustomAttribute<PacketAttribute>();
        Fields = Type.GetFieldCaches();

        HasSequence = packetAttribute.Sequence;
        IsSubHeader = type.GetCustomAttribute<SubPacketAttribute>() is not null;
        IsDynamic = Fields.Any(x => x.Property.GetCustomAttribute<DynamicAttribute>() is not null);
    }

    public byte Header { get; private set; }
    public byte SubHeader { get; set; }
    public Type Type { get; private set; }
    public uint Size { get; private set; }
    public bool IsDynamic { get; private set; }
    public bool IsSubHeader { get; private set; }
    public bool HasSequence { get; private set; }
        
    public void UpdateDynamicSize(object packet, uint packet_size)
    {
        if (_dynamicValueProperty.PropertyType == typeof(string))
        {
            var msg = (string)_dynamicValueProperty.GetValue(packet);
            _dynamicSizeProperty.SetValue(packet, (ushort)((packet_size + msg.Length + 1) & 0xFFFF));
                
        }
        else if (_dynamicValueProperty.PropertyType.IsArray)
        {
            var arr = (Array)_dynamicValueProperty.GetValue(packet);
            _dynamicSizeProperty.SetValue(packet, (ushort)((packet_size + (_subTypes[1].Size * arr.Length) + 1) & 0xFFFF));
        }
        else
        {
            Debug.Assert(false);
        }

        var sizeField = Type.GetProperties().FirstOrDefault(field => CustomAttributeExtensions.GetCustomAttribute<SizeAttribute>((MemberInfo)field) != null);
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

    public uint GetDynamicSize(object packet)
    {
        Debug.Assert(IsDynamic);
        Debug.Assert(_dynamicSizeProperty != null);

        var dynSize = _dynamicSizeProperty.GetValue(packet);
        return (uint) dynSize;
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
            
        if(dynSize is Array)
        {
            return Size + GetDynamicSize(packet);
        }

        Debug.Assert(false);
        return Size;
    }

    public PacketCache CreateGeneralCache()
    { 
        // TODO
        // Debug.Assert(_subPacketAttribute != null);
        // return new PacketCache(Header, Type, _subPacketAttribute.Position) {
        return new PacketCache(Header, Type) {
            HasSequence = false
        };
    }
}