using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Where a press of Create got its readings: read from the model on this press, or
    /// reused off the run before. Said in the report either way, with why.
    /// </summary>
    public sealed class ReadingsSource
    {
        private ReadingsSource(bool reused, string why)
        {
            Reused = reused;
            Why = why ?? string.Empty;
        }

        public static ReadingsSource Read(string why)
        {
            return new ReadingsSource(false, why);
        }

        public static ReadingsSource Held(string why)
        {
            return new ReadingsSource(true, why);
        }

        public static readonly ReadingsSource ReadOnThisPress = Read("read on this press");

        public bool Reused { get; }

        public string Why { get; }
    }

    /// <summary>
    /// A refusal exists so a person can answer a question, and answering it should not cost
    /// the answer again. Every region choice used to read every ticked plot from the start,
    /// about eight minutes on 78 plots, and the identical areas confirm the same. The
    /// readings already exist on the run the pane holds, so a press after a refusal applies
    /// the choice to them and reads nothing.
    ///
    /// This is not a cache with a lifetime of its own. The run before is trusted when it is
    /// the same model, the same template and file, the same two parameters and the same
    /// plots, every ticked plot has a reading on it that was not refused, and it wrote
    /// nothing. Anything else is named as the reason the model is read again. A model edited between the refusal and
    /// the pick is the one thing none of those can see, and the report's line says which of
    /// the two happened so a person knows what the numbers came off.
    /// </summary>
    public static class HeldReadings
    {
        public static ReadingsSource Decide(
            KpiCreateRun held,
            string documentTitle,
            KpiTemplate template,
            string templatePath,
            string componentParameter,
            string referenceParameter,
            IEnumerable<string> ticked)
        {
            if (template == null) throw new ArgumentNullException("template");

            if (held == null) return ReadingsSource.Read("no run is held, so the model was read");

            if (held.Wrote)
            {
                return ReadingsSource.Read("the run before wrote its workbook, so the model was read again");
            }

            if (!string.Equals(held.DocumentTitle, documentTitle ?? string.Empty, StringComparison.Ordinal))
            {
                return ReadingsSource.Read("the run before was on " + Said(held.DocumentTitle)
                    + " and this press is on " + Said(documentTitle) + ", so the model was read again");
            }

            if (held.Template == null || !string.Equals(held.Template.Name, template.Name, StringComparison.Ordinal))
            {
                return ReadingsSource.Read("the run before was for " + (held.Template == null ? "no template" : held.Template.Name)
                    + " and this press is for " + template.Name + ", so the model was read again");
            }

            if (!string.Equals(held.TemplatePath, templatePath ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return ReadingsSource.Read("the template file changed since the run before, so the model was read again");
            }

            if (!string.Equals(held.ComponentParameter, componentParameter ?? string.Empty, StringComparison.Ordinal)
                || !string.Equals(held.ReferenceParameter, referenceParameter ?? string.Empty, StringComparison.Ordinal))
            {
                return ReadingsSource.Read("a parameter picked on the pane changed since the run before, so the model was read again");
            }

            List<string> wanted = Ordered(ticked);
            if (!wanted.SequenceEqual(held.Reconciliation.Ticked, StringComparer.Ordinal))
            {
                return ReadingsSource.Read("the plots ticked changed since the run before, so the model was read again");
            }

            List<string> read = held.Readings.Select(one => one.PlotId).ToList();
            List<string> unread = wanted.Where(one => !read.Contains(one, StringComparer.Ordinal)).ToList();
            if (unread.Count > 0 || held.Readings.Count != wanted.Count)
            {
                return ReadingsSource.Read("the run before holds no reading for "
                    + (unread.Count > 0 ? string.Join(", ", unread.ToArray()) : "every plot once")
                    + ", so the model was read again");
            }

            // A plot whose read threw, or was refused off a schedule, holds nothing worth
            // reusing, and the model fixed in between is the whole reason for the second press.
            // Reusing it would hand the same refusal back for ever with nothing else changed.
            List<string> refused = held.Readings
                .Where(one => one.ReadRefusals.Count > 0)
                .Select(one => one.PlotId)
                .ToList();
            if (refused.Count > 0)
            {
                return ReadingsSource.Read("the run before could not read " + string.Join(", ", refused.ToArray())
                    + ", so the model was read again");
            }

            return ReadingsSource.Held("the readings of the run before, on the same model, template, parameters and plots, "
                + "with the choices made since applied to them");
        }

        /// <summary>
        /// The run before's readings with each plot's chosen region applied where a choice was
        /// made. A plot with no choice keeps the reading it had, the single region a first run
        /// settled on included, because that reading already carries it.
        /// </summary>
        public static IReadOnlyList<PlotReading> Applied(IEnumerable<PlotReading> held, Func<string, string> chosenFor)
        {
            if (chosenFor == null) throw new ArgumentNullException("chosenFor");

            var applied = new List<PlotReading>();
            foreach (PlotReading reading in (held ?? Enumerable.Empty<PlotReading>()).Where(one => one != null))
            {
                string chosen = chosenFor(reading.PlotId) ?? string.Empty;
                applied.Add(chosen.Length == 0 ? reading : reading.WithChosenRegion(chosen));
            }

            return applied;
        }

        /// <summary>
        /// The same trimming, distinct and natural order <see cref="Reconciliation.Of"/> gives
        /// the ticked list, so the two compare plot for plot.
        /// </summary>
        private static List<string> Ordered(IEnumerable<string> ticked)
        {
            return (ticked ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .Select(one => one.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(one => one, NaturalOrder.Comparer)
                .ToList();
        }

        private static string Said(string title)
        {
            return string.IsNullOrEmpty(title) ? "(no title)" : title;
        }
    }
}
