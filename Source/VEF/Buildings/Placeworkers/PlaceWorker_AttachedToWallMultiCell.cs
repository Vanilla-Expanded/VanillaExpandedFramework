﻿using System.Linq;
using RimWorld;
using Verse;

namespace VEF.Buildings;

public class PlaceWorker_AttachedToWallMultiCell : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 centerPos, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        var cells = GenAdj.CellsOccupiedBy(centerPos, rot, checkingDef.Size).ToList();
        foreach (var loc in cells)
        {
            var list = loc.GetThingList(map);
            for (var i = 0; i < list.Count; i++)
            {
                var otherThing = list[i];
                if (GenConstruct.BuiltDefOf(otherThing.def) is ThingDef { building: not null } otherThingDef)
                {
                    // Don't allow building if on top of a wall
                    if (otherThingDef.Fillage == FillCategory.Full)
                        return false;
                    // Don't allow building if there's another building facing the same direction
                    if (otherThingDef.building.isAttachment && otherThing.Rotation == rot)
                        return "SomethingPlacedOnThisWall".Translate();
                }
            }

            // Move 1 tile towards the wall, skip the check if it would overlap with the current building
            var adjancentPos = loc + GenAdj.CardinalDirections[rot.AsInt];
            if (cells.Contains(adjancentPos))
                continue;
            // Not possible to build if facing map edge, as there's only void there - no walls to attach to
            if (!adjancentPos.InBounds(map))
                return false;

            list = adjancentPos.GetThingList(map).ToList();

            var isFullFillage = false;
            var cannotSupportAttachment = false;

            for (var i = 0; i < list.Count; i++)
            {
                if (GenConstruct.BuiltDefOf(list[i].def) is ThingDef { building: not null } otherThing)
                {
                    // If there's a building that doesn't support attachments it will result in different failure message
                    if (!otherThing.building.supportsWallAttachments)
                    {
                        cannotSupportAttachment = true;
                    }
                    // Break if there's a full wall that supports attachments
                    else if (otherThing.Fillage == FillCategory.Full)
                    {
                        isFullFillage = true;
                        break;
                    }
                }
            }

            if (!isFullFillage)
            {
                // A failure message depending on if there's a building or not
                if (cannotSupportAttachment)
                    return "CannotSupportAttachment".Translate();
                return "MustPlaceOnWall".Translate();
            }
        }

        // All the checks passed, we're allowed to build here
        return true;
    }
}