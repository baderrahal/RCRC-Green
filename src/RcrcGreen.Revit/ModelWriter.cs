using System;
using System.Collections.Generic;
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
            Dictionary<string, ElementId> boxIdByName,
            SheetDefinition sheetLayout)
        {
            var madeSoFar = new Dictionary<string, ElementId>(StringComparer.Ordinal);

            // Views before sheets, always, because a sheet places views this run may only just
            // have created. The plan puts sheets last already and this does not rely on it.
            foreach (RunItem item in plan.Items.Where(one => one.Kind != RunItemKind.Sheet))
            {
                MakeOne(document, item, outcome, definitions, boxIdByName, madeSoFar);
            }

            foreach (RunItem item in plan.Items.Where(one => one.Kind == RunItemKind.Sheet))
            {
                try
                {
                    MakeSheet(document, item, outcome, sheetLayout, madeSoFar);
                }
                catch (Autodesk.Revit.Exceptions.ApplicationException failed)
                {
                    outcome.Refused(new RunRefusal(item.PlotId, null, "Revit refused that sheet. " + failed.Message));
                }
                catch (InvalidOperationException failed)
                {
                    outcome.Refused(new RunRefusal(item.PlotId, null, "Revit would not make that sheet. " + failed.Message));
                }
                catch (ArgumentException failed)
                {
                    outcome.Refused(new RunRefusal(item.PlotId, null, "Revit refused an argument on that sheet. " + failed.Message));
                }
            }
        }

        private static void MakeOne(
            Document document,
            RunItem item,
            RunOutcome outcome,
            Dictionary<ViewType, CapturedSchedule> definitions,
            Dictionary<string, ElementId> boxIdByName,
            Dictionary<string, ElementId> madeSoFar)
        {
            try
            {
                switch (item.Kind)
                {
                    case RunItemKind.Schedule:
                        MakeSchedule(document, item, outcome, definitions, madeSoFar);
                        break;
                    case RunItemKind.Section:
                        MakeSection(document, item, outcome, boxIdByName, madeSoFar);
                        break;
                    default:
                        MakePlanView(document, item, outcome, boxIdByName, madeSoFar);
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
            Dictionary<string, ElementId> madeSoFar)
        {
            View sibling = SiblingOfType(document, item.Type);
            if (sibling == null)
            {
                outcome.Refused(NoSibling(item));
                return;
            }

            var siblingPlan = sibling as ViewPlan;
            if (siblingPlan == null || siblingPlan.GenLevel == null)
            {
                outcome.Refused(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "The only " + item.Type + " in this model is a " + sibling.ViewType
                    + " that sits on no level, so there is nothing to say which level a new one "
                    + "belongs on. Picking one would be a guess."));
                return;
            }

            ElementId familyTypeId = sibling.GetTypeId();
            if (familyTypeId == ElementId.InvalidElementId)
            {
                outcome.Refused(new RunRefusal(
                    item.PlotId, item.Type, "That view type's sibling has no view family type."));
                return;
            }

            ViewPlan made = ViewPlan.Create(document, familyTypeId, siblingPlan.GenLevel.Id);
            made.Name = item.Name;
            outcome.Made(item);
            madeSoFar[item.Name] = made.Id;

            ApplySiblingTemplate(made, sibling, item, outcome);
            SetPlotId(made, item, outcome);

            ElementId boxId;
            if (boxIdByName.TryGetValue(item.PlotId, out boxId))
            {
                Parameter holder = made.get_Parameter(ScopeBoxScanner.ScopeBoxParameter);
                if (holder == null || holder.IsReadOnly)
                {
                    outcome.NeedsAttention(new RunRefusal(
                        item.PlotId,
                        item.Type,
                        "Created, but the scope box could not be set. The view template it "
                        + "inherited may be controlling it. Set it by hand."));
                }
                else
                {
                    holder.Set(boxId);
                }
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
            Dictionary<string, ElementId> madeSoFar)
        {
            View sibling = SiblingOfType(document, item.Type);
            if (sibling == null)
            {
                outcome.Refused(NoSibling(item));
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
                document, sibling.GetTypeId(), SectionBoxFor(plot, across));

            made.Name = item.Name;
            outcome.Made(item);
            madeSoFar[item.Name] = made.Id;

            ApplySiblingTemplate(made, sibling, item, outcome);
            SetPlotId(made, item, outcome);

            // No scope box on a section. Its own section box is what bounds it, and a scope box
            // on top would crop it to something nobody asked for.
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
        /// A sheet built like one the user set up by hand.
        ///
        /// The title block, where a view sits and how several lay out were the three things
        /// that stopped sheets being made. The team answered all three with the same answer:
        /// copy the sheet somebody already got right. So none of it is decided here.
        /// </summary>
        private static void MakeSheet(
            Document document,
            RunItem item,
            RunOutcome outcome,
            SheetDefinition layout,
            Dictionary<string, ElementId> madeSoFar)
        {
            if (layout == null || !layout.CanBeUsed)
            {
                outcome.Refused(new RunRefusal(item.PlotId, null,
                    item.Name + " was not made, because no usable source sheet was captured."));
                return;
            }

            FamilySymbol block = TitleBlock(document, layout);
            if (block == null)
            {
                outcome.Refused(new RunRefusal(item.PlotId, null,
                    item.Name + " was not made. This model has no title block type named "
                    + layout.TitleBlockFamilyName + " " + layout.TitleBlockTypeName + "."));
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

                outcome.Refused(new RunRefusal(item.PlotId, null,
                    item.Name + " was not made. A sheet numbered " + item.SheetNumber
                    + " is already in this model." + kept));
                return;
            }

            sheet.Name = item.SheetName;
            outcome.Made(item);

            Parameter plot = sheet.LookupParameter(ModelScanner.PlotIdParameterName);
            if (plot != null && !plot.IsReadOnly) plot.Set(item.PlotId);

            PlaceViews(document, item, outcome, layout, sheet, madeSoFar);
        }

        private static void PlaceViews(
            Document document,
            RunItem item,
            RunOutcome outcome,
            SheetDefinition layout,
            ViewSheet sheet,
            Dictionary<string, ElementId> madeSoFar)
        {
            foreach (SheetViewPlacement placement in layout.Views)
            {
                string wanted = item.PlotId + "-(" + placement.Type.Code + ") " + placement.Type.ViewName;

                ElementId viewId = ViewNamed(document, wanted, madeSoFar);
                if (viewId == ElementId.InvalidElementId)
                {
                    outcome.NeedsAttention(new RunRefusal(item.PlotId, null,
                        item.Name + " was made without " + wanted
                        + ", because that view is not in the model and was not marked to be made."));
                    continue;
                }

                var point = new XYZ(placement.CentreX, placement.CentreY, 0.0);

                if (placement.IsASchedule)
                {
                    ScheduleSheetInstance.Create(document, sheet.Id, viewId, point);
                    continue;
                }

                if (!Viewport.CanAddViewToSheet(document, sheet.Id, viewId))
                {
                    // Almost always because it is already on another sheet. Moving it would
                    // take it off a drawing somebody else made.
                    outcome.NeedsAttention(new RunRefusal(item.PlotId, null,
                        item.Name + " was made without " + wanted + ", because Revit will not put "
                        + "that view on this sheet. A view already placed on another sheet cannot "
                        + "be placed twice."));
                    continue;
                }

                Viewport.Create(document, sheet.Id, viewId, point);
            }
        }

        private static FamilySymbol TitleBlock(Document document, SheetDefinition layout)
        {
            return new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .FirstOrDefault(symbol =>
                    string.Equals(symbol.FamilyName, layout.TitleBlockFamilyName, StringComparison.Ordinal)
                    && string.Equals(symbol.Name, layout.TitleBlockTypeName, StringComparison.Ordinal));
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
        /// Any view of the same type on any plot, template or sheet excluded. It is the view
        /// the team built, so how it is set up is the answer to how a new one should be.
        /// </summary>
        private static View SiblingOfType(Document document, ViewType type)
        {
            foreach (View view in new FilteredElementCollector(document)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(view => !view.IsTemplate && !(view is ViewSheet)))
            {
                ParsedViewName parsed;
                if (!ViewNameParser.TryParse(view.Name, out parsed)) continue;
                if (parsed.Type.Equals(type)) return view;
            }

            return null;
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
            View made, View sibling, RunItem item, RunOutcome outcome)
        {
            if (sibling.ViewTemplateId == ElementId.InvalidElementId)
            {
                outcome.NeedsAttention(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "Created with no view template, because " + sibling.Name + " has none either. "
                    + "The scale, detail level, discipline and phase filter are whatever a new "
                    + "view gets by default."));
                return;
            }

            made.ViewTemplateId = sibling.ViewTemplateId;
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

            plot.Set(item.PlotId);
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

            CapturedSchedule wanted = captured.ForPlot(item.PlotId);

            ViewSchedule made = wanted.IsASheetList
                ? ViewSchedule.CreateSheetList(document)
                : ViewSchedule.CreateSchedule(document, CategoryIdFor(document, wanted.CategoryName));

            made.Name = wanted.NameFor(item.PlotId);

            Autodesk.Revit.DB.ScheduleDefinition definition = made.Definition;
            definition.IncludeLinkedFiles = wanted.IncludesLinkedFiles;

            Dictionary<string, SchedulableField> available = definition.GetSchedulableFields()
                .GroupBy(field => field.GetName(document), StringComparer.Ordinal)
                .ToDictionary(byName => byName.Key, byName => byName.First(), StringComparer.Ordinal);

            // Added in the captured order, because the order is what the schedule looks like and
            // a field list in a different order is a different schedule to the person reading it.
            var fieldByName = new Dictionary<string, ScheduleField>(StringComparer.Ordinal);
            var missingFields = new List<string>();

            foreach (string name in wanted.FieldsInOrder)
            {
                if (fieldByName.ContainsKey(name)) continue;

                SchedulableField schedulable;
                if (!available.TryGetValue(name, out schedulable))
                {
                    missingFields.Add(name);
                    continue;
                }

                fieldByName.Add(name, definition.AddField(schedulable));
            }

            var missingFilters = new List<string>();
            foreach (ScheduleFilterRule rule in wanted.Filters)
            {
                ScheduleField field;
                if (!fieldByName.TryGetValue(rule.ParameterName, out field))
                {
                    missingFilters.Add(rule.ToString());
                    continue;
                }

                definition.AddFilter(new ScheduleFilter(field.FieldId, ScheduleFilterType.Equal, rule.Value));
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
                outcome.NeedsAttention(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "Created without " + missingFields.Count + " of its fields, because they are "
                    + "not schedulable for this category here. Missing: "
                    + string.Join(", ", missingFields.ToArray())
                    + ". The schedule is short of those columns."));
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

        private static ElementId CategoryIdFor(Document document, string categoryName)
        {
            foreach (Category category in document.Settings.Categories.Cast<Category>())
            {
                if (string.Equals(category.Name, categoryName, StringComparison.Ordinal))
                {
                    return category.Id;
                }
            }

            throw new InvalidOperationException(
                "This model has no category named " + categoryName + ", so that schedule cannot be built.");
        }
    }
}
