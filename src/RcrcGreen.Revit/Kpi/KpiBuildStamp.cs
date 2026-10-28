using System.Reflection;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// The stamp the Revit project bakes into this assembly's product version at build time,
    /// read back off the assembly itself so nothing types it. Null where the build carries none,
    /// which <see cref="RcrcGreen.Core.Kpi.BuildStamp"/> prints as not stamped.
    /// </summary>
    internal static class KpiBuildStamp
    {
        public static string Read()
        {
            AssemblyInformationalVersionAttribute stamp = typeof(KpiBuildStamp).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return stamp?.InformationalVersion;
        }
    }
}
