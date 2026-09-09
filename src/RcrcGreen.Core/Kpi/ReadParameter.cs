using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One parameter read off one element, as plain values.
    ///
    /// Printed is what Revit shows in a Properties panel, so an area comes back with its unit
    /// and rounding. Raw is the number underneath, which for a length is feet and for an area
    /// square feet whatever the project displays. Both are kept because the difference between
    /// them is one of the nine questions.
    /// </summary>
    public sealed class ReadParameter
    {
        public const string Shared = "shared";

        public const string BuiltIn = "built-in";

        /// <summary>
        /// A project parameter and a family parameter look the same from the outside, both
        /// with a positive id and no GUID, so the two are not told apart here.
        /// </summary>
        public const string ProjectOrFamily = "project or family";

        public ReadParameter(
            string name,
            string kind,
            string storageType,
            string guid,
            bool hasValue,
            string printed,
            string raw)
        {
            if (name == null) throw new ArgumentNullException("name");

            Name = name;
            Kind = kind ?? string.Empty;
            StorageType = storageType ?? string.Empty;
            Guid = guid ?? string.Empty;
            HasValue = hasValue;
            Printed = printed ?? string.Empty;
            Raw = raw ?? string.Empty;
        }

        public string Name { get; }

        public string Kind { get; }

        public string StorageType { get; }

        /// <summary>
        /// Empty unless the parameter is shared. The finished tool will most likely look a
        /// parameter up by this rather than by name, so it is worth having in the file.
        /// </summary>
        public string Guid { get; }

        public bool HasValue { get; }

        public string Printed { get; }

        public string Raw { get; }

        /// <summary>
        /// True when this is the parameter somebody is looking for, compared exactly, because
        /// PRX_Plot_ID and PRX_Ref Plot ID differ by punctuation and are two parameters.
        /// </summary>
        public bool IsNamed(string exactly)
        {
            return string.Equals(Name, exactly, StringComparison.Ordinal);
        }
    }
}
