using Verse;
using RimWorld;
using HarmonyLib;
using System;
using System.Text;

namespace VFECore
{
    public class StatWorker_CaravanMassCarryCapacity : StatWorker
    {
        public static bool includeVanillaMassCapacityCalculation = true;
        public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
        {
            var pawn = req.Thing as Pawn;
            if (pawn != null)
            {
                if (includeVanillaMassCapacityCalculation)
                {
                    MassUtility_Capacity_Patch.includeStatWorkerResult = false;
                    val += MassUtility.Capacity(pawn); 
                    MassUtility_Capacity_Patch.includeStatWorkerResult = true;
                }
            }
            base.FinalizeValue(req, ref val, applyPostProcess);
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            var sb = new StringBuilder(base.GetExplanationUnfinalized(req, numberSense));
            var pawn = req.Thing as Pawn;
            if (includeVanillaMassCapacityCalculation && pawn != null)
            {
                MassUtility_Capacity_Patch.includeStatWorkerExplanation = false;
                MassUtility.Capacity(pawn, sb);
                MassUtility_Capacity_Patch.includeStatWorkerExplanation = true;
            }
            return sb.ToString().TrimEndNewlines();
        }
    }

    [HarmonyPatch(typeof(MassUtility), nameof(MassUtility.Capacity))]
    public static class MassUtility_Capacity_Patch
    {
        public static bool includeStatWorkerExplanation = true;
        public static bool includeStatWorkerResult = true;
        public static void Postfix(Pawn p, StringBuilder explanation, ref float __result)
        {
            StatWorker_CaravanMassCarryCapacity.includeVanillaMassCapacityCalculation = false;
            if (includeStatWorkerResult)
            {
                __result += p.GetStatValue(VFEDefOf.VEF_MassCarryCapacity);
            }
            if (includeStatWorkerExplanation)
            {
                explanation?.AppendInNewLine(VFEDefOf.VEF_MassCarryCapacity.Worker.GetExplanationFull(StatRequest.For(p), ToStringNumberSense.Offset, __result));
            }
            StatWorker_CaravanMassCarryCapacity.includeVanillaMassCapacityCalculation = true;
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

}
