using System;

namespace QuantumCore.Core.Utils
{
    public static class MathUtils
    {
        public static double Distance(int x1, int y1, int x2, int y2)
        {
            var a = x1 - x2;
            var b = y1 - y2;

            return Math.Sqrt(a * a + b * b);
        }
    }
}