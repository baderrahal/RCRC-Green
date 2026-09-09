using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One of the nine questions with what the scan found for it. Answered means the thing
    /// the question asks about was found in the model, not that the answer is right, which
    /// only the team can say.
    /// </summary>
    public sealed class KpiAnswer
    {
        public KpiAnswer(int number, string question, string answer, bool answered)
        {
            if (number < 1 || number > 9) throw new ArgumentOutOfRangeException("number");
            if (question == null) throw new ArgumentNullException("question");
            if (answer == null) throw new ArgumentNullException("answer");

            Number = number;
            Question = question;
            Answer = answer;
            Answered = answered;
        }

        public int Number { get; }

        public string Question { get; }

        public string Answer { get; }

        public bool Answered { get; }
    }
}
