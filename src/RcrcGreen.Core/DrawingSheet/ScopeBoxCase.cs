namespace RcrcGreen.Core
{
    /// <summary>
    /// What is true of one view when the scope box question is asked of it. The letters are
    /// the ones the brief and the dialog use, so a count on screen and a heading in the file
    /// can be matched up without a translation table.
    /// </summary>
    public enum ScopeBoxCase
    {
        /// <summary>
        /// A. The view name does not read as a plot, so there is nothing to look for.
        /// </summary>
        NameDoesNotParse,

        /// <summary>
        /// B. The view has no scope box parameter, or it is read only. A drafting view and a
        /// schedule both land here, and neither is a fault.
        /// </summary>
        CannotHoldAScopeBox,

        /// <summary>
        /// C. No scope box yet, and one exists with exactly the plot name. The only case that
        /// writes.
        /// </summary>
        ReadyToAssign,

        /// <summary>
        /// D. No scope box, and none in the model carries the plot name. Reported, never
        /// created, because inventing a scope box would put geometry in the model that nobody
        /// asked for.
        /// </summary>
        NoMatchingScopeBox,

        /// <summary>
        /// E. Already carries the scope box its name asks for. Nothing to do.
        /// </summary>
        AlreadyRight,

        /// <summary>
        /// F. Already carries a scope box, and it is a different one. Reported and left alone,
        /// because someone put it there on purpose and this tool does not know why.
        /// </summary>
        HoldsADifferentScopeBox
    }
}
