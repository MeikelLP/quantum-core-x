using System.IO;
using QuantumCore.Core.Utils;

namespace QuantumCore.Core.Types
{
    public class LzoXtea
    {
        private static readonly uint[] Keys = { 0x2A4A1, 0x45415AA, 0x185A8BE7, 0x1AAD6AB };
        private const string ItemProtoDecryptedName = "item_proto_decrypted";

        private uint _size;
        private uint _xteaSize;

        private readonly Lzo _lzoInstance;
        
        public LzoXtea(uint size, uint xteaSize)
        {
            _size = size;
            _xteaSize = xteaSize;
            _lzoInstance = new Lzo(size);
        }

        public byte[] Decode(byte[] input)
        {
            var decrypted = Xtea.Decrypt(input, _xteaSize, Keys, 32); //188836
            File.WriteAllBytes(ItemProtoDecryptedName, decrypted);
            if (decrypted[0] != 'M' || decrypted[1] != 'C' || decrypted[2] != 'O' || decrypted[3] != 'Z')
            {
                throw new InvalidDataException("Failed to decrypt data stream");
            }
            //decrypted = Xtea.Decrypt(decrypted, 188856, key, 32); //188836
            
            return _lzoInstance.Decode(decrypted);
        }
    }
}