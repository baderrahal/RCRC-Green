using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Runs every name in a scan through <see cref="ViewNameParser"/> and counts the answers.
    ///
    /// This is the part of the scan the first real model was needed for. The parser was
    /// written from four example names, and a sheet numbered 600QD named SOFTSCAPE SCHEDULES
    /// matches no part of that pattern. The tallies say how far off the guess was, and the
    /// list of refusals says what the naming actually looks like.
    /// </summary>
    public sealed class NameParseSummary
    {
        public const string ViewNameKind = "view name";

        public const string SheetNameKind = "sheet name";

        public const string SheetNumberKind = "sheet number";

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
        /// View templates are not read here. The brief named three kinds and a template is
        /// listed in the report under its own heading instead.
        /// </summary>
        public static NameParseSummary Of(ModelScan scan)
        {
            if (scan == null) throw new ArgumentNullException("scan");

            return new NameParseSummary(new List<NameParseTally>
            {
                Count(ViewNameKind, scan.Views.Where(view => !view.IsTemplate).Select(view => view.Name)),
                Count(SheetNameKind, scan.Sheets.Select(sheet => sheet.SheetName)),
                Count(SheetNumberKind, scan.Sheets.Select(sheet => sheet.SheetNumber))
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
