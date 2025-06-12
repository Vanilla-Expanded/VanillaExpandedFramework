using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Apparels
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class VanillaExpandedFramework_Pawn_Kill_Patch
    {
        private static bool Prefix(Pawn __instance)
        {
            if (__instance?.apparel?.WornApparel != null)
            {
                foreach (var apparel in __instance.apparel.WornApparel)
                {
                    var extension = apparel.def.GetModExtension<ApparelExtension>();
                    if (extension != null && extension.preventKilling)
                    {
                        var pawnBodyPercentage = (float)__instance.health.hediffSet.GetNotMissingParts()
                            .Sum(x => x.def.GetMaxHealth(__instance))
                            / (float)__instance.def.race.body.AllParts.Sum(x => x.def.GetMaxHealth(__instance));
                        if (extension.preventKillingUntilHealthHPPercentage < pawnBodyPercentage
                            && (!extension.preventKillingUntilBrainMissing || __instance.health.hediffSet.GetBrain() != null))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}