using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace VEF.Apparels
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class VanillaExpandedFramework_Pawn_Kill_Patch
    {
        private static bool Prefix(Pawn __instance, out List<Thing> __state)
        {
            List<Thing> gearToRemove = null;
            var allowedToDie = true;

            HandleEquipment(__instance.apparel?.WornApparel);
            if (allowedToDie)
                HandleEquipment(__instance.equipment?.AllEquipmentListForReading);

            if (!allowedToDie)
            {
                __state = null;
                return false;
            }

            __state = gearToRemove;
            return true;

            void HandleEquipment<T>(List<T> list) where T : Thing
            {
                if (list == null)
                    return;

                foreach (var equipment in list)
                {
                    var extension = equipment.def.GetModExtension<ApparelExtension>();
                    if (extension == null)
                        continue;

                    if (extension.preventKilling)
                    {
                        var pawnBodyPercentage = (float)__instance.health.hediffSet.GetNotMissingParts()
                            .Sum(x => x.def.GetMaxHealth(__instance))
                            / (float)__instance.def.race.body.AllParts.Sum(x => x.def.GetMaxHealth(__instance));
                        if (extension.preventKillingUntilHealthHPPercentage < pawnBodyPercentage
                            && (!extension.preventKillingUntilBrainMissing || __instance.health.hediffSet.GetBrain() != null))
                        {
                            allowedToDie = false;
                            return;
                        }
                    }

                    if (extension.destroyedOnDeath)
                    {
                        gearToRemove ??= [];
                        gearToRemove.Add(equipment);
                    }
                }
            }
        }

		private static void Postfix(Pawn __instance, List<Thing> __state)
        {
            // Make sure the pawn actually died and we have any equipment to remove
            if (!__instance.Dead || __state.NullOrEmpty())
                return;

            foreach (var thing in __state)
            {
                if (!thing.Destroyed)
                    thing.Destroy();
            }
        }
    }
}