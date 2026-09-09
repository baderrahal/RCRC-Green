using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Everything one KPI Scan read out of a document, as plain values. The Revit side fills
    /// this in and hands it here, and nothing in it knows what a Document is.
    ///
    /// Every part is optional at the constructor so a read that failed in one section still
    /// hands back the other eight, with the failure named in <see cref="Skipped"/>. A report
    /// missing a section reads as a section that found nothing, and that has been the fault
    /// here before.
    /// </summary>
    public sealed class KpiScan
    {
        public KpiScan(
            DocumentFacts document,
            IEnumerable<ReadParameter> projectInformation,
            TitleBlockFacts titleBlocks,
            LinkFacts links,
            ScheduleFacts schedules,
            IEnumerable<string> skipped,
            bool projectInformationRead)
        {
            if (document == null) throw new ArgumentNullException("document");

            Document = document;
            ProjectInformation = (projectInformation ?? Enumerable.Empty<ReadParameter>())
                .Where(one => one != null)
                .ToList();
            TitleBlocks = titleBlocks ?? TitleBlockFacts.Nothing();
            Links = links ?? LinkFacts.Nothing();
            Schedules = schedules ?? ScheduleFacts.Nothing();
            Skipped = (skipped ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrEmpty(one))
                .ToList();
            ProjectInformationRead = projectInformationRead;
        }

        public DocumentFacts Document { get; }

        /// <summary>
        /// Every parameter on the ProjectInfo element, no cap.
        /// </summary>
        public IReadOnlyList<ReadParameter> ProjectInformation { get; }

        /// <summary>
        /// False when the Project Information read threw, so an empty list is a read that
        /// did not happen rather than a model with no parameters.
        /// </summary>
        public bool ProjectInformationRead { get; }

        public TitleBlockFacts TitleBlocks { get; }

        public LinkFacts Links { get; }

        public ScheduleFacts Schedules { get; }

        /// <summary>
        /// Every read that did not happen, with why. A section that was refused prints this
        /// rather than a zero, because a zero reads as an answer.
        /// </summary>
        public IReadOnlyList<string> Skipped { get; }
    }
}
