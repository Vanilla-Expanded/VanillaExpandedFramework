﻿using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch(typeof(CompAffectedByFacilities))]
[HarmonyPatch(nameof(CompAffectedByFacilities.CanPotentiallyLinkTo_Static))]
[HarmonyPatch([typeof(ThingDef), typeof(IntVec3), typeof(Rot4), typeof(ThingDef), typeof(IntVec3), typeof(Rot4), typeof(Map)])]
[HarmonyPatchCategory(VEF_Mod.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_CompAffectedByFacilities_CanPotentiallyLinkTo_Static_Patch
{
    private static bool Prepare(MethodBase method)
    {
        // Only check for DefModExtension on first pass (method is null) and only patch if any
        // def in game uses our DefModExtension and uses facilities linking on interaction spots.
        // Slight downside, the patch won't be applied when hot-loading defs, only at startup.
        return method != null || DefDatabase<ThingDef>.AllDefs.Any(def => def.GetModExtension<FacilityExtension>() is { linkOnInteractionSpots: true });
    }

    private static bool Prefix(ThingDef facilityDef, IntVec3 facilityPos, Rot4 facilityRot, ThingDef myDef, IntVec3 myPos, Rot4 myRot, Map myMap, ref bool __result)
    {
        if (facilityDef.GetModExtension<FacilityExtension>() is not { linkOnInteractionSpots: true })
            return true;

        if (myDef.HasSingleOrMultipleInteractionCells)
        {
            var cells = ThingUtility.InteractionCellsWhenAt(myDef, myPos, myRot, myMap, true);
            var rect = GenAdj.OccupiedRect(facilityPos, facilityRot, facilityDef.Size);
            if (cells.Any(cell => rect.Contains(cell)))
            {
                __result = true;
                return true;
            }
        }

        __result = false;
        return false;
    }
}