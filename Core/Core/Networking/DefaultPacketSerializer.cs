using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace QuantumCore.Core.Networking;

public class DefaultPacketSerializer : IPacketSerializer
{
    private readonly IPacketManager _packetManager;

    public DefaultPacketSerializer(IPacketManager packetManager)
    {
        _packetManager = packetManager;
    }

    public byte[] Serialize(Type type, object obj)
    {
        var packetCache = _packetManager.GetPacket(type);
        var size = CalculateSize(type, obj);

        var ret = new byte[size];

        using var ms = new MemoryStream(ret);
        using var bw = new BinaryWriter(ms, Encoding.ASCII);
            
        // Write header
        if (packetCache.HasHeader)
        {
            bw.Write(packetCache.Header);
        }

        foreach (var field in packetCache.Fields)
        {
            var fieldType = field.FieldType;

            if (fieldType.IsArray)
            {
                var array = (Array) field.GetValue(obj);
                for (var i = 0; i < ((Array)field.Property.GetValue(obj))!.Length; i++)
                {
                    // todo: implement array of an enum
                    var arrValue = array.GetValue(i);
                    if (arrValue?.GetType().IsClass == true)
                    {
                        Serialize(array.GetType().GetElementType(), arrValue);
                    }
                    else
                    {
                        WriteField(arrValue, fieldType.GetElementType(), bw, field);
                    }
                }   
            }
            else
            {
                // Use the field type or if the field type is an enum use the enum type defined at the field attribute
                WriteField(field.GetValue(obj), fieldType.IsEnum ? field.EnumType : fieldType, bw, field);
            }
        }

        return ret;
    }
    public byte[] Serialize<T>(T obj)
    {
        return Serialize(typeof(T), obj);
    }


    public object Deserialize(Type type, ReadOnlySpan<byte> bytes)
    {
        throw new NotImplementedException();
        // var expectedSize = Size;
        // if (Header != 0)
        // {
        //     expectedSize--;
        // }
        //     
        // if (data.Length != expectedSize) throw new ArgumentException("Invalid data stream given", nameof(data));
        // if (obj.GetType() != Type) throw new ArgumentException("Invalid packet given", nameof(obj));
        //
        // using var ms = new MemoryStream(data);
        // using var br = new BinaryReader(ms, Encoding.ASCII);
        //
        // byte subHeader = 0;
        //     
        // foreach (var field in _properties)
        // {
        //     var type = field.FieldType;
        //     var multiplier = 1;
        //     Array array = null;
        //
        //     if (type.IsArray)
        //     {
        //         type = type.GetElementType();
        //         multiplier = (int) field.ArrayLength;
        //         array = Array.CreateInstance(type, multiplier);
        //     }
        //
        //     if (type.IsEnum)
        //     {
        //         type = field.EnumType;
        //     }
        //
        //     for (var i = 0; i < multiplier; i++)
        //     {
        //         object value = null;
        //         if (type == typeof(uint))
        //         {
        //             value = br.ReadUInt32();
        //         }
        //         else if (type == typeof(int))
        //         {
        //             value = br.ReadInt32();
        //         }
        //         else if (type == typeof(ushort))
        //         {
        //             value = br.ReadUInt16();
        //         }
        //         else if (type == typeof(short))
        //         {
        //             value = br.ReadInt16();
        //         }
        //         else if (type == typeof(float))
        //         {
        //             value = br.ReadSingle();
        //         }
        //         else if (type == typeof(byte))
        //         {
        //             value = br.ReadByte();
        //         }
        //         else if (type == typeof(string))
        //         {
        //             var chars = br.ReadChars((int) field.Length);
        //             var idx = Array.IndexOf(chars, '\0');
        //             value = new string(chars, 0, idx < 0 ? chars.Length : idx);
        //         }
        //         else if (type == typeof(float))
        //         {
        //             value = br.ReadSingle();
        //         }
        //         else if (type.IsClass)
        //         {
        //             var subType = _subTypes[field.Position];
        //             var instance = Activator.CreateInstance(type);
        //             subType.Deserialize(instance, br.ReadBytes((int) subType.Size));
        //             value = instance;
        //         }
        //         else
        //         {
        //             Debug.Assert(false);
        //         }
        //
        //         if (array != null)
        //         {
        //             array.SetValue(value, i);
        //         }
        //         else
        //         {
        //             if (IsSubHeader && field is SubHeaderField)
        //             {
        //                 subHeader = (byte) value;
        //             }
        //             else
        //             {
        //                 field.SetValue(obj, value);
        //             }
        //         }
        //     }
        //
        //     if (array != null) field.SetValue(obj, array);
        // }
        //
        // return subHeader;
    }
    
    public T Deserialize<T>(ReadOnlySpan<byte> bytes)
    {
        return (T) Deserialize(typeof(T), bytes);
    }

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
        // else if (type.IsClass)
        // {
        //     var subType = _subTypes[field.Position];
        //     bw.Write(subType.Serialize(value));
        // }
        else
            Debug.Assert(false);
    }

    private uint CalculateSize(Type type, object packet)
    {
        var packetCache = _packetManager.GetPacket(type);
        uint size = 1; // header

        // TODO sub packets

        foreach (var field in packetCache.Fields)
        {
            var fieldType = field.FieldType;
            uint multiplier = 1;

            if (fieldType.IsEnum)
            {
                fieldType = field.EnumType;
            }

            if (fieldType == typeof(uint) || fieldType == typeof(int))
            {
                size += 4 * multiplier;
            }
            else if (fieldType == typeof(ushort) || fieldType == typeof(short))
            {
                size += 2 * multiplier;
            }
            else if (fieldType == typeof(byte))
            {
                size += 1 * multiplier;
            }
            else if (fieldType == typeof(string))
            {
                Debug.Assert(field.Length > 0);
                size += field.Length * multiplier;
            }
            else if (fieldType == typeof(float))
            {
                size += 4 * multiplier;
            }
            else if (fieldType is { IsArray: true })
            {
                var arr = (Array)field.GetValue(packet);
                foreach (var item in arr)
                {
                    size += CalculateSize(field.Property.PropertyType.GetElementType(), item);
                }
            }
            else
            {
                throw new InvalidOperationException(
                    $"Packet {type.Name} contains invalid type {fieldType.Name} for property {field.Property.Name}!");
            }
        }

        return size;
    }
}