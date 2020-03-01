using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public static class Patch_Pawn_HealthTracker
    {

        [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.CheckForStateChange))]
        public static class CheckForStateChange
        {

            public static void Postfix(Pawn_HealthTracker __instance, Pawn ___pawn)
            {
                if (!__instance.Downed && ___pawn.equipment != null && ___pawn.equipment.OffHandShield() is ThingWithComps shield)
                {
                    // Not enough hands to use shields
                    if (!___pawn.CanUseShields())
                        ___pawn.equipment.TryDropEquipment(shield, out ThingWithComps s, ___pawn.PositionHeld);

                    // Cannot manipulate
                    else if (!__instance.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                    {
                        if (___pawn.kindDef.destroyGearOnDrop)
                            ___pawn.equipment.DestroyEquipment(shield);

                        else if (___pawn.InContainerEnclosed)
                            ___pawn.equipment.TryTransferEquipmentToContainer(shield, ___pawn.holdingOwner);

                        else if (___pawn.SpawnedOrAnyParentSpawned)
                            ___pawn.equipment.TryDropEquipment(shield, out ThingWithComps s, ___pawn.PositionHeld, true);

                        else
                            ___pawn.equipment.DestroyEquipment(shield);
                    }
                }
            }

        }

    }

}
