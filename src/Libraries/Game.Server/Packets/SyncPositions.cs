using System.Buffers;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x08, EDirection.INCOMING, Sequence = true)]
// TODO: enhance generator to support variable length arrays by the TotalSize field
public class SyncPositions : IPacketSerializable
{
      private const int ELEMENT_SIZE = 12; // SyncPositionElement = uint + int + int
      private const int HEADER_SIZE = 1;
      private const int TOTAL_SIZE_FIELD_SIZE = 2;

      public ushort TotalSize { get; private init; }
      public SyncPositionElement[] Positions { get; private init; } = [];

      public ushort GetSize()
      {
          return (ushort)(TOTAL_SIZE_FIELD_SIZE + Positions.Length * ELEMENT_SIZE);
      }

      public void Serialize(byte[] bytes, in int offset = 0)
      {
          var totalSize = TotalSize != 0
              ? TotalSize
              : (ushort)(HEADER_SIZE + TOTAL_SIZE_FIELD_SIZE + Positions.Length * ELEMENT_SIZE);
          BitConverter.GetBytes(totalSize).CopyTo(bytes, offset);

          for (var i = 0; i < Positions.Length; i++)
          {
              var elementOffset = offset + TOTAL_SIZE_FIELD_SIZE + i * ELEMENT_SIZE;
              var element = Positions[i];
              BitConverter.GetBytes(element.Vid).CopyTo(bytes, elementOffset);
              BitConverter.GetBytes(element.X).CopyTo(bytes, elementOffset + 4);
              BitConverter.GetBytes(element.Y).CopyTo(bytes, elementOffset + 8);
          }
      }

      public static SyncPositions Deserialize(ReadOnlySpan<byte> bytes, in int offset = 0)
      {
          var totalSize = BitConverter.ToUInt16(bytes.Slice(offset, TOTAL_SIZE_FIELD_SIZE));
          var payloadSize = Math.Max(0, totalSize - HEADER_SIZE - TOTAL_SIZE_FIELD_SIZE);
          var count = payloadSize / ELEMENT_SIZE;

          var positions = new SyncPositionElement[count];
          for (var i = 0; i < count; i++)
          {
              var elementOffset = offset + TOTAL_SIZE_FIELD_SIZE + i * ELEMENT_SIZE;
              positions[i] = new SyncPositionElement
              {
                  Vid = BitConverter.ToUInt32(bytes.Slice(elementOffset, 4)),
                  X = BitConverter.ToInt32(bytes.Slice(elementOffset + 4, 4)),
                  Y = BitConverter.ToInt32(bytes.Slice(elementOffset + 8, 4))
              };
          }

          return new SyncPositions
          {
              TotalSize = totalSize,
              Positions = positions
          };
      }
      
      public static T Deserialize<T>(ReadOnlySpan<byte> bytes, in int offset = 0)
          where T : IPacketSerializable
      {
          return (T)(object)Deserialize(bytes, offset);
      }

      public static async ValueTask<object> DeserializeFromStreamAsync(Stream stream)
      {
          var buffer = ArrayPool<byte>.Shared.Rent(NetworkingConstants.BufferSize);
          try
          {
              var totalSize = await stream.ReadValueFromStreamAsync<ushort>(buffer);
              var payloadSize = Math.Max(0, totalSize - HEADER_SIZE - TOTAL_SIZE_FIELD_SIZE);
              var count = payloadSize / ELEMENT_SIZE;

              var positions = new SyncPositionElement[count];
              for (var i = 0; i < count; i++)
              {
                  positions[i] = new SyncPositionElement
                  {
                      Vid = await stream.ReadValueFromStreamAsync<uint>(buffer),
                      X = await stream.ReadValueFromStreamAsync<int>(buffer),
                      Y = await stream.ReadValueFromStreamAsync<int>(buffer)
                  };
              }

              var remaining = payloadSize - count * ELEMENT_SIZE;
              while (remaining > 0)
              {
                  var chunk = Math.Min(remaining, buffer.Length);
                  await stream.ReadExactlyAsync(buffer.AsMemory(0, chunk));
                  remaining -= chunk;
              }

              return new SyncPositions
              {
                  TotalSize = totalSize,
                  Positions = positions
              };
          }
          finally
          {
              ArrayPool<byte>.Shared.Return(buffer);
          }
      }

      public static byte Header => 0x08;
      public static byte? SubHeader => null;
      public static bool HasStaticSize => false;
      public static bool HasSequence => true;
}
