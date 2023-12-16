using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GraphU;

public static class BitmapExtensions
{
    public static void DrawPixel(this WriteableBitmap writeableBitmap, int x, int y, byte[]? colorData = null)
    {
        colorData ??= new byte[] { 0, 0, 0, 0 };

        Int32Rect rect = new Int32Rect(
            x, 
            y, 
            1, 
            1);

        writeableBitmap.WritePixels( rect, colorData, 4, 0);
    }

    public static void Fill(this WriteableBitmap writeableBitmap, byte[]? colorData = null )
    {
        colorData ??= new byte[] { 255, 255, 255, 255 };

        for(int x = 0; x < writeableBitmap.Width; x++)
        for(int y = 0; y < writeableBitmap.Height; y++)
        {
            var rect = new Int32Rect(x, y, 1, 1);
            writeableBitmap.WritePixels( rect, colorData, 4, 0);
        }
    }

    public static void DrawLine(this WriteableBitmap writeableBitmap, Line line)
    {
        int x = (int)line.X1;
        int y = (int)line.Y1;
        int x2 = (int)line.X2;
        int y2 = (int)line.Y2;
        
        int w = x2 - x ;
        int h = y2 - y ;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0 ;
        if (w<0) dx1 = -1 ; else if (w>0) dx1 = 1 ;
        if (h<0) dy1 = -1 ; else if (h>0) dy1 = 1 ;
        if (w<0) dx2 = -1 ; else if (w>0) dx2 = 1 ;
        int longest = Math.Abs(w) ;
        int shortest = Math.Abs(h) ;
        if (!(longest>shortest)) {
            longest = Math.Abs(h) ;
            shortest = Math.Abs(w) ;
            if (h<0) dy2 = -1 ; else if (h>0) dy2 = 1 ;
            dx2 = 0 ;            
        }
        int numerator = longest >> 1 ;
        for (int i=0;i<=longest;i++) {
            if (x >= 0 && x < writeableBitmap.Width && y >= 0 && y <= writeableBitmap.Height)
                writeableBitmap.DrawPixel(x, y, (line.Stroke as SolidColorBrush)?.Color.ToByteArray()) ;
            numerator += shortest ;
            if (!(numerator<longest)) {
                numerator -= longest ;
                x += dx1 ;
                y += dy1 ;
            } else {
                x += dx2 ;
                y += dy2 ;
            }
        }
    }

    public static byte[] ToByteArray(this Color color)
    {
        return new byte[] { color.B, color.G, color.R, color.A };
    }
}