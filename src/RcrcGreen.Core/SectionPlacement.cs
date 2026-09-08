using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Where a cross section goes for one plot: the two ends of the line, the way the view
    /// looks, and how far it looks.
    /// </summary>
    public sealed class SectionPlacement
    {
        private SectionPlacement(
            string plotName,
            SectionAxis axis,
            Point3D start,
            Point3D end,
            Vector3D viewDirection,
            double depth)
        {
            PlotName = plotName;
            Axis = axis;
            Start = start;
            End = end;
            ViewDirection = viewDirection;
            Depth = depth;
        }

        public string PlotName { get; }

        public SectionAxis Axis { get; }

        public Point3D Start { get; }

        public Point3D End { get; }

        /// <summary>
        /// Unit length and horizontal. It is the line direction turned a quarter turn, taken as
        /// the line direction crossed with world up, so the same box and axis always give the
        /// same side.
        /// </summary>
        public Vector3D ViewDirection { get; }

        /// <summary>
        /// The distance from the line to the face of the box the view looks at, which is half
        /// the box extent on that axis because the line sits in the middle.
        /// </summary>
        public double Depth { get; }

        public double Length
        {
            get
            {
                double dx = End.X - Start.X;
                double dy = End.Y - Start.Y;
                double dz = End.Z - Start.Z;
                return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
            }
        }

        /// <summary>
        /// The axis has no default here on purpose. The caller decides, because the choice
        /// belongs to whoever is looking at the plot.
        /// </summary>
        public static SectionPlacement Across(PlotBox box, SectionAxis axis)
        {
            if (box == null) throw new ArgumentNullException("box");
            if (axis != SectionAxis.ShortSide && axis != SectionAxis.LongSide)
            {
                throw new ArgumentOutOfRangeException("axis", axis, "Not a section axis.");
            }
            if (box.WidthX <= 0.0 || box.WidthY <= 0.0)
            {
                throw new ArgumentException(
                    "Plot " + box.PlotName + " has a flat scope box, so no line can cross it.",
                    "box");
            }

            // A tie goes to Y as the short side. Some plot somewhere will be square, and a
            // rule that picks nothing there would place the section differently on each run.
            bool yIsShorter = box.WidthY <= box.WidthX;
            bool alongY = axis == SectionAxis.ShortSide ? yIsShorter : !yIsShorter;

            if (alongY)
            {
                return new SectionPlacement(
                    box.PlotName,
                    axis,
                    new Point3D(box.CentreX, box.MinY, box.CentreZ),
                    new Point3D(box.CentreX, box.MaxY, box.CentreZ),
                    new Vector3D(1.0, 0.0, 0.0),
                    box.WidthX / 2.0);
            }

            return new SectionPlacement(
                box.PlotName,
                axis,
                new Point3D(box.MinX, box.CentreY, box.CentreZ),
                new Point3D(box.MaxX, box.CentreY, box.CentreZ),
                new Vector3D(0.0, -1.0, 0.0),
                box.WidthY / 2.0);
        }

        /// <summary>
        /// The middle of the line. It sits on the centre of the box in X and Y, which is what
        /// across the middle of the plot means.
        /// </summary>
        public Point3D Midpoint
        {
            get
            {
                return new Point3D(
                    (Start.X + End.X) / 2.0,
                    (Start.Y + End.Y) / 2.0,
                    (Start.Z + End.Z) / 2.0);
            }
        }
    }
}
