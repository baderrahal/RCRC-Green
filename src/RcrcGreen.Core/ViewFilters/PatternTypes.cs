using System;

namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// The two answers a pattern override can hold, none and solid. The ported run body
    /// carries the same two words inline and stays as ported, so these exist for the pane
    /// and the settings file: one record for everything outside the port.
    /// </summary>
    public static class PatternTypes
    {
        public const string None = "none";

        public const string Solid = "solid";

        public static bool IsSolid(string patternType)
        {
            return string.Equals(patternType, Solid, StringComparison.OrdinalIgnoreCase);
        }
    }
}
