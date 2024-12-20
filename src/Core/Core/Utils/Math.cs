namespace QuantumCore.Core.Utils;

public static class MathUtils
{
    public static double Distance(int x1, int y1, int x2, int y2)
    {
        var a = x1 - x2;
        var b = y1 - y2;

        return Math.Sqrt(a * a + b * b);
    }

    /// <summary>
    /// Returns the rotation in degrees
    /// </summary>
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

    public static int MinMax(int min, int value, int max)
    {
        var temp = (min > value ? min : value);
        return (max < temp) ? max : temp;
    }

    public static (double X, double Y) GetDeltaByDegree(double degree)
    {
        var radian = DegreeToRadian(degree);

        var x = Math.Sin(radian);
        var y = Math.Cos(radian);
        return (x, y);
    }

    public static double DegreeToRadian(double degree)
    {
        return degree * Math.PI / 180.0;
    }
}
