using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// A plot and the extent of its scope box, as a minimum and a maximum corner.
    /// </summary>
    public sealed class PlotBox
    {
        public PlotBox(
            string plotName,
            double minX, double minY, double minZ,
            double maxX, double maxY, double maxZ)
        {
            if (plotName == null) throw new ArgumentNullException("plotName");

            RequireOrdered(minX, maxX, "X");
            RequireOrdered(minY, maxY, "Y");
            RequireOrdered(minZ, maxZ, "Z");

            PlotName = plotName;
            MinX = minX;
            MinY = minY;
            MinZ = minZ;
            MaxX = maxX;
            MaxY = maxY;
            MaxZ = maxZ;
        }

        public string PlotName { get; }

        public double MinX { get; }

        public double MinY { get; }

        public double MinZ { get; }

        public double MaxX { get; }

        public double MaxY { get; }

        public double MaxZ { get; }

        public double WidthX
        {
            get { return MaxX - MinX; }
        }

        public double WidthY
        {
            get { return MaxY - MinY; }
        }

        public double HeightZ
        {
            get { return MaxZ - MinZ; }
        }

        public double CentreX
        {
            get { return (MinX + MaxX) / 2.0; }
        }

        public double CentreY
        {
            get { return (MinY + MaxY) / 2.0; }
        }

        public double CentreZ
        {
            get { return (MinZ + MaxZ) / 2.0; }
        }

        public Point3D Centre
        {
            get { return new Point3D(CentreX, CentreY, CentreZ); }
        }

        // An infinite bound survives the ordering check and then turns every centre into NaN,
        // which reads as a placement rather than as a failure. Both are refused here instead.
        private static void RequireOrdered(double min, double max, string axis)
        {
            if (double.IsNaN(min) || double.IsNaN(max)
                || double.IsInfinity(min) || double.IsInfinity(max))
            {
                throw new ArgumentException("The " + axis + " extent is not a real number.");
            }
            if (max < min)
            {
                throw new ArgumentException(
                    "The " + axis + " maximum is below the " + axis + " minimum.");
            }
        }
    }
}
