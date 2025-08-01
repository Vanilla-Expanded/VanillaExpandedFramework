﻿using HarmonyLib;
using Verse;

namespace VEF.Apparels
{
    [HarmonyPatch(typeof(HediffSet), nameof(HediffSet.BleedRateTotal), MethodType.Getter)]
    public static class VanillaExpandedFramework_HediffSet_BleedRateTotal_Patch
    {
        public static void Postfix(ref float __result, HediffSet __instance)
        {
            if (__result > 0 && __instance.pawn?.apparel?.WornApparel != null)
            {
                foreach (var apparel in __instance.pawn.apparel.WornApparel)
                {
                    var extension = apparel.def.GetModExtension<ApparelExtension>();
                    if (extension != null && extension.preventBleeding)
                    {
                        __result = 0;
                    }
                }
            }
        }
    }
}