using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// What a run actually did, as against what it set out to do.
    ///
    /// The first real run listed four plan views under PLAN VIEWS and the same four under NOT
    /// CREATED, REFUSED BY REVIT, while the panel said nothing was made. Nothing was made. The
    /// created sections were printing <see cref="RunPlan.Items"/>, which is the intention, and
    /// the refusal sections were printing what happened. Two sources for one fact again.
    ///
    /// So the report now reads this and never the plan. Something reaches a created section
    /// only by being handed to <see cref="Made"/> after the call that made it returned.
    /// </summary>
    public sealed class RunOutcome
    {
        private readonly List<RunItem> _made = new List<RunItem>();
        private readonly List<RunRefusal> _refused = new List<RunRefusal>();
        private readonly List<RunRefusal> _attention = new List<RunRefusal>();
        private readonly List<RunRefusal> _leftBehind = new List<RunRefusal>();
        private readonly List<RunRefusal> _setUp = new List<RunRefusal>();

        /// <summary>
        /// A run that was never confirmed, or one that found nothing to do. Made is empty and
        /// stays empty, which is what makes every created section read zero.
        /// </summary>
        public static RunOutcome NothingWasWritten()
        {
            return new RunOutcome();
        }

        /// <summary>
        /// Call this only after the thing exists in the model. Everything in here is printed
        /// as created, so anything added before the creating call returns is a claim the model
        /// may not back.
        /// </summary>
        public void Made(RunItem item)
        {
            if (item == null) throw new ArgumentNullException("item");
            _made.Add(item);
        }

        public void Refused(RunRefusal refusal)
        {
            if (refusal == null) throw new ArgumentNullException("refusal");
            _refused.Add(refusal);
        }

        /// <summary>
        /// Made, and not the same as made right. A schedule short of a column and a view with
        /// no template are both in the model and both wrong.
        /// </summary>
        public void NeedsAttention(RunRefusal refusal)
        {
            if (refusal == null) throw new ArgumentNullException("refusal");
            _attention.Add(refusal);
        }

        /// <summary>
        /// Created wrong, and the delete that should have removed it was refused. This is the
        /// one list that gets worse the longer nobody reads it.
        /// </summary>
        public void LeftInTheModel(RunRefusal refusal)
        {
            if (refusal == null) throw new ArgumentNullException("refusal");
            _leftBehind.Add(refusal);
        }

        /// <summary>
        /// Where each new view took its family type, level, template and far clip from. Not a
        /// problem and not a refusal, which is why it has a list of its own.
        ///
        /// A run created a view whose template and family type looked as though they had come
        /// from two different places, and there was no way to check because nothing said which
        /// view either came from. Now the report names it.
        /// </summary>
        public void NoteSetup(RunRefusal note)
        {
            if (note == null) throw new ArgumentNullException("note");
            _setUp.Add(note);
        }

        public IReadOnlyList<RunRefusal> SetUp
        {
            get { return _setUp; }
        }

        public IReadOnlyList<RunItem> Created
        {
            get { return _made; }
        }

        public IReadOnlyList<RunItem> CreatedOfKind(RunItemKind kind)
        {
            return _made.Where(item => item.Kind == kind).ToList();
        }

        public IReadOnlyList<RunRefusal> NotCreated
        {
            get { return _refused; }
        }

        public IReadOnlyList<RunRefusal> Attention
        {
            get { return _attention; }
        }

        public IReadOnlyList<RunRefusal> LeftBehind
        {
            get { return _leftBehind; }
        }

        public int CreatedCount
        {
            get { return _made.Count; }
        }

        public int NotCreatedCount
        {
            get { return _refused.Count + _leftBehind.Count; }
        }

        /// <summary>
        /// Names that turn up both as created and as not created. It is always empty in a run
        /// that behaved, and a run that produces one has a bug in it worth shouting about
        /// rather than a model worth blaming.
        ///
        /// A refusal carries the same name string an item would have, which is what lets this
        /// be a comparison rather than an argument.
        /// </summary>
        public IReadOnlyList<string> BothWays
        {
            get
            {
                var createdNames = new HashSet<string>(
                    _made.Select(item => item.Name), StringComparer.Ordinal);

                return _refused.Concat(_leftBehind)
                    .Select(one => one.Name)
                    .Where(createdNames.Contains)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(name => name, StringComparer.Ordinal)
                    .ToList();
            }
        }
    }
}
