using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RcrcGreen.Core
{
    /// <summary>
    /// What the new view setup file holds, and every line of it that could not be read. An
    /// unreadable line is kept and said, never dropped.
    /// </summary>
    public sealed class NewViewSetupFileContents
    {
        internal NewViewSetupFileContents(
            IReadOnlyList<NewViewAnswers> answers, IReadOnlyList<string> notRead)
        {
            Answers = answers;
            NotRead = notRead;
        }

        public IReadOnlyList<NewViewAnswers> Answers { get; }

        public IReadOnlyList<string> NotRead { get; }
    }

    /// <summary>
    /// Reading and writing the new view setup file.
    ///
    /// Tab separated, five fields: the view code, the view name, the view family type, the
    /// view template and the level. The level field may be empty, because a section takes no
    /// level, but the field is always there so a line is always five fields and a missing
    /// answer cannot be told apart from a shorter format. Nothing here touches a file.
    /// </summary>
    public static class NewViewSetupFile
    {
        public const char Separator = '\t';

        public const char NoteMark = '#';

        public static NewViewSetupFileContents Read(string text)
        {
            var answers = new List<NewViewAnswers>();
            var notRead = new List<string>();

            if (string.IsNullOrEmpty(text))
            {
                return new NewViewSetupFileContents(answers, notRead);
            }

            string[] lines = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            for (int at = 0; at < lines.Length; at++)
            {
                string line = lines[at];
                if (line.Trim().Length == 0) continue;
                if (line.TrimStart().Length > 0 && line.TrimStart()[0] == NoteMark) continue;

                string[] fields = line.Split(Separator);
                if (fields.Length != 5
                    || fields[0].Trim().Length == 0
                    || fields[1].Length == 0
                    || fields[2].Trim().Length == 0
                    || fields[3].Trim().Length == 0)
                {
                    notRead.Add("line "
                        + (at + 1).ToString(CultureInfo.InvariantCulture) + ", " + line);
                    continue;
                }

                answers.Add(new NewViewAnswers(
                    new ViewType(fields[0].Trim(), fields[1]),
                    fields[2],
                    fields[3],
                    fields[4],
                    NewViewSource.Unset));
            }

            return new NewViewSetupFileContents(answers, notRead);
        }

        public static string Write(IEnumerable<NewViewAnswers> answers)
        {
            var text = new StringBuilder();

            text.Append("# RCRC Green, the three answers for a view type with no example in ")
                .Append("the model.").Append(Line);
            text.Append("# One line per view type, tab separated: view code, view name, view ")
                .Append("family type, view template, level. The level is empty for a section, ")
                .Append("which takes none.").Append(Line);
            text.Append("# Written by the Drawing Sheet panel whenever an answer is picked in ")
                .Append("step 5.").Append(Line);

            foreach (NewViewAnswers one in (answers ?? Enumerable.Empty<NewViewAnswers>())
                .Where(one => one != null
                    && one.FamilyTypeName.Length > 0 && one.TemplateName.Length > 0)
                .OrderBy(one => one.Type))
            {
                text.Append(one.Type.Code).Append(Separator)
                    .Append(one.Type.ViewName).Append(Separator)
                    .Append(one.FamilyTypeName).Append(Separator)
                    .Append(one.TemplateName).Append(Separator)
                    .Append(one.LevelName).Append(Line);
            }

            return text.ToString();
        }

        private const string Line = "\r\n";
    }
}
