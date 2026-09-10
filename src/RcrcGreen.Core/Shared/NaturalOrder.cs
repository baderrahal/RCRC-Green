using System;
using System.Collections.Generic;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Orders text so a run of digits inside it counts as a number. DM-2 comes before DM-100
    /// and code 200 comes before code 1000, which is how anyone reading down the side of the
    /// grid expects to find a plot.
    ///
    /// Plain text order puts DM-100 above DM-2, and a grid that looks sorted but is not gets
    /// read against the wrong row without leaving a trace.
    /// </summary>
    public sealed class NaturalOrder : IComparer<string>
    {
        public static readonly NaturalOrder Comparer = new NaturalOrder();

        private NaturalOrder()
        {
        }

        public int Compare(string left, string right)
        {
            if (ReferenceEquals(left, right)) return 0;
            if (left == null) return -1;
            if (right == null) return 1;

            int leftAt = 0;
            int rightAt = 0;

            while (leftAt < left.Length && rightAt < right.Length)
            {
                bool leftIsDigits = IsDigit(left[leftAt]);
                bool rightIsDigits = IsDigit(right[rightAt]);

                if (leftIsDigits != rightIsDigits)
                {
                    // A fixed side for the mixed case, so the order stays transitive.
                    return leftIsDigits ? -1 : 1;
                }

                int leftEnd = RunEnd(left, leftAt, leftIsDigits);
                int rightEnd = RunEnd(right, rightAt, rightIsDigits);

                int difference = leftIsDigits
                    ? CompareNumbers(left, leftAt, leftEnd, right, rightAt, rightEnd)
                    : CompareLetters(left, leftAt, leftEnd, right, rightAt, rightEnd);

                if (difference != 0) return difference;

                leftAt = leftEnd;
                rightAt = rightEnd;
            }

            if (leftAt < left.Length) return 1;
            if (rightAt < right.Length) return -1;

            // Two strings can run out equal here, DM-07 against DM-7 for one. They are still
            // different strings and a set keyed on this order must keep both.
            return string.CompareOrdinal(left, right);
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static int RunEnd(string text, int from, bool digits)
        {
            int at = from;
            while (at < text.Length && IsDigit(text[at]) == digits)
            {
                at++;
            }
            return at;
        }

        // Both run comparisons stay inside their own run. Reading past the end of one would
        // compare a run against whatever follows the other, and the order would stop being
        // transitive, which a sorted set silently turns into lost entries.
        private static int CompareLetters(
            string left, int leftFrom, int leftEnd,
            string right, int rightFrom, int rightEnd)
        {
            int leftLength = leftEnd - leftFrom;
            int rightLength = rightEnd - rightFrom;
            int shared = Math.Min(leftLength, rightLength);

            int difference = string.CompareOrdinal(left, leftFrom, right, rightFrom, shared);
            if (difference != 0) return difference;
            if (leftLength != rightLength) return leftLength < rightLength ? -1 : 1;
            return 0;
        }

        private static int CompareNumbers(
            string left, int leftFrom, int leftEnd,
            string right, int rightFrom, int rightEnd)
        {
            int leftStart = SkipZeros(left, leftFrom, leftEnd);
            int rightStart = SkipZeros(right, rightFrom, rightEnd);

            int leftDigits = leftEnd - leftStart;
            int rightDigits = rightEnd - rightStart;

            // Compared by length first, so a run of any size works without parsing it into a
            // number that could overflow.
            if (leftDigits != rightDigits) return leftDigits < rightDigits ? -1 : 1;

            return string.CompareOrdinal(left, leftStart, right, rightStart, leftDigits);
        }

        private static int SkipZeros(string text, int from, int end)
        {
            int at = from;
            while (at < end - 1 && text[at] == '0')
            {
                at++;
            }
            return at;
        }
    }
}
