using System;
using System.IO;
using System.Security;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// What comparing two paths came back with. Three answers rather than two, because a path
    /// that cannot be resolved is not the same as one that is different, and the guard that
    /// reads this refuses on both.
    /// </summary>
    public enum SamePath
    {
        Different,

        Same,

        /// <summary>
        /// One of them could not be turned into an absolute path at all. **A check that cannot
        /// see its own subject has to refuse**, which is the rule the writing hook was rewritten
        /// under, so this answer refuses the same way Same does.
        /// </summary>
        Unreadable
    }

    /// <summary>
    /// Whether two paths name one file.
    ///
    /// **This exists because nothing stopped the output path from being the template path.** The
    /// name box is prefilled with the template's own file name, the output folder is browsed for
    /// and can be the templates folder, and the delete that clears the way for the copy would
    /// have taken the client's GRP KPI Checklist with it. No copy, no undo, and every later run
    /// of that template impossible.
    ///
    /// The comparison is the absolute canonical form of each, compared without case, which is
    /// how Windows compares a path. **It is textual and that is its limit.** A junction, a
    /// symbolic link, a substituted drive or an 8.3 short name reaches one file under two names
    /// that do not resolve to one string, and nothing here catches that. Asking the file system
    /// for an identity means opening both files, which is the thing being guarded against.
    ///
    /// The name is plural because `Autodesk.Revit.DB.FilePath` is a real type and the Revit
    /// side of this guard would not compile beside a Core class of that name.
    /// </summary>
    public static class FilePaths
    {
        public static SamePath Compare(string one, string other)
        {
            string first = Canonical(one);
            string second = Canonical(other);

            if (first.Length == 0 || second.Length == 0) return SamePath.Unreadable;

            return string.Equals(first, second, StringComparison.OrdinalIgnoreCase)
                ? SamePath.Same
                : SamePath.Different;
        }

        /// <summary>
        /// The absolute path with any relative step worked out and any trailing separator off,
        /// or empty when the path cannot be resolved. The separator is trimmed here rather than
        /// left to the caller, because GetFullPath keeps one and a path with it reads as a
        /// different file from the same path without it.
        /// </summary>
        public static string Canonical(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;

            try
            {
                string full = Path.GetFullPath(path.Trim());

                return full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            catch (ArgumentException)
            {
                return string.Empty;
            }
            catch (NotSupportedException)
            {
                return string.Empty;
            }
            catch (PathTooLongException)
            {
                return string.Empty;
            }
            catch (SecurityException)
            {
                return string.Empty;
            }
        }
    }
}
