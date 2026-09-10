using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One cell of the grid, named by the plot down the side and the view type across the top.
    /// </summary>
    public sealed class PlotViewKey : IEquatable<PlotViewKey>
    {
        public PlotViewKey(string plotId, ViewType viewType)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");
            if (viewType == null) throw new ArgumentNullException("viewType");

            PlotId = plotId;
            ViewType = viewType;
        }

        public string PlotId { get; }

        public ViewType ViewType { get; }

        public bool Equals(PlotViewKey other)
        {
            if (ReferenceEquals(other, null)) return false;
            return string.Equals(PlotId, other.PlotId, StringComparison.Ordinal)
                && ViewType.Equals(other.ViewType);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PlotViewKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (PlotId.GetHashCode() * 397) ^ ViewType.GetHashCode();
            }
        }

        public override string ToString()
        {
            return PlotId + " " + ViewType;
        }
    }
}
