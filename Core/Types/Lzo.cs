using System.IO;
using System.IO.Compression;
using lzo.net;

namespace QuantumCore.Types
{
    public class Lzo
    {
        private uint _size;
        
        public Lzo(uint size)
        {
            _size = size;
        }
        
        public byte[] Decode(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var lzo = new LzoStream(ms, CompressionMode.Decompress);
            var buffer = new byte[_size];
            lzo.Read(buffer, 4, data.Length - 4);
            return buffer;
        }
    }
}