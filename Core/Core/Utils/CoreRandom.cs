using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

        public static bool PercentageCheck(long percentage)
        {
            return GenerateInt32(1, 101) <= percentage;
        }

        public static int GenerateInt32(int fromInclusive, int toExclusive)
        {
            return RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);
        }

        public static uint GenerateUInt32(uint fromInclusive, uint toExclusive)
        {
            if (fromInclusive >= toExclusive)
            {
                throw new ArgumentException("fromInclusive must be smaller than toExclusive", nameof(fromInclusive));
            }
            
            var range = toExclusive - fromInclusive - 1;
            if (range == 0)
            {
                return fromInclusive;
            }
            
            var mask = range;
            mask |= mask >> 1;
            mask |= mask >> 2;
            mask |= mask >> 4;
            mask |= mask >> 8;
            mask |= mask >> 16;
            
            Span<uint> random = stackalloc uint[1];
            uint result;
            do
            {
                RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(random));
                result = mask & random[0];
            } while (result > range);

            return result + fromInclusive;
        }

        public static T GetRandom<T>(List<T> list)
        {
            if (list.Count == 0) return default;

            return list[RandomNumberGenerator.GetInt32(list.Count)];
        }
    }
}