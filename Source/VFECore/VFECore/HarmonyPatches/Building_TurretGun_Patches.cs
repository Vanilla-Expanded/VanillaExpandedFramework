using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using VFEPirates;

namespace VFECore
{

    [HarmonyPatch(typeof(Building_TurretGun), "IsValidTarget")]
    class Building_TurretGun_IsValidTarget_Patch
    {
        public static void Postfix(Thing t, Building_TurretGun __instance, ref bool __result)
        {
            if (__instance.AttackVerb is Verb_ShootCone verbShootCone)
            {
                __result &= verbShootCone.InCone(t.Position, verbShootCone.caster.Position, verbShootCone.Caster.Rotation, verbShootCone.VerbProps.coneAngle);
            }
        }
    }

    [HarmonyPatch(typeof(Building_TurretGun), "DrawExtraSelectionOverlays")]
    public static class Building_TurretGun_DrawExtraSelectionOverlays_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            bool flag = false; //only apply patch to first occurence of DrawRadiusRing
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].operand as MethodInfo == typeof(GenDraw).GetMethod("DrawRadiusRing", new Type[] { typeof(IntVec3), typeof(float) }) && !flag)
                {
                    flag = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Building_TurretGun_DrawExtraSelectionOverlays_Transpiler), nameof(DrawConeForDirectionalTurret)));
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
        public static void DrawConeForDirectionalTurret(IntVec3 center, float radius, Building_TurretGun instance)
        {
            //Don't draw radius ring for directional turrets
            if (!(instance.AttackVerb is Verb_ShootCone))
            {
                GenDraw.DrawRadiusRing(instance.Position, radius);
            }
        }
    }

    [HarmonyPatch(typeof(Building_TurretGun), "DrawExtraSelectionOverlays")]
    public static class Building_TurretGun_DrawExtraSelectionOverlays_Patch
    {
        public static void Postfix(Building_TurretGun __instance)
        {
            //Draw cone instead of radius ring for directional turret
            if ((__instance.AttackVerb is Verb_ShootCone verb_ShootCone))
            {
                verb_ShootCone.DrawHighlight(verb_ShootCone.CurrentTarget);
            }
        }
    }

    //Don't rotate directional turrets. 
    [HarmonyPatch(typeof(TurretTop), "get_CurRotation")]
    class TurretTop_get_CurRotation_Patch
    {
        public static bool Prefix(ref Building_Turret ___parentTurret, ref int ___ticksUntilIdleTurn, ref float __result)
        {
            if (___parentTurret.AttackVerb is Verb_ShootCone)
            {
                var currentTarget = ___parentTurret.CurrentTarget;
                if (!currentTarget.IsValid)
                {
                    __result = ___parentTurret.Rotation.AsAngle;
                    return false;
                }
            }
            return true;
        }
    }
}