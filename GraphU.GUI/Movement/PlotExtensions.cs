using System.Drawing;
using System.Linq;
using ScottPlot;

namespace GraphU.Movement;

public static class PlotExtensions
{
    public static void PutSquare(this Plot plot, Square square)
    {
        var squarePoints = square.Points;
        var xs = squarePoints.Select(point => point.X).ToArray();
        var ys = squarePoints.Select(point => point.Y).ToArray();
        var pol = plot.AddPolygon(xs, ys, fillColor: Color.Transparent, lineColor: Color.FromArgb(127, 255, 0), lineWidth:5);
    }
}