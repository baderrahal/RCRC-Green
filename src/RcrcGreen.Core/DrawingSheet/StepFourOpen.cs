namespace RcrcGreen.Core
{
    /// <summary>
    /// Which sheet definition is open in step 4, and which of the two lists inside it.
    ///
    /// Step 4 drew every definition open, so seven of them over 35 sub plots came to seven
    /// title block dropdowns, seven twelve line view checklists and 245 plot rows in one pane.
    /// One definition is open at a time now, and the view list and the plot rows inside it open
    /// on their own.
    ///
    /// It is here rather than as three ints on the panel because removing a definition shifts
    /// every index past it, which is an off-by-one nobody can test in the Revit project.
    /// </summary>
    public sealed class StepFourOpen
    {
        /// <summary>
        /// Nothing open. A position no definition can have, so it needs no second field to
        /// say whether the number means anything.
        /// </summary>
        public const int None = -1;

        private StepFourOpen(int sheet, int views, int rows)
        {
            Sheet = sheet;
            Views = views;
            Rows = rows;
        }

        public static readonly StepFourOpen Nothing = new StepFourOpen(None, None, None);

        public int Sheet { get; }

        public int Views { get; }

        public int Rows { get; }

        public bool IsSheetOpen(int at)
        {
            return at >= 0 && Sheet == at;
        }

        public bool AreViewsOpen(int at)
        {
            return at >= 0 && Views == at;
        }

        public bool AreRowsOpen(int at)
        {
            return at >= 0 && Rows == at;
        }

        /// <summary>
        /// Opens one definition and shuts whatever was open, along with the two lists inside
        /// it. Opening a definition to change its title block should not also unroll 35 rows.
        /// </summary>
        public StepFourOpen Opening(int at, bool open)
        {
            return open && at >= 0
                ? new StepFourOpen(at, None, None)
                : Nothing;
        }

        /// <summary>
        /// The view list inside a definition. Opening it on a definition that is not the open
        /// one opens that definition too, because a list cannot be shown under a shut block.
        /// </summary>
        public StepFourOpen OpeningViews(int at, bool open)
        {
            if (!open || at < 0) return new StepFourOpen(Sheet, None, Rows);

            return new StepFourOpen(at, at, Rows == at ? Rows : None);
        }

        public StepFourOpen OpeningRows(int at, bool open)
        {
            if (!open || at < 0) return new StepFourOpen(Sheet, Views, None);

            return new StepFourOpen(at, Views == at ? Views : None, at);
        }

        /// <summary>
        /// A definition added at the end is opened, because one just added has no title block
        /// and no views and its two shut lines would say so with no way to act on it.
        /// </summary>
        public StepFourOpen AfterAdding(int at)
        {
            return Opening(at, true);
        }

        /// <summary>
        /// What stays open once a definition is taken out. Anything past it moves down one,
        /// and the removed one itself shuts rather than opening its neighbour.
        /// </summary>
        public StepFourOpen AfterRemoving(int removed)
        {
            if (removed < 0) return this;

            return new StepFourOpen(
                Moved(Sheet, removed), Moved(Views, removed), Moved(Rows, removed));
        }

        private static int Moved(int open, int removed)
        {
            if (open == None || open == removed) return None;

            return open > removed ? open - 1 : open;
        }
    }
}
