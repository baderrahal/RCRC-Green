using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One filter row on a schedule, as plain values.
    /// </summary>
    public sealed class ScheduleFilterRule
    {
        public ScheduleFilterRule(string parameterName, string value)
        {
            if (parameterName == null) throw new ArgumentNullException("parameterName");

            ParameterName = parameterName;
            Value = value ?? string.Empty;
        }

        /// <summary>
        /// The name as it reads in the model, spaces and all. The plot parameter on a quantity
        /// schedule is "PRX_Ref Plot ID", with spaces, and it is a different parameter from
        /// PRX_Plot_ID on the Sheet List.
        /// </summary>
        public string ParameterName { get; }

        public string Value { get; }

        /// <summary>
        /// True when this rule is the one naming the plot, so create knows which value to swap
        /// and leaves every other rule alone. HARDSCAPE and SHRUBS AND LAWN are both category
        /// Floors and are told apart only by the rules that are not this one.
        /// </summary>
        public bool NamesThePlot
        {
            get { return PlotId.IsPlotId(Value); }
        }

        public ScheduleFilterRule ForPlot(string plotId)
        {
            return NamesThePlot ? new ScheduleFilterRule(ParameterName, plotId) : this;
        }

        public override string ToString()
        {
            return ParameterName + " equals " + Value;
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
            IEnumerable<string> fieldsInOrder,
            IEnumerable<ScheduleFilterRule> filters,
            bool includesLinkedFiles,
            bool isASheetList)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (categoryName == null) throw new ArgumentNullException("categoryName");

            Type = type;
            CategoryName = categoryName;

            // Order is the whole point of the field list, so it is kept exactly as read and
            // never sorted or deduplicated into something tidier.
            FieldsInOrder = (fieldsInOrder ?? Enumerable.Empty<string>())
                .Where(field => field != null)
                .ToList();

            Filters = (filters ?? Enumerable.Empty<ScheduleFilterRule>())
                .Where(rule => rule != null)
                .ToList();

            IncludesLinkedFiles = includesLinkedFiles;
            IsASheetList = isASheetList;
        }

        public ViewType Type { get; }

        public string CategoryName { get; }

        public IReadOnlyList<string> FieldsInOrder { get; }

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
                IsASheetList);
        }

        /// <summary>
        /// The name the new schedule is given, the same shape every view and sheet here uses.
        /// </summary>
        public string NameFor(string plotId)
        {
            return plotId + "-(" + Type.Code + ") " + Type.ViewName;
        }
    }
}
