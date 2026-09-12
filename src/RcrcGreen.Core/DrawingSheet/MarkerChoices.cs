using System.Collections.Generic;
using System.Globalization;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Every marker the two dropdowns in step 1 offer. A plot's marker is set by the user and
    /// never derived: the old rule read the letter off the plot's own sheet numbers, and on
    /// the measured model only DM-11 has real numbers, so 159 of 160 plots had nothing to
    /// read.
    ///
    /// Letters run A to Z, then AA to ZZ, then AAA to ZZZ, the way NG05 uses Q. Numbers run
    /// 001 to 999, the way NG03 uses 001. Each list is built once and shared by every plot
    /// row, because the letters alone are 18,278 strings.
    /// </summary>
    public static class MarkerChoices
    {
        private static IReadOnlyList<string> _letters;

        private static IReadOnlyList<string> _numbers;

        public static IReadOnlyList<string> Letters
        {
            get
            {
                if (_letters == null) _letters = BuildLetters();
                return _letters;
            }
        }

        public static IReadOnlyList<string> Numbers
        {
            get
            {
                if (_numbers == null) _numbers = BuildNumbers();
                return _numbers;
            }
        }

        private static IReadOnlyList<string> BuildLetters()
        {
            var built = new List<string>(26 + 26 * 26 + 26 * 26 * 26);

            for (char one = 'A'; one <= 'Z'; one++)
            {
                built.Add(one.ToString());
            }

            for (char one = 'A'; one <= 'Z'; one++)
            {
                for (char two = 'A'; two <= 'Z'; two++)
                {
                    built.Add(new string(new[] { one, two }));
                }
            }

            for (char one = 'A'; one <= 'Z'; one++)
            {
                for (char two = 'A'; two <= 'Z'; two++)
                {
                    for (char three = 'A'; three <= 'Z'; three++)
                    {
                        built.Add(new string(new[] { one, two, three }));
                    }
                }
            }

            return built;
        }

        private static IReadOnlyList<string> BuildNumbers()
        {
            var built = new List<string>(999);

            for (int at = 1; at <= 999; at++)
            {
                built.Add(at.ToString("000", CultureInfo.InvariantCulture));
            }

            return built;
        }
    }
}
