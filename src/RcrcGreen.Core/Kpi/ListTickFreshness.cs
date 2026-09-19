using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Whether the last press of Tick the list still stands.
    ///
    /// **TICKING A WORKBOOK ROW TICKS ITS PLOTS**, which is `TickingATemplate.Ticked`, so a row
    /// ticked after Tick the list adds that template's plots on top of the team's list and a row
    /// unticked takes some of the list's plots off again. Either way the ticks are no longer the
    /// plots the list named, and nothing on the pane said so.
    ///
    /// **NOTHING HERE TICKS OR UNTICKS ANYTHING.** Bader's rule: the pane says the list tick is
    /// out of date and offers the button again, and the person presses it. A pane that quietly
    /// re-ticked would be moving the ticks without being asked, which is the fault the hand
    /// choices already cost this tool a round.
    /// </summary>
    public sealed class ListTickFreshness
    {
        private readonly IReadOnlyList<string> _rowsAtThePress;

        private ListTickFreshness(bool everPressed, IReadOnlyList<string> rowsAtThePress)
        {
            EverPressed = everPressed;
            _rowsAtThePress = rowsAtThePress ?? new List<string>();
        }

        /// <summary>
        /// Tick the list has not been pressed on this model. **Nothing is out of date**, because
        /// there is no press for a row to have moved out from under.
        /// </summary>
        public static readonly ListTickFreshness NeverPressed =
            new ListTickFreshness(false, null);

        /// <summary>
        /// Tick the list was just pressed, with the workbook rows that were ticked at that
        /// moment. **The rows are the FILE NAMES the pane ticks by**, which is what
        /// `KpiPanel.TickFor` compares, so the two cannot come apart.
        /// </summary>
        public static ListTickFreshness Pressed(IEnumerable<string> workbookRowsTicked)
        {
            return new ListTickFreshness(
                true,
                (workbookRowsTicked ?? Enumerable.Empty<string>())
                    .Select(one => one ?? string.Empty)
                    .ToList());
        }

        public bool EverPressed { get; }

        /// <summary>
        /// The workbook rows as they stood at the press, in the order they were given.
        /// </summary>
        public IReadOnlyList<string> RowsAtThePress
        {
            get { return _rowsAtThePress; }
        }

        /// <summary>
        /// **IT IS A SET AND NOT A COUNTER.** Ticking a row and unticking it again puts the
        /// ticks back where the list press left them, so the list tick still stands and the
        /// pane says nothing. The question is whether the ticks that press produced are still
        /// the ticks, not whether anybody touched a box.
        ///
        /// **Compared with `StringComparer.Ordinal`**, which is what `PlotTicks`,
        /// `PlotsInTheModel.Holds` and `KpiPanel.TickFor` all use. A second comparison here
        /// would be two rules for one question.
        /// </summary>
        public bool OutOfDate(IEnumerable<string> workbookRowsTickedNow)
        {
            if (!EverPressed) return false;

            var now = new HashSet<string>(
                (workbookRowsTickedNow ?? Enumerable.Empty<string>())
                    .Select(one => one ?? string.Empty),
                StringComparer.Ordinal);

            var then = new HashSet<string>(_rowsAtThePress, StringComparer.Ordinal);

            return !now.SetEquals(then);
        }

        public const string OutOfDateLine =
            "A workbook row has been ticked or unticked since Tick the list was pressed, so the "
            + "ticked plots are no longer the plots the list names. Press Tick the list again to "
            + "put them back. Nothing has been ticked or unticked for you.";

        /// <summary>
        /// The line the pane shows, empty where the list tick still stands or was never pressed.
        /// **A line about nothing is one the team reads past on every other press.**
        /// </summary>
        public string InWords(IEnumerable<string> workbookRowsTickedNow)
        {
            return OutOfDate(workbookRowsTickedNow) ? OutOfDateLine : string.Empty;
        }
    }
}
