using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// How a schedule filter holds its value.
    ///
    /// Revit stores a filter value in one of four ways and rebuilding it as the wrong one is
    /// refused. Two schedules were lost to that: both filter on PRX_Included In Budget equals
    /// Yes, which is a Yes/No parameter, so Revit holds it as the integer 1. Capture flattened
    /// every value to text and create handed the text back, and Revit answered that the filter
    /// value is not valid for the field and filter type.
    /// </summary>
    public enum FilterValueKind
    {
        Text = 0,

        /// <summary>
        /// Includes every Yes/No parameter, which Revit holds as 1 or 0 rather than as words.
        /// </summary>
        WholeNumber = 1,

        Number = 2,

        /// <summary>
        /// A reference to another element, held as its id.
        /// </summary>
        ElementReference = 3
    }

    /// <summary>
    /// One filter value, with the kind it has to go back as.
    ///
    /// The text form is kept alongside the typed value rather than derived on demand, because
    /// the text is what the report prints and what a plot swap replaces, and re-deriving it in
    /// two places is how this repo has produced a bug five times.
    /// </summary>
    public sealed class FilterValue
    {
        private FilterValue(FilterValueKind kind, string asText, long whole, double number)
        {
            Kind = kind;
            AsText = asText ?? string.Empty;
            WholeNumber = whole;
            Number = number;
        }

        public static FilterValue Text(string value)
        {
            return new FilterValue(FilterValueKind.Text, value ?? string.Empty, 0L, 0.0);
        }

        public static FilterValue OfWholeNumber(int value)
        {
            return new FilterValue(
                FilterValueKind.WholeNumber,
                value.ToString(CultureInfo.InvariantCulture),
                value,
                value);
        }

        public static FilterValue OfNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentException("That filter value is not a real number.", "value");
            }

            return new FilterValue(
                FilterValueKind.Number,
                value.ToString("R", CultureInfo.InvariantCulture),
                (long)value,
                value);
        }

        public static FilterValue OfElementReference(long elementId)
        {
            return new FilterValue(
                FilterValueKind.ElementReference,
                elementId.ToString(CultureInfo.InvariantCulture),
                elementId,
                elementId);
        }

        public FilterValueKind Kind { get; }

        public string AsText { get; }

        /// <summary>
        /// The integer form, for a whole number or an element reference.
        /// </summary>
        public long WholeNumber { get; }

        public double Number { get; }

        public override string ToString()
        {
            return AsText;
        }
    }

    /// <summary>
    /// One filter row on a schedule, as plain values.
    /// </summary>
    public sealed class ScheduleFilterRule
    {
        public ScheduleFilterRule(string parameterName, FilterValue value)
        {
            if (parameterName == null) throw new ArgumentNullException("parameterName");
            if (value == null) throw new ArgumentNullException("value");

            ParameterName = parameterName;
            Held = value;
        }

        public ScheduleFilterRule(string parameterName, string value)
            : this(parameterName, FilterValue.Text(value))
        {
        }

        /// <summary>
        /// The name as it reads in the model, spaces and all. The plot parameter on a quantity
        /// schedule is "PRX_Ref Plot ID", with spaces, and it is a different parameter from
        /// PRX_Plot_ID on the Sheet List.
        /// </summary>
        public string ParameterName { get; }

        public FilterValue Held { get; }

        public string Value
        {
            get { return Held.AsText; }
        }

        public FilterValueKind Kind
        {
            get { return Held.Kind; }
        }

        /// <summary>
        /// True when this rule is the one naming the plot, so create knows which value to swap
        /// and leaves every other rule alone. HARDSCAPE and SHRUBS AND LAWN are both category
        /// Floors and are told apart only by the rules that are not this one.
        /// </summary>
        public bool NamesThePlot
        {
            get { return Held.Kind == FilterValueKind.Text && PlotId.IsPlotId(Value); }
        }

        public ScheduleFilterRule ForPlot(string plotId)
        {
            return NamesThePlot
                ? new ScheduleFilterRule(ParameterName, FilterValue.Text(plotId))
                : this;
        }

        public override string ToString()
        {
            return ParameterName + " equals " + Value;
        }
    }

    /// <summary>
    /// What a schedule field is, which decides whether it can be added to a new schedule at all.
    /// </summary>
    public enum ScheduleFieldKind
    {
        /// <summary>
        /// A parameter of the scheduled elements. It appears in GetSchedulableFields and can be
        /// added by name.
        /// </summary>
        AParameter = 0,

        /// <summary>
        /// A formula, a percentage, a count or a combined parameter. It is defined inside the
        /// schedule that holds it rather than read off the model, so it is not in
        /// GetSchedulableFields and no amount of name matching will find it.
        /// </summary>
        Calculated = 1
    }

    /// <summary>
    /// One field on a schedule, in the order it appears.
    ///
    /// Three schedules came out short of a field and the report said they were not schedulable
    /// for the category, which sends somebody to look at the category. The kind is captured now,
    /// so a calculated field is named as one.
    /// </summary>
    public sealed class ScheduleFieldEntry
    {
        public ScheduleFieldEntry(string name, ScheduleFieldKind kind)
        {
            if (name == null) throw new ArgumentNullException("name");

            Name = name;
            Kind = kind;
        }

        public static ScheduleFieldEntry Parameter(string name)
        {
            return new ScheduleFieldEntry(name, ScheduleFieldKind.AParameter);
        }

        public static ScheduleFieldEntry Calculated(string name)
        {
            return new ScheduleFieldEntry(name, ScheduleFieldKind.Calculated);
        }

        public string Name { get; }

        public ScheduleFieldKind Kind { get; }

        public bool IsCalculated
        {
            get { return Kind == ScheduleFieldKind.Calculated; }
        }

        /// <summary>
        /// What the report says when this field could not be added. The two cases send somebody
        /// to two different places, so they do not share a sentence.
        /// </summary>
        public string WhyItIsMissing()
        {
            return IsCalculated
                ? Name + ", a calculated field defined inside the source schedule, which Revit "
                    + "does not offer to a new one and which has to be written again by hand"
                : Name + ", which this category does not offer as a field here";
        }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Everything needed to build one schedule, with no Revit type anywhere in it.
    ///
    /// This sits between reading a schedule and writing one. Duplicating the nearest schedule
    /// would have been shorter and would have left the tool useless on a fresh project holding
    /// none, which is the project it will be pointed at next. Capture fills one of these in,
    /// create builds from one, and duplicating is the two run back to back. Loading one from a
    /// file for a model that has no schedules to capture is then a small round rather than a
    /// rewrite.
    /// </summary>
    public sealed class ScheduleDefinition
    {
        public ScheduleDefinition(
            ViewType type,
            string categoryName,
            IEnumerable<ScheduleFieldEntry> fieldsInOrder,
            IEnumerable<ScheduleFilterRule> filters,
            bool includesLinkedFiles,
            bool isASheetList,
            long categoryBuiltInValue = 0L,
            IEnumerable<string> filtersNotRead = null,
            IEnumerable<string> fieldsNotRead = null)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (categoryName == null) throw new ArgumentNullException("categoryName");

            Type = type;
            CategoryName = categoryName;
            CategoryBuiltInValue = categoryBuiltInValue;

            // Order is the whole point of the field list, so it is kept exactly as read and
            // never sorted or deduplicated into something tidier.
            FieldsInOrder = (fieldsInOrder ?? Enumerable.Empty<ScheduleFieldEntry>())
                .Where(field => field != null)
                .ToList();

            Filters = (filters ?? Enumerable.Empty<ScheduleFilterRule>())
                .Where(rule => rule != null)
                .ToList();

            IncludesLinkedFiles = includesLinkedFiles;
            IsASheetList = isASheetList;

            FiltersNotRead = Kept(filtersNotRead);
            FieldsNotRead = Kept(fieldsNotRead);
        }

        private static IReadOnlyList<string> Kept(IEnumerable<string> notRead)
        {
            return (notRead ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrEmpty(one))
                .ToList();
        }

        public ViewType Type { get; }

        /// <summary>
        /// What the category is called, for the report. It is not what a new schedule is built
        /// from any more. KERBS is built on Slab Edges, and looking that name up in
        /// Document.Settings.Categories found nothing, so the schedule could not be made while
        /// the model plainly held the category.
        /// </summary>
        public string CategoryName { get; }

        /// <summary>
        /// Revit's own number for the category, which is what a new schedule is built from. Zero
        /// when the source schedule sits on a category that is not one of Revit's built-in ones.
        /// </summary>
        public long CategoryBuiltInValue { get; }

        public bool HasBuiltInCategory
        {
            get { return CategoryBuiltInValue != 0L; }
        }

        public IReadOnlyList<ScheduleFieldEntry> FieldsInOrder { get; }

        public IReadOnlyList<ScheduleFilterRule> Filters { get; }

        /// <summary>
        /// Every schedule in this model has it ticked. It is captured rather than assumed,
        /// because the next model may not.
        /// </summary>
        public bool IncludesLinkedFiles { get; }

        /// <summary>
        /// A Sheet List is made by a different call from a quantity schedule and filters on
        /// PRX_Plot_ID rather than PRX_Ref Plot ID, so which one this is has to survive capture.
        /// </summary>
        public bool IsASheetList { get; }

        /// <summary>
        /// Filters on the source schedule that capture could not read, each named as far as
        /// the read got. Both used to be dropped with a bare continue, which is how a schedule
        /// short of the filter that tells HARDSCAPE from SHRUBS AND LAWN could be built and
        /// read as correct on a drawing.
        /// </summary>
        public IReadOnlyList<string> FiltersNotRead { get; }

        /// <summary>
        /// Fields on the source schedule whose id resolved to nothing at capture, named by
        /// position because the name is exactly what could not be read.
        /// </summary>
        public IReadOnlyList<string> FieldsNotRead { get; }

        public bool LostAFilterAtCapture
        {
            get { return FiltersNotRead.Count > 0; }
        }

        /// <summary>
        /// Why a definition that lost a filter at capture refuses to build anything. The same
        /// rule a filter lost at write time follows, one step earlier: a schedule missing a
        /// filter shows every plot's elements and reads as correct on a drawing.
        /// </summary>
        public string WhyTheCaptureLossRefusesIt()
        {
            return FiltersNotRead.Count
                + (FiltersNotRead.Count == 1 ? " filter" : " filters")
                + " on the source schedule could not be read when it was captured: "
                + string.Join(", ", FiltersNotRead.ToArray())
                + ". A schedule missing a filter shows every plot's elements and reads as "
                + "correct on a drawing, so nothing is built from this definition.";
        }

        /// <summary>
        /// Why a schedule that exists cannot be made for another plot. Saying no plot has it
        /// sent the user looking for a schedule that is plainly there.
        /// </summary>
        public string WhyItCannotBeAimed()
        {
            return "That schedule exists in this model and filters on no plot, so there is no "
                + "plot filter to swap and it cannot be aimed at another plot. Add the plot "
                + "filter to the source schedule and refresh.";
        }

        /// <summary>
        /// The parameter the plot is filtered on, empty when no rule names a plot. Read off the
        /// captured filters rather than decided here, because which parameter a schedule uses
        /// is a fact about the model and not a rule this code gets to make.
        /// </summary>
        public string PlotParameterName
        {
            get
            {
                ScheduleFilterRule naming = Filters.FirstOrDefault(rule => rule.NamesThePlot);
                return naming == null ? string.Empty : naming.ParameterName;
            }
        }

        public bool CanBeMadeForAnotherPlot
        {
            get { return PlotParameterName.Length > 0; }
        }

        /// <summary>
        /// The same schedule aimed at another plot. Only the rule naming a plot changes. Every
        /// other rule, the field order, the category and the link setting are carried straight
        /// across, because they are what makes it that schedule rather than a different one.
        /// </summary>
        public ScheduleDefinition ForPlot(string plotId)
        {
            if (string.IsNullOrEmpty(plotId)) throw new ArgumentNullException("plotId");

            return new ScheduleDefinition(
                new ViewType(Type.Code, Type.ViewName),
                CategoryName,
                FieldsInOrder,
                Filters.Select(rule => rule.ForPlot(plotId)),
                IncludesLinkedFiles,
                IsASheetList,
                CategoryBuiltInValue,
                FiltersNotRead,
                FieldsNotRead);
        }

        /// <summary>
        /// The name the new schedule is given, the same shape every view and sheet here uses.
        /// </summary>
        public string NameFor(string plotId)
        {
            return ViewNaming.Of(plotId, Type);
        }

        /// <summary>
        /// What the report says when a schedule cannot be built at all. It names the category
        /// both ways, because the name is what a person recognises and the number is what
        /// actually failed to resolve.
        /// </summary>
        public string WhyTheCategoryIsNoGood()
        {
            return HasBuiltInCategory
                ? "This model does not hold category " + CategoryName + ", number "
                    + CategoryBuiltInValue.ToString(CultureInfo.InvariantCulture)
                    + ", so that schedule cannot be built."
                : CategoryName.Length == 0
                    ? "The source schedule sits on a category this tool could not read at all."
                    : CategoryName + " is not one of Revit's own categories, so there is no "
                        + "number to build a new schedule from and the name alone is not enough.";
        }
    }
}
