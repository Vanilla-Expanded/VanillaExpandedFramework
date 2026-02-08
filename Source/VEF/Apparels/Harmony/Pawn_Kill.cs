using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace VEF.Apparels
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class VanillaExpandedFramework_Pawn_Kill_Patch
    {
        private static bool Prefix(Pawn __instance)
        {
            if (__instance != null)
            {
                if (PreventsDowning(__instance.apparel?.WornApparel) || PreventsDowning(__instance.equipment?.AllEquipmentListForReading))
                    return false;
            }

            return true;

            bool PreventsDowning<T>(List<T> list) where T : Thing
            {
                if (list == null)
                    return false;

                foreach (var equipment in list)
                {
                    var extension = equipment.def.GetModExtension<ApparelExtension>();
                    if (extension is { preventKilling: true })
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

                return true;
            }
        }
    }
}