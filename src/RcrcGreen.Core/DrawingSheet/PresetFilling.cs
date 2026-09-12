namespace RcrcGreen.Core
{
    /// <summary>
    /// What steps 2 and 4 say about the preset they were filled from.
    ///
    /// It is worked out by comparing what the steps hold now against what the preset asked
    /// for, rather than by a flag the panel sets whenever something changes. A tick, a sheet
    /// added, a title block picked, a views per sheet changed and a sheet removed can all move
    /// them, and the flag for the sixth one is the one nobody remembers to set.
    /// </summary>
    public static class PresetFilling
    {
        /// <summary>
        /// Empty when nothing filled the steps, which is what they say on a fresh panel and
        /// after a refresh that read a different model.
        /// </summary>
        public static string InWords(Preset filledFrom, Preset now)
        {
            if (filledFrom == null || !filledFrom.IsNamed) return string.Empty;

            return filledFrom.SameAnswer(now)
                ? "Filled from " + filledFrom.Name + "."
                : "Filled from " + filledFrom.Name + ", changed since. Save as keeps the change "
                    + "under a name.";
        }
    }
}
