using System;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;

namespace GraphU.Fractals;

public struct Rect
{
    public int Width { get; set; }
    public int Height { get; set; }
    
    public double RotationDegree { get; set; }
    public Color Color { get; set; }

    public int RotationCenterX { get; set; }
    public int RotationCenterY { get; set; }

    public Point[] AsPoints()
    {
        var points = new Point[4];

        points[0] = new Point(
            (int)(RotationCenterX + Width / 2.0 * Math.Cos(RotationDegree) - Height / 2.0 * Math.Sin(RotationDegree)),
            (int)(RotationCenterY + Width / 2.0 * Math.Sin(RotationDegree) + Height / 2.0 * Math.Cos(RotationDegree)));

        
        points[1] = new Point(
            (int)(RotationCenterX - Width / 2.0 * Math.Cos(RotationDegree) - Height / 2.0 * Math.Sin(RotationDegree)),
            (int)(RotationCenterY - Width / 2.0 * Math.Sin(RotationDegree) + Height / 2.0 * Math.Cos(RotationDegree)));


        points[2] = new Point(
            (int)(RotationCenterX - Width / 2.0 * Math.Cos(RotationDegree) + Height / 2.0 * Math.Sin(RotationDegree)),
            (int)(RotationCenterY - Width / 2.0 * Math.Sin(RotationDegree) - Height / 2.0 * Math.Cos(RotationDegree)));
        
        points[3] = new Point(
            (int)(RotationCenterX + Width / 2.0 * Math.Cos(RotationDegree) + Height / 2.0 * Math.Sin(RotationDegree)),
            (int)(RotationCenterY + Width / 2.0 * Math.Sin(RotationDegree) - Height / 2.0 * Math.Cos(RotationDegree)));

        return points;
    }
    
    
    
    public Line[] AsLines()
    {
        var points = this.AsPoints();

        return new Line[]
        {
            new() { X1 = points[0].X, Y1 = points[0].Y, X2 = points[1].X, Y2 = points[1].Y, Stroke = new SolidColorBrush(Color) },
            new() { X1 = points[1].X, Y1 = points[1].Y, X2 = points[2].X, Y2 = points[2].Y, Stroke = new SolidColorBrush(Color) },
            new() { X1 = points[2].X, Y1 = points[2].Y, X2 = points[3].X, Y2 = points[3].Y, Stroke = new SolidColorBrush(Color) },
            new() { X1 = points[3].X, Y1 = points[3].Y, X2 = points[0].X, Y2 = points[0].Y, Stroke = new SolidColorBrush(Color) }
        };
    }
}