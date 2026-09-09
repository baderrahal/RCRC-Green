namespace RcrcGreen.Core
{
    /// <summary>
    /// How far a cross section looks. One number, the same on every section the tool makes.
    ///
    /// It used to be taken off the sibling section, on the reasoning that the model is the
    /// better answer. The model turned out to have no answer. Four real sections read 3.0480,
    /// 3.0480, 5.0199 and 42.1054 feet, which is 0.93, 0.93, 1.53 and 12.83 metres, so whichever
    /// sibling happened to be picked decided the depth. The team chose 1 metre instead.
    ///
    /// The number lives in Core rather than in the Revit project because the run report has to
    /// be able to print it. The conversion into feet stays at the Revit boundary in
    /// SectionDefaults, because feet is Revit's unit and nothing else in the tool works in it.
    /// </summary>
    public static class SectionDepth
    {
        public const double Metres = 1.0;
    }
}
