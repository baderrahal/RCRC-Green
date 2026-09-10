using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// What kind of view a sibling is, which decides what it can answer.
    /// </summary>
    public enum SiblingKind
    {
        /// <summary>
        /// Anything that is neither a plan nor a section. It can still give a family type, a
        /// template and the crop settings, and it cannot give a level.
        /// </summary>
        Other,

        Plan,

        Section
    }

    /// <summary>
    /// The three crop settings a view carries, which travel together because Revit will not let
    /// the second two mean anything without the first.
    ///
    /// Two of the three are copied off the sibling. Annotation Crop is not, any more. Copying it
    /// was tried and did not fix anything, because PL-17-(010) Overall Plan has it off and the
    /// new view inherited the fault faithfully. See <see cref="AnnotationCropChoice"/>.
    /// </summary>
    public sealed class ViewCrop
    {
        public ViewCrop(bool cropActive, bool cropRegionVisible, bool annotationCrop)
        {
            CropActive = cropActive;
            CropRegionVisible = cropRegionVisible;
            AnnotationCrop = annotationCrop;
        }

        /// <summary>
        /// Crop View. Revit refuses Annotation Crop while this is off, so it is copied first
        /// and copied even though nobody asked for it by name.
        /// </summary>
        public bool CropActive { get; }

        public bool CropRegionVisible { get; }

        public bool AnnotationCrop { get; }

        /// <summary>
        /// The two settings a new view really does take from the sibling. Annotation Crop is
        /// said separately, because it is the tool's own and printing it here as though it had
        /// been read off a view is the kind of line that cost this repo two rounds.
        /// </summary>
        public string CopiedInWords()
        {
            return (CropActive ? "crop on" : "crop off")
                + ", " + (CropRegionVisible ? "crop region shown" : "crop region hidden");
        }

        public override string ToString()
        {
            return CopiedInWords()
                + ", " + (AnnotationCrop ? "annotation crop on" : "annotation crop off");
        }
    }

    /// <summary>
    /// Annotation Crop on a new plan view. It is on, always, whatever the sibling has.
    ///
    /// Copying it from the sibling was the last round's answer and it did not work. The report
    /// said it plainly: DM-11-(010) Overall Plan was set up from PL-17-(010) Overall Plan, which
    /// has annotation crop off, so the new view got it off and every neighbouring plot's section
    /// marker drew straight through it.
    ///
    /// The model is inconsistent, so there is nothing to copy. This is the same shape as the
    /// section depth: the team named a value, the tool applies it, and the report says whose it
    /// is rather than letting it read as something found in the model.
    /// </summary>
    public sealed class AnnotationCropChoice
    {
        private AnnotationCropChoice(string siblingName, bool theSiblingHadItOn)
        {
            SiblingName = siblingName ?? string.Empty;
            TheSiblingHadItOn = theSiblingHadItOn;
        }

        public static AnnotationCropChoice ForAPlanView(SiblingView sibling)
        {
            return sibling == null
                ? new AnnotationCropChoice(string.Empty, false)
                : new AnnotationCropChoice(sibling.ViewName, sibling.Crop.AnnotationCrop);
        }

        /// <summary>
        /// Always true. It is a property rather than a constant so the writer reads the decision
        /// from here rather than holding a second copy of it.
        /// </summary>
        public bool On
        {
            get { return true; }
        }

        public bool TheSiblingHadItOn { get; }

        public string SiblingName { get; }

        public string InWords()
        {
            string whose = "Annotation crop is on, which is the tool's setting on every plan "
                + "view rather than anything read off a view.";

            if (SiblingName.Length == 0) return whose;

            return TheSiblingHadItOn
                ? whose + " " + SiblingName + " has it on as well."
                : whose + " " + SiblingName + " has it off, and a view with it off draws the "
                    + "section markers of neighbouring plots through itself.";
        }
    }

    /// <summary>
    /// One view the model already holds, read into plain values.
    ///
    /// Everything a new view is set up from comes off ONE of these: the family type, the view
    /// template, the level for a plan, and the three crop settings. They are on one object so
    /// they cannot come from two different views, which is the whole point of the type.
    ///
    /// The first run that created views produced a view whose template read (010) Overall Plan
    /// and whose family type read (200) General Arrangement Layout. Both were copied from the
    /// same sibling four lines apart, so the code could not have split them, but nothing in the
    /// report said which view they came from and there was no way to check. Now there is.
    ///
    /// The far clip offset used to be here too. It is not any more. Four real sections read
    /// 0.93, 0.93, 1.53 and 12.83 metres, so the model has no rule to copy and the sibling made
    /// it a lottery. See <see cref="SectionDepth"/>.
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
            ViewCrop crop)
        {
            if (viewName == null) throw new ArgumentNullException("viewName");
            if (type == null) throw new ArgumentNullException("type");

            ViewName = viewName;
            Type = type;
            Kind = kind;
            FamilyTypeName = familyTypeName ?? string.Empty;
            TemplateName = templateName ?? string.Empty;
            LevelName = levelName ?? string.Empty;
            Crop = crop ?? new ViewCrop(false, false, false);
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

        public ViewCrop Crop { get; }

        /// <summary>
        /// What the report says about where a new view was set up from. One line, naming the
        /// view and every setting taken off it, so a Properties panel is never needed to check
        /// it again.
        /// </summary>
        public string InWords()
        {
            return "Set up from " + ViewName + ": family type " + Named(FamilyTypeName)
                + ", view template " + Named(TemplateName)
                + (Kind == SiblingKind.Plan ? ", level " + Named(LevelName) : string.Empty)
                + ", " + Crop.CopiedInWords() + ".";
        }

        private static string Named(string what)
        {
            return what.Length == 0 ? "none" : what;
        }

        /// <summary>
        /// Why this view cannot set up a new one of the wanted kind.
        ///
        /// The model draws one view type both ways and the choice falls back to any view of
        /// the type, so a plan asked of a section was refused as sitting on no level, which
        /// describes a section as a plan with a fault, and a section asked of a plan reached
        /// Revit and came back as a bad argument. Both named the wrong cause. This names the
        /// kind it is and the kind that was wanted.
        /// </summary>
        public string WrongKindInWords(SiblingKind wanted)
        {
            return "No " + Type + " in this model is a " + KindInWords(wanted)
                + ". The nearest is " + ViewName + ", which is a " + KindInWords(Kind)
                + ", so there is nothing to take "
                + (wanted == SiblingKind.Plan ? "a level and a family type" : "a family type")
                + " from. Make one by hand on any plot and this will follow it.";
        }

        private static string KindInWords(SiblingKind kind)
        {
            switch (kind)
            {
                case SiblingKind.Plan:
                    return "plan view";
                case SiblingKind.Section:
                    return "section";
                default:
                    return "view that is neither a plan nor a section";
            }
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
    ///
    /// It is a lottery on this model and it is meant to stay one. Three (010) views were set up
    /// from three different siblings carrying three different family types, all copied
    /// faithfully. Picking the most common family type would hide that behind a winner the team
    /// has not chosen. The scan report names the disagreement instead.
    /// </summary>
    public static class SiblingChoice
    {
        /// <summary>
        /// The first candidate of that view type which can answer everything asked of it, and
        /// failing that the first of that view type at all.
        ///
        /// The kind matters because a plan needs a level, and the model holds view types drawn
        /// both ways. Taking the first match regardless meant a run could refuse a plan view
        /// because the first view of that type happened to be a section, while a perfectly good
        /// plan sat further down the list.
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
}
