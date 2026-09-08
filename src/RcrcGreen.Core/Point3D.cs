using System.Globalization;

namespace RcrcGreen.Core
{
    /// <summary>
    /// A point in the model, in whatever unit the caller is working in. Plain numbers so the
    /// geometry can be tested without Revit loaded.
    /// </summary>
    public struct Point3D
    {
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }

        public double Y { get; }

        public double Z { get; }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture, "({0}, {1}, {2})", X, Y, Z);
        }
    }
}
