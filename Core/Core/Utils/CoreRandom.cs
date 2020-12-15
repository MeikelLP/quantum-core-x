using System.Collections.Generic;
using System.Security.Cryptography;

namespace QuantumCore.Core.Utils
{
    public class CoreRandom
    {
        public static uint GenerateUInt32()
        {
            var r1 = (uint) RandomNumberGenerator.GetInt32(1 << 30);
            var r2 = (uint) RandomNumberGenerator.GetInt32(1 << 2);
            return (r1 << 2) | r2;
        }

        public static T GetRandom<T>(List<T> list)
        {
            if (list.Count == 0) return default;

            return list[RandomNumberGenerator.GetInt32(list.Count)];
        }
    }
}