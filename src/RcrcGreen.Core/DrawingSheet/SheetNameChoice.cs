using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Which of the four places a sheet's name came from. It is shown beside the box, because
    /// a name somebody typed for every plot at once and a name this one plot overrides are
    /// different things to the person deciding whether to change it.
    ///
    /// Not <see cref="SheetNameSource"/>, which answers a different question: which settings
    /// file a view type's saved sheet name came out of.
    /// </summary>
    public enum SheetNameFrom
    {
        /// <summary>
        /// Nothing typed and nothing proposed. The sheet is not made until a name arrives.
        /// </summary>
        Nothing = 0,

        /// <summary>
        /// Typed once on the definition and used on every sheet it makes.
        /// </summary>
        Definition = 1,

        /// <summary>
        /// Typed on this plot's own row, over the definition's.
        /// </summary>
        OnThisPlot = 2,

        /// <summary>
        /// The saved sheet name for the one view's type, or the derivation when neither
        /// settings file holds one. Which of those two it was is
        /// <see cref="PlannedSheet.NamedInWords"/>, and is not repeated here.
        /// </summary>
        Proposed = 3
    }

    /// <summary>
    /// The name one sheet comes out with, and which of the four places it came from.
    ///
    /// **A typed name belongs to the definition, not to the plot.** A sheet holding more than
    /// one view has no view to name it after, so somebody types it, and it used to be asked for
    /// once per plot. On a run over 35 sub plots that is 35 boxes wanting the same words, and
    /// the sixth definition read "34 rows still need a name or a number" after the user had
    /// typed it. The model settles it: 600001A, 600002A and 600005A are all called HARDSCAPE
    /// SCHEDULES on three different sub plots, so the name never varies by plot.
    ///
    /// The per-plot box stays as an override, because nothing measured says a plot can never
    /// want its own, and taking the ability away would be inventing a rule.
    ///
    /// A sheet holding exactly one view is unaffected. Its name comes from the sheet name table
    /// as it always has.
    /// </summary>
    public sealed class SheetNameChoice
    {
        private SheetNameChoice(string name, SheetNameFrom source)
        {
            Name = name ?? string.Empty;
            Source = source;
        }

        /// <summary>
        /// The four places, in the order they win.
        /// </summary>
        /// <param name="onThisPlot">What this plot's own row holds, or null when its box was
        /// never touched. An empty string is a real answer: it is somebody clearing the box,
        /// and it has to beat the definition's name or the clearing would do nothing.</param>
        /// <param name="onTheDefinition">What the definition's one box holds, or null when it
        /// was never touched. Empty means the same thing it does on a row.</param>
        /// <param name="planned">The sheet, which knows whether it names itself.</param>
        public static SheetNameChoice Of(
            string onThisPlot, string onTheDefinition, PlannedSheet planned)
        {
            if (onThisPlot != null)
            {
                return new SheetNameChoice(onThisPlot, SheetNameFrom.OnThisPlot);
            }

            if (onTheDefinition != null)
            {
                return new SheetNameChoice(onTheDefinition, SheetNameFrom.Definition);
            }

            string proposed = planned == null ? string.Empty : planned.ProposedName;

            return proposed.Length == 0
                ? new SheetNameChoice(string.Empty, SheetNameFrom.Nothing)
                : new SheetNameChoice(proposed, SheetNameFrom.Proposed);
        }

        public string Name { get; }

        public SheetNameFrom Source { get; }

        /// <summary>
        /// Whether somebody typed this, which is what decides if the report calls the name
        /// derived. A cleared box is still typed: they chose to empty it.
        /// </summary>
        public bool WasTyped
        {
            get
            {
                return Source == SheetNameFrom.Definition
                    || Source == SheetNameFrom.OnThisPlot;
            }
        }

        public bool HasName
        {
            get { return Name.Length > 0; }
        }

        /// <summary>
        /// What the line under a row's name box says. Empty for a name proposed from the
        /// table, because <see cref="PlannedSheet.NamedInWords"/> already says where that
        /// came from and two sentences for one fact is two facts.
        /// </summary>
        public string InWords()
        {
            switch (Source)
            {
                case SheetNameFrom.Definition:
                    return "From this sheet's name, used on every plot.";

                case SheetNameFrom.OnThisPlot:
                    return "Typed for this plot, over the name on the sheet.";

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// What the definition says above its rows: the one name and how many plots take it.
        /// </summary>
        public static string OnEveryPlotInWords(string name, int plots)
        {
            if (plots <= 0) return "No plot is ticked, so this name goes on nothing yet.";

            string given = (name ?? string.Empty).Trim().Length == 0
                ? "This sheet has no name yet, so nothing is made on "
                : "Named " + name + " on ";

            return given + (plots == 1 ? "the 1 ticked sub plot." : plots + " ticked sub plots.");
        }
    }
}
