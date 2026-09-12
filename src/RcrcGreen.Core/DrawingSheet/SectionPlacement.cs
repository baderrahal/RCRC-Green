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
            Point3D start,
            Point3D end,
            Vector3D viewDirection,
            double depth)
        {
            PlotName = plotName;
            Start = start;
            End = end;
            ViewDirection = viewDirection;
            Depth = depth;
        }

        public string PlotName { get; }

        public Point3D Start { get; }

        public Point3D End { get; }

        /// <summary>
        /// Unit length and horizontal. It is the line direction turned a quarter turn, taken as
        /// the line direction crossed with world up, so the same box and axis always give the
        /// same side.
        /// </summary>
        public Vector3D ViewDirection { get; }

        /// <summary>
        /// How far the view looks, exactly as the caller supplied it, in the same unit as the
        /// <see cref="PlotBox"/> numbers. Nothing here derives it from the size of the box.
        /// </summary>
        public double Depth { get; }

        public double Length
        {
            get
            {
                double dx = End.X - Start.X;
                double dy = End.Y - Start.Y;
                double dz = End.Z - Start.Z;

                // Squaring the deltas first flushes a tiny line to zero and overflows a very
                // long one to infinity. Scaling by the largest of them keeps both readable.
                double largest = Math.Max(Math.Abs(dx), Math.Max(Math.Abs(dy), Math.Abs(dz)));
                if (largest == 0.0) return 0.0;

                double ux = dx / largest;
                double uy = dy / largest;
                double uz = dz / largest;
                return largest * Math.Sqrt((ux * ux) + (uy * uy) + (uz * uz));
            }
        }

        /// <summary>
        /// The axis, the depth and the cut length have no default here on purpose. The
        /// caller decides all three, because the choice belongs to whoever is looking at
        /// the plot, and because a length carries a unit that Core knows nothing about.
        ///
        /// The line is CENTRED on the box and is the length it is given. It used to run
        /// from one edge of the box to the other, which on NS-32 is 36.4747 metres and
        /// produced a viewport wider than the sheet it had to sit on.
        /// </summary>
        /// <param name="depth">How far the view looks, in the same unit as the box numbers.</param>
        /// <param name="cutLength">How long the cut is, in the same unit as the box numbers.</param>
        public static SectionPlacement Across(
            PlotBox box, SectionAxis axis, double depth, double cutLength)
        {
            if (box == null) throw new ArgumentNullException("box");
            if (axis != SectionAxis.ShortSide && axis != SectionAxis.LongSide)
            {
                throw new ArgumentOutOfRangeException("axis", axis, "Not a section axis.");
            }
            if (double.IsNaN(depth) || double.IsInfinity(depth))
            {
                throw new ArgumentException("The depth is not a real number.", "depth");
            }
            if (double.IsNaN(cutLength) || double.IsInfinity(cutLength) || cutLength <= 0.0)
            {
                throw new ArgumentException(
                    "The cut length is not a real length.", "cutLength");
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

            double half = cutLength / 2.0;

            if (alongY)
            {
                return new SectionPlacement(
                    box.PlotName,
                    new Point3D(box.CentreX, box.CentreY - half, box.CentreZ),
                    new Point3D(box.CentreX, box.CentreY + half, box.CentreZ),
                    new Vector3D(1.0, 0.0, 0.0),
                    depth);
            }

            return new SectionPlacement(
                box.PlotName,
                new Point3D(box.CentreX - half, box.CentreY, box.CentreZ),
                new Point3D(box.CentreX + half, box.CentreY, box.CentreZ),
                new Vector3D(0.0, -1.0, 0.0),
                depth);
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
