using System.Diagnostics;

namespace QuantumCore.Core.Utils;

public static class Xtea
{
    private const uint DELTA = 0x9E3779B9;
	    
    public static byte[] Decrypt(byte[] input, uint size, uint[] key, uint rounds)
    {
        Debug.Assert(key.Length == 4); // 128 bits key
		    
        var output = new byte[size];
        var steps = input.Length / 8;
        var currentStep = 0;

        var inputPosition = 0;
        var outputPosition = 0;

        while (currentStep < steps)
        {
            var u1 = BitConverter.ToUInt32(input, inputPosition);
            var u2 = BitConverter.ToUInt32(input, inputPosition + sizeof(uint));
			    
            DecryptStep(rounds, ref u1, ref u2, key);

            output[outputPosition++] = (byte) (u1 & 0xFF);
            output[outputPosition++] = (byte) ((u1 >> 8) & 0xFF);
            output[outputPosition++] = (byte) ((u1 >> 16) & 0xFF);
            output[outputPosition++] = (byte) ((u1 >> 24) & 0xFF);
			    
            output[outputPosition++] = (byte) (u2 & 0xFF);
            output[outputPosition++] = (byte) ((u2 >> 8) & 0xFF);
            output[outputPosition++] = (byte) ((u2 >> 16) & 0xFF);
            output[outputPosition++] = (byte) ((u2 >> 24) & 0xFF);
			    
            currentStep++;
            inputPosition += 2 * sizeof(uint);
        }

        return output;
    }

    private static void DecryptStep(uint rounds, ref uint u1, ref uint u2, uint[] key)
    {
        var sum = DELTA * rounds;
        for (var i = 0; i < rounds; i++)
        {
            u2 -= (((u1 << 4) ^ (u1 >> 5)) + u1) ^ (sum + key[(sum >> 11) & 3]);
            sum -= DELTA;
            u1 -= (((u2 << 4) ^ (u2 >> 5)) + u2) ^ (sum + key[sum & 3]);
        }
    }
}
