using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One scope box and its extent in feet, which is the unit Revit works in.
    ///
    /// Nothing here is validated. This is a read and report path, and a scope box with an
    /// extent nobody expected is exactly the thing the report exists to show. Hand these
    /// numbers to <see cref="PlotBox"/> only when something is going to be placed.
    /// </summary>
    public sealed class ScannedScopeBox
    {
        public ScannedScopeBox(
            string name,
            bool hasBounds,
            double minX, double minY, double minZ,
            double maxX, double maxY, double maxZ)
        {
            if (name == null) throw new ArgumentNullException("name");

            Name = name;
            HasBounds = hasBounds;
            MinX = minX;
            MinY = minY;
            MinZ = minZ;
            MaxX = maxX;
            MaxY = maxY;
            MaxZ = maxZ;
        }

        public static ScannedScopeBox WithoutBounds(string name)
        {
            return new ScannedScopeBox(name, false, 0, 0, 0, 0, 0, 0);
        }

        public string Name { get; }

        /// <summary>
        /// False when Revit gave back no bounding box for it, which it can do.
        /// </summary>
        public bool HasBounds { get; }

        public double MinX { get; }

        public double MinY { get; }

        public double MinZ { get; }

        public double MaxX { get; }

        public double MaxY { get; }

        public double MaxZ { get; }
    }
}
