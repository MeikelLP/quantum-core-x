using System.Runtime.CompilerServices;
using System.Text;

namespace QuantumCore.Networking;

public static class SerializerExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<T> ReadEnumFromStreamAsync<T>(this Stream stream, byte[] buffer)
    {
        await stream.ReadExactlyAsync(buffer.AsMemory(0, 1));
        return (T) (object)buffer[0];
    }
        
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static async ValueTask<T> ReadValueFromStreamAsync<T>(this Stream stream, byte[] buffer)
        where T : ISpanParsable<T> // should match all relevant types
    {
        var length = typeof(T) switch {
            { } type when type == typeof(Half) || type == typeof(short) || type == typeof(ushort) => 2,
            { } type when type == typeof(int) || type == typeof(uint) || type == typeof(float) => 4,
            { } type when type == typeof(long) || type == typeof(ulong) || type == typeof(double) => 8,
            { } type when type == typeof(byte) || type == typeof(sbyte) || type == typeof(bool) => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(T), $"Type {typeof(T)} cannot be handled")
        };
        await stream.ReadExactlyAsync(buffer.AsMemory(0, length));
        return typeof(T) switch {
            { } type when type == typeof(int) => (T) (object) BitConverter.ToInt32(buffer.AsSpan(0, length)),
            { } type when type == typeof(uint) => (T) (object) BitConverter.ToUInt32(buffer.AsSpan(0, length)),
            { } type when type == typeof(float) => (T) (object) BitConverter.ToSingle(buffer.AsSpan(0, length)),
            { } type when type == typeof(Half) => (T) (object) BitConverter.ToHalf(buffer.AsSpan(0, length)),
            { } type when type == typeof(short) => (T) (object) BitConverter.ToInt16(buffer.AsSpan(0, length)),
            { } type when type == typeof(ushort) => (T) (object) BitConverter.ToUInt16(buffer.AsSpan(0, length)),
            { } type when type == typeof(long) => (T) (object) BitConverter.ToInt64(buffer.AsSpan(0, length)),
            { } type when type == typeof(ulong) => (T) (object) BitConverter.ToUInt64(buffer.AsSpan(0, length)),
            { } type when type == typeof(double) => (T) (object) BitConverter.ToDouble(buffer.AsSpan(0, length)),
            { } type when type == typeof(bool) => (T) (object) BitConverter.ToBoolean(buffer.AsSpan(0, length)),
            { } type when type == typeof(byte) => (T) (object) buffer[0],
            { } type when type == typeof(sbyte) => (T) (object) buffer[0],
            _ => throw new ArgumentOutOfRangeException(nameof(T), $"Type {typeof(T)} cannot be handled")
        };
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<string> ReadStringFromStreamAsync(this Stream stream, byte[] buffer, int size)
    {
        await stream.ReadExactlyAsync(buffer.AsMemory(0, size));
        var str = System.Text.Encoding.ASCII.GetString(buffer.AsSpan(0, size));
        // null bytes may be appended
        return str.TrimEnd('\0');
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<byte[]> ReadByteArrayFromStreamAsync(this Stream stream, byte[] buffer, int size)
    {
        await stream.ReadExactlyAsync(buffer.AsMemory(0, size));
        return buffer[..size];
    }
        
    // TODO write test
    public static void WriteString(this byte[] bytes, string? str, in int index, in int length)
    {
        if (!string.IsNullOrWhiteSpace(str))
        {
            var asciiBytes = Encoding.ASCII.GetBytes(str, 0, Math.Min(str.Length, length));
            asciiBytes.CopyTo(bytes.AsMemory(index, length));
            bytes[index + length] = 0; // terminate string
        }
        else
        {
            Array.Clear(bytes, index, length);
        }
    }
}