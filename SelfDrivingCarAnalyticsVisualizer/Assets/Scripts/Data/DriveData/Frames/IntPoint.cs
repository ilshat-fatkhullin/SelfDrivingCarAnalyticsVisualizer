using geniikw.DataRenderer2D.Polygon;
using System.Collections;
using System.Collections.Generic;

public class IntPoint
{
    public int X;

    public int Y;

    public int Z;

    public UnityEngine.Vector3 ToVector()
    {
        return new UnityEngine.Vector3(X, Y, Z);
    }
}

public class IntPointEqualityComparer : IEqualityComparer<IntPoint>
{
    public bool Equals(IntPoint x, IntPoint y)
    {
        return x.X == y.X && x.Y == y.Y && x.Z == y.Z;
    }

    public int GetHashCode(IntPoint obj)
    {
        return obj.X * obj.Y * obj.Z;
    }
}