using Verse;
using RimWorld;
using HarmonyLib;
using System;

namespace VFECore
{
    [HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
    public static class StatExtension_GetStatValue_Patch
    {
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(Thing thing, StatDef stat, bool applyPostProcess, ref float __result)
        {
            if (stat == StatDefOf.RangedWeapon_Cooldown && thing?.ParentHolder is Pawn_EquipmentTracker eq)
            {
                __result /= eq.pawn.GetStatValue(VFEDefOf.VEF_RangeAttackSpeedFactor);
            }
        }
    }



    [HarmonyPatch(typeof(Tool), "AdjustedCooldown", new Type[] { typeof(Thing) })]
    public static class Tool_AdjustedCooldown_Patch
    {
        public static void Postfix(Thing ownerEquipment, ref float __result)
        {
            if (ownerEquipment?.ParentHolder is Pawn_EquipmentTracker eq)
            {
                __result /= eq.pawn.GetStatValue(VFEDefOf.VEF_MeleeAttackSpeedFactor);
            }
        }
    }

    [HarmonyPatch(typeof(VerbProperties), "AdjustedMeleeDamageAmount", new Type[] { typeof(Verb), typeof(Pawn) })]
    public static class AdjustedMeleeDamageAmount_Patch
    {
        public static void Postfix(Verb ownerVerb, Pawn attacker, ref float __result)
        {
            __result *= attacker.GetStatValue(VFEDefOf.VEF_MeleeAttackDamageFactor);
        }
    }

    [HarmonyPatch(typeof(Projectile), "DamageAmount", MethodType.Getter)]
    public static class Projectile_DamageAmount_Patch
    {
        public static void Postfix(Projectile __instance, ref float __result)
        {
            if (__instance.Launcher is Pawn attacker)
            {
                __result *= attacker.GetStatValue(VFEDefOf.VEF_RangeAttackDamageFactor);
            }
        }
    }

}
