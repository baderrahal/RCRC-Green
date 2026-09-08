using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// What kind of view a sibling is, which decides what it can answer.
    /// </summary>
    public enum SiblingKind
    {
        /// <summary>
        /// Anything that is neither a plan nor a section. It can still give a family type and a
        /// template, and it cannot give a level or a far clip.
        /// </summary>
        Other,

        Plan,

        Section
    }

    /// <summary>
    /// One view the model already holds, read into plain values.
    ///
    /// Everything a new view is set up from comes off ONE of these. The family type, the level,
    /// the view template and, for a section, the far clip offset. They are on one object so
    /// they cannot come from two different views, which is the whole point of the type.
    ///
    /// The first run that created views produced a view whose template read (010) Overall Plan
    /// and whose family type read (200) General Arrangement Layout. Both were copied from the
    /// same `sibling` local four lines apart, so the code could not have split them, but
    /// nothing in the report said which view they came from and there was no way to check.
    /// Now there is: the source is named on the object and printed in the report.
    /// </summary>
    public sealed class SiblingView
    {
        public SiblingView(
            string viewName,
            ViewType type,
            SiblingKind kind,
            string familyTypeName,
            string templateName,
            string levelName,
            double farClipFeet,
            bool hasFarClip)
        {
            if (viewName == null) throw new ArgumentNullException("viewName");
            if (type == null) throw new ArgumentNullException("type");
            if (hasFarClip && (double.IsNaN(farClipFeet) || double.IsInfinity(farClipFeet)))
            {
                throw new ArgumentException("That far clip offset is not a real length.", "farClipFeet");
            }

            ViewName = viewName;
            Type = type;
            Kind = kind;
            FamilyTypeName = familyTypeName ?? string.Empty;
            TemplateName = templateName ?? string.Empty;
            LevelName = levelName ?? string.Empty;
            FarClipFeet = farClipFeet;
            HasFarClip = hasFarClip;
        }

        /// <summary>
        /// The one thing that makes this checkable. Every setting below came off the view with
        /// this name, and the report says so.
        /// </summary>
        public string ViewName { get; }

        public ViewType Type { get; }

        public SiblingKind Kind { get; }

        public string FamilyTypeName { get; }

        /// <summary>
        /// Empty when the sibling carries no template, which is reported rather than filled in.
        /// </summary>
        public string TemplateName { get; }

        public string LevelName { get; }

        public double FarClipFeet { get; }

        /// <summary>
        /// False when the view is not a section or its far clip is not set. The depth then falls
        /// back to the value the team named, and the report says which of the two was used.
        /// </summary>
        public bool HasFarClip { get; }

        public bool HasTemplate
        {
            get { return TemplateName.Length > 0; }
        }

        /// <summary>
        /// What the report says about where a new view was set up from. One line, naming the
        /// view and both settings, so a Properties panel is never needed to check it again.
        /// </summary>
        public string InWords()
        {
            return "Set up from " + ViewName + ": family type " + Named(FamilyTypeName)
                + ", view template " + Named(TemplateName)
                + (Kind == SiblingKind.Plan ? ", level " + Named(LevelName) : string.Empty) + ".";
        }

        private static string Named(string what)
        {
            return what.Length == 0 ? "none" : what;
        }

        public override string ToString()
        {
            return ViewName;
        }
    }

    /// <summary>
    /// Picks the one view a new view is set up from.
    ///
    /// A view type can have several views in the model and they need not agree. Picking here
    /// rather than in the Revit code is what lets a test hold the chosen family type against
    /// the chosen template and prove they belong to the same view.
    /// </summary>
    public static class SiblingChoice
    {
        /// <summary>
        /// The first candidate of that view type which can answer everything asked of it, and
        /// failing that the first of that view type at all.
        ///
        /// The kind matters because a plan needs a level and a section needs a far clip, and
        /// the model holds view types drawn both ways. Taking the first match regardless meant
        /// a run could refuse a plan view because the first view of that type happened to be a
        /// section, while a perfectly good plan sat further down the list.
        /// </summary>
        public static SiblingView For(
            ViewType type, SiblingKind wanted, IEnumerable<SiblingView> candidates)
        {
            if (type == null) throw new ArgumentNullException("type");

            List<SiblingView> ofThatType = (candidates ?? Enumerable.Empty<SiblingView>())
                .Where(one => one != null)
                .Where(one => one.Type.Equals(type))
                .ToList();

            return ofThatType.FirstOrDefault(one => one.Kind == wanted)
                ?? ofThatType.FirstOrDefault();
        }
    }

    /// <summary>
    /// How deep a new section looks, and which of the two sources said so.
    /// </summary>
    public sealed class SectionDepthChoice
    {
        private SectionDepthChoice(double feet, bool fromTheSibling, string siblingName)
        {
            Feet = feet;
            FromTheSibling = fromTheSibling;
            SiblingName = siblingName ?? string.Empty;
        }

        public double Feet { get; }

        public bool FromTheSibling { get; }

        public string SiblingName { get; }

        /// <summary>
        /// The sibling's far clip offset when it has one, and the value the team named when it
        /// does not.
        ///
        /// SectionDefaults held 10 metres, chosen before anybody had looked at a section in this
        /// model. A real one reads 42.1054 feet, which is 12.83 metres. The model is the better
        /// answer wherever it has one, and the number the team named is the fallback rather than
        /// the rule.
        /// </summary>
        public static SectionDepthChoice For(SiblingView sibling, double fallbackFeet)
        {
            if (sibling != null && sibling.HasFarClip)
            {
                return new SectionDepthChoice(sibling.FarClipFeet, true, sibling.ViewName);
            }

            return new SectionDepthChoice(
                fallbackFeet, false, sibling == null ? string.Empty : sibling.ViewName);
        }

        public string InWords()
        {
            string howFar = SectionDepth.InMetres(Feet).ToString("0.##", CultureInfo.InvariantCulture)
                + " metres";

            if (FromTheSibling) return "Looks " + howFar + ", taken from " + SiblingName + ".";

            return "Looks " + howFar + ", the value the team named, because "
                + (SiblingName.Length == 0 ? "no section of that type was found" : SiblingName
                    + " has no far clip offset set")
                + ".";
        }
    }
}
