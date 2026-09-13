namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// The line weight range a row can ask for. Default, 0, means keep the view's own
    /// weight, and Heaviest is Revit's top weight. The ported run body checks the same
    /// range inline and stays as ported, so these exist for the pane: one record for
    /// everything outside the port.
    /// </summary>
    public static class LineWeights
    {
        public const int Default = 0;

        public const int Heaviest = 16;
    }
}
