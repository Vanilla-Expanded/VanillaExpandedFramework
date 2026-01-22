using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace VEF.Weapons
{

    [HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAiming", new Type[] { typeof(Thing), typeof(Vector3), typeof(float) }), StaticConstructorOnStartup]
    public static class VanillaExpandedFramework_PawnRenderUtility_Draw_EquipmentAiming_Patch
    {
        static void Prefix(Thing eq, ref Vector3 drawLoc, ref float aimAngle)
        {
            if (eq.GetPawnAsHolder() is not { } pawn) return;
            if (eq is not IDrawnWeaponWithRotation gun) return;

            if (pawn.stances.curStance is Stance_Busy { neverAimWeapon: false, focusTarg.IsValid: true })
            {
                drawLoc -= new Vector3(0f, 0f, 0.4f).RotatedBy(aimAngle);
                aimAngle = (aimAngle + gun.RotationOffset) % 360;
                drawLoc += new Vector3(0f, 0f, 0.4f).RotatedBy(aimAngle);
            }

        }
    }
}
