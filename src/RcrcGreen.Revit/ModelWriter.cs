using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.Revit.DB;
using RcrcGreen.Core;

using ViewType = RcrcGreen.Core.ViewType;
using CapturedSchedule = RcrcGreen.Core.ScheduleDefinition;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Makes the views, sections, schedules and sheets a run decided on.
    ///
    /// Everything here runs inside one transaction opened by the caller, so the whole run is
    /// one undo. Nothing here decides what to make. <see cref="RunPlan"/> did that before the
    /// transaction was opened, which is what lets the confirmation offer a real count.
    ///
    /// Nothing is ever skipped quietly, and nothing is ever recorded as made before the call
    /// that makes it has returned. Both halves of that matter. The first real run listed four
    /// plan views as created and as refused at the same time, because the created half was
    /// printing the plan.
    /// </summary>
    internal static class ModelWriter
    {
        public static void Make(
            Document document,
            RunPlan plan,
            RunOutcome outcome,
            Dictionary<ViewType, CapturedSchedule> definitions,
            Dictionary<string, ElementId> boxIdByName)
        {
            var madeSoFar = new Dictionary<string, ElementId>(StringComparer.Ordinal);

            // Read once, before anything is created, so no view this run makes can become the
            // sibling another item is set up from.
            SiblingReader siblings = SiblingReader.Of(document);

            // Views before sheets, always, because a sheet places views this run may only just
            // have created. The plan puts sheets last already and this does not rely on it.
            foreach (RunItem item in plan.Items.Where(one => one.Kind != RunItemKind.Sheet))
            {
                MakeOne(document, item, outcome, definitions, boxIdByName, madeSoFar, siblings);
            }

            foreach (RunItem item in plan.Items.Where(one => one.Kind == RunItemKind.Sheet))
            {
                try
                {
                    MakeSheet(document, item, outcome, madeSoFar);
                }
                catch (Autodesk.Revit.Exceptions.ApplicationException failed)
                {
                    outcome.Refused(RunRefusal.ForSheet(item.PlotId, item.SheetNumber,
                        item.SheetName, "Revit refused that sheet. " + failed.Message));
                }
                catch (InvalidOperationException failed)
                {
                    outcome.Refused(RunRefusal.ForSheet(item.PlotId, item.SheetNumber,
                        item.SheetName, "Revit would not make that sheet. " + failed.Message));
                }
                catch (ArgumentException failed)
                {
                    outcome.Refused(RunRefusal.ForSheet(item.PlotId, item.SheetNumber,
                        item.SheetName, "Revit refused an argument on that sheet. " + failed.Message));
                }
            }
        }

        private static void MakeOne(
            Document document,
            RunItem item,
            RunOutcome outcome,
            Dictionary<ViewType, CapturedSchedule> definitions,
            Dictionary<string, ElementId> boxIdByName,
            Dictionary<string, ElementId> madeSoFar,
            SiblingReader siblings)
        {
            try
            {
                switch (item.Kind)
                {
                    case RunItemKind.Schedule:
                        MakeSchedule(document, item, outcome, definitions, madeSoFar);
                        break;
                    case RunItemKind.Section:
                        MakeSection(document, item, outcome, boxIdByName, madeSoFar, siblings);
                        break;
                    default:
                        MakePlanView(document, item, outcome, boxIdByName, madeSoFar, siblings);
                        break;
                }
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException failed)
            {
                outcome.Refused(new RunRefusal(item.PlotId, item.Type, "Revit refused it. " + failed.Message));
            }
            catch (InvalidOperationException failed)
            {
                outcome.Refused(new RunRefusal(item.PlotId, item.Type, "Revit would not do it. " + failed.Message));
            }
            catch (ArgumentException failed)
            {
                outcome.Refused(new RunRefusal(item.PlotId, item.Type, "Revit refused an argument. " + failed.Message));
            }
        }

        /// <summary>
        /// A fresh plan view. Never a duplicate of another plot's, so it carries no annotation,
        /// no dimensions, no tags and no detailing, which is what the team asked for.
        ///
        /// Everything about how it is set up comes off the sibling, meaning a view of the same
        /// type the model already holds on another plot. The family type, the level and the
        /// view template are read from that one view rather than looked up by name.
        ///
        /// Name matching used to do the first and the third and both were wrong. The view family
        /// type for (010) Location Key Plan is named (010) Key Location Plan, the words swapped,
        /// and eight templates start with (200) General Arrangement Layout so a prefix match
        /// gave no answer at all. The sibling is the view the team built. It cannot be wrong
        /// about itself.
        /// </summary>
        private static void MakePlanView(
            Document document,
            RunItem item,
            RunOutcome outcome,
            Dictionary<string, ElementId> boxIdByName,
            Dictionary<string, ElementId> madeSoFar,
            SiblingReader siblings)
        {
            Sibling sibling = siblings.For(item.Type, SiblingKind.Plan);
            if (sibling == null)
            {
                outcome.Refused(NoSibling(item));
                return;
            }

            // The choice falls back to any view of the type when none is a plan, and this
            // model draws one view type both ways. A section here used to be refused as
            // sitting on no level, which describes a section as a plan with a fault.
            if (sibling.Facts.Kind != SiblingKind.Plan)
            {
                outcome.Refused(new RunRefusal(
                    item.PlotId, item.Type, sibling.Facts.WrongKindInWords(SiblingKind.Plan)));
                return;
            }

            var siblingPlan = sibling.View as ViewPlan;
            if (siblingPlan == null || siblingPlan.GenLevel == null)
            {
                outcome.Refused(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "The nearest " + item.Type + " in this model is " + sibling.Facts.ViewName
                    + ", which sits on no level, so there is nothing to say which level a new one "
                    + "belongs on. Picking one would be a guess."));
                return;
            }

            ElementId familyTypeId = sibling.View.GetTypeId();
            if (familyTypeId == ElementId.InvalidElementId)
            {
                outcome.Refused(new RunRefusal(
                    item.PlotId, item.Type,
                    sibling.Facts.ViewName + " has no view family type to copy."));
                return;
            }

            ViewPlan made = ViewPlan.Create(document, familyTypeId, siblingPlan.GenLevel.Id);
            if (!Renamed(document, made, item, outcome, item.Name)) return;
            outcome.Made(item);
            madeSoFar[item.Name] = made.Id;

            Finishing(
                outcome,
                said => new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "Created, but setting it up did not finish. " + said + " Its view template, "
                    + "crop, " + ModelScanner.PlotIdParameterName + " or scope box may not be "
                    + "set. Check it by hand."),
                () => FinishPlanView(made, sibling, item, outcome, boxIdByName));
        }

        /// <summary>
        /// Everything a plan view gets after it exists and has its name.
        /// </summary>
        private static void FinishPlanView(
            ViewPlan made,
            Sibling sibling,
            RunItem item,
            RunOutcome outcome,
            Dictionary<string, ElementId> boxIdByName)
        {
            ApplySiblingTemplate(made, sibling, item, outcome);

            // Annotation crop is forced on rather than copied. Copying it was the last round's
            // answer and it did not work: PL-17-(010) Overall Plan has it off, so the new view
            // inherited the fault and every neighbouring plot's section marker drew through it.
            AnnotationCropChoice annotation = AnnotationCropChoice.ForAPlanView(sibling.Facts);
            ApplySiblingCrop(made, sibling, item, outcome, annotation.On);

            SetPlotId(made, item, outcome);
            SaySetUpFrom(item, outcome, sibling.Facts.InWords() + " " + annotation.InWords());

            ElementId boxId;
            if (boxIdByName.TryGetValue(item.PlotId, out boxId))
            {
                // A plan view gets the scope box. A section does not, because a real one in this
                // model has none and its own section box is what bounds it. The return of Set
                // is checked because Revit can answer false without throwing, and the scope
                // box assignment command already treats that false as a refusal worth naming.
                Parameter holder = made.get_Parameter(ScopeBoxScanner.ScopeBoxParameter);
                if (holder == null || holder.IsReadOnly || !holder.Set(boxId))
                {
                    outcome.NeedsAttention(new RunRefusal(
                        item.PlotId,
                        item.Type,
                        "Created, but the scope box could not be set. The view template it "
                        + "inherited may be controlling it. Set it by hand."));
                }
            }
        }

        /// <summary>
        /// What runs after an item has been recorded as made.
        ///
        /// A throw from Revit in there used to land in the catch around the whole item as
        /// refused, so the item sat in Created and in NotCreated at once, BothWays found it and
        /// the report opened saying it contradicts itself and is a bug in the tool. The thing
        /// is in the model. The true state is created and needing attention, with what Revit
        /// said, and that is what is recorded now.
        /// </summary>
        private static void Finishing(
            RunOutcome outcome, Func<string, RunRefusal> attention, Action finish)
        {
            try
            {
                finish();
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException failed)
            {
                outcome.NeedsAttention(attention("Revit refused it. " + failed.Message));
            }
            catch (InvalidOperationException failed)
            {
                outcome.NeedsAttention(attention("Revit would not do it. " + failed.Message));
            }
            catch (ArgumentException failed)
            {
                outcome.NeedsAttention(attention("Revit refused an argument. " + failed.Message));
            }
        }

        /// <summary>
        /// A cross section, cut across the middle of the plot's scope box the short way.
        ///
        /// This is why the first real run refused every (400). ViewPlan.Create can never make a
        /// section, and the message blamed the level. Which types need this rather than a plan
        /// is decided by <see cref="RunPlan"/> off the kind of the view the model already holds,
        /// not off the code in the name, so nothing here says that 400 means section.
        /// </summary>
        private static void MakeSection(
            Document document,
            RunItem item,
            RunOutcome outcome,
            Dictionary<string, ElementId> boxIdByName,
            Dictionary<string, ElementId> madeSoFar,
            SiblingReader siblings)
        {
            Sibling sibling = siblings.For(item.Type, SiblingKind.Section);
            if (sibling == null)
            {
                outcome.Refused(NoSibling(item));
                return;
            }

            // A plan's family type used to reach ViewSection.CreateSection here and come back
            // as Revit refused an argument, which names the wrong cause.
            if (sibling.Facts.Kind != SiblingKind.Section)
            {
                outcome.Refused(new RunRefusal(
                    item.PlotId, item.Type, sibling.Facts.WrongKindInWords(SiblingKind.Section)));
                return;
            }

            ElementId boxId;
            if (!boxIdByName.TryGetValue(item.PlotId, out boxId))
            {
                outcome.Refused(new RunRefusal(
                    item.PlotId, item.Type, "No scope box is named " + item.PlotId
                    + ", so there is nowhere to cut."));
                return;
            }

            BoundingBoxXYZ extent = document.GetElement(boxId).get_BoundingBox(null);
            if (extent == null)
            {
                outcome.Refused(new RunRefusal(
                    item.PlotId, item.Type, "Scope box " + item.PlotId + " has no bounding box, "
                    + "so its middle cannot be worked out."));
                return;
            }

            PlotBox plot;
            SectionPlacement across;
            try
            {
                plot = new PlotBox(
                    item.PlotId,
                    extent.Min.X, extent.Min.Y, extent.Min.Z,
                    extent.Max.X, extent.Max.Y, extent.Max.Z);

                // One depth on every section the tool makes. It used to come off the sibling,
                // and four real sections in this model read 0.93, 0.93, 1.53 and 12.83 metres,
                // so the model had no rule to copy and the sibling decided it by accident.
                across = SectionPlacement.Across(
                    plot, SectionAxis.ShortSide, SectionDefaults.SectionDepthFeet);
            }
            catch (ArgumentException refused)
            {
                // A flat or unreadable box. Core refuses it at the door rather than turning
                // every centre into NaN, which would come back looking like a placement.
                outcome.Refused(new RunRefusal(item.PlotId, item.Type, refused.Message));
                return;
            }

            if (plot.HeightZ <= 0.0)
            {
                outcome.Refused(new RunRefusal(
                    item.PlotId, item.Type, "Scope box " + item.PlotId + " has no height, so a "
                    + "section across it would have nothing to show."));
                return;
            }

            ViewSection made = ViewSection.CreateSection(
                document, sibling.View.GetTypeId(), SectionBoxFor(plot, across));

            if (!Renamed(document, made, item, outcome, item.Name)) return;
            outcome.Made(item);
            madeSoFar[item.Name] = made.Id;

            Finishing(
                outcome,
                said => new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "Created, but setting it up did not finish. " + said + " Its view template, "
                    + "crop or " + ModelScanner.PlotIdParameterName + " may not be set. Check "
                    + "it by hand."),
                () => FinishSection(made, sibling, item, outcome));
        }

        /// <summary>
        /// Everything a section gets after it exists and has its name.
        /// </summary>
        private static void FinishSection(
            ViewSection made, Sibling sibling, RunItem item, RunOutcome outcome)
        {
            ApplySiblingTemplate(made, sibling, item, outcome);

            // A section still copies all three. No section has ever been created by this tool,
            // so there is no evidence that a section inherits the same fault a plan view did.
            ApplySiblingCrop(made, sibling, item, outcome, sibling.Facts.Crop.AnnotationCrop);

            SetPlotId(made, item, outcome);
            SaySetUpFrom(item, outcome, sibling.Facts.InWords() + " Looks "
                + SectionDepth.Metres.ToString("0.#", CultureInfo.InvariantCulture)
                + " metre, which is the tool's setting rather than anything read off a view.");

            // No scope box on a section. A real one in this model has none, its own section box
            // is what bounds it, and a scope box on top would crop it to something nobody asked
            // for. The plot's box is still what says where to cut, it is just not set on the
            // finished view.
        }

        /// <summary>
        /// The box Revit builds a section from.
        ///
        /// Its transform is the section's own frame. BasisZ points back at the viewer, so the
        /// view looks along the negative of it, which is why the direction Core hands back is
        /// negated here. BasisY is world up. BasisX falls out of the other two, and lands along
        /// the section line, which means the drawing reads along that line.
        ///
        /// Min and Max are in that frame. X is half the line either side of the middle, Y is
        /// the height of the scope box, and Z runs from the depth behind the cut up to the cut
        /// itself, which is what makes the view look forward rather than backward.
        /// </summary>
        private static BoundingBoxXYZ SectionBoxFor(PlotBox plot, SectionPlacement across)
        {
            XYZ looking = new XYZ(
                across.ViewDirection.X, across.ViewDirection.Y, across.ViewDirection.Z).Normalize();

            XYZ basisZ = looking.Negate();
            XYZ basisX = XYZ.BasisZ.CrossProduct(basisZ).Normalize();
            XYZ basisY = basisZ.CrossProduct(basisX).Normalize();

            Transform frame = Transform.Identity;
            frame.Origin = new XYZ(across.Midpoint.X, across.Midpoint.Y, across.Midpoint.Z);
            frame.BasisX = basisX;
            frame.BasisY = basisY;
            frame.BasisZ = basisZ;

            double halfLength = across.Length / 2.0;
            double halfHeight = plot.HeightZ / 2.0;

            return new BoundingBoxXYZ
            {
                Transform = frame,
                Min = new XYZ(-halfLength, -halfHeight, -across.Depth),
                Max = new XYZ(halfLength, halfHeight, 0.0)
            };
        }

        /// <summary>
        /// One sheet off one row of the step 4 table, made for one plot.
        ///
        /// It used to copy a sheet that already existed, which meant somebody had to have laid
        /// one out first and meant picking an empty one gave an empty sheet. A row now carries
        /// its own slice of the ticked views, its name and its number, generated or typed, and
        /// where each view sits is worked out by SheetLayout.
        /// </summary>
        private static void MakeSheet(
            Document document,
            RunItem item,
            RunOutcome outcome,
            Dictionary<string, ElementId> madeSoFar)
        {
            SheetToMake wanted = item.Sheet;

            FamilySymbol block = TitleBlock(document, wanted);
            if (block == null)
            {
                outcome.Refused(RunRefusal.ForSheet(item.PlotId, item.SheetNumber, item.SheetName,
                    item.Name + " was not made. This model has no title block type named "
                    + wanted.TitleBlock + "."));
                return;
            }

            if (!block.IsActive) block.Activate();

            ViewSheet sheet = ViewSheet.Create(document, block.Id);

            try
            {
                sheet.SheetNumber = item.SheetNumber;
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
                // Revit keeps sheet numbers unique. The new sheet is already in the model at
                // this point, so it is deleted again rather than left carrying whatever number
                // Revit picked for it.
                string kept = Deleted(document, sheet.Id)
                    ? " It was deleted again."
                    : " IT IS STILL IN THE MODEL under the number Revit gave it, and has to be "
                        + "sorted out by hand.";

                outcome.Refused(RunRefusal.ForSheet(item.PlotId, item.SheetNumber, item.SheetName,
                    item.Name + " was not made. A sheet numbered " + item.SheetNumber
                    + " is already in this model." + kept));
                return;
            }

            try
            {
                sheet.Name = wanted.SheetName;
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException failed)
            {
                // A typed name can hold a character Revit forbids. The sheet is already in the
                // model under its number by now, so it goes the way a refused number goes,
                // rather than staying under the name Revit gave it with the report saying not
                // made.
                string kept = Deleted(document, sheet.Id)
                    ? " It was deleted again."
                    : " IT IS STILL IN THE MODEL under the name Revit gave it, and has to be "
                        + "sorted out by hand.";

                outcome.Refused(RunRefusal.ForSheet(item.PlotId, item.SheetNumber, item.SheetName,
                    item.Name + " was not made. Revit refused the name " + wanted.SheetName
                    + ". Revit said: " + failed.Message + kept));
                return;
            }

            // The size can only be read after the sheet exists, because Sheet Width and Sheet
            // Height live on the title block Revit places on it. So the sheet is made, measured,
            // and taken away again when it cannot be measured and views were ticked for it.
            SheetSize size = SheetSize.NotRead(wanted.TitleBlock);
            DrawingArea area = null;

            if (wanted.Views.Count > 0)
            {
                size = SizeOf(document, sheet, wanted);

                if (!size.CanBeUsed)
                {
                    string kept = Deleted(document, sheet.Id)
                        ? " It was deleted again."
                        : " IT IS STILL IN THE MODEL, empty, and has to be deleted by hand.";

                    outcome.Refused(RunRefusal.ForSheet(item.PlotId, item.SheetNumber,
                        item.SheetName, size.WhyNotInWords(item.Name) + kept));
                    return;
                }

                // Worked out here rather than where the views are placed, so the line the report
                // carries about this sheet names the area the views really went into.
                area = DrawingArea.InsideTheTitleBlock(size.WidthFeet, size.HeightFeet);

                SaySheetSetUpFrom(item, outcome,
                    size.InWords() + " " + area.InWords() + " " + wanted.ProvenanceInWords());
            }

            outcome.Made(item);

            Finishing(
                outcome,
                said => RunRefusal.ForSheet(
                    item.PlotId,
                    item.SheetNumber,
                    item.SheetName,
                    item.Name + " was made, but placing its views did not finish. " + said
                    + " Some of its views may be missing from it. Check the sheet by hand."),
                () => FinishSheet(document, item, outcome, sheet, size, area, madeSoFar));
        }

        /// <summary>
        /// Everything a sheet gets after it exists with its number and its name.
        /// </summary>
        private static void FinishSheet(
            Document document,
            RunItem item,
            RunOutcome outcome,
            ViewSheet sheet,
            SheetSize size,
            DrawingArea area,
            Dictionary<string, ElementId> madeSoFar)
        {
            // The return of Set is checked because Revit can answer false without throwing,
            // and a sheet without its plot is not found by the Sheet List and not counted when
            // the next sheet on that plot is proposed a number.
            Parameter plot = sheet.LookupParameter(ModelScanner.PlotIdParameterName);
            if (plot == null || plot.IsReadOnly || !plot.Set(item.PlotId))
            {
                outcome.NeedsAttention(RunRefusal.ForSheet(item.PlotId, item.SheetNumber,
                    item.SheetName, item.Name + " was made, but " + ModelScanner.PlotIdParameterName
                    + " could not be set on it, so the Sheet List will not find it. Set it by "
                    + "hand."));
            }

            PlaceViews(document, item, outcome, sheet, size, area, madeSoFar);
        }

        /// <summary>
        /// How big the new sheet is.
        ///
        /// Three sheets were created empty because this was read off the title block TYPE.
        /// Sheet Width and Sheet Height are read-only INSTANCE parameters, so on a FamilySymbol
        /// get_Parameter returns nothing at all, nothing became zero, and the guard below fired
        /// on a real A1 sheet. They are read off the block placed on the sheet now.
        ///
        /// A title block family that does not drive those two parameters is measured instead,
        /// because a block that is drawn at A1 is still A1.
        /// </summary>
        private static SheetSize SizeOf(Document document, ViewSheet sheet, SheetToMake wanted)
        {
            // The placed block is not findable by a collector until the document catches up.
            document.Regenerate();

            FamilyInstance placed = new FilteredElementCollector(document, sheet.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType()
                .OfType<FamilyInstance>()
                .FirstOrDefault();

            if (placed == null) return SheetSize.NotRead(wanted.TitleBlock);

            SheetSize fromParameters = SheetSize.Of(
                SheetSizeSource.TitleBlockParameters,
                ModelScanner.Number(placed.get_Parameter(BuiltInParameter.SHEET_WIDTH)),
                ModelScanner.Number(placed.get_Parameter(BuiltInParameter.SHEET_HEIGHT)),
                wanted.TitleBlock);

            if (fromParameters.CanBeUsed) return fromParameters;

            BoundingBoxXYZ across = placed.get_BoundingBox(sheet);
            if (across == null) return SheetSize.NotRead(wanted.TitleBlock);

            return SheetSize.Of(
                SheetSizeSource.TitleBlockOutline,
                across.Max.X - across.Min.X,
                across.Max.Y - across.Min.Y,
                wanted.TitleBlock);
        }

        /// <summary>
        /// Every view the row carries, at the spot Core worked out for it.
        ///
        /// The division never puts more views on a row than its grid holds, so the overflow
        /// guard here has never fired. It is a guard rather than a trust, because a view
        /// silently left off a sheet reads as finished.
        /// </summary>
        private static void PlaceViews(
            Document document,
            RunItem item,
            RunOutcome outcome,
            ViewSheet sheet,
            SheetSize size,
            DrawingArea area,
            Dictionary<string, ElementId> madeSoFar)
        {
            SheetToMake wanted = item.Sheet;
            IReadOnlyList<ViewType> placing = wanted.Views;
            if (placing.Count == 0) return;

            IReadOnlyList<ViewportSpot> spots = SheetLayout.For(
                area ?? DrawingArea.InsideTheTitleBlock(size.WidthFeet, size.HeightFeet),
                wanted.ViewsPerSheet);

            var landed = new List<OnTheSheet>();

            for (int at = 0; at < placing.Count; at++)
            {
                ViewType type = placing[at];
                string named = item.PlotId + "-(" + type.Code + ") " + type.ViewName;

                if (at >= spots.Count)
                {
                    outcome.NeedsAttention(RunRefusal.ForSheet(item.PlotId, item.SheetNumber,
                        item.SheetName, item.Name + " was made without "
                        + string.Join(", ", placing.Skip(at).Select(one => one.ToString()).ToArray())
                        + ", because the sheet lays out " + spots.Count
                        + " and this row carries more. That is a bug in the division."));
                    break;
                }

                ElementId viewId = ViewNamed(document, named, madeSoFar);
                if (viewId == ElementId.InvalidElementId)
                {
                    outcome.NeedsAttention(RunRefusal.ForSheet(item.PlotId, item.SheetNumber,
                        item.SheetName, item.Name + " was made without " + named
                        + ", because that view is not in the model and was not marked to be "
                        + "made."));
                    continue;
                }

                var point = new XYZ(spots[at].CentreX, spots[at].CentreY, 0.0);

                if (document.GetElement(viewId) is ViewSchedule)
                {
                    landed.Add(new OnTheSheet
                    {
                        Schedule = ScheduleSheetInstance.Create(document, sheet.Id, viewId, point),
                        ViewName = named,
                        Wanted = spots[at]
                    });
                    continue;
                }

                if (!Viewport.CanAddViewToSheet(document, sheet.Id, viewId))
                {
                    // Almost always because it is already on another sheet. Moving it would
                    // take it off a drawing somebody else made.
                    outcome.NeedsAttention(RunRefusal.ForSheet(item.PlotId, item.SheetNumber,
                        item.SheetName, item.Name + " was made without " + named
                        + ", because Revit will not put that view on this sheet. A view already "
                        + "placed on another sheet cannot be placed twice."));
                    continue;
                }

                landed.Add(new OnTheSheet
                {
                    Viewport = Viewport.Create(document, sheet.Id, viewId, point),
                    ViewName = named,
                    Wanted = spots[at]
                });
            }

            // One regeneration, then every placement is read back off what Revit really made
            // rather than off the spot that was asked for. The first sheets came out with views
            // the user called too small, and the report could not say where anything sat or at
            // what scale.
            if (landed.Count == 0) return;
            document.Regenerate();

            bool moved = false;
            foreach (OnTheSheet one in landed)
            {
                moved |= PutTheCentreWhereItWasAsked(document, item, outcome, sheet, one);
            }

            // Only when something really moved. A regeneration nobody needs is time the user
            // waits for and a second chance for Revit to refuse something.
            if (moved) document.Regenerate();

            foreach (OnTheSheet one in landed)
            {
                NotePlacement(document, outcome, sheet, one, size);
            }
        }

        /// <summary>
        /// Puts a schedule where the layout asked for it.
        ///
        /// **A viewport is placed by its centre and a schedule by its top left corner**, and both
        /// are handed the centre Core works out, so every schedule on the first real run landed
        /// half its own size right and down. 010QA measured it: a schedule 207.4 by 187.4 mm
        /// asked for 420.5 by 297.0 came out centred on 522.1 by 203.3, which is 93.7 low and
        /// exactly half its height.
        ///
        /// The correction is measured rather than worked out from the size, because how big a
        /// schedule comes out is not known until Revit has drawn it, and measuring means nothing
        /// here has to assume which corner Revit used.
        ///
        /// **Only a schedule is moved.** Every plan view on that run landed on the centre it was
        /// asked for, so the viewports are right and nothing here touches them. A viewport's
        /// bounding box is not its centre either: it takes in the view title drawn under the
        /// view, so correcting one against its box would move a placement that is already right.
        /// </summary>
        private static bool PutTheCentreWhereItWasAsked(
            Document document, RunItem item, RunOutcome outcome, ViewSheet sheet, OnTheSheet placed)
        {
            if (placed.Schedule == null || placed.Wanted == null) return false;

            BoundingBoxXYZ across = placed.Schedule.get_BoundingBox(sheet);
            if (across == null)
            {
                // A skip with nothing written down is the fault this repo has paid for twice.
                // The placement is still reported, so the centre in the report is the wrong one
                // and this line is what says why.
                outcome.NeedsAttention(RunRefusal.ForSheet(item.PlotId, item.SheetNumber,
                    item.SheetName, item.Name + " holds " + placed.ViewName
                    + " and Revit gave no bounding box for it, so it could not be moved to the "
                    + "centre it was asked for. A schedule is placed by its corner and sits half "
                    + "its own size right and down until it is moved. Drag it by hand."));
                return false;
            }

            SheetMove move = CornerPlacement.MoveToPutTheCentreAt(
                (across.Min.X + across.Max.X) / 2.0,
                (across.Min.Y + across.Max.Y) / 2.0,
                placed.Wanted.CentreX,
                placed.Wanted.CentreY);

            if (move.Nowhere) return false;

            ElementTransformUtils.MoveElement(
                document, placed.Schedule.Id, new XYZ(move.Across, move.Up, 0.0));

            return true;
        }

        /// <summary>
        /// One thing put on the new sheet, held until the read back after the regeneration.
        /// </summary>
        private sealed class OnTheSheet
        {
            public Viewport Viewport;

            public ScheduleSheetInstance Schedule;

            public string ViewName;

            /// <summary>
            /// Where Core asked for its centre. Kept so what landed can be held against what was
            /// asked for, which is the whole of the correction above.
            /// </summary>
            public ViewportSpot Wanted;
        }

        /// <summary>
        /// Where one placement really landed. The scale is the view's own, off its template. A
        /// sheet has no scale: what a sheet shows under Scale is a readout of the views placed
        /// on it, so nothing here sets one anywhere.
        /// </summary>
        private static void NotePlacement(
            Document document,
            RunOutcome outcome,
            ViewSheet sheet,
            OnTheSheet placed,
            SheetSize size)
        {
            if (placed.Viewport != null)
            {
                XYZ centre = placed.Viewport.GetBoxCenter();
                Outline box = placed.Viewport.GetBoxOutline();

                var view = document.GetElement(placed.Viewport.ViewId) as View;

                outcome.NotePlacement(new ViewportRecord(
                    sheet.SheetNumber,
                    sheet.Name,
                    placed.ViewName,
                    view == null ? 0 : view.Scale,
                    centre == null ? 0.0 : centre.X,
                    centre == null ? 0.0 : centre.Y,
                    box == null ? 0.0 : box.MaximumPoint.X - box.MinimumPoint.X,
                    box == null ? 0.0 : box.MaximumPoint.Y - box.MinimumPoint.Y,
                    size.WidthFeet,
                    size.HeightFeet,
                    false));
                return;
            }

            if (placed.Schedule == null) return;

            BoundingBoxXYZ across = placed.Schedule.get_BoundingBox(sheet);

            outcome.NotePlacement(new ViewportRecord(
                sheet.SheetNumber,
                sheet.Name,
                placed.ViewName,
                0,
                across == null ? 0.0 : (across.Min.X + across.Max.X) / 2.0,
                across == null ? 0.0 : (across.Min.Y + across.Max.Y) / 2.0,
                across == null ? 0.0 : across.Max.X - across.Min.X,
                across == null ? 0.0 : across.Max.Y - across.Min.Y,
                size.WidthFeet,
                size.HeightFeet,
                true));
        }

        private static FamilySymbol TitleBlock(Document document, SheetToMake wanted)
        {
            return new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .FirstOrDefault(symbol =>
                    string.Equals(symbol.Name, wanted.TitleBlockTypeName, StringComparison.Ordinal)
                    && (wanted.TitleBlockFamilyName.Length == 0
                        || string.Equals(symbol.FamilyName, wanted.TitleBlockFamilyName, StringComparison.Ordinal)));
        }

        /// <summary>
        /// Anything made earlier in this same transaction first, because a collector will not
        /// always see it before a regeneration, then the views the model already held.
        /// </summary>
        private static ElementId ViewNamed(
            Document document, string wanted, Dictionary<string, ElementId> madeSoFar)
        {
            ElementId made;
            if (madeSoFar.TryGetValue(wanted, out made)) return made;

            View found = new FilteredElementCollector(document)
                .OfClass(typeof(View))
                .Cast<View>()
                .FirstOrDefault(view => !view.IsTemplate
                    && !(view is ViewSheet)
                    && string.Equals(view.Name, wanted, StringComparison.Ordinal));

            return found == null ? ElementId.InvalidElementId : found.Id;
        }

        /// <summary>
        /// Where a new view was set up from, named in the report so it never again takes a
        /// Properties panel to find out. It is not a problem, so it goes under the same heading
        /// as everything else worth reading rather than under a refusal.
        /// </summary>
        private static void SaySheetSetUpFrom(RunItem item, RunOutcome outcome, string what)
        {
            outcome.NoteSetup(new RunRefusal(item.PlotId, null, item.Name + ". " + what));
        }

        private static void SaySetUpFrom(RunItem item, RunOutcome outcome, string what)
        {
            outcome.NoteSetup(new RunRefusal(item.PlotId, item.Type, what));
        }

        private static RunRefusal NoSibling(RunItem item)
        {
            return new RunRefusal(
                item.PlotId,
                item.Type,
                "No plot in this model has a " + item.Type + " to copy the setup from, so the "
                + "view family type, the level and the view template are all unknown. Make one "
                + "by hand on any plot and this will follow it.");
        }

        /// <summary>
        /// The template the sibling carries, whatever it is called. Prefix matching on the name
        /// used to do this and could not answer, because eight templates start with
        /// (200) General Arrangement Layout and Scale 250 against Scale 500 is not this tool's
        /// choice to make. The sibling has already made it.
        /// </summary>
        private static void ApplySiblingTemplate(
            View made, Sibling sibling, RunItem item, RunOutcome outcome)
        {
            if (sibling.View.ViewTemplateId == ElementId.InvalidElementId)
            {
                outcome.NeedsAttention(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "Created with no view template, because " + sibling.Facts.ViewName
                    + " has none either. The scale, detail level, discipline and phase filter are "
                    + "whatever a new view gets by default."));
                return;
            }

            made.ViewTemplateId = sibling.View.ViewTemplateId;
        }

        /// <summary>
        /// Crop View, Crop Region Visible and Annotation Crop, all off the same sibling.
        ///
        /// Every view the first full run created had Annotation Crop off while the views the
        /// team built have it on, so the section markers of neighbouring plots drew straight
        /// through the new views and none of them was usable as a drawing.
        ///
        /// Crop View is set first because Revit will not turn Annotation Crop on for a view
        /// whose crop is off. It is copied rather than forced on, like everything else here.
        /// This runs after the template, so anything the template controls refuses and is
        /// reported rather than silently losing to it.
        /// </summary>
        private static void ApplySiblingCrop(
            View made, Sibling sibling, RunItem item, RunOutcome outcome, bool annotationCrop)
        {
            ViewCrop wanted = sibling.Facts.Crop;
            var refused = new List<string>();

            try
            {
                made.CropBoxActive = wanted.CropActive;
                made.CropBoxVisible = wanted.CropRegionVisible;
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                refused.Add("Crop View and Crop Region Visible");
            }

            // The return of Set is checked because Revit can answer false without throwing.
            // Ignoring it here is how annotation crop could come out still off while the
            // report read clean, which is the fault the user has reported more than once.
            Parameter annotation =
                made.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE);

            if (annotation == null || annotation.IsReadOnly
                || !annotation.Set(annotationCrop ? 1 : 0))
            {
                refused.Add("Annotation Crop");
            }

            if (refused.Count == 0) return;

            outcome.NeedsAttention(new RunRefusal(
                item.PlotId,
                item.Type,
                "Created, but " + string.Join(" and ", refused.ToArray())
                + " could not be set from " + sibling.Facts.ViewName
                + ". The view template it inherited may be controlling that. Set it by hand. "
                + "A view with Annotation Crop off draws the section markers of neighbouring "
                + "plots through itself."));
        }

        private static void SetPlotId(View made, RunItem item, RunOutcome outcome)
        {
            Parameter plot = made.LookupParameter(ModelScanner.PlotIdParameterName);

            if (plot == null)
            {
                outcome.NeedsAttention(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "Created, but it carries no " + ModelScanner.PlotIdParameterName
                    + " parameter, so the Sheet List will not find it."));
                return;
            }

            if (plot.IsReadOnly)
            {
                outcome.NeedsAttention(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "Created, but " + ModelScanner.PlotIdParameterName + " is read only on it, "
                    + "so the plot could not be set. Set it by hand."));
                return;
            }

            // Checked like the scope box is, because Revit can answer false without throwing
            // and a view without its plot is not found by the Sheet List.
            if (!plot.Set(item.PlotId))
            {
                outcome.NeedsAttention(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "Created, but " + ModelScanner.PlotIdParameterName + " could not be set. "
                    + "The view template it inherited may be controlling it. Set it by hand."));
            }
        }

        /// <summary>
        /// A schedule built from a captured definition with the plot swapped.
        ///
        /// A filter that will not go on is fatal and a field that will not go on is not, and
        /// the two are handled differently on purpose. A quantity schedule missing its plot
        /// filter shows every plot's elements and reads as correct on a drawing, so one is
        /// deleted again inside the same transaction and reported as refused. A schedule short
        /// of a column is visibly short, so it is kept and named in the report.
        /// </summary>
        private static void MakeSchedule(
            Document document,
            RunItem item,
            RunOutcome outcome,
            Dictionary<ViewType, CapturedSchedule> definitions,
            Dictionary<string, ElementId> madeSoFar)
        {
            CapturedSchedule captured;
            if (!definitions.TryGetValue(item.Type, out captured))
            {
                outcome.Refused(new RunRefusal(
                    item.PlotId, item.Type, "No definition was captured for that schedule."));
                return;
            }

            // The capture excludes these already, and the writer checks again because it
            // trusts no list it did not build. A definition that lost a filter at capture
            // must never reach CreateSchedule, for the same reason a filter lost at write
            // time deletes the schedule again.
            if (captured.LostAFilterAtCapture)
            {
                outcome.Refused(new RunRefusal(
                    item.PlotId, item.Type, captured.WhyTheCaptureLossRefusesIt()));
                return;
            }

            CapturedSchedule wanted = captured.ForPlot(item.PlotId);

            ViewSchedule made;
            if (wanted.IsASheetList)
            {
                made = ViewSchedule.CreateSheetList(document);
            }
            else
            {
                ElementId category = CategoryIdFor(document, wanted);
                if (category == ElementId.InvalidElementId)
                {
                    outcome.Refused(new RunRefusal(
                        item.PlotId, item.Type, wanted.WhyTheCategoryIsNoGood()));
                    return;
                }

                made = ViewSchedule.CreateSchedule(document, category);
            }

            if (!Renamed(document, made, item, outcome, wanted.NameFor(item.PlotId))) return;

            // Added in the captured order, because the order is what the schedule looks like and
            // a field list in a different order is a different schedule to the person reading it.
            var fieldByName = new Dictionary<string, ScheduleField>(StringComparer.Ordinal);
            var missingFields = new List<ScheduleFieldEntry>();
            var missingFilters = new List<string>();
            var addedOnce = new List<string>();

            // Inside the same delete-again shape the number and the rename use. A throw from
            // AddField or AddFilter used to land in the catch around the whole item as refused,
            // with a schedule named for the plot and short of the filter being added still in
            // the model. Whether AddFilter throws on a filter the source schedule carried is
            // UNKNOWN without Revit, which is why the guard is here rather than assumed away.
            try
            {
                Autodesk.Revit.DB.ScheduleDefinition definition = made.Definition;
                definition.IncludeLinkedFiles = wanted.IncludesLinkedFiles;

                Dictionary<string, SchedulableField> available = definition.GetSchedulableFields()
                    .GroupBy(field => field.GetName(document), StringComparer.Ordinal)
                    .ToDictionary(byName => byName.Key, byName => byName.First(), StringComparer.Ordinal);

                foreach (ScheduleFieldEntry entry in wanted.FieldsInOrder)
                {
                    if (fieldByName.ContainsKey(entry.Name))
                    {
                        // Recorded rather than skipped. When a source schedule carries two
                        // fields under one display name is UNKNOWN, and a bare continue here
                        // was the shape the rules file names as a lie by omission.
                        addedOnce.Add(entry.Name);
                        continue;
                    }

                    SchedulableField schedulable;
                    if (!available.TryGetValue(entry.Name, out schedulable))
                    {
                        missingFields.Add(entry);
                        continue;
                    }

                    fieldByName.Add(entry.Name, definition.AddField(schedulable));
                }

                foreach (ScheduleFilterRule rule in wanted.Filters)
                {
                    ScheduleField field;
                    if (!fieldByName.TryGetValue(rule.ParameterName, out field))
                    {
                        missingFilters.Add(rule.ToString());
                        continue;
                    }

                    ScheduleFilter rebuilt;
                    if (!TryRebuild(field.FieldId, rule, out rebuilt))
                    {
                        missingFilters.Add(rule.ToString());
                        continue;
                    }

                    definition.AddFilter(rebuilt);
                }
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException failed)
            {
                UnfinishedScheduleTakenOut(document, made, item, outcome,
                    "Revit refused it. " + failed.Message);
                return;
            }
            catch (InvalidOperationException failed)
            {
                UnfinishedScheduleTakenOut(document, made, item, outcome,
                    "Revit would not do it. " + failed.Message);
                return;
            }
            catch (ArgumentException failed)
            {
                UnfinishedScheduleTakenOut(document, made, item, outcome,
                    "Revit refused an argument. " + failed.Message);
                return;
            }

            if (missingFilters.Count > 0)
            {
                string lost = string.Join(", ", missingFilters.ToArray());
                string named = made.Name;

                // The delete is guarded on its own rather than left to the catch around the
                // whole item. If Revit refuses it there, the report would say the schedule was
                // deleted while the model still held it, and a report that disagrees with the
                // model is the one thing worth more than the schedule.
                if (Deleted(document, made.Id))
                {
                    outcome.Refused(new RunRefusal(
                        item.PlotId,
                        item.Type,
                        "Created and then deleted again, because " + missingFilters.Count
                        + " of its filters could not be applied. Not applied: " + lost
                        + ". A schedule missing a filter shows every plot's elements and reads as "
                        + "correct on a drawing, so it is not left in the model."));
                    return;
                }

                outcome.LeftInTheModel(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "STILL IN THE MODEL, named " + named + ". It is missing " + missingFilters.Count
                    + " of its filters, not applied: " + lost + ". Revit refused to delete it "
                    + "again, so it is there and it is wrong. It shows every plot's elements and "
                    + "will read as correct on a drawing. Delete it by hand."));
                return;
            }

            outcome.Made(item);
            madeSoFar[item.Name] = made.Id;

            if (missingFields.Count > 0)
            {
                int calculated = missingFields.Count(one => one.IsCalculated);

                outcome.NeedsAttention(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "Created without " + missingFields.Count + " of its fields, so it is short of "
                    + "those columns. Missing: "
                    + string.Join("; ", missingFields.Select(one => one.WhyItIsMissing()).ToArray())
                    + "." + (calculated == 0
                        ? string.Empty
                        : " A calculated field has to be written again in the new schedule by "
                            + "hand, because Revit keeps it inside the schedule that defines it "
                            + "rather than offering it to a new one.")));
            }

            if (addedOnce.Count > 0)
            {
                outcome.NeedsAttention(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "Created with " + addedOnce.Count
                    + (addedOnce.Count == 1 ? " field" : " fields")
                    + " the captured definition names twice, added once: "
                    + string.Join("; ", addedOnce.ToArray())
                    + ". Whether the source schedule really holds two fields under one name is "
                    + "UNKNOWN, so check its columns against the source by hand."));
            }

            // A field capture could not read is a column this schedule silently lacks, and
            // whatever it was cannot even be named. Said here for the same reason a field the
            // category refuses is said, so the schedule is never one column short in silence.
            if (wanted.FieldsNotRead.Count > 0)
            {
                outcome.NeedsAttention(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "Created without " + wanted.FieldsNotRead.Count
                    + (wanted.FieldsNotRead.Count == 1 ? " field" : " fields")
                    + " the source schedule holds that could not be read at capture: "
                    + string.Join("; ", wanted.FieldsNotRead.ToArray())
                    + ". Open the source schedule to see what sits there and add it by hand."));
            }
        }

        /// <summary>
        /// A schedule Revit threw on while its fields and filters were going on. It is already
        /// named for the plot, and one short of the filter that was being added shows every
        /// plot's elements and reads as correct on a drawing, so it is deleted again with the
        /// delete checked, the same way a lost filter is.
        /// </summary>
        private static void UnfinishedScheduleTakenOut(
            Document document, ViewSchedule made, RunItem item, RunOutcome outcome, string said)
        {
            string named = made.Name;

            if (Deleted(document, made.Id))
            {
                outcome.Refused(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "Created and then deleted again, because Revit threw while its fields and "
                    + "filters were being added. " + said + " A schedule short of a filter shows "
                    + "every plot's elements and reads as correct on a drawing, so it is not left "
                    + "in the model."));
                return;
            }

            outcome.LeftInTheModel(new RunRefusal(
                item.PlotId,
                item.Type,
                "STILL IN THE MODEL, named " + named + ". Revit threw while its fields and "
                + "filters were being added, " + said + " and then refused to delete it again, so "
                + "it is there and may be short of a filter. It would show every plot's elements "
                + "and read as correct on a drawing. Delete it by hand."));
        }

        /// <summary>
        /// Names a thing Revit just created, and takes it away again when the name is
        /// refused.
        ///
        /// The name is refused when a view with it already exists, which means the mark went
        /// stale between the panel's read and the run in a shared model, and when a typed view
        /// type name holds a character Revit forbids. Without this the created element stayed
        /// under Revit's default name while the report said not created, and a report that
        /// disagrees with the model is the fault this repo treats as worst. The plan refuses a
        /// stale mark first, and this is the writer trusting no list it did not build, the same
        /// way MakeSheet handles a refused number. Which of the two it was is Revit's to say,
        /// so the refusal quotes it rather than blaming a duplicate that may not be there.
        /// </summary>
        private static bool Renamed(
            Document document, Element made, RunItem item, RunOutcome outcome, string wanted)
        {
            try
            {
                made.Name = wanted;
                return true;
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException failed)
            {
                string kept = Deleted(document, made.Id)
                    ? " It was deleted again."
                    : " IT IS STILL IN THE MODEL under the name Revit gave it, and has to be "
                        + "sorted out by hand.";

                outcome.Refused(new RunRefusal(item.PlotId, item.Type,
                    "Revit refused the name " + wanted + ", so the new one could not take it. "
                    + "Revit said: " + failed.Message + kept));
                return false;
            }
        }

        /// <summary>
        /// A filter rebuilt as the kind it was captured as.
        ///
        /// Two schedules were refused with "the filter value is not valid for the field and
        /// filter type". Both filter on PRX_Included In Budget equals Yes, which is a Yes/No
        /// parameter that Revit holds as the integer 1. Every captured value used to be handed
        /// back as a string, and a string is only right for one of the four kinds.
        /// </summary>
        private static bool TryRebuild(
            ScheduleFieldId fieldId, ScheduleFilterRule rule, out ScheduleFilter rebuilt)
        {
            rebuilt = null;

            try
            {
                switch (rule.Kind)
                {
                    case FilterValueKind.WholeNumber:
                        rebuilt = new ScheduleFilter(
                            fieldId, ScheduleFilterType.Equal, (int)rule.Held.WholeNumber);
                        return true;

                    case FilterValueKind.Number:
                        rebuilt = new ScheduleFilter(
                            fieldId, ScheduleFilterType.Equal, rule.Held.Number);
                        return true;

                    case FilterValueKind.ElementReference:
                        rebuilt = new ScheduleFilter(
                            fieldId,
                            ScheduleFilterType.Equal,
                            new ElementId(rule.Held.WholeNumber));
                        return true;

                    default:
                        rebuilt = new ScheduleFilter(fieldId, ScheduleFilterType.Equal, rule.Value);
                        return true;
                }
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                // Revit refuses a value that does not suit the field. It is reported as a lost
                // filter, which deletes the schedule again, rather than left half applied.
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// True only when the element really is gone. Deleting one made earlier in the same
        /// transaction ought to work and has never been run, so what happens when it does not
        /// is written down rather than assumed away.
        /// </summary>
        private static bool Deleted(Document document, ElementId made)
        {
            try
            {
                ICollection<ElementId> gone = document.Delete(made);
                return gone != null && gone.Count > 0;
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// The category a new schedule is built on, found by Revit's own number for it.
        ///
        /// It used to be found by walking Document.Settings.Categories looking for a matching
        /// display name. KERBS is built on Slab Edges, that walk found nothing, and the schedule
        /// was refused with "this model has no category named Slab Edges" on a model that has
        /// it. Two lookups for one fact, and the capture side used a third: Category.GetCategory
        /// resolves any category id, so it could produce a name the create side could never find.
        /// </summary>
        private static ElementId CategoryIdFor(Document document, CapturedSchedule wanted)
        {
            if (!wanted.HasBuiltInCategory) return ElementId.InvalidElementId;

            var builtIn = (BuiltInCategory)wanted.CategoryBuiltInValue;
            if (!Enum.IsDefined(typeof(BuiltInCategory), builtIn)) return ElementId.InvalidElementId;

            Category found = Category.GetCategory(document, builtIn);
            return found == null ? ElementId.InvalidElementId : found.Id;
        }
    }
}
