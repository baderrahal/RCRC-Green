using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Runs every view name in a scan through <see cref="ViewNameParser"/> and counts the
    /// answers.
    ///
    /// This is the part of the scan the first real model was needed for. The parser was
    /// written from four example names, and a model called NG05 has no dash in it. The tally
    /// says how far off the guess was, and the list of refusals says what the naming actually
    /// looks like.
    ///
    /// Sheet names and sheet numbers were tallied here too, and the tally read 0 of 1,385
    /// parsed on every scan. Neither is shaped like a view name: a sheet is named after its
    /// view with no plot and no code, 010QF LIST OF DRAWINGS on the first real model, and
    /// numbered by code, plot letter and sheet letter. A count of failures nobody can act on
    /// reads as a fault in the model, so both tallies are gone.
    /// </summary>
    public sealed class NameParseSummary
    {
        public const string ViewNameKind = "view name";

        /// <summary>
        /// How many refusals the report prints per kind. The tally still counts them all.
        /// </summary>
        public const int ShownPerKind = 20;

        private NameParseSummary(IReadOnlyList<NameParseTally> tallies)
        {
            Tallies = tallies;
        }

        public IReadOnlyList<NameParseTally> Tallies { get; }

        public int ParsedTotal
        {
            get { return Tallies.Sum(tally => tally.Parsed); }
        }

        public int NotParsedTotal
        {
            get { return Tallies.Sum(tally => tally.NotParsed.Count); }
        }

        /// <summary>
        /// View templates are not read here. A template is listed in the report under its own
        /// heading instead.
        /// </summary>
        public static NameParseSummary Of(ModelScan scan)
        {
            if (scan == null) throw new ArgumentNullException("scan");

            return new NameParseSummary(new List<NameParseTally>
            {
                Count(ViewNameKind, scan.Views.Where(view => !view.IsTemplate).Select(view => view.Name))
            });
        }

        public NameParseTally For(string kind)
        {
            NameParseTally found = Tallies.FirstOrDefault(
                tally => string.Equals(tally.Kind, kind, StringComparison.Ordinal));
            if (found == null)
            {
                throw new ArgumentException("No tally was taken for " + kind + ".", "kind");
            }
            return found;
        }

        private static NameParseTally Count(string kind, IEnumerable<string> names)
        {
            int parsed = 0;
            var refused = new List<string>();

            foreach (string name in names)
            {
                ParsedViewName ignored;
                if (ViewNameParser.TryParse(name, out ignored))
                {
                    parsed++;
                }
                else
                {
                    refused.Add(name ?? string.Empty);
                }
            }

            return new NameParseTally(kind, parsed, refused);
        }
    }
}
