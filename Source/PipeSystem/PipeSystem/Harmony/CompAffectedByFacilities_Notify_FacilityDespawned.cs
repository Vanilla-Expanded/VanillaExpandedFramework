using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace PipeSystem
{
   
    [HarmonyPatch(typeof(CompAffectedByFacilities))]
    [HarmonyPatch("Notify_FacilityDespawned")]
    public static class PipeSystem_CompAffectedByFacilities_Notify_FacilityDespawned_Patch
    {
        public static void Postfix(CompAffectedByFacilities __instance)
        {
            CompAdvancedResourceProcessor processor = __instance.parent.GetComp<CompAdvancedResourceProcessor>();
            if (processor != null) {
                processor.overclockMultiplier = Math.Min(processor.overclockMultiplier, processor.Props.maxOverclock * processor.parent.GetStatValue(PSDefOf.VEF_BuildingMaxOverclockFactor));           
            }
        }
    }
}