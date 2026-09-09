using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.Revit.DB;
using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// Reads parameters off elements into plain values, and tallies parameter names over a
    /// set of elements. Every reader in this folder goes through here, so a value is read one
    /// way and printed one way.
    /// </summary>
    internal static class ParameterReading
    {
        /// <summary>
        /// One parameter as plain values. The printed form is what a Properties panel shows
        /// and the raw form is the number underneath, both kept because one of the nine
        /// questions is the difference between them.
        /// </summary>
        public static ReadParameter Read(Parameter parameter)
        {
            if (parameter == null) throw new ArgumentNullException("parameter");

            Definition definition = parameter.Definition;
            string name = definition == null ? string.Empty : definition.Name;

            string guid = string.Empty;
            if (parameter.IsShared)
            {
                try
                {
                    guid = parameter.GUID.ToString();
                }
                catch (InvalidOperationException)
                {
                    // IsShared said yes and the GUID still refused, which the API allows for
                    // a parameter whose definition has gone. An empty GUID is what is printed.
                }
            }

            return new ReadParameter(
                name,
                Kind(parameter),
                parameter.StorageType.ToString(),
                guid,
                parameter.HasValue,
                Printed(parameter),
                Raw(parameter));
        }

        public static List<ReadParameter> ReadAll(Element element)
        {
            var read = new List<ReadParameter>();
            if (element == null) return read;

            foreach (Parameter parameter in element.Parameters)
            {
                read.Add(Read(parameter));
            }

            return read;
        }

        /// <summary>
        /// Shared when Revit says so, built-in when the id is negative, which is how Revit
        /// numbers its own. Anything else is a project parameter or a family parameter, and
        /// from the outside those two look the same.
        /// </summary>
        private static string Kind(Parameter parameter)
        {
            if (parameter.IsShared) return ReadParameter.Shared;
            if (parameter.Id != null && parameter.Id.Value < 0) return ReadParameter.BuiltIn;
            return ReadParameter.ProjectOrFamily;
        }

        /// <summary>
        /// Empty when there is no value, which is different from a value that is an empty
        /// string only in a report, and the report says (no value) for the first.
        /// </summary>
        public static string Printed(Parameter parameter)
        {
            if (parameter == null || !parameter.HasValue) return string.Empty;

            try
            {
                if (parameter.StorageType == StorageType.String)
                {
                    return parameter.AsString() ?? string.Empty;
                }

                return parameter.AsValueString() ?? string.Empty;
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                return "(could not be read)";
            }
            catch (InvalidOperationException)
            {
                return "(could not be read)";
            }
        }

        public static string Raw(Parameter parameter)
        {
            if (parameter == null || !parameter.HasValue) return string.Empty;

            try
            {
                switch (parameter.StorageType)
                {
                    case StorageType.Double:
                        return parameter.AsDouble().ToString("0.########", CultureInfo.InvariantCulture);
                    case StorageType.Integer:
                        return parameter.AsInteger().ToString(CultureInfo.InvariantCulture);
                    case StorageType.ElementId:
                        ElementId id = parameter.AsElementId();
                        return id == null ? string.Empty : "id " + id.Value.ToString(CultureInfo.InvariantCulture);
                    case StorageType.String:
                        return parameter.AsString() ?? string.Empty;
                    default:
                        return string.Empty;
                }
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                return "(could not be read)";
            }
            catch (InvalidOperationException)
            {
                return "(could not be read)";
            }
        }

        /// <summary>
        /// True when the parameter is there and holds something other than an empty string.
        /// </summary>
        public static bool HoldsAValue(Parameter parameter)
        {
            return parameter != null && parameter.HasValue && Printed(parameter).Length > 0;
        }

        /// <summary>
        /// Every parameter name over the elements, with how many carry it and how many hold a
        /// value. A name is counted once per element however many parameters share it.
        /// </summary>
        public static List<ParameterTally> Tally(IEnumerable<Element> elements)
        {
            var carrying = new Dictionary<string, int>(StringComparer.Ordinal);
            var withValue = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (Element element in elements)
            {
                if (element == null) continue;

                var seen = new HashSet<string>(StringComparer.Ordinal);
                var seenWithValue = new HashSet<string>(StringComparer.Ordinal);

                foreach (Parameter parameter in element.Parameters)
                {
                    Definition definition = parameter.Definition;
                    string name = definition == null ? string.Empty : definition.Name;
                    if (name.Length == 0) continue;

                    seen.Add(name);
                    if (HoldsAValue(parameter)) seenWithValue.Add(name);
                }

                foreach (string name in seen) Bump(carrying, name);
                foreach (string name in seenWithValue) Bump(withValue, name);
            }

            return carrying
                .Select(pair => new ParameterTally(pair.Key, pair.Value, Held(withValue, pair.Key)))
                .ToList();
        }

        public static void Bump(Dictionary<string, int> counts, string key)
        {
            int already;
            counts.TryGetValue(key, out already);
            counts[key] = already + 1;
        }

        public static List<NameCount> Counted(Dictionary<string, int> counts)
        {
            return counts.Select(pair => new NameCount(pair.Key, pair.Value)).ToList();
        }

        private static int Held(Dictionary<string, int> counts, string key)
        {
            int found;
            return counts.TryGetValue(key, out found) ? found : 0;
        }

        public static string NameOf(Document document, ElementId id)
        {
            if (document == null || id == null || id == ElementId.InvalidElementId) return string.Empty;

            Element found = document.GetElement(id);
            return found == null ? string.Empty : found.Name;
        }
    }
}
