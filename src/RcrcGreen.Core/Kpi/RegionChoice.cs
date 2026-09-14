using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Which of a plot's filled regions carries its intervention area.
    ///
    /// **Every plot has two filled regions in the 00 link**, one CADASTRAL LIMIT and one OUT OF
    /// SCOPE (PRESENTATION), and one region holding an area answers itself whatever it is
    /// called. **Where more than one holds an area the client's note decides**, measured on the
    /// 14:29 run: 98 plots took their area off the type the note names and NONE off a type it
    /// does not name. The 9 September reading of NS-19 and NS-06 taking theirs off cadastral was
    /// wrong, because those plots carry BOTH and one column was read.
    ///
    /// **This rule lived in two places**, once in the Revit handler and once in the test
    /// fixture, and the two were not the same rule. The fixture took the FIRST region whatever
    /// it held and however many there were, so every reconciliation and merge test was written
    /// against a choice the running tool would not have made. It is here now, the handler and
    /// the fixture both call it, and the tests reach the one that runs.
    /// </summary>
    /// <summary>
    /// How a plot's region was chosen, so the report can say whether a person answered or the
    /// tool did. **A route printed beside the type is what separates Bader's decision from
    /// somebody's click**, and neither reads as the other on a run of 78 plots.
    /// </summary>
    public enum RegionRoute
    {
        /// <summary>Nothing was chosen, and <see cref="RegionChoice.WhyUnchosen"/> says why.</summary>
        Nothing,

        /// <summary>A person picked it on the pane after a refusal.</summary>
        ChosenByHand,

        /// <summary>Exactly one of the plot's regions holds an area, so it answers itself.</summary>
        TheOnlyOneHoldingAnArea,

        /// <summary>
        /// More than one held an area and one of them was the type the client's note names.
        /// **Bader's decision of 14 September**, and the report says the note chose rather
        /// than a person.
        /// </summary>
        TheTypeTheNoteNames
    }

    /// <summary>
    /// The type chosen and how. One record, so the two cannot drift: the route is set where
    /// the choice is made and never worked out again from the name.
    /// </summary>
    public sealed class RegionPick
    {
        private RegionPick(string typeName, RegionRoute route)
        {
            TypeName = typeName ?? string.Empty;
            Route = route;
        }

        public static readonly RegionPick Nothing = new RegionPick(string.Empty, RegionRoute.Nothing);

        public static RegionPick ByHand(string typeName)
        {
            return new RegionPick(typeName, RegionRoute.ChosenByHand);
        }

        public static RegionPick TheOnlyOne(string typeName)
        {
            return new RegionPick(typeName, RegionRoute.TheOnlyOneHoldingAnArea);
        }

        public static RegionPick TheNote(string typeName)
        {
            return new RegionPick(typeName, RegionRoute.TheTypeTheNoteNames);
        }

        public string TypeName { get; }

        public RegionRoute Route { get; }

        public bool WasChosen
        {
            get { return Route != RegionRoute.Nothing; }
        }

        /// <summary>
        /// The route in the report's own words. **A count of picks made by the note and a count
        /// made by a person are two different facts about a run**, and a column printing only
        /// the type name says neither.
        /// </summary>
        public string InWords
        {
            get
            {
                switch (Route)
                {
                    case RegionRoute.ChosenByHand: return "chosen by hand on the pane";
                    case RegionRoute.TheOnlyOneHoldingAnArea: return "the only region holding an area";
                    case RegionRoute.TheTypeTheNoteNames:
                        return "more than one held an area and this is the type the client's note names";
                    default: return "nothing chosen";
                }
            }
        }
    }

    public static class RegionChoice
    {
        /// <summary>
        /// Which of a plot's regions to take the area from, and how it was decided.
        ///
        /// A person's pick wins outright, because it is an answer and everything below is the
        /// tool working one out. **Exactly one region holding an area answers itself.**
        ///
        /// **MORE THAN ONE, WITH THE NOTE'S TYPE AMONG THEM, TAKES THE NOTE'S TYPE.** Bader's
        /// decision of 14 September, off the 14:29 run: 51 of 78 street plots wrote nothing,
        /// every one because two regions held an area, and every one of the 51 offered
        /// RCRC_OUT OF SCOPE (PRESENTATION) as one of its two. That is 51 questions, 51 clicks
        /// and 51 presses to answer a question the client already answered in the note. The
        /// same run measured the note holding on every plot that chose: 98 off the note's type
        /// and NONE off a type it does not name.
        ///
        /// **The question stays where the note's type is not among them**, because that is
        /// still something the data cannot settle. So is the note's type held by two of them
        /// at once, which no model has shown and which the note cannot separate either.
        /// </summary>
        public static RegionPick Pick(IEnumerable<RegionArea> regions, string chosenByHand)
        {
            if (!string.IsNullOrEmpty(chosenByHand)) return RegionPick.ByHand(chosenByHand);

            if (regions == null) return RegionPick.Nothing;

            List<RegionArea> holding = regions
                .Where(one => one != null && one.HoldsAnArea)
                .ToList();

            if (holding.Count == 1) return RegionPick.TheOnlyOne(holding[0].TypeName);
            if (holding.Count == 0) return RegionPick.Nothing;

            List<RegionArea> named = holding.Where(one => IsTheNote(one.TypeName)).ToList();

            return named.Count == 1 ? RegionPick.TheNote(named[0].TypeName) : RegionPick.Nothing;
        }

        private static bool IsTheNote(string typeName)
        {
            return !string.IsNullOrWhiteSpace(typeName)
                && string.Equals(typeName.Trim(), TheNoteNames, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// **The filled region type the client's note names for the area cell**, quoted off the
        /// reference copy that came with the reissued STREETS template:
        /// REVIT 00 LINK / ID FILLED REGION "RCRC_OUT OF SCOPE (PRESENTATION)" /
        /// PRX_Intervention Area.
        ///
        /// **THE 14:29 RUN SETTLED IT AND THE NOTE HOLDS.** Of 156 plots wanting an area, 98
        /// took it off this type, ZERO off a type the note does not name, and 58 chose no region
        /// at all. The 9 September reading of NS-19 and NS-06 taking their area off CADASTRAL
        /// LIMIT was wrong: those plots carry BOTH, and one column was read.
        ///
        /// **It still decides nothing where one region holds an area**, because that region
        /// answers itself whatever it is called. It decides only where more than one holds one
        /// and it is among them, which is the case the tool used to ask 51 questions about.
        ///
        /// **This is the ONE place the type name lives.** Nothing else holds a hard coded type,
        /// and RCRC_CADASTRAL LIMIT is written nowhere in the tool.
        /// </summary>
        public const string TheNoteNames = "RCRC_OUT OF SCOPE (PRESENTATION)";

        /// <summary>
        /// The type an area really came off, held against the type the client's note names, for
        /// one column of one row of the report.
        /// </summary>
        public static string AgainstTheNote(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return "nothing chosen";

            return string.Equals(typeName.Trim(), TheNoteNames, StringComparison.OrdinalIgnoreCase)
                ? "the type the note names"
                : "NOT the type the note names";
        }

        /// <summary>
        /// Why nothing was chosen, for a report that would otherwise print an empty cell and
        /// leave somebody guessing which of the two cases it was.
        /// </summary>
        public static string WhyUnchosen(IEnumerable<RegionArea> regions)
        {
            List<RegionArea> held = (regions ?? Enumerable.Empty<RegionArea>())
                .Where(one => one != null)
                .ToList();

            List<RegionArea> holding = held.Where(one => one.HoldsAnArea).ToList();

            if (held.Count == 0) return "the plot has no filled region in the link";
            if (holding.Count == 0) return "no filled region on this plot holds an area";

            // **IT ASKS Pick RATHER THAN DECIDING AGAIN.** Two rules for one question is the
            // fault this repo keeps paying for, and it would bite here the moment the note's
            // type settles a pair: the pick would choose and the reason would still print a
            // question, which is a report at war with the workbook beside it.
            if (Pick(held, null).WasChosen) return string.Empty;

            if (holding.Count > 1)
            {
                // **The note's type settles this case now**, so reaching here means either it is
                // not among them, which is a question the data cannot settle, or it is among
                // them twice, which the note cannot separate either. The reason says which,
                // because the two need different answers from a person.
                int named = holding.Count(one => IsTheNote(one.TypeName));
                string names = string.Join(" and ", holding.Select(one => one.TypeName).ToArray());

                return named > 1
                    ? Count(holding.Count) + " hold an area, " + names + ", and " + named
                        + " of them are " + TheNoteNames + ", so the client's note cannot say which"
                    : Count(holding.Count) + " hold an area, " + names + ", none of them "
                        + TheNoteNames + ", and the type name cannot say which is the plot's";
            }

            return string.Empty;
        }

        private static string Count(int many)
        {
            return many + " filled regions";
        }
    }
}
