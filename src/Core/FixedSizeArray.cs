using System.Buffers;
using System.Runtime.CompilerServices;

namespace QuantumCore;

/// <summary>
/// hack to get fixed size array from pool
/// https://stackoverflow.com/a/77446013/8321861
/// Cannot be used in async calls because it may break the CLR
/// </summary>
/// <typeparam name="T"></typeparam>
internal readonly ref struct FixedSizeArray<T>
{
    private sealed class RawArrayData
    {
        public int Length;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        public uint Padding;
        public byte Data;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
    }

    private readonly int _originalSize;
    public readonly int RequestedSize;
    public readonly T[] Array = null!;


    public FixedSizeArray(int size)
    {
        RequestedSize = size;
        if (RequestedSize == 0)
        {
            Array = [];
            return;
        }

        // hack to get fixed size array from pool
        // https://stackoverflow.com/a/77446013/8321861
        Array = ArrayPool<T>.Shared.Rent(size);
        _originalSize = Array.Length;
        var unmanagedArray = Unsafe.As<RawArrayData>(Array);
        unmanagedArray.Length = size;
    }

    public void Dispose()
    {
        if (RequestedSize == 0) return;
        var unmanagedArray = Unsafe.As<RawArrayData>(Array);
        unmanagedArray.Length = _originalSize;
        ArrayPool<T>.Shared.Return(Array);
    }
}