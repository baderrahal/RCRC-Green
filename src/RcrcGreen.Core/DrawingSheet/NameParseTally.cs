using System;
using System.Collections.Generic;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One kind of name, how many of them the parser understood, and how many it did not.
    /// </summary>
    public sealed class NameParseTally
    {
        public NameParseTally(string kind, int parsed, IReadOnlyList<string> notParsed)
        {
            if (kind == null) throw new ArgumentNullException("kind");
            if (notParsed == null) throw new ArgumentNullException("notParsed");

            Kind = kind;
            Parsed = parsed;
            NotParsed = notParsed;
        }

        /// <summary>
        /// What was being read, in the words the report uses. View name, sheet name, sheet number.
        /// </summary>
        public string Kind { get; }

        public int Parsed { get; }

        /// <summary>
        /// Every name of this kind the parser refused, verbatim and in the order found. The
        /// report shows a capped slice of it. Nothing is dropped here, because the whole point
        /// of this run is to see what the real naming looks like.
        /// </summary>
        public IReadOnlyList<string> NotParsed { get; }

        public int Total
        {
            get { return Parsed + NotParsed.Count; }
        }
    }
}
