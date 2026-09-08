using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class NaturalOrderTests
    {
        [Fact]
        public void TheTeamsOwnPlotNumbersComeOutInTheOrderAReaderExpects()
        {
            string[] plots = { "DM-1", "DM-2", "DM-9", "DM-10", "DM-41", "DM-100", "DM-120", "PF-3" };

            string[] shuffled = { "DM-100", "PF-3", "DM-9", "DM-1", "DM-120", "DM-41", "DM-10", "DM-2" };
            Array.Sort(shuffled, NaturalOrder.Comparer);

            Assert.Equal(plots, shuffled);
        }

        [Fact]
        public void PlainTextOrderIsTheThingThisAvoids()
        {
            string[] byText = { "DM-100", "DM-2" };
            Array.Sort(byText, StringComparer.Ordinal);
            Assert.Equal(new[] { "DM-100", "DM-2" }, byText);

            string[] byNumber = { "DM-100", "DM-2" };
            Array.Sort(byNumber, NaturalOrder.Comparer);
            Assert.Equal(new[] { "DM-2", "DM-100" }, byNumber);
        }

        [Fact]
        public void LeadingZerosDoNotChangeTheNumberButStillKeepTwoStringsApart()
        {
            Assert.Equal(0, Math.Sign(NaturalOrder.Comparer.Compare("010", "010")));
            Assert.True(NaturalOrder.Comparer.Compare("010", "10") != 0);
            Assert.True(NaturalOrder.Comparer.Compare("010", "9") > 0);
            Assert.True(NaturalOrder.Comparer.Compare("9", "010") < 0);
        }

        [Fact]
        public void ASortedSetKeepsEveryDistinctStringItIsGiven()
        {
            var held = new SortedSet<string>(NaturalOrder.Comparer)
            {
                "DM-7", "DM-07", "DM-007", "DM-8"
            };

            Assert.Equal(4, held.Count);
        }

        [Fact]
        public void TheOrderIsTransitiveAcrossAWideMixOfShapes()
        {
            string[] mixed =
            {
                "DM-2", "DM-10", "2", "10", "AB", "", "A1B2", "A1B10", "A10B2",
                "DM-2a", "DM-2-", "007", "7", "PF-12-(200) X", "PF-12-(1000) X"
            };

            foreach (string left in mixed)
            {
                foreach (string right in mixed)
                {
                    int forwards = Math.Sign(NaturalOrder.Comparer.Compare(left, right));
                    int backwards = Math.Sign(NaturalOrder.Comparer.Compare(right, left));

                    Assert.Equal(forwards, -backwards);
                }
            }

            foreach (string a in mixed)
            {
                foreach (string b in mixed)
                {
                    foreach (string c in mixed)
                    {
                        int ab = Math.Sign(NaturalOrder.Comparer.Compare(a, b));
                        int bc = Math.Sign(NaturalOrder.Comparer.Compare(b, c));
                        int ac = Math.Sign(NaturalOrder.Comparer.Compare(a, c));

                        if (ab < 0 && bc < 0)
                        {
                            Assert.True(ac < 0, a + " then " + b + " then " + c);
                        }
                        if (ab == 0 && bc == 0)
                        {
                            Assert.Equal(0, ac);
                        }
                    }
                }
            }
        }

        [Fact]
        public void ANullSortsBeforeAnything()
        {
            Assert.True(NaturalOrder.Comparer.Compare(null, "DM-1") < 0);
            Assert.True(NaturalOrder.Comparer.Compare("DM-1", null) > 0);
            Assert.Equal(0, NaturalOrder.Comparer.Compare(null, null));
        }
    }
}
