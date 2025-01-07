using Verse;
using RimWorld;
using HarmonyLib;
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace VFECore
{
    [HarmonyPatch(typeof(StatWorker), "GetBaseValueFor")]
    public static class StatWorker_GetBaseValueFor_Patch
    {
        public static void Postfix(StatDef ___stat, StatRequest request, ref float __result)
        {
            if (___stat == VFEDefOf.VEF_MassCarryCapacity && request.Thing is Pawn pawn)
            {
                MassUtility_Capacity_Patch.includeStatWorkerResult = false;
                __result += MassUtility.Capacity(pawn);
                MassUtility_Capacity_Patch.includeStatWorkerResult = true;
            }
        }
    }

    [HarmonyPatch(typeof(MassUtility), nameof(MassUtility.Capacity))]
    public static class MassUtility_Capacity_Patch
    {
        public static bool includeStatWorkerResult = true;
        public static MethodInfo SetCarryCapacityInfo = AccessTools.Method(typeof(MassUtility_Capacity_Patch), "SetCarryCapacity");
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            foreach (var code in codeInstructions)
            {
                yield return code;
                if (code.opcode == OpCodes.Stloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Call, SetCarryCapacityInfo);
                }
            }
        }

        public static void SetCarryCapacity(Pawn p, ref float __result)
        {
            if (includeStatWorkerResult)
            {
                __result = p.GetStatValue(VFEDefOf.VEF_MassCarryCapacity);
            }
        }
    }

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
        public static void Postfix(Projectile __instance, ref int __result)
        {
            if (__instance.Launcher is Pawn attacker)
            {
                __result = (int)(__result * attacker.GetStatValue(VFEDefOf.VEF_RangeAttackDamageFactor));
            }
        }
    }

    [HarmonyPatch(typeof(JobDriver_Lovin))]
    [HarmonyPatch("GenerateRandomMinTicksToNextLovin")]
    public static class VFECore_JobDriver_Lovin_GenerateRandomMinTicksToNextLovin_Patch
    {
        [HarmonyPostfix]
        public static void ModifyMTB(ref int __result, Pawn pawn)
        {

            
                __result = (int)(__result * pawn.GetStatValue(VFEDefOf.VEF_MTBLovinFactor));
            



        }
    }

}
