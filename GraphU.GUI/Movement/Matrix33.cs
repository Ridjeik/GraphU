using System.Windows.Media.Media3D;

namespace GraphU.Movement;

public class Matrix33
{
    public double X11 { get; set; }
    public double X12 { get; set; }
    public double X13 { get; set; }
    public double X21 { get; set; }
    public double X22 { get; set; }    
    public double X23 { get; set; }
    public double X31 { get; set; }
    public double X32 { get; set; }    
    public double X33 { get; set; }

    public Matrix33(double x11, double x12, double x13, double x21, double x22, double x23, double x31, double x32, double x33)
    {
        X11 = x11;
        X12 = x12;
        X13 = x13;
        X21 = x21;
        X22 = x22;
        X23 = x23;
        X31 = x31;
        X32 = x32;
        X33 = x33;
    }
    
    public static Vector3D operator *(Matrix33 mat, Vector3D vec)
    {
        return new Vector3D(vec.X * mat.X11 + vec.Y * mat.X12 + vec.Z * mat.X13, vec.X * mat.X21 + vec.Y * mat.X22 + vec.Z * mat.X23, vec.X * mat.X31 + vec.Y * mat.X32 + vec.Z * mat.X33);
    }
    
    public static Matrix33 operator *(Matrix33 mat1, Matrix33 mat2)
    {
        return new Matrix33(
            mat1.X11 * mat2.X11 + mat1.X12 * mat2.X21 + mat1.X13 * mat2.X31,
            mat1.X11 * mat2.X12 + mat1.X12 * mat2.X22 + mat1.X13 * mat2.X32,
            mat1.X11 * mat2.X13 + mat1.X12 * mat2.X23 + mat1.X13 * mat2.X33,
            mat1.X21 * mat2.X11 + mat1.X22 * mat2.X21 + mat1.X23 * mat2.X31,
            mat1.X21 * mat2.X12 + mat1.X22 * mat2.X22 + mat1.X23 * mat2.X32,
            mat1.X21 * mat2.X13 + mat1.X22 * mat2.X23 + mat1.X23 * mat2.X33,
            mat1.X31 * mat2.X11 + mat1.X32 * mat2.X21 + mat1.X33 * mat2.X31,
            mat1.X31 * mat2.X12 + mat1.X32 * mat2.X22 + mat1.X33 * mat2.X32,
            mat1.X31 * mat2.X13 + mat1.X32 * mat2.X23 + mat1.X33 * mat2.X33
        );
    }

}