using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace VanillaWeaponsExpandedLaser.HarmonyPatches
{

    [HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAiming", new Type[] { typeof(Thing), typeof(Vector3), typeof(float) }), StaticConstructorOnStartup]
    public static class CYA_PawnRenderUtility_Draw_EquipmentAiming_GunDrawing_Patch
    {
        static FieldInfo pawnField;

        static CYA_PawnRenderUtility_Draw_EquipmentAiming_GunDrawing_Patch()
        {
            pawnField = typeof(PawnRenderer).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        static void Prefix(ref Thing eq, ref Vector3 drawLoc, ref float aimAngle, PawnRenderer __instance)
        {
            if(__instance != null)
            {
                Pawn pawn = pawnField.GetValue(__instance) as Pawn;
                if (pawn == null) return;

                IDrawnWeaponWithRotation gun = eq as IDrawnWeaponWithRotation;
                if (gun == null) return;

                Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
                if (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
                {
                    drawLoc -= new Vector3(0f, 0f, 0.4f).RotatedBy(aimAngle);
                    aimAngle = (aimAngle + gun.RotationOffset) % 360;
                    drawLoc += new Vector3(0f, 0f, 0.4f).RotatedBy(aimAngle);
                }
            }
            
        }
    }
}
