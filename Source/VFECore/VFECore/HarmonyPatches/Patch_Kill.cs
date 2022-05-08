using HarmonyLib;
using System.Linq;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Patch_Kill
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

        private static void Postfix(Pawn __instance)
        {
            if (__instance.Dead)
            {
                var extension = __instance.def.GetModExtension<ThingDefExtension>();
                if (extension != null && extension.destroyCorpse)
                {
                    if (__instance.Corpse != null && !__instance.Corpse.Destroyed)
                    {
                        __instance.Corpse.Destroy();
                    }
                }
            }
        }
    }
}