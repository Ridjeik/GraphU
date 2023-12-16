using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace GraphU.Movement;

public class Square
{
    private Point[] points;

    public Point Center => new(points.Average(points => points.X), points.Average(points => points.Y));

    public Square(Point[] points)
    {
        if (points.Length is not 4)
        {
            throw new ArgumentException(nameof(this.Points));
        }
        this.points = points;
    }

    public Square(Point a, Point b, Point c, Point d)
    {
        if (a.X != b.X && a.X != c.X && a.X != d.X)
            throw new ArgumentException(nameof(a));
        if (b.X != a.X && b.X != c.X && a.X != d.X)
            throw new ArgumentException(nameof(b));
        if (c.X != a.X && c.X != b.X && c.X != d.X)
            throw new ArgumentException(nameof(c));
        if (d.X != a.X && d.X != b.X && d.X != c.X)
            throw new ArgumentException(nameof(d));

        var squarePoints = new[] { a, b, c, d };
        
        var distinctXs = squarePoints.Select(point => point.X).Distinct().ToArray();
        var distinctYs = squarePoints.Select(point => point.Y).Distinct().ToArray();

        var xs = new[] { distinctXs[0], distinctXs[0], distinctXs[1], distinctXs[1] };
        var ys = new[] { distinctYs[0], distinctYs[1], distinctYs[1], distinctYs[0] };
        
        this.points = xs.Zip(ys).Select(tuple => new Point(tuple.First, tuple.Second)) .ToArray();
    }

    public Point[] Points => points;

    public IEnumerable<Vector3D> AsListOfExtendedVectors()
    {
        return Points.Select(point => point.AsExtendedVector());
    }

    public static Square FromListOfExtendedVectors(IEnumerable<Vector3D> vectors3D)
    {
        var points = vectors3D.Select(Point.FromExtendedVector).ToArray();
        return new Square(points);
    }
}