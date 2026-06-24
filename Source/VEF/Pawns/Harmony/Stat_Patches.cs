using Verse;
using RimWorld;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;

namespace VEF.Pawns
{
    [HarmonyPatch(typeof(MassUtility), nameof(MassUtility.Capacity))]
    public static class VanillaExpandedFramework_MassUtility_Capacity_Patch
    {
        public static bool includeStatWorkerResult = true;
        public static MethodInfo SetCarryCapacityInfo = AccessTools.Method(typeof(VanillaExpandedFramework_MassUtility_Capacity_Patch), "SetCarryCapacity");
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
                __result = p.GetStatValue(VEFDefOf.VEF_MassCarryCapacity);
            }
        }
    }

    [HarmonyPatch(typeof(VerbProperties), "AdjustedMeleeDamageAmount", new Type[] { typeof(Verb), typeof(Pawn) })]
    public static class VanillaExpandedFramework_AdjustedMeleeDamageAmount_Patch
    {
        public static void Postfix(Verb ownerVerb, Pawn attacker, ref float __result)
        {
            __result *= attacker.GetStatValue(VEFDefOf.VEF_MeleeAttackDamageFactor);
        }
    }

    [HarmonyPatch(typeof(Projectile), "DamageAmount", MethodType.Getter)]
    public static class VanillaExpandedFramework_Projectile_DamageAmount_Patch
    {
        public static void Postfix(Projectile __instance, ref int __result)
        {
            if (__instance.Launcher is Pawn attacker)
            {
                __result = (int)(__result * attacker.GetStatValue(VEFDefOf.VEF_RangeAttackDamageFactor));
            }
        }
    }

    [HarmonyPatch(typeof(JobDriver_Lovin))]
    [HarmonyPatch("GenerateRandomMinTicksToNextLovin")]
    public static class VanillaExpandedFramework_JobDriver_Lovin_GenerateRandomMinTicksToNextLovin_Patch
    {
        [HarmonyPostfix]
        public static void ModifyMTB(ref int __result, Pawn pawn)
        {

            
                __result = (int)(__result * pawn.GetStatValue(VEFDefOf.VEF_MTBLovinFactor));
            



        }
    }

    [HarmonyPatch(typeof(HediffGiver_Heat), nameof(HediffGiver_Heat.OnIntervalPassed))]
    public static class VanillaExpandedFramework_HediffGiver_Heat_OnIntervalPassed_Patch
    {
       
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            MethodInfo ModifyHeatstrokeSeverityAdvance = AccessTools.Method(typeof(VanillaExpandedFramework_HediffGiver_Heat_OnIntervalPassed_Patch), "ModifyHeatstrokeSeverityAdvance");
            FieldInfo hediffField = AccessTools.DeclaredField(typeof(HediffGiver), nameof(HediffGiver.hediff));

            for (var i = 0; i < codes.Count; i++)
            {

                if (i > 0 && codes[i - 1].LoadsField(hediffField) && codes[i].opcode == OpCodes.Ldloc_S && codes[i].operand is LocalBuilder lb && lb.LocalIndex == 5)
                {

                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Call, ModifyHeatstrokeSeverityAdvance);

                }

                else yield return codes[i];
            }
        }

        public static float ModifyHeatstrokeSeverityAdvance(Pawn p, float rate)
        {
            if (p != null)
            {
                float heatStrokeStat = p.GetStatValue(InternalDefOf.VEF_HeatstrokeBuildupMultiplier);                
                return rate*heatStrokeStat;               
            }
            return rate;
        }
    }

}
