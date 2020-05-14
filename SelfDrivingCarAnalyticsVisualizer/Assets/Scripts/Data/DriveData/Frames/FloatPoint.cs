public class FloatPoint
{
    public float X;

    public float Y;

    public float Z;

    public UnityEngine.Vector3 ToVector()
    {
        return new UnityEngine.Vector3(X, Y, Z);
    }
}
