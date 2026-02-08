using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace VEF.Apparels
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDowned")]
    public static class VanillaExpandedFramework_Pawn_HealthTracker_ShouldBeDowned_Patch
    {
        private static bool Prefix(Pawn ___pawn)
        {
            if (___pawn != null)
            {
                if (PreventsDowning(___pawn.apparel?.WornApparel) || PreventsDowning(___pawn.equipment?.AllEquipmentListForReading))
                    return false;
            }

            return true;
        }

        private static bool PreventsDowning<T>(List<T> list) where T : Thing
        {
            if (list == null)
                return false;

            foreach (var thing in list)
            {
                if (thing.def.GetModExtension<ApparelExtension>() is { preventDowning: true })
                    return false;
            }

            return true;
        }
    }
}