using System.Windows.Media.Media3D;

namespace GraphU.Movement;

public class Point
{
    public double X { get; init; }
    
    public double Y { get; init; }

    public Point() : this(0,0)
    {
    }

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }

    public Vector3D AsExtendedVector()
    {
        return new Vector3D(this.X, this.Y, 1);
    }

    public static Point FromExtendedVector(Vector3D vec)
    {
        return new Point { X = vec.X / vec.Z, Y = vec.Y / vec.Z };
    }

    public override string ToString()
    {
        return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}";
    }
}