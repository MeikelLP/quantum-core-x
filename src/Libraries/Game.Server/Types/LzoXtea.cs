using QuantumCore.Core.Utils;

namespace QuantumCore.Core.Types
{
    public class LzoXtea
    {
        private uint[] _key;
        private uint _size;
        private uint _xteaSize;

        private readonly Lzo _lzoInstance;

        public LzoXtea(uint size, uint xteaSize, params uint[] key)
        {
            _key = key;
            _size = size;
            _xteaSize = xteaSize;
            _lzoInstance = new Lzo(size);
        }

        public byte[] Decode(byte[] input)
        {
            var decrypted = Xtea.Decrypt(input, _xteaSize, _key, 32);
            if (decrypted[0] != 'M' || decrypted[1] != 'C' || decrypted[2] != 'O' || decrypted[3] != 'Z')
            {
                throw new InvalidDataException("Failed to decrypt data stream");
            }

            return _lzoInstance.Decode(decrypted);
        }
    }
}
