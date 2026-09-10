using System.Globalization;

namespace RcrcGreen.Core
{
    /// <summary>
    /// A direction. Nothing here normalises for you, so build one that is already unit length
    /// when that is what the caller needs.
    /// </summary>
    public struct Vector3D
    {
        public Vector3D(double x, double y, double z)
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
