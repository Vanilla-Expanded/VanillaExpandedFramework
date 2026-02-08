using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace VEF.Apparels
{
    [HarmonyPatch(typeof(HediffSet), nameof(HediffSet.BleedRateTotal), MethodType.Getter)]
    public static class VanillaExpandedFramework_HediffSet_BleedRateTotal_Patch
    {
        public static void Postfix(ref float __result, HediffSet __instance)
        {
            if (__result > 0 && __instance.pawn != null)
            {
                if (PreventsBleeding(__instance.pawn.apparel?.WornApparel) || PreventsBleeding(__instance.pawn.equipment?.AllEquipmentListForReading))
                    __result = 0;
            }
        }

        private static bool PreventsBleeding<T>(List<T> list) where T : Thing
        {
            if (list == null)
                return false;

            foreach (var apparel in list)
            {
                var extension = apparel.def.GetModExtension<ApparelExtension>();
                if (extension is { preventBleeding: true })
                    return true;
            }

            return false;
        }
    }
}