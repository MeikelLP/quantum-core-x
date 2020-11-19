using System.IO;
using QuantumCore.Core.Utils;

namespace QuantumCore.Types
{
    public class LzoXtea
    {
        private uint _size;
        
        public LzoXtea(uint size)
        {
            _size = size;
        }

        public byte[] Decode(byte[] input)
        {
            uint[] key = {
                0x2A4A1, 0x45415AA, 0x185A8BE7, 0x1AAD6AB
            };
            var decrypted = Xtea.Decrypt(input, 188856, key, 32); //188836
            File.WriteAllBytes("item_proto_decrypted", decrypted);
            if (decrypted[0] != 'M' || decrypted[1] != 'C' || decrypted[2] != 'O' || decrypted[3] != 'Z')
            {
                throw new InvalidDataException("Failed to decrypt data stream");
            }
            //decrypted = Xtea.Decrypt(decrypted, 188856, key, 32); //188836
            var lzo = new Lzo(_size);
            
            return lzo.Decode(decrypted);
        }
    }
}