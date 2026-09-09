using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Section 1. The document, how big it is, how long the read took and the units the
    /// project shows.
    ///
    /// The unit id is Revit's own name for the unit, such as autodesk.unit.unit:squareMeters-1.0.1,
    /// kept beside the label because the label is what a person reads and the id is what the
    /// finished tool will compare against.
    /// </summary>
    public sealed class DocumentFacts
    {
        public DocumentFacts(
            string title,
            string path,
            int elementInstances,
            int elementTypes,
            double readSeconds,
            ProjectUnit area,
            ProjectUnit length)
        {
            if (title == null) throw new ArgumentNullException("title");
            if (elementInstances < 0) throw new ArgumentOutOfRangeException("elementInstances");
            if (elementTypes < 0) throw new ArgumentOutOfRangeException("elementTypes");
            if (double.IsNaN(readSeconds) || readSeconds < 0.0) throw new ArgumentOutOfRangeException("readSeconds");

            Title = title;
            Path = path ?? string.Empty;
            ElementInstances = elementInstances;
            ElementTypes = elementTypes;
            ReadSeconds = readSeconds;
            Area = area ?? ProjectUnit.Unknown;
            Length = length ?? ProjectUnit.Unknown;
        }

        public string Title { get; }

        /// <summary>
        /// Empty for a document that has never been saved. A cloud model carries its cloud
        /// path here rather than a drive letter.
        /// </summary>
        public string Path { get; }

        public int ElementInstances { get; }

        public int ElementTypes { get; }

        public double ReadSeconds { get; }

        public ProjectUnit Area { get; }

        public ProjectUnit Length { get; }
    }

    /// <summary>
    /// One project unit setting: the label a person sees, Revit's id for it and how many
    /// decimal places it rounds to. The rounding is here because it is the difference between
    /// the raw number and the printed one.
    /// </summary>
    public sealed class ProjectUnit
    {
        public static readonly ProjectUnit Unknown = new ProjectUnit("UNKNOWN", string.Empty, double.NaN);

        public ProjectUnit(string label, string id, double accuracy)
        {
            if (label == null) throw new ArgumentNullException("label");

            Label = label;
            Id = id ?? string.Empty;
            Accuracy = accuracy;
        }

        public string Label { get; }

        public string Id { get; }

        /// <summary>
        /// Revit's rounding step, such as 0.01. NaN when it could not be read.
        /// </summary>
        public double Accuracy { get; }

        public bool IsKnown
        {
            get { return !double.IsNaN(Accuracy) || Id.Length > 0; }
        }
    }
}
