using System;
using Serilog;

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

        public static double Rotation(double x, double y)
        {
            var vectorLength = Math.Sqrt(x * x + y * y);

            var normalizedX = x / vectorLength;
            var normalizedY = y / vectorLength;
            var upVectorX = 0;
            var upVectorY = 1;

            var rotationRadians = -(Math.Atan2(normalizedY, normalizedX) - Math.Atan2(upVectorY, upVectorX));

            var rotationDegress = rotationRadians * (180 / Math.PI);
            if (rotationDegress < 0) rotationDegress += 360;
            return rotationDegress;
        }

        public static int CompareTwoNumbers(object a, object b)
        {
            try
            {
                var numA = Convert.ToDouble(a);
                var numB = Convert.ToDouble(b);

                if (numA > numB)
                {
                    return 1;
                }

                if (numB > numA)
                {
                    return -1;
                }

                return 0;
            }
            catch (Exception)
            {
                Log.Error($"Can't compare {a.GetType().Name} with {b.GetType().Name}");
                return 0;
            }
        }
    }
}