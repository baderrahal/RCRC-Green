using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// Reads and writes the ViewFilters.json shape, an array of <see cref="FilterConfig"/>
    /// rows by their own property names. install.ps1 copies the shipped file beside the
    /// add-in and the pane starts from it. The tool writes no settings file yet: Written
    /// exists so the tests can round trip the rows and so the shipped file is proven
    /// readable by the same reader the pane uses.
    /// </summary>
    public static class ViewFiltersFile
    {
        public const string FileName = "ViewFilters.json";

        public static ViewFiltersFileRead Read(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return ViewFiltersFileRead.Refused("The file is empty.");
            }

            FilterConfig[] parsed;
            try
            {
                DataContractJsonSerializer reader =
                    new DataContractJsonSerializer(typeof(FilterConfig[]));
                using (MemoryStream held = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    parsed = (FilterConfig[])reader.ReadObject(held);
                }
            }
            catch (Exception failed)
            {
                return ViewFiltersFileRead.Refused(
                    "The file does not read as filter rows. " + failed.Message);
            }

            if (parsed == null)
            {
                return ViewFiltersFileRead.Refused("The file does not read as filter rows.");
            }

            if (parsed.Any(row => row == null))
            {
                return ViewFiltersFileRead.Refused(
                    "The file holds an entry that is not a filter row.");
            }

            return ViewFiltersFileRead.Good(new List<FilterConfig>(parsed));
        }

        public static string Written(IReadOnlyList<FilterConfig> rows)
        {
            DataContractJsonSerializer writer =
                new DataContractJsonSerializer(typeof(FilterConfig[]));
            using (MemoryStream held = new MemoryStream())
            {
                writer.WriteObject(held, (rows ?? new List<FilterConfig>()).ToArray());
                return Encoding.UTF8.GetString(held.ToArray());
            }
        }
    }
}
