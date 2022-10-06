using HarmonyLib;
using UnityEngine;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming))]
    public static class PawnRenderer_DrawEquipmentAiming_Patch
    {
        [HarmonyDelegate(typeof(PawnRenderer), "CarryWeaponOpenly")]
        public delegate bool CarryWeaponOpenly();
        [HarmonyPriority(Priority.First)]
        public static void Prefix(PawnRenderer __instance, Pawn ___pawn, Thing eq, ref Vector3 drawLoc, ref float aimAngle, CarryWeaponOpenly carryWeaponOpenly)
        {
            var thingDefExtension = eq.def.GetModExtension<ThingDefExtension>();
            if (thingDefExtension?.weaponCarryDrawOffsets != null)
            {
                if (carryWeaponOpenly())
                {
                    var pawn = ___pawn;
                    var pawnRot = pawn.Rotation;
                    if (pawnRot == Rot4.South)
                    {
                        drawLoc += thingDefExtension.weaponCarryDrawOffsets.south.drawOffset;
                        aimAngle += thingDefExtension.weaponCarryDrawOffsets.south.angleOffset;
                    }
                    else if (pawnRot == Rot4.North)
                    {
                        drawLoc += thingDefExtension.weaponCarryDrawOffsets.north.drawOffset;
                        aimAngle += thingDefExtension.weaponCarryDrawOffsets.north.angleOffset;
                    }
                    else if (pawnRot == Rot4.East)
                    {
                        drawLoc += thingDefExtension.weaponCarryDrawOffsets.east.drawOffset;
                        aimAngle += thingDefExtension.weaponCarryDrawOffsets.east.angleOffset;
                    }
                    else if (pawnRot == Rot4.West)
                    {
                        drawLoc += thingDefExtension.weaponCarryDrawOffsets.west.drawOffset;
                        aimAngle += thingDefExtension.weaponCarryDrawOffsets.west.angleOffset;
                    }
                }
            }
        }
    }
}