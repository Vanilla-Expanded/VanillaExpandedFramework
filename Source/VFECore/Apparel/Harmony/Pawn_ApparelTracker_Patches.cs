using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaApparelExpanded
{
    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Wear")]
    public static class ApparelTracker_Wear
    {
        public static void Postfix(Apparel newApparel, Pawn_ApparelTracker __instance)
        {
            if (__instance.pawn != null)
            {
                var apparelExtension = newApparel.def.GetModExtension<ApparelExtension>();
                if (apparelExtension != null && (apparelExtension.workDisables?.Any() ?? false))
                {
                    __instance.pawn.Notify_DisabledWorkTypesChanged();
                }

            }
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "TryDrop", 
        new Type[] { typeof(Apparel), typeof(Apparel), typeof(IntVec3), typeof(bool) },
    new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal })]
    public static class Patch_TryDrop
    {
        private static void Postfix(Pawn_ApparelTracker __instance, Apparel ap)
        {
            if (__instance.pawn != null)
            {
                var apparelExtension = ap.def.GetModExtension<ApparelExtension>();
                if (apparelExtension != null && (apparelExtension.workDisables?.Any() ?? false))
                {
                    __instance.pawn.Notify_DisabledWorkTypesChanged();
                }
            }
        }
    }
}