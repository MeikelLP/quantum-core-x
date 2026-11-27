namespace QuantumCore.API.Core.Models;

public class Animation
{
    public float MotionDuration { get; private set; }
    public float AccumulationX { get; private set; }
    public float AccumulationY { get; private set; }
    public float AccumulationZ { get; private set; }

    public Animation(float motionDuration, float accumulationX, float accumulationY, float accumulationZ)
    {
        MotionDuration = motionDuration;
        AccumulationX = accumulationX;
        AccumulationY = accumulationY;
        AccumulationZ = accumulationZ;
    }
}