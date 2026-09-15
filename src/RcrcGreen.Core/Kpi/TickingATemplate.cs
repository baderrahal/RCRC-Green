using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Ticking a workbook row ticks the plots that will go into it, and unticking it takes them
    /// off again.
    ///
    /// **The specification this replaces was wrong.** The first press over several templates
    /// ticked MOSQUES, PARKING, SCHOOLS and STREETS and six plots, all of them SC, and three of
    /// the four templates read no ticked plot belongs to them. The tool did exactly what it was
    /// told. A person who ticks MOSQUES has already said which plots they mean, and making them
    /// find a grouping button and press that too is the same fact asked for twice.
    ///
    /// **IT TICKS BY THE SPLIT'S OWN RULE AND NEVER BY THE PREFIX.** The grouping buttons this
    /// replaces gathered plots with <see cref="PlotPrefixes.For"/>, which reads the two letters
    /// at the front of a plot identifier and nothing else, while the split that decides which
    /// workbook a plot really goes into reads PRX_Component first. Two rules for one question is the
    /// fault this repository keeps paying for: a plot ticked by the prefix could then land in no
    /// workbook at all, and the row's count would be a number nothing else agreed with. So this
    /// asks <see cref="PlotsPerTemplate.For"/>, the one rule, and the row's count is what will
    /// really go in.
    ///
    /// **A PLOT TICKED OR UNTICKED BY HAND WINS.** Ticking a template is a starting point rather
    /// than a lock, so a plot the user took off by hand stays off when the template it belongs
    /// to is ticked, and the row then says 19 of its 20 plots rather than 20. The hand list is
    /// what the pane holds and this reads it.
    ///
    /// **THE TWO PRESSES ARE SYMMETRIC NOW, AND THEY WERE NOT.** <see cref="Ticked"/> took the
    /// hand list and <see cref="Unticked"/> did not, so unticking a row removed every plot of
    /// that template including ones the person had ticked by hand, and nothing recorded it.
    /// Unticking a row undoes exactly what ticking it did: it skips the plots put on by hand the
    /// way ticking skips the plots taken off by hand, so ticking the row again restores the same
    /// state. Both read one <see cref="HandTicks"/>, which is the one record of what a person
    /// chose.
    /// </summary>
    public static class TickingATemplate
    {
        /// <summary>
        /// Every plot the model holds that belongs to one template, by the split's own rule, in
        /// the order the model lists them.
        /// </summary>
        public static IReadOnlyList<string> PlotsOf(
            KpiTemplate template, IEnumerable<string> plots, Func<string, string> componentOf)
        {
            if (componentOf == null) throw new ArgumentNullException("componentOf");
            if (template == null) return new List<string>();

            return (plots ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .Where(one => ReferenceEquals(PlotsPerTemplate.For(one, componentOf(one)).Template, template))
                .ToList();
        }

        /// <summary>
        /// The ticks after a template row is ticked: what was ticked already, plus every plot of
        /// that template the user has not held off by hand. It ADDS rather than replaces,
        /// because several templates can be ticked at once now and a replacement would empty the
        /// row ticked before it.
        /// </summary>
        public static PlotTicks Ticked(
            PlotTicks ticks,
            KpiTemplate template,
            Func<string, string> componentOf,
            HandTicks byHand)
        {
            if (ticks == null) throw new ArgumentNullException("ticks");

            HandTicks hand = byHand ?? HandTicks.None;

            PlotTicks next = ticks;
            foreach (string plotId in PlotsOf(template, ticks.Plots.All, componentOf))
            {
                if (hand.IsOff(plotId)) continue;
                next = next.With(plotId);
            }

            return next;
        }

        /// <summary>
        /// The ticks after a template row is unticked: its plots come off, every other row's
        /// stay, and **a plot the person put on by hand stays on**. A plot that belongs to no
        /// other ticked template and that nobody picked simply goes.
        ///
        /// **THAT LAST CLAUSE IS THE FIX.** It took no hand list at all, so a person who ticked
        /// three mosque plots by hand, then ticked the MOSQUES row for the rest, then changed
        /// their mind and unticked the row, lost their three with nothing said. A hand untick
        /// survived a row tick and a hand tick did not survive a row untick, which is the
        /// asymmetry this pair is named for.
        /// </summary>
        public static PlotTicks Unticked(
            PlotTicks ticks, KpiTemplate template, Func<string, string> componentOf, HandTicks byHand)
        {
            if (ticks == null) throw new ArgumentNullException("ticks");

            HandTicks hand = byHand ?? HandTicks.None;

            PlotTicks next = ticks;
            foreach (string plotId in PlotsOf(template, ticks.Plots.All, componentOf))
            {
                if (hand.IsOn(plotId)) continue;
                next = next.Without(plotId);
            }

            return next;
        }

        /// <summary>
        /// What a row says about how many of its plots are really going in, for a template whose
        /// plots the user has edited by hand. **The count is what will actually go in, never how
        /// many the template could take.**
        ///
        /// **IT BUILT THIS SENTENCE AND NOTHING SHOWED IT.** Its only references were two lines
        /// of its own test file, so it was green and had never reached a screen, for every round
        /// since it was written. It is the line that answers why a plot of a ticked template is
        /// not going in, on the row a person is looking at when they press, and the whole of the
        /// eighty second pass went by asking that question of a report instead. **A description
        /// of the tool is not the tool**, and a method tested and never called is that shape with
        /// a green tick on it.
        /// </summary>
        /// <summary>
        /// The line one ticked row shows, counted off the ticks and the model's own plot list.
        ///
        /// **THE PANE COUNTS NOTHING.** The row needs how many of a template's plots are going
        /// in and how many belong to it, and both come off the split's own rule, so working them
        /// out beside the control that draws them would be a second record of the row's count.
        /// It is here with the sentence, and the pane's whole share of it is one call.
        /// </summary>
        public static string RowLine(
            KpiTemplate template, PlotTicks ticks, Func<string, string> componentOf)
        {
            if (ticks == null) return string.Empty;

            IReadOnlyList<string> belong = PlotsOf(template, ticks.Plots.All, componentOf);

            return SomeOfThem(template, belong.Count(one => ticks.IsTicked(one)), belong.Count);
        }

        public static string SomeOfThem(KpiTemplate template, int going, int couldGo)
        {
            if (template == null) throw new ArgumentNullException("template");
            if (going >= couldGo) return string.Empty;

            return template.Name + ": " + going + " of the " + couldGo
                + " plots that belong to it, the rest unticked by hand.";
        }
    }
}
