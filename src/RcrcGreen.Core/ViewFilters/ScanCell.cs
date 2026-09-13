namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// One square of the scan grid, a plot against a filter prefix. Exists is a filter the
    /// run will find by name. WillCreate is a missing one the run can copy from an exemplar.
    /// CannotCreate is a missing one with no exemplar, an exemplar whose own tail is not a
    /// plot id, or an exemplar whose rules cannot be copied, which are the second edit's
    /// three refusals.
    /// </summary>
    public enum ScanCell
    {
        Exists,
        WillCreate,
        CannotCreate
    }
}
