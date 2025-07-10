using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Apparels
{
    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Wear")]
    public static class VanillaExpandedFramework_ApparelTracker_Wear_Patch
    {
        public static void Postfix(Apparel newApparel, Pawn_ApparelTracker __instance)
        {
            if (__instance.pawn != null)
            {
                var apparelExtension = newApparel.def.GetModExtension<ApparelExtension>();
                if (apparelExtension != null && (apparelExtension.workDisables != WorkTags.None))
                {
                    __instance.pawn.Notify_DisabledWorkTypesChanged();
                }

            }
        }
    }

  
}