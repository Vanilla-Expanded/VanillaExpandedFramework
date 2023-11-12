using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), "ExposeData")]
    public static class Pawn_RelationsTracker_ExposeData_Patch
    {
        public static Dictionary<Pawn_RelationsTracker, PregnancyApproachData> pawnPregnancyApproachData = new();
        public static void Postfix(Pawn_RelationsTracker __instance)
        {
            var data = __instance.GetAdditionalPregnancyApproachData();
            Scribe_Deep.Look(ref data, "additionalPregnancyApproachData");
            if (data != null)
            {
                pawnPregnancyApproachData[__instance] = data;
            }
        }
        public static PregnancyApproachData GetAdditionalPregnancyApproachData(this Pawn_RelationsTracker tracker)
        {
            if (tracker is not null)
            {
                if (!pawnPregnancyApproachData.TryGetValue(tracker, out var data) || data is null)
                {
                    pawnPregnancyApproachData[tracker] = data = new PregnancyApproachData();
                }
                data.partners ??= new Dictionary<Pawn, PregnancyApproachDef>();
                return data;
            }
            throw new System.Exception("Pawn_RelationsTracker was null by some reason");
        }
    }
}
